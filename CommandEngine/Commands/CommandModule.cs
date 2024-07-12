using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        {
            bool result = CommandManager.GetNextCommand(DefaultPrompt, newline, commands);
            if (!result)
                ConsoleExt.WriteErrorLine(InvalidCommandMessage);
            return result;
        }
        public bool NextCommand(string prompt, bool newline)
        {
            bool result = CommandManager.GetNextCommand(prompt, newline, commands);
            if (!result)
                ConsoleExt.WriteErrorLine(InvalidCommandMessage);
            return result;
        }
    }
}
