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
        public delegate void ExecutionDelegate(List<string> arguments);
        public delegate TReturn ExecutionDelegate<TReturn>(List<string> arguments);

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

        private void ParseInput(string inputStr)
        {
            StringBuilder input = new StringBuilder(inputStr);
            StringBuilder args = new StringBuilder(inputStr.Length);
            arguments.Clear();

            // Parse command
            while (input.Length > 0)
            {
                // Spaces between arguments are eaten by parser
                while (input[0] == ' ')
                    input.Remove(0, 1);

                // Quotes are eaten by parser, everything inside them is added as an argument.
                // If no closing quote is found, the rest of the command line is returned.
                if (input[0] == '"')
                {
                    for (int i = 1; i < input.Length; ++i)
                    {
                        // Backslash adds the next character in the string to the arugment.
                        if (input[i] == '\\' && i < input.Length - 1)
                        {
                            args.Append(input[++i]);
                        }
                        else if (input[i] != '"')
                        {
                            args.Append(input[i]);
                        }
                    }

                    input.Remove(0, args.Length);
                    AddArgument(ref args);
                    continue;
                }
                // Return the next space seperated string
                else
                {
                    for (int i = 0; i < input.Length && input[i] != ' '; ++i)
                    {
                        args.Append(input[i]);
                    }

                    input.Remove(0, args.Length);
                    AddArgument(ref args);
                    continue;
                }
            }
        }
        private void AddArgument(ref StringBuilder sb)
        {
            arguments.Add(sb.ToString());
            sb.Clear();
        }
    }

    /// <summary>
    /// Derived version of <see cref="ConsoleCommand{T}"/> for commands that are of return type <see cref="void"/>
    /// </summary>
    public class ConsoleCommand : ConsoleCommand<object>
    {
        public ConsoleCommand(string name, ExecutionDelegate d) : base(name, delegate (List<string> args) { d(args); return null; })
        { }

        // Hide the base version, so that we avoid any possible stray nulls surfacing
        public new void Execute(string inputStr)
        {
            base.Execute(inputStr);
        }
    }
}
