using Mirror.Weaver;

using System.IO;

namespace Mirror_External_Weaver
{

    class Program
    {
        static void Main(string[] args)
        {
            Weaver.WeaveAssembly(Path.GetFullPath(args[0].Trim('"')), new string[] { Path.GetFullPath(args[1].Trim('"')) });
        }

        public static string mirrorDllLocation = "Mirror.dll";
    }
}
