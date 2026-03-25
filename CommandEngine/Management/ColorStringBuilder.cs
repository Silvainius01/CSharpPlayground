using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static CommandEngine.StringBuilderManager;

namespace CommandEngine
{
    public struct ColoredString
    {
        public ConsoleColor color;
        public string str;

        public ColoredString(string str, ConsoleColor color)
        {
            this.color = color;
            this.str = str;
        }
    }

    public class ColorStringBuilder : SmartStringBuilder
    {
        ConsoleColor currColor = DefaultColor;
        List<ColoredString> strings = new List<ColoredString>();

        public ColorStringBuilder() { }
        public ColorStringBuilder(ConsoleColor startcolor) { currColor = startcolor; }
        public ColorStringBuilder(string tabString) : base(tabString) { }
        public ColorStringBuilder(string startString, ConsoleColor startColor)
        {
            builder = new StringBuilder(startString);
            currColor = startColor;
        }
        public ColorStringBuilder(string tabString, string startString, ConsoleColor startColor) : base(tabString)
        {
            builder = new StringBuilder(startString);
            currColor = startColor;
        }

        void SaveCurrent()
        {
            if (builder.Length > 0)
            {
                strings.Add(new ColoredString(builder.ToString(), currColor));
                builder.Clear();
            }
        }
        internal void SetColor(ConsoleColor color)
        {
            if (color != currColor)
                SaveCurrent();
            currColor = color;
        }

        public ColorStringBuilder Append(string str, ConsoleColor color)
        {
            SetColor(color);
            Append(str);
            return this;
        }
        public ColorStringBuilder AppendLine(string str, ConsoleColor color)
        {
            SetColor(color);
            AppendLine(str);
            return this;
        }
        public ColorStringBuilder AppendNewline(string str, ConsoleColor color)
        {
            SetColor(color);
            Append($"{NewlineString}{str}");
            return this;
        }
        public ColorStringBuilder NewlineAppend(string str, ConsoleColor color)
        {
            SetColor(color);
            Append($"{NewlineString}{str}");
            return this;
        }

        public ColorStringBuilder Append(int tabCount, string str, ConsoleColor color)
        {
            SetColor(color);
            CheckCapacity(str.Length, tabCount);

            AppendTabs(tabCount);
            Append(str);
            return this;
        }
        public ColorStringBuilder AppendLine(int tabCount, string str, ConsoleColor color)
        {
            SetColor(color);
            CheckCapacity(str.Length + 2, tabCount);

            AppendTabs(tabCount);
            AppendLine(str);
            return this;
        }
        public ColorStringBuilder AppendNewline(int tabCount, string str, ConsoleColor color)
        {
            SetColor(color);
            CheckCapacity(str.Length + NewlineString.Length, tabCount);

            AppendTabs(tabCount);
            Append($"{str}{NewlineString}");
            return this;
        }
        public ColorStringBuilder NewlineAppend(int tabCount, string str, ConsoleColor color)
        {
            SetColor(color);
            CheckCapacity(str.Length + NewlineString.Length, tabCount);

            Append(NewlineString);
            AppendTabs(tabCount);
            Append(str);
            return this;
        }

        /// <summary>Appends a colored string to this builder, but doesnt set the color. </summary>
        /// <param name="cstr"></param>
        /// <returns></returns>
        public ColorStringBuilder Append(ColoredString cstr)
        {
            SaveCurrent();
            strings.Add(cstr);
            return this;
        }
        public ColorStringBuilder Append(ColorStringBuilder cb)
        {
            foreach (var cstr in cb.strings)
                Append(cstr);
            Append(cb.ToString(), cb.currColor);
            return this;
        }

        public void Write(bool clear = false)
        {
            foreach (var str in strings)
                ConsoleExt.WriteInternal(str);
            ConsoleExt.WriteInternal(new ColoredString(builder.ToString(), currColor));
            Console.ResetColor();
            if (clear) Clear();
        }
        public void WriteLine(bool clear = false)
        {
            Write(clear);
            Console.WriteLine();
        }

        public new void Clear()
        {
            strings.Clear();
            builder.Clear();
            Console.ResetColor();
        }

        public int TotalLength()
        {
            int length = builder.Length;
            for (int i = 0; i < strings.Count; ++i)
            {
                length += strings[i].str.Length;
            }
            return length;
        }
    }
}
