using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine
{
    public class SmartStringBuilder
    {
        public string TabString { get; set; } = "\t";
        public string NewString { get; set; } = "\n";
        public StringBuilder builder;

        public SmartStringBuilder() : this("\t", "\n") { }
        public SmartStringBuilder(string tabString) : this(tabString, "\n") { }
        public SmartStringBuilder(string tabString, string newString)
        {
            this.TabString = tabString;
            this.NewString = newString;
            this.builder = new StringBuilder();
        }

        public SmartStringBuilder Append(string str) 
        {
            builder.Append(str); 
            return this;
        }
        public SmartStringBuilder AppendLine(string str)
        {
            builder.AppendLine(str);
            return this;
        }
        /// <summary>Append  <paramref name="str"/> followed by <see cref="SmartStringBuilder.NewString"/></summary>
        public SmartStringBuilder AppendNewline(string str)
        {
            builder.Append($"{NewString}{str}");
            return this;
        }
        /// <summary>Append <see cref="SmartStringBuilder.NewString"/>, follwed by <paramref name="str"/></summary>
        public SmartStringBuilder NewlineAppend(string str)
        {
            builder.Append($"{NewString}{str}");
            return this;
        }

        public SmartStringBuilder Append(int tabCount, string str)
        {
            CheckCapacity(str.Length, tabCount);
            AppendTabs(tabCount);
            builder.Append(str);
            return this;
        }
        public SmartStringBuilder AppendLine(int tabCount, string str)
        {
            CheckCapacity(str.Length+2, tabCount);
            AppendTabs(tabCount);
            builder.AppendLine(str);
            return this;
        }
        /// <summary>Append  <paramref name="str"/> followed by <see cref="SmartStringBuilder.NewString"/></summary>
        public SmartStringBuilder AppendNewline(int tabCount, string str)
        {
            CheckCapacity(str.Length + NewString.Length, tabCount);
            AppendTabs(tabCount);
            builder.Append($"{str}{NewString}");
            return this;
        }
        /// <summary>Append <see cref="SmartStringBuilder.NewString"/>, follwed by <paramref name="str"/></summary>
        public SmartStringBuilder NewlineAppend(int tabCount, string str)
        {
            CheckCapacity(str.Length + NewString.Length, tabCount);
            builder.Append(NewString);
            AppendTabs(tabCount);
            builder.Append(str);
            return this;
        }

        public SmartStringBuilder AppendTabs(int tabCount)
        {
            CheckCapacity(0, tabCount);
            for (int i = 0; i < tabCount; ++i)
                builder.Append(TabString);
            return this;
        }

        public void Clear()
        {
            builder.Clear();
        }

        /// <summary>Checks the capacity against the amount of characters to be added, then increases capacity if neccessary.</summary>
        /// <param name="length">Length of the string(s) to be appended</param>
        /// <param name="tabCount">Amount of tabs being appended</param>
        protected void CheckCapacity(int length, int tabCount)
        {
            int nextCap = builder.Length + length + TabString.Length * tabCount;
            if (nextCap > builder.Capacity)
                builder.Capacity = nextCap;
        }

        public override string ToString() => builder.ToString();
    }
}
