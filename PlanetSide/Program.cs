﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DaybreakGames.Census.Stream;
using Microsoft.Extensions.Logging;

namespace PlanetSide
{
    public class Program
    {
        public static readonly ILoggerFactory LoggerFactory =
            Microsoft.Extensions.Logging.LoggerFactory.Create(builder => builder.AddConsole());

        private static readonly ILogger<Program> Logger = LoggerFactory.CreateLogger<Program>();

        static string TeamStatsJsonPath = $"./ChartTest/TeamStats.json";

        private static void Main(string[] args)
        {
            Logger.LogInformation("Creating Handler");
            CensusHandler handler = new CensusHandler();
            List<TeamStats> teamStats = new List<TeamStats>();
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

                    teamReportBuilder.AppendLine($"Team: {team.teamName}");
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

            string key = "OutfitWars_CharacterEvents";
            NexusTeam vs = new NexusTeam(48, "Vanu Sovereignty", "1");
            NexusTeam tr = new NexusTeam(48, "Terran Republic", "2");
            NexusTeam nc = new NexusTeam(48, "New Conglomerate", "3");
            CensusWebsocket socket = handler.AddSubscription(key, new CensusStreamSubscription()
            {
                Characters = new[] { "all" },
                Worlds = new[] { "17" },
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

        static async void SendTeamStatsJson(List<TeamStats> teamStats)
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
