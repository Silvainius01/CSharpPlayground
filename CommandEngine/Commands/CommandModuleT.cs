using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CommandEngine
{
    public class CommandModule<T> : BaseCommandModule<ConsoleCommand<T>, T>
    {
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
