using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Mirror.Weaver
{
    /// <summary>
    /// Processes [Command] methods in NetworkBehaviour
    /// </summary>
    public static class CommandProcessor
    {
        /*
            // generates code like:
            public void CmdThrust(float thrusting, int spin)
            {
                NetworkWriter networkWriter = new NetworkWriter();
                networkWriter.Write(thrusting);
                networkWriter.WritePackedUInt32((uint)spin);
                base.SendCommandInternal(cmdName, networkWriter, cmdName);
            }

            public void CallCmdThrust(float thrusting, int spin)
            {
                // whatever the user was doing before
            }

            Originally HLAPI put the send message code inside the Call function
            and then proceeded to replace every call to CmdTrust with CallCmdTrust

            This method moves all the user's code into the "CallCmd" method
            and replaces the body of the original method with the send message code.
            This way we do not need to modify the code anywhere else,  and this works
            correctly in dependent assemblies
        */
        public static MethodDefinition ProcessCommandCall(TypeDefinition td, MethodDefinition md, CustomAttribute commandAttr)
        {
            MethodDefinition cmd = MethodProcessor.SubstituteMethod(td, md);

            ILProcessor worker = md.Body.GetILProcessor();

            NetworkBehaviourProcessor.WriteSetupLocals(worker);

            // NetworkWriter writer = new NetworkWriter();
            NetworkBehaviourProcessor.WriteCreateWriter(worker);

            // write all the arguments that the user passed to the Cmd call
            if (!NetworkBehaviourProcessor.WriteArguments(worker, md, RemoteCallType.Command))
                return null;

            string cmdName = md.Name;
            int channel = commandAttr.GetField("channel", 0);
            bool requiresAuthority = commandAttr.GetField("requiresAuthority", true);

            // invoke internal send and return
            // load 'base.' to call the SendCommand function with
            worker.Emit(OpCodes.Ldarg_0);
            worker.Emit(OpCodes.Ldtoken, td);
            // invokerClass
            worker.Emit(OpCodes.Call, WeaverTypes.getTypeFromHandleReference);
            worker.Emit(OpCodes.Ldstr, cmdName);
            // writer
            worker.Emit(OpCodes.Ldloc_0);
            worker.Emit(OpCodes.Ldc_I4, channel);
            // requiresAuthority ? 1 : 0
            worker.Emit(requiresAuthority ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
            worker.Emit(OpCodes.Call, WeaverTypes.sendCommandInternal);

            NetworkBehaviourProcessor.WriteRecycleWriter(worker);

            worker.Emit(OpCodes.Ret);
            return cmd;
        }

        /*
            // generates code like:
            protected static void InvokeCmdCmdThrust(NetworkBehaviour obj, NetworkReader reader, NetworkConnection senderConnection)
            {
                if (!NetworkServer.active)
                {
                    return;
                }
                ((ShipControl)obj).CmdThrust(reader.ReadSingle(), (int)reader.ReadPackedUInt32());
            }
        */
        public static MethodDefinition ProcessCommandInvoke(TypeDefinition td, MethodDefinition method, MethodDefinition cmdCallFunc)
        {
            MethodDefinition cmd = new MethodDefinition(Weaver.InvokeRpcPrefix + method.Name,
                MethodAttributes.Family | MethodAttributes.Static | MethodAttributes.HideBySig,
                WeaverTypes.Import(typeof(void)));

            ILProcessor worker = cmd.Body.GetILProcessor();
            Instruction label = worker.Create(OpCodes.Nop);

            NetworkBehaviourProcessor.WriteServerActiveCheck(worker, method.Name, label, "Command");

            // setup for reader
            worker.Emit(OpCodes.Ldarg_0);
            worker.Emit(OpCodes.Castclass, td);

            if (!NetworkBehaviourProcessor.ReadArguments(method, worker, RemoteCallType.Command))
                return null;

            AddSenderConnection(method, worker);

            // invoke actual command function
            worker.Emit(OpCodes.Callvirt, cmdCallFunc);
            worker.Emit(OpCodes.Ret);

            NetworkBehaviourProcessor.AddInvokeParameters(cmd.Parameters);

            td.Methods.Add(cmd);
            return cmd;
        }

        static void AddSenderConnection(MethodDefinition method, ILProcessor worker)
        {
            foreach (ParameterDefinition param in method.Parameters)
            {
                if (NetworkBehaviourProcessor.IsSenderConnection(param, RemoteCallType.Command))
                {
                    // NetworkConnection is 3nd arg (arg0 is "obj" not "this" because method is static)
                    // example: static void InvokeCmdCmdSendCommand(NetworkBehaviour obj, NetworkReader reader, NetworkConnection connection)
                    worker.Emit(OpCodes.Ldarg_2);
                }
            }
        }
    }
}
