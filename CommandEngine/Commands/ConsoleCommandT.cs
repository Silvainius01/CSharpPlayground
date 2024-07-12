using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CommandEngine
{
    /// <summary>
    /// Executes a delegate to desired functionality based on a passed input string.
    /// </summary>
    /// <typeparam name="T">Return type of <see cref="Execute(string)"/></typeparam>
    public class ConsoleCommand<T>
    {
        public struct ArgumentInfo
        {
            public bool IsOptional { get; private set; }
            public string Name { get; private set; }
            public string Description { get; private set; }
            public Type ExpectedType { get; private set; }

            public string GetShortHelpString()
            {
                if (IsOptional)
                    return $"[{this.Name}: {ExpectedType.FullName}]";
                else return $"<{this.Name}: {ExpectedType.FullName}>";
            }

            public string GetLongHelpString(int tabCount)
                => GetLongHelpString(new SmartStringBuilder(), tabCount).ToString();
            public SmartStringBuilder GetLongHelpString(SmartStringBuilder sb, int tabCount)
            {
                sb.AppendNewline(tabCount, GetShortHelpString());
                ++tabCount;
                sb.AppendNewline(tabCount, Description);
                sb.AppendNewline(tabCount, $"IsOptional: {IsOptional}");
                --tabCount;

                return sb;
            }
        }
        public class Flag
        {
            public bool IsSet { get; internal set; }
            public bool IsMandatory { get; private set; }
            public string Name { get; private set; }
            public string Alias { get; private set; }
            public string Description { get; private set; }
            public List<ArgumentInfo> ArgumentsInfo { get; private set; }

            public Flag(string name, string alias, params ArgumentInfo[] arguments)
            {
                Name = name;
                Alias = alias;
                ArgumentsInfo = new List<ArgumentInfo>(arguments);
            }

            public string GetShortHelpString()
            {
                StringBuilder sb = new StringBuilder();

                sb.Append($"-{Alias}, --{Name} ");
                foreach (var arg in ArgumentsInfo)
                    sb.Append(arg.GetShortHelpString());

                return sb.ToString();
            }

            public string GetLongHelpString(int tabCount)
                => GetLongHelpString(new SmartStringBuilder(), tabCount).ToString();
            public SmartStringBuilder GetLongHelpString(SmartStringBuilder sb, int tabCount)
            {
                sb.AppendNewline(tabCount, GetShortHelpString());
                ++tabCount;
                sb.AppendNewline(tabCount, Description);
                foreach (var arg in ArgumentsInfo)
                    sb.AppendNewline(arg.GetLongHelpString(tabCount));
                --tabCount;

                return sb;
            }
        }

        public delegate bool ExecutionDelegate<TReturn>(List<string> args, out TReturn result);

        public string Name { get; private set; }
        public string Description { get; private set; } = string.Empty;
        public string Category { get; private set; } = string.Empty;

        public List<string> arguments = new List<string>();

        protected ExecutionDelegate<T> ExecuteDelegate { get; set; }
        protected List<Flag> flags = new List<Flag>();
        protected List<ArgumentInfo> argumentsInfo = new List<ArgumentInfo>();

        public ConsoleCommand(string name, ExecutionDelegate<T> executionDelegate, string description = "", string category = "")
        {
            Name = name;
            ExecuteDelegate = executionDelegate;
            Description = description;
            Category = category;
        }

        internal virtual bool Execute(string inputStr, out T result)
        {
            ParseInput(inputStr, out string errorMsg);
            return ExecuteDelegate(arguments, out result);
        }

        protected bool ParseInput(string input, out string errorMsg)
        {
            bool quote = false;
            StringBuilder builder = new StringBuilder(input.Length);

            void AddArgument()
            {
                // Only add an argument if it contains characters.
                if (builder.Length > 0)
                {
                    arguments.Add(builder.ToString());
                }
                builder.Clear();
            }

            arguments.Clear();

            // Parse command
            for (int index = 0; index < input.Length; ++index)
            {
                switch (input[index])
                {
                    // Backslash always adds next character
                    // Eat back slash if it's the last character.
                    case '\\':
                        if (index != input.Length - 1)
                            builder.Append(input[++index]);
                        break;
                    case ' ':
                    case '\t': // Eat whitespace, unless inside quotes.
                        if (quote)
                            builder.Append(input[index]);
                        else AddArgument();
                        break;
                    case '"': // Toggle quote state. If at the end, add the current argument.
                        quote = !quote;
                        if (!quote)
                            AddArgument();
                        break;
                    default: // Add any other character to the argument.
                        builder.Append(input[index]);
                        break;
                }
            }

            // Add any characters as an arugment to the end.
            AddArgument();

            errorMsg = "Parsed Successfully :)";
            return true;
        }

        public static KeyValuePair<string, ConsoleCommand<T>> Create(string name, ExecutionDelegate<T> d)
            => new KeyValuePair<string, ConsoleCommand<T>>(name, new ConsoleCommand<T>(name, d));

        public string GetShortHelpString()
        {
            StringBuilder sb = new StringBuilder($"{Name} ");

            foreach (var arg in argumentsInfo)
                sb.Append(arg.GetShortHelpString());
            foreach (var flag in flags)
                sb.Append($"\n\t{flag.GetShortHelpString()}");

            return sb.ToString();
        }
    }
}
