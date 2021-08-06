using System;

namespace Mirror.Weaver
{
    public static class Log
    {
        public static Action<string> Warning = Console.WriteLine;
        public static Action<string> Error = Console.WriteLine;
    }
}
