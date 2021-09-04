using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine
{
    public class CommandModule
    {
        string DefaultPrompt;
        Dictionary<string, ConsoleCommand> commands = new Dictionary<string, ConsoleCommand>();

        public CommandModule(string defaultPrompt) 
        {
            DefaultPrompt = defaultPrompt;
        }

        public void Add(string name, ConsoleCommand.ExecutionDelegate executionDelegate)
        {
            commands.Add(name, new ConsoleCommand(name, executionDelegate));
        }
        public void Add(ConsoleCommand command)
        {
            commands.Add(command.Name, command);
        }

        public void NextCommand(bool newLine)
        {
            CommandManager.GetNextCommand(DefaultPrompt, newLine, commands);
        }
        public void NextCommand(string prompt, bool newline)
        {
            CommandManager.GetNextCommand(prompt, newline, commands);
        }
    }

    public class CommandModule<T>
    {
        string DefaultPrompt;
        Dictionary<string, ConsoleCommand<T>> commands = new Dictionary<string, ConsoleCommand<T>>();

        public CommandModule(string defaultPrompt)
        {
            DefaultPrompt = defaultPrompt;
        }

        public void Add(string name, ConsoleCommand<T>.ExecutionDelegate<T> executionDelegate)
        {
            commands.Add(name, new ConsoleCommand<T>(name, executionDelegate));
        }

        public T NextCommand()
        {
            return CommandManager.GetNextCommand(DefaultPrompt, true, commands);
        }
        public T NextCommand(string prompt, bool newline)
        {
            return CommandManager.GetNextCommand(prompt, newline, commands);
        }
    }
}
