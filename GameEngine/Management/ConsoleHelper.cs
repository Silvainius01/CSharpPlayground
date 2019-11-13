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

namespace GameEngine
{
    public static class CommandManager
    {
        static Dictionary<string, ConsoleCommand> universalCommands = new Dictionary<string, ConsoleCommand>();
        
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

        public static void GetNextUniversalCommand(string message, bool newline)
        {
            string input = UserInputPrompt(message, newline);
            string command = input.Split(' ')[0];

            if (universalCommands.ContainsKey(command))
                universalCommands[input].Execute(input.Remove(0, command.Length));
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
                universalCommands[input].Execute(input.Remove(0, command.Length));
            else if (additonalCommands.ContainsKey(command))
                additonalCommands[input].Execute(input.Remove(0, command.Length));
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

            if (universalCommands.ContainsKey(command))
                universalCommands[input].Execute(input.Remove(0, command.Length));
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
                universalCommands[input].Execute(input.Remove(0, command.Length));
            else if (additonalCommands.ContainsKey(command))
                additonalCommands[input].Execute(input.Remove(0, command.Length));
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
        public static void ParseUniversalCommand(string input)
        {
            string command = input.Split(' ')[0];

            if (universalCommands.ContainsKey(command))
                universalCommands[input].Execute(input.Remove(0, command.Length));
            else
            {
                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Command '{command}' is not valid.");
                Console.ForegroundColor = color;
            }
        }

        public static void ParseUniversalCommand(string input, Dictionary<string, ConsoleCommand> additonalCommands)
        {
            string command = input.Split(' ')[0];

            if (universalCommands.ContainsKey(command))
                universalCommands[input].Execute(input.Remove(0, command.Length));
            else if (additonalCommands.ContainsKey(command))
                additonalCommands[input].Execute(input.Remove(0, command.Length));
            else
            {
                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Command '{command}' is not valid.");
                Console.ForegroundColor = color;
            }
        }

        public static void ParseCommandSet(string input, Dictionary<string, ConsoleCommand> commands)
        {
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

        public static void RegisterUniversalCommand(ConsoleCommand command)
        {
            if (!universalCommands.ContainsKey(command.Name))
                universalCommands.Add(command.Name, command);
        }
    }

    public class ConsoleCommand
    {
        public delegate void ExecutionDelegate(List<string> arguments);

        public string Name { get; set; }
        ExecutionDelegate ExecuteDelegate { get; set; }
        public List<string> arguments = new List<string>(); 

        public ConsoleCommand(string name, ExecutionDelegate executionDelegate)
        {
            Name = name;
            ExecuteDelegate = executionDelegate;
        }

        public void Execute(string inputStr)
        {
            ParseInput(inputStr);
            ExecuteDelegate(arguments);
        }

        private void ParseInput(string inputStr)
        {
            StringBuilder input = new StringBuilder(inputStr);
            StringBuilder args = new StringBuilder(inputStr.Length);
            arguments.Clear();

            // Parse command
            while (input.Length > 0)
            {
                // Spaces between arguments are eaten by parser
                while (input[0] == ' ')
                    input.Remove(0, 1);

                // Quotes are eaten by parser, everything inside them is added as an argument.
                // If no closing quote is found, the rest of the command line is returned.
                if (input[0] == '"')
                {
                    for (int i = 1; i < input.Length; ++i)
                    {
                        // Backslash adds the next character in the string to the arugment.
                        if (input[i] == '\\' && i < input.Length - 1)
                        {
                            args.Append(input[++i]);
                        }
                        else if (input[i] != '"')
                        {
                            args.Append(input[i]);
                        }
                    }

                    input.Remove(0, args.Length);
                    AddArgument(ref args);
                    continue;
                }
                // Return the next space seperated string
                else
                {
                    for (int i = 0; i < input.Length && input[i] != ' '; ++i)
                    {
                        args.Append(input[i]);
                    }

                    input.Remove(0, args.Length);
                    AddArgument(ref args);
                    continue;
                }
            }
        }
        private void AddArgument(ref StringBuilder sb)
        {
            arguments.Add(sb.ToString());
            sb.Clear();
        }
    }

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