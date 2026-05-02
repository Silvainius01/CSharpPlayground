using CommandEngine;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PlanetSide.StatProcessing.TeamBuilders;
using PlanetSide.Websocket;
using PlanetSide.WebsocketServer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace PlanetSide
{
    public class Program
    {
        public static readonly ILoggerFactory LoggerFactory =
            Microsoft.Extensions.Logging.LoggerFactory.Create(builder => builder.AddConsole());

        private static readonly ILogger<Program> Logger = LoggerFactory.CreateLogger<Program>();

        static CommandModule module = new CommandModule("Enter Start Up Command");

        private static void Main(string[] args)
        {
            module.Add(new ConsoleCommand("tracker", StartTracker));
            module.Add(new ConsoleCommand("server", StartSocketServer));
            module.Add(new ConsoleCommand("client", StartSocketClient));
            module.Add(new ConsoleCommand("cam", WarpgateCam));
            module.Add(new ConsoleCommand("addChar", AddCharacterTest));
            module.Add(new ConsoleCommand("csv", CsvTest));

            while (true)
            {
                module.NextCommand(false);
            }
        }

        private static void StartTracker(List<string> args)
        {
            Logger.LogInformation("Creating Handler");
            StringBuilder teamReportBuilder = new StringBuilder();
            List<PlanetStats> teamStats = new List<PlanetStats>();
            List<PlanetSideTeam> activeTeams = new List<PlanetSideTeam>();

            Tracker.PopulateTables();

            //activeTeams.AddRange(GetNexusTeams(handler));
            activeTeams.Add(new FactionTeam("Vanu Sovereignty", 1, "17"));
            activeTeams.Add(new FactionTeam("New Conglomerate", 2, "17"));
            activeTeams.Add(new FactionTeam("Terran Republic", 3, "17"));

            foreach (var team in activeTeams)
            {
                team.StartStream();
                teamStats.Add(team.TeamStats);
            }

            while (true)
            {
                teamReportBuilder.Clear();
                foreach (var team in activeTeams)
                {
                    var stats = team.TeamStats;

                    teamReportBuilder.AppendLine($"Team: {team.TeamName}");
                    teamReportBuilder.AppendLine($"\tInfantry: ");
                    teamReportBuilder.AppendLine($"\t\t KDR: {stats.Kills} / {stats.Deaths} = {stats.KDR}");
                    teamReportBuilder.AppendLine($"\t\t HSR: {stats.Headshots} / {stats.Kills} = {stats.HSR}");
                    teamReportBuilder.AppendLine($"\tVehicles: ");
                    teamReportBuilder.AppendLine($"\t\tvKDR: {stats.VehicleKills} / {stats.VehicleDeaths} = {stats.vKDR}");
                    teamReportBuilder.AppendLine($"\n");
                }

                teamReportBuilder.Append("Next report in 10 seconds");
                Logger.LogInformation(teamReportBuilder.ToString());
                System.Threading.Thread.Sleep(10000);
            }
        }

        private static void StartSocketServer(List<string> args)
        {
            int zone = -1;
            string world = "all";
            ReportServer reporter = null;

            if (args.Count > 2)
                world = args[2];
            if (args.Count > 3 && int.TryParse(args[3], out int intergerInput))
                zone = intergerInput;

            switch (args[0])
            {
                case "cs":
                    reporter = new CommSmashReporter(args[1], world, zone)
                    {
                        DebugEventNames = false
                    };
                    break;
                case "koth":
                    reporter = new KothReporter(args[1], world, zone)
                    {
                        DebugEventNames = true
                    };
                    break;
                case "hamma":
                    reporter = new HammaBowlReporter(args[1], world, zone)
                    {
                        DebugEventNames = true
                    };
                    break;
                default:
                    return;
            }

            reporter.StartServer();
        }

        static void ReadTeamFile(string file)
        {
            string fileContents = string.Empty;
            using (StreamReader reader = new StreamReader(file))
                fileContents = reader.ReadToEnd();

            SerializedTeam team = new SerializedTeam();
            var serializer = JsonSerializer.CreateDefault();
            var jObject = JsonConvert.DeserializeObject<JObject>(fileContents);
            var tokenReader = new JTokenReader(jObject);
            object? obj = serializer.Deserialize(tokenReader);

            int x = 0;
        }

        private static void StartSocketClient(List<string> args)
        {
            SubscriptionClient.Start();
        }

        private static void WarpgateCam(List<string> args)
        {
            SmartStringBuilder builder = new SmartStringBuilder();

            Vector3 FromHeading(double heading)
                => new Vector3(Math.Cos(heading), 0, Math.Sin(heading));

            double westWarpHeight = 24.250;
            double southWarpHeight = 27.260;
            double camWestHeight = 374.544;
            double camHeightRelative = camWestHeight - westWarpHeight;

            Vector3 westWarpCenter = new Vector3(2046.600, 0, -3122.980);
            Vector3 westWarpSpawn = new Vector3(1926.690, 0, -3004.080);
            Vector3 westWarpDirection = Vector3.Direction(westWarpSpawn, westWarpCenter);

            Vector3 camWest = new Vector3(1716.047, 0, -2795.857);
            Vector3 camWestRelative = camWest - westWarpCenter;
            Vector3 camWestDirection = camWestRelative.Normal();
            double camWestMagnitude = camWestRelative.Magnitude;

            Vector3 camWestNew = westWarpDirection * camWestMagnitude;
            camWestNew.Y = westWarpHeight + camHeightRelative;
            camWestNew += westWarpCenter;

            Vector3 southWarpCenter = new Vector3(-3302.800, 0, -86.440);
            Vector3 southWarpSpawn = new Vector3(-3120.670, 0, -45.070);
            Vector3 southWarpDirection = Vector3.Direction(southWarpSpawn, southWarpCenter);

            Vector3 camSouth = southWarpDirection * camWestMagnitude;
            camSouth.Y = camHeightRelative + southWarpHeight;
            camSouth += southWarpCenter;

            builder.NewlineAppend("");
            builder.NewlineAppend($"camWestOld: {camWest}");
            builder.NewlineAppend(1, $"P: {camWest}");
            builder.NewlineAppend(1, $"M: {camWestMagnitude}");
            builder.NewlineAppend($"camWestNew: {camWestNew}");
            builder.NewlineAppend(1, $"P: {camWestNew}");
            builder.NewlineAppend(1, $"M: {(camWestNew - westWarpCenter).Magnitude}");
            builder.NewlineAppend($"camSouthNew: ");
            builder.NewlineAppend(1, $"P: {camSouth}");
            builder.NewlineAppend(1, $"M: {(camSouth - southWarpCenter).Magnitude}");

            Console.WriteLine(builder.ToString());
        }

        private static void AddCharacterTest(List<string> args)
        {
            using (ManagedColorBuilder mcb = new ManagedColorBuilder("character_test"))
            {
                ColorStringBuilder builder = mcb.Builder;

                if (args.Count == 0)
                {
                    ConsoleExt.WriteError("Please enter a set of names");
                    return;
                }
                else if (args.Count == 1)
                {
                    if (PlayerTable.TryGetOrAddCharacterByName(args[0], out var cData))
                    {
                        builder.AppendNewline(cData.Name, ConsoleColor.Gray);
                        builder.AppendNewline(cData.CensusId);
                        builder.AppendNewline(cData.FactionId.ToString());
                    }
                    else builder.AppendNewline($"Failed to add '{args[0]}'", ConsoleColor.Red);
                }
                else if (PlayerTable.TryAddCharactersByName(args.ToArray()))
                {
                    foreach (var name in args)
                    {
                        if (PlayerTable.TryGetCharacterByName(name, out var cData))
                        {
                            builder.AppendNewline(cData.Name, ConsoleColor.Gray);
                            builder.AppendNewline(cData.CensusId);
                            builder.AppendNewline(cData.FactionId.ToString());
                        }
                        else builder.AppendNewline($"Failed to add '{args[0]}'", ConsoleColor.Red);
                    }
                }
                else builder.AppendNewline($"Failed to add any characters.", ConsoleColor.Red);

                builder.Append("\n");
                builder.WriteLine(true);
            }
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

                    SetPlayerTeam teamHamma = new SetPlayerTeam("Hamma", 1, "19", records);

                    Console.WriteLine("\nTEAM HAMMA");
                    
                    foreach (var player in teamHamma.TeamPlayers.Values)
                        Console.WriteLine($"  {player.Alias}: {player.Data.Name} ({player.Data.CensusId})");
                    Console.WriteLine("\n");
                }
            }
        }
    }
}
