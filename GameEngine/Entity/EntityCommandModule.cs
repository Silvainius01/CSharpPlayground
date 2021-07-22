using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine
{
    public class EntityCommandModule : Component
    {
        Dictionary<string, ConsoleCommand> entityCommands = new Dictionary<string, ConsoleCommand>();

        public void GetNextCommand(string message, bool newline, bool allowUniversalCommands)
        {
            if (allowUniversalCommands)
                CommandManager.GetNextUniversalCommand(message, newline, entityCommands);
            else CommandManager.GetNextCommand(message, newline, entityCommands);
        }

        public async Task GetNextCommandAsync(string message, bool newline, bool allowUniversalCommands)
        {
            if (allowUniversalCommands)
                await CommandManager.GetNextUniversalCommandAsync(message, newline, entityCommands);
            else await CommandManager.GetNextCommandAsync(message, newline, entityCommands);
        }

        public void ParseCommand(string input, bool allowUniversalCommands)
        {
            if (allowUniversalCommands)
                CommandManager.ParseUniversalCommand(input, entityCommands);
            else CommandManager.ParseCommandSet(input, entityCommands);
        }

        public void RegisterCommand(ConsoleCommand command)
        {
            if (!entityCommands.ContainsKey(command.Name))
                entityCommands.Add(command.Name, command);
        }
    }
}
