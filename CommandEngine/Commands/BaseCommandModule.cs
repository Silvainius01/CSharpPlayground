using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CommandEngine
{
    public abstract class BaseCommandModule<TCommand, TReturn> where TCommand : ConsoleCommand<TReturn>
    {
        public string HelpString { get; private set; } = string.Empty;

        protected string DefaultPrompt { get; set; }
        protected string InvalidCommandMessage = "Not a recognized command!";
        protected Dictionary<string, TCommand> commands;
        protected Dictionary<string, List<TCommand>> commandsByCategory = new Dictionary<string, List<TCommand>>();

        private bool helpStringGenerated = false;

        public void Add(string name, TCommand command) 
        {
            if (command.Category is null)
                throw new ArgumentNullException("A command's category cannot be null!");

            helpStringGenerated = false;
            commands.Add(command.Name, command);

            if (commandsByCategory.ContainsKey(command.Category))
            {
                var list = commandsByCategory[command.Category];
                list.Add(command);
                list.Sort((a, b) => a.Name.CompareTo(b.Name));
            }
            else
            {
                commandsByCategory.Add(command.Category, new List<TCommand> { command });
            }
        }

        public abstract bool NextCommand(bool newline, out TReturn result);
        public abstract bool NextCommand(string prompt, bool newline, out TReturn result);

        public string GetHelpString()
        {
            if (helpStringGenerated)
                return HelpString;

            SmartStringBuilder sb = new SmartStringBuilder();
            List<string> categoryNames = commandsByCategory.Keys.ToList();
            categoryNames.Sort();

            for (int i = 0; i < categoryNames.Count; ++i)
            {
                int tabCount = 0;
                string categoryName = categoryNames[i];
                List<TCommand> commands = commandsByCategory[categoryName];

                if (categoryName == string.Empty)
                    continue;

                sb.AppendNewline(tabCount, $"{categoryNames[i]} Commands: ");
                ++tabCount;
                for (int j = 0; j < commands.Count; ++j)
                {
                    var command = commands[j];
                    sb.AppendNewline(tabCount, $"{command.Name}");
                }
                --tabCount;
            }

            return string.Empty;
        }
    }
}
