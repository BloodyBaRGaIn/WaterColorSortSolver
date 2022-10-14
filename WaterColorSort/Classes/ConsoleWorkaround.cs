using System;
using System.Threading.Tasks;

namespace WaterColorSort.Classes
{
    internal static class ConsoleWorkaround
    {
        static ConsoleWorkaround()
        {
            _ = Task.Run(ExitConsole);
        }

        private static void ExitConsole()
        {
            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Escape)
                {
                    Environment.Exit(0);
                }
            }
        }

        internal static void Write(object obj)
        {
#if DEBUG
            Console.Write(obj);
#endif
        }

        internal static void WriteLine(object obj)
        {
#if DEBUG
            Console.WriteLine(obj);
#endif
        }
    }
}
