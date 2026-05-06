using CommandEngine;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PlanetSide.Websocket;
using PlanetSide.WebsocketServer;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace PlanetSide
{
    public class Program
    {
        public static readonly ILoggerFactory LoggerFactory =
            Microsoft.Extensions.Logging.LoggerFactory.Create(builder => 
                builder.SetMinimumLevel(LogLevel.Information).AddConsole());

        private static readonly ILogger<Program> Logger = LoggerFactory.CreateLogger<Program>();

        static CommandModule module = new CommandModule("Enter Start Up Command");

        private static void Main(string[] args)
        {
            module.Add(new ConsoleCommand("server", StartSocketServer));
            module.Add(new ConsoleCommand("client", StartSocketClient));
            module.Add(new ConsoleCommand("csv", CsvTest));

            while (true)
            {
                module.NextCommand(false);
            }
        }

        private static void StartSocketServer(List<string> args)
        {
            int zone = -1;
            int world = -1;
            PlanetSideReporter reporter = null;

            if (args.Count < 2 && !int.TryParse(args[2], out world))
            {
                if (args[1] != "all")
                {
                    Logger.LogError("Must supply a world id");
                    return;
                }
            }
            if (args.Count < 3 && !int.TryParse(args[2], out zone))
            {
                Logger.LogError("Must supply a valid zone id");
                return;
            }

            switch (args[0])
            {
                case "cs":
                    reporter = new CommSmashReporter("56854", world, zone);
                    break;
                case "koth":
                    reporter = new KothReporter("56854", world, zone);
                    break;
                case "hamma":
                    if (args.Count < 5)
                    {
                        Logger.LogError("Hamma Bowl Reporter requires 2 team csv file names as arguments.");
                        return;
                    }
                    reporter = new HammaBowlReporter(args[3], args[4], "56854", world, zone);
                    break;
                default:
                    return;
            }

            reporter.Initialize();
            reporter.StartServer();

            while (reporter.IsActive)
            {
                Thread.Sleep(500);
                reporter.serverCommands.NextCommand(false);
            }
        }

        private static void StartSocketClient(List<string> args)
        {
            SubscriptionClient.Start();
        }

        private static void CsvTest(List<string> args)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
            };

            using (var reader = new StreamReader($"{Directory.GetCurrentDirectory()}\\_data\\csv.csv"))
            {
                using (var csv = new CsvReader(reader, config))
                {
                    csv.Context.RegisterClassMap<PlayerCsvEntryMap>();
                    var records = csv.GetRecords<PlayerCsvEntry>().ToArray();

                    SetPlayerTeam teamHamma = new SetPlayerTeam(0, "Hamma", 19, records);

                    Console.WriteLine("\nTEAM HAMMA");

                    foreach (var player in teamHamma.TeamPlayers.Values)
                        Console.WriteLine($"  {player.Alias}: {player.Data.Name} ({player.Data.CensusId})");
                    Console.WriteLine("\n");
                }
            }
        }
    }
}
