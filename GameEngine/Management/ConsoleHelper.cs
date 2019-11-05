using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine
{
    public static class ConsolePrompts
    {
        static Dictionary<string, ConsoleCommand> universalCommands = new Dictionary<string, ConsoleCommand>();

        

        #region Sync
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

        public static void GetNextUniversalCommand(string message, bool newline)
        {
            string input = UserInputPrompt(message, newline);
            string command = input.Split(' ')[0];

            if (universalCommands.ContainsKey(command))
                universalCommands[input].ExecuteDelegate(input.Remove(0, command.Length));
            else
            {
                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Command '{command}' is not valid.");
                Console.ForegroundColor = color;
            }
        }

        public static void GetNextUniversalCommand(string message, bool newline, Dictionary<string, ConsoleCommand> additonalCommands)
        {
            string input = UserInputPrompt(message, newline);
            string command = input.Split(' ')[0];

            if (universalCommands.ContainsKey(command))
                universalCommands[input].ExecuteDelegate(input.Remove(0, command.Length));
            else if (additonalCommands.ContainsKey(command))
                additonalCommands[input].ExecuteDelegate(input.Remove(0, command.Length));
            else
            {
                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Command '{command}' is not valid.");
                Console.ForegroundColor = color;
            }
        }

        public static void GetNextCommand(string message, bool newline, Dictionary<string, ConsoleCommand> commands)
        {
            string input = UserInputPrompt(message, newline);
            string command = input.Split(' ')[0];

            if (commands.ContainsKey(command))
                commands[command].ExecuteDelegate(input.Remove(0, command.Length));
            else
            {
                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Command '{command}' is not valid.");
                Console.ForegroundColor = color;
            }
        }
        #endregion

        #region Async
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

            if (universalCommands.ContainsKey(command))
                universalCommands[input].ExecuteDelegate(input.Remove(0, command.Length));
            else
            {
                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Command '{command}' is not valid.");
                Console.ForegroundColor = color;
            }
        }

        public static async Task GetNextUniversalCommandAsync(string message, bool newline, Dictionary<string, ConsoleCommand> additonalCommands)
        {
            string input = await UserInputPromptAsync(message, newline);
            string command = input.Split(' ')[0];

            if (universalCommands.ContainsKey(command))
                universalCommands[input].ExecuteDelegate(input.Remove(0, command.Length));
            else if (additonalCommands.ContainsKey(command))
                additonalCommands[input].ExecuteDelegate(input.Remove(0, command.Length));
            else
            {
                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Command '{command}' is not valid.");
                Console.ForegroundColor = color;
            }
        }

        public static async Task GetNextCommandAsync(string message, bool newline, Dictionary<string, ConsoleCommand> commands)
        {
            string input = await UserInputPromptAsync(message, newline);
            string command = input.Split(' ')[0];

            if (commands.ContainsKey(command))
                commands[command].ExecuteDelegate(input.Remove(0, command.Length));
            else
            {
                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Command '{command}' is not valid.");
                Console.ForegroundColor = color;
            }
        }
        #endregion

        public static void RegisterUniversalCommand(ConsoleCommand command)
        {
            if (!universalCommands.ContainsKey(command.Name))
                universalCommands.Add(command.Name, command);
        }
    }

    public class ConsoleCommand
    {
        public delegate void ExecutionDelegate(string input);

        public string Name { get; set; }
        public ExecutionDelegate ExecuteDelegate { get; set; }

        public static string GetNextArg(string input)
        {
            StringBuilder sb = new StringBuilder(input);

            // Spaces between arguments are eaten by parser
            while (sb[0] == ' ')
                sb.Remove(0, 1);
            if(sb[0] == '"')
            {
                for(int i = 1; i < sb.Length; ++i)
                {
                    if (sb[i] != '"')
                        continue;

                    // Remove quotes, and everything after them
                    sb.Remove(i, sb.Length-i);
                    sb.Remove(0, 1);
                    
                    // Return what was inside the quotes as the next arg
                    return sb.ToString();
                }

                // If no closing quote is found, just return the rest of the command line.
                sb.Remove(0, 1);
                return sb.ToString();
            }

            // Return the next string seperated by a space if no other format is encountered.
            return sb.ToString().Split(' ')[0];
        }
    }

    public class EntityCommandModule : Component
    {
        Dictionary<string, ConsoleCommand> entityCommands = new Dictionary<string, ConsoleCommand>();

        public void GetNextCommand(string message, bool newline, bool allowUniversalCommands)
        {
            if (allowUniversalCommands)
                ConsolePrompts.GetNextUniversalCommand(message, newline, entityCommands);
            else ConsolePrompts.GetNextCommand(message, newline, entityCommands);
        }

        public async Task GetNextCommandAsync(string message, bool newline, bool allowUniversalCommands)
        {
            if (allowUniversalCommands)
                await ConsolePrompts.GetNextUniversalCommandAsync(message, newline, entityCommands);
            else await ConsolePrompts.GetNextCommandAsync(message, newline, entityCommands);
        }

        public void RegisterCommand(ConsoleCommand command)
        {
            if (!entityCommands.ContainsKey(command.Name))
                entityCommands.Add(command.Name, command);
        }
    }
}