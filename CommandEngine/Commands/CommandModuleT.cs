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

        public override bool NextCommand(bool newline, out T result)
            => CommandManager.GetNextCommand(DefaultPrompt, newline, commands, out result);
        public override bool NextCommand(string prompt, bool newline, out T result)
            => CommandManager.GetNextCommand(prompt, newline, commands, out result);
    }
}
