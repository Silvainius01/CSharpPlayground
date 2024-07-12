using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CommandEngine
{
    public class CommandModule : BaseCommandModule<ConsoleCommand, object>
    {
        private Dictionary<string, List<ConsoleCommand>> commandsByCategory = new Dictionary<string, List<ConsoleCommand>>();

        public CommandModule(string defaultPrompt) 
        {
            DefaultPrompt = defaultPrompt;
        }
        public CommandModule(string defaultPrompt, string invalidMsg)
        {
            DefaultPrompt = defaultPrompt;
            InvalidCommandMessage = invalidMsg;
        }

        public void Add(string name, ConsoleCommand.ExecutionDelegate executionDelegate)
        {
            this.Add(new ConsoleCommand(name, executionDelegate));
        }

        public bool NextCommand(bool newline)
            => CommandManager.GetNextCommand(DefaultPrompt, newline, commands);
        public bool NextCommand(string prompt, bool newline)
            => CommandManager.GetNextCommand(prompt, newline, commands);

        /// <summary>
        /// <para>Use <see cref="NextCommand(bool)"/> instead. </para>
        /// </summary>
        [Obsolete("", true)]
        public override bool NextCommand(bool newline, out object alwaysNull)
        {
            alwaysNull = null;
            return NextCommand(newline);
        }

        /// <summary>
        /// <para>Reccomended to use <see cref="NextCommand(string, bool)"/> instead. </para>
        /// </summary>
        [Obsolete("", true)]
        public override bool NextCommand(string prompt, bool newline, out object alwaysNull)
        {
            alwaysNull = null;
            return NextCommand(prompt, newline);
        }
    }
}
