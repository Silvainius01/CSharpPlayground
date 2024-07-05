using DaybreakGames.Census.Stream;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PlanetSide
{
    public class Tracker
    {
        static string TeamStatsJsonPath = $"./ChartTest/TeamStats.json";
        private static readonly ILogger<Tracker> Logger = Program.LoggerFactory.CreateLogger<Tracker>();

        public static void StartTracker()
        {
            Logger.LogInformation("Creating Handler");
            CensusHandler handler = new CensusHandler();
            List<PlanetStats> teamStats = new List<PlanetStats>();
            StringBuilder teamReportBuilder = new StringBuilder();

            PopulateTables(handler);
            var teams = GetNexusTeams(handler);

            foreach (var team in teams)
            {
                team.StartStream(handler);
                teamStats.Add(team.teamStats);
            }

            while (true)
            {
                teamReportBuilder.Clear();
                foreach (var team in teams)
                {
                    var stats = team.teamStats;

                    teamReportBuilder.AppendLine($"Team: {team.TeamName}");
                    teamReportBuilder.AppendLine($"\tInfantry: ");
                    teamReportBuilder.AppendLine($"\t\t KDR: {stats.Kills} / {stats.Deaths} = {stats.KDR}");
                    teamReportBuilder.AppendLine($"\t\t HSR: {stats.Headshots} / {stats.Kills} = {stats.HSR}");
                    teamReportBuilder.AppendLine($"\tVehicles: ");
                    teamReportBuilder.AppendLine($"\t\tvKDR: {stats.VehicleKills} / {stats.VehicleDeaths} = {stats.vKDR}");
                    teamReportBuilder.AppendLine($"\n");
                }

                SendTeamStatsJson(teamStats);

                teamReportBuilder.Append("Next report in 10 seconds");
                Logger.LogInformation(teamReportBuilder.ToString());
                System.Threading.Thread.Sleep(10000);
            }
        }

        static void PopulateTables(CensusHandler handler)
        {
            Logger.LogInformation("Populating Tables");
            List<Task> tableTasks = new List<Task>()
            {
                ExperienceTable.Populate(handler),
                WeaponTable.Populate(handler)
            };

            foreach (var task in tableTasks)
                task.Wait();
        }

        static List<NexusTeam> GetNexusTeams(CensusHandler handler)
        {
            Logger.LogInformation("Generating Nexus Teams");

            string world = "all";
            string key = "OutfitWars_CharacterEvents";
            NexusTeam vs = new NexusTeam(48, "Vanu Sovereignty", "1", world);
            NexusTeam tr = new NexusTeam(48, "Terran Republic", "2", world);
            NexusTeam nc = new NexusTeam(48, "New Conglomerate", "3", world);
            CensusWebsocket socket = handler.AddSubscription(key, new CensusStreamSubscription()
            {
                Characters = new[] { "all" },
                Worlds = new[] { world },
                EventNames = new[] { "Death", "GainExperience", "VehicleDestroy" },
                LogicalAndCharactersWithWorlds = true
            });

            List<Task> tasks = new List<Task>()
            {
                vs.GenerateRandomTeam(key, handler),
                tr.GenerateRandomTeam(key, handler),
                nc.GenerateRandomTeam(key, handler),
                handler.ConnectClientAsync(key)
            };
            Task.WhenAll(tasks).Wait();
            Logger.LogInformation("All Nexus Teams Generated");

            return new List<NexusTeam>() { vs, tr, nc };
        }

        static async void SendTeamStatsJson(List<PlanetStats> teamStats)
        {
            string json = JsonSerializer.Serialize(teamStats);
            using (FileStream fstream = File.Create(TeamStatsJsonPath))
            {
                using (StreamWriter writer = new StreamWriter(fstream))
                {
                    writer.WriteLine(json);
                }
            }


        }
    }
}
