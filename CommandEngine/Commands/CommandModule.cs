using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandEngine
{
    public class CommandModule
    {
        string DefaultPrompt;
        string InvalidCommandMessage = "Not a recognized command!";
        internal Dictionary<string, ConsoleCommand> commands = new Dictionary<string, ConsoleCommand>();

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
            commands.Add(name, new ConsoleCommand(name, executionDelegate));
        }
        public void Add(ConsoleCommand command)
        {
            commands.Add(command.Name, command);
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

    public class CommandModule<T>
    {
        string DefaultPrompt;
        protected internal Dictionary<string, ConsoleCommand<T>> commands = new Dictionary<string, ConsoleCommand<T>>();

        public CommandModule(string defaultPrompt)
        {
            DefaultPrompt = defaultPrompt;
        }

        public void Add(string name, ConsoleCommand<T>.ExecutionDelegate<T> executionDelegate)
        {
            commands.Add(name, new ConsoleCommand<T>(name, executionDelegate));
        }

        public bool NextCommand(out T result)
        {
            return CommandManager.GetNextCommand(DefaultPrompt, true, commands, out result);
        }
        public bool NextCommand(string prompt, bool newline, out T result)
        {
            return CommandManager.GetNextCommand(prompt, newline, commands, out result);
        }
    }
}
