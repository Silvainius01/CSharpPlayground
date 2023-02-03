using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using System.Xml.Serialization;

namespace CommandEngine
{
    public static class CommandManager
    {
        public static CommandModule globalCommands = new CommandModule(
            "Enter Next Global Command: ",
            "Not a recognized global command.");
        
        //static DictionaryByType enumCommandDict = new DictionaryByType();
        
        #region Console Sync
        public static string UserInputPrompt(string message, bool newline)
        {
            if (newline)
                Console.WriteLine($"{message}:  ");
            else Console.Write($"{message}:  ");
            return Console.ReadLine();
        }

        public static bool YesNoPrompt(string message, bool newline)
        {
            while (true)
            {
                string input = UserInputPrompt($"{message} (y/n)", newline);
                if (input.Length >= 1)
                {
                    switch (input[0])
                    {
                        case 'y':
                        case 'Y':
                            return true;
                        case 'n':
                        case 'N':
                            return false;
                    }
                }
                Console.WriteLine("Invalid input, please try again.");
                newline = true;
            }
        }

        public static void GetNextGlobalCommand(string message, bool newline)
        {
            globalCommands.NextCommand(message, newline);
        }

        public static bool GetNextCommand(string message, bool newline, Dictionary<string, ConsoleCommand> commands)
        {
            string input = UserInputPrompt(message, newline);
            bool success = ParseCommandSet(input, commands);
            return success || ParseCommandSet(input, globalCommands.commands);
        }
        public static bool GetNextCommand<T>(string message, bool newline, Dictionary<string, ConsoleCommand<T>> commands, out T result)
        {
            string input = UserInputPrompt(message, newline);
            bool success = ParseCommandSet(input, commands, out result);
            return success;
        }

        //public static TEnum GetNextEnumCommand<TEnum>(string message, bool newline) where TEnum : struct, Enum
        //{
        //    if (!enumCommandDict.ContainsType<EnumCommandModule<TEnum>>())
        //        enumCommandDict.Add(new EnumCommandModule<TEnum>());
        //    return enumCommandDict.Get<EnumCommandModule<TEnum>>().GetValueFromCommand(message, newline);
        //}
        //public static bool TryGetNextEnumCommand<TEnum>(string message, bool newline, out TEnum value) where TEnum : struct, Enum
        //{
        //    if (!enumCommandDict.ContainsType<EnumCommandModule<TEnum>>())
        //        enumCommandDict.Add(new EnumCommandModule<TEnum>());
        //    return enumCommandDict.Get<EnumCommandModule<TEnum>>().TryGetValueFromCommand(message, newline, out value);
        //}
        #endregion

        #region Console Async
        public static async Task<string> UserInputPromptAsync(string message, bool newline)
        {
            if (newline)
                Console.WriteLine($"{message}:  ");
            else Console.Write($"{message}:  ");
            return await Task.Run(() => Console.ReadLine());
        }

        public static async Task<bool> YesNoPromptAsync(string message, bool newline)
        {
            while (true)
            {
                string input = await UserInputPromptAsync($"{message} (y/n)", newline);
                if (input.Length >= 1)
                {
                    switch (input[0])
                    {
                        case 'y':
                        case 'Y':
                            return true;
                        case 'n':
                        case 'N':
                            return false;
                    }
                }
                Console.WriteLine("Invalid input, please try again.");
                newline = true;
            }
        }

        public static async Task GetNextUniversalCommandAsync(string message, bool newline)
        {
            string input = await UserInputPromptAsync(message, newline);
            string command = input.Split(' ')[0];

            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Currently refactoring async global commands.");
            Console.ForegroundColor = color;

        }

        public static async Task GetNextUniversalCommandAsync(string message, bool newline, Dictionary<string, ConsoleCommand> additonalCommands)
        {
            string input = await UserInputPromptAsync(message, newline);
            string command = input.Split(' ')[0];

            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Currently refactoring async global commands.");
            Console.ForegroundColor = color;
        }

        public static async Task GetNextCommandAsync(string message, bool newline, Dictionary<string, ConsoleCommand> commands)
        {
            string input = await UserInputPromptAsync(message, newline);
            string command = input.Split(' ')[0];

            if (commands.ContainsKey(command))
                commands[command].Execute(input.Remove(0, command.Length));
            else
            {
                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Command '{command}' is not valid.");
                Console.ForegroundColor = color;
            }
        }
        #endregion

        #region From Input
        public static bool ParseCommandSet(string input, Dictionary<string, ConsoleCommand> commands)
        {
            string command = input.Split(' ')[0];

            if (commands.ContainsKey(command))
            {
                commands[command].Execute(input.Remove(0, command.Length));
                return true;
            }
            return false;
        }
        public static bool ParseCommandSet<T>(string input, Dictionary<string, ConsoleCommand<T>> commands, out T result)
        {
            string command = input.Split(' ')[0];

            if (commands.ContainsKey(command))
            {
                bool success = commands[command].Execute(input.Remove(0, command.Length), out result);
                return true;
            }

            result = default(T);
            return false;
        }
        #endregion
    }
}