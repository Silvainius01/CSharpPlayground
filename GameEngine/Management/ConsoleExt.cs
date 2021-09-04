using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine
{
    public class ConsoleExt
    {
        public static void Write(string str, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write(str);
            Console.ResetColor();
        }
        public static void WriteLine(string str, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(str);
            Console.ResetColor();
        }

        public static void WriteError(string str) => Write(str, ConsoleColor.Red);
        public static void WriteErrorLine(string str) => WriteLine(str, ConsoleColor.Red);

        public static void WriteWarning(string str) => Write(str, ConsoleColor.Yellow);
        public static void WriteWarningLine(string str) => WriteLine(str, ConsoleColor.Yellow);

        public static void Write(ColoredString coloredString)
        {
            Console.ForegroundColor = coloredString.color;
            Console.Write(coloredString.str);
            Console.ResetColor();
        }
        public static void WriteLine(ColoredString coloredString)
        {
            Console.ForegroundColor = coloredString.color;
            Console.WriteLine(coloredString.str);
            Console.ResetColor();
        }

        public static void WriteLine(IEnumerable<ColoredString> strings)
        {
            foreach (var str in strings)
                WriteLineInternal(str);
        }
        public static void Write(IEnumerable<ColoredString> strings)
        {
            foreach (var str in strings)
                WriteInternal(str);
        }

        internal static void WriteInternal(ColoredString coloredString)
        {
            Console.ForegroundColor = coloredString.color;
            Console.Write(coloredString.str);
        }
        internal static void WriteLineInternal(ColoredString coloredString)
        {
            Console.ForegroundColor = coloredString.color;
            Console.WriteLine(coloredString.str);
        }
    }
}
