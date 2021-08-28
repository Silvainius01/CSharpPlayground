using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine
{
    public interface IConsoleCommand { }
    /// <summary>
    /// Executes a delegate to desired functionality based on a passed input string.
    /// </summary>
    /// <typeparam name="T">Return type of <see cref="Execute(string)"/></typeparam>
    public class ConsoleCommand<T> : IConsoleCommand
    {
        public delegate TReturn ExecutionDelegate<TReturn>(List<string> args);

        public string Name { get; set; }
        ExecutionDelegate<T> ExecuteDelegate { get; set; }
        public List<string> arguments = new List<string>();

        public ConsoleCommand(string name, ExecutionDelegate<T> executionDelegate)
        {
            Name = name;
            ExecuteDelegate = executionDelegate;
        }

        public virtual T Execute(string inputStr)
        {
            ParseInput(inputStr);
            return ExecuteDelegate(arguments);
        }

        private void ParseInput(string input)
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
            for(int index = 0; index < input.Length; ++index) 
            {
                switch(input[index])
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
        
        public ConsoleCommand(string name, ExecutionDelegate d) : base(name, delegate (List<string> args) { d(args); return null; })
        { }

        // Hide the base version, so that we avoid any possible stray nulls surfacing
        public new void Execute(string inputStr)
        {
            base.Execute(inputStr);
        }

        public static KeyValuePair<string, ConsoleCommand> Create(string name, ExecutionDelegate d)
            => new KeyValuePair<string, ConsoleCommand>(name, new ConsoleCommand(name, d));
    }
}
