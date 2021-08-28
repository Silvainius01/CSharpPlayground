using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine
{
    public class ConsoleExt
    {
        public static void WriteLine(ColoredString coloredString)
        {
            Console.ForegroundColor = coloredString.color;
            Console.WriteLine(coloredString.str);
            Console.ResetColor();
        }

        public static void Write(ColoredString coloredString)
        {
            Console.ForegroundColor = coloredString.color;
            Console.Write(coloredString.str);
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

        internal static void WriteLineInternal(ColoredString coloredString)
        {
            Console.ForegroundColor = coloredString.color;
            Console.WriteLine(coloredString.str);
        }

        internal static void WriteInternal(ColoredString coloredString)
        {
            Console.ForegroundColor = coloredString.color;
            Console.Write(coloredString.str);
        }
    }
}
