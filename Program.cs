using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirror.Weaver;

namespace Mirror_External_Weaver
{

    class Program
    {
        static void Main(string[] args)
        {
            Weaver.WeaveAssembly(args[0], new string[] { args[1] });
        }

        public static string mirrorDllLocation = "Mirror.dll";
    }
}
