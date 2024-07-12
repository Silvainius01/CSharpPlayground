using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandEngine
{
    public interface IConsoleCommand { }
    /// <summary>
    /// Executes a delegate to desired functionality based on a passed input string.
    /// </summary>
    /// <typeparam name="T">Return type of <see cref="Execute(string)"/></typeparam>
    public class ConsoleCommand<T> : IConsoleCommand
    {
        public struct ConsoleArgument
        {
            public bool IsOptional { get; private set; }
            public string Name { get; private set; }
            public string Value { get; internal set; }
            public string Description { get; private set; }
            public Type ExpectedType { get; private set; }

            public string GenerateHelpString()
            {
                var t = typeof(void);
                if (IsOptional)
                    return $"[{this.Name}: {ExpectedType.FullName}]";
                else return $"<{this.Name}: {ExpectedType.FullName}>";
            }
        }
        public struct ConsoleOption
        {
            public string ShortName { get; private set; }
            public string LongName { get; private set; }
        }

        public delegate bool ExecutionDelegate<TReturn>(List<string> args, out TReturn result);

        public string Name { get; private set; }
        public string Description { get; private set; } = string.Empty;
        public string Category { get; private set; } = string.Empty;


        public List<string> arguments = new List<string>();
        private ExecutionDelegate<T> ExecuteDelegate { get; set; }

        public ConsoleCommand(string name, ExecutionDelegate<T> executionDelegate, string description = "", string category = "")
        {
            Name = name;
            ExecuteDelegate = executionDelegate;
            Description = description;
            Category = category;
        }
        protected ConsoleCommand(string name, string description, string category)
        {
            Name = name;
            Description = description;
            Category = category;
            ExecuteDelegate = null;
        }

        internal virtual bool Execute(string inputStr, out T result)
        {
            ParseInput(inputStr);
            return ExecuteDelegate(arguments, out result);
        }

        protected void ParseInput(string input)
        {
            StringBuilder builder = new StringBuilder(input.Length);
            arguments.Clear();

            void AddArgument()
            {
                // Only add an argument if it contains characters.
                if (builder.Length > 0)
                    arguments.Add(builder.ToString());
                builder.Clear();
            }

            // Parse command
            bool quote = false;
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
                    case '\t': // Eat whitespace, unless inside a quotes.
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
        }

        public static KeyValuePair<string, ConsoleCommand<T>> Create(string name, ExecutionDelegate<T> d)
            => new KeyValuePair<string, ConsoleCommand<T>>(name, new ConsoleCommand<T>(name, d));

    }

    /// <summary>
    /// Derived version of <see cref="ConsoleCommand{T}"/> for commands that are of return type <see cref="void"/>
    /// </summary>
    public class ConsoleCommand : ConsoleCommand<object>
    {
        public delegate void ExecutionDelegate(List<string> args);
        private ExecutionDelegate ExecuteDelegate { get; set; }

        public ConsoleCommand(string name, ExecutionDelegate executionDelegate, string description = "", string category = "")
            : base(name, description, category)
        {
            ExecuteDelegate = executionDelegate;
        }

        internal virtual void Execute(string inputStr)
        {
            ParseInput(inputStr);
            ExecuteDelegate(arguments);
        }
        internal override bool Execute(string inputStr, out object result)
        {
            result = null;
            Execute(inputStr);
            return true;
        }

        public static KeyValuePair<string, ConsoleCommand> Create(string name, ExecutionDelegate d)
            => new KeyValuePair<string, ConsoleCommand>(name, new ConsoleCommand(name, d));
    }
}
