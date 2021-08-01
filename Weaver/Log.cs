using System;

namespace Mirror.Weaver
{
    public static class Log
    {

        public static void Warning(string msg)
        {
            Console.WriteLine("Warning: " + msg);
        }

        public static void Error(string msg)
        {
            Console.WriteLine("Error: " + msg);
        }
    }
}
