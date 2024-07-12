using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandEngine
{
    /// <summary>
    /// Derived version of <see cref="ConsoleCommand{T}"/> for commands that are of return type <see cref="void"/>
    /// </summary>
    public class ConsoleCommand : ConsoleCommand<object?>
    {
        public delegate void ExecutionDelegate(List<string> args);
        private new ExecutionDelegate ExecuteDelegate { get; set; }

        public ConsoleCommand(string name, ExecutionDelegate executionDelegate, string description = "", string category = "")
            : base(name,
                  (List<string> args, out object? result) => { result = null; executionDelegate(args); return true; },
                  description,
                  category)
        {
            ExecuteDelegate = executionDelegate;
        }

        internal virtual void Execute(string inputStr)
        {
            ParseInput(inputStr, out string errorMsg);
            ExecuteDelegate(arguments);
        }
        internal override bool Execute(string inputStr, out object? alwaysNull)
        {
            alwaysNull = null;
            Execute(inputStr);
            return true;
        }

        public static KeyValuePair<string, ConsoleCommand> Create(string name, ExecutionDelegate d)
            => new KeyValuePair<string, ConsoleCommand>(name, new ConsoleCommand(name, d));
    }
}
