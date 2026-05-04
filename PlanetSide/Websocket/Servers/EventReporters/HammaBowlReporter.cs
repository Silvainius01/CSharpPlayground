using CommandEngine;
using CsvHelper;
using CsvHelper.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace PlanetSide.Websocket
{
    public struct PlayerCsvEntry
    {
        public string Alias { get; set; }
        public string CensusId { get; set; }
    }
    public class PlayerCsvEntryMap : ClassMap<PlayerCsvEntry>
    {
        public PlayerCsvEntryMap()
        {
            Map(m => m.Alias).Name("Alias");
            Map(m => m.CensusId).Name("CensusId");
        }
    }

    public class HammaBowlReporter : PlanetSideReporter
    {
        public string TeamOneName;
        public string TeamTwoName;

        public HammaBowlReporter(string team1, string team2, string port, string world, int zone) : base(port, world, zone)
        {
            TeamOneName = team1;
            TeamTwoName = team2;
        }

        protected override List<PlanetSideTeam> GenerateTeams()
        {
            List<PlanetSideTeam> teams = new List<PlanetSideTeam>(2);

            teams.Add(ReadTeamCsv(1, TeamOneName));
            teams.Add(ReadTeamCsv(2, TeamTwoName));

            foreach (var team in teams)
                using (ManagedStringBuilder msb = new ManagedStringBuilder("HammaBowlTeam"))
                {
                    var builder = msb.Builder;

                    builder.NewlineAppend($"TEAM {team.TeamName.ToUpper()} [{Tracker.FactionIdToName(team.FactionId)}]");

                    foreach (var player in team.TeamPlayers.Values)
                        builder.NewlineAppend($"  {player.Alias}: {player.Data.Name} ({player.Data.CensusId})");
                    builder.Append("\n");

                    Console.WriteLine(builder.ToString());
                }

            return teams;
        }
        protected override List<LeaderboardRequest> GenerateLeaderboardRequests()
        {
            return new List<LeaderboardRequest>();
        }

        protected override ServerReport GenerateReport()
        {
            return GenerateBowlReport();
        }
        private ServerReport GenerateBowlReport()
        {
            var report = new HammaReport()
            {
                TeamOneStats = activeTeams[0].TeamStats,
                TeamTwoStats = activeTeams[1].TeamStats
            };

            return new ServerReport()
            {
                Topic = "bowl_stats",
                Data = JsonConvert.SerializeObject(report)
            };
        }

        private SetPlayerTeam ReadTeamCsv(int teamId, string teamName)
        {
            string teamCsvName = "team" + teamName;
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
            };

            using (var reader = new StreamReader($"{Directory.GetCurrentDirectory()}\\_data\\{teamCsvName}.csv"))
            {
                using (var csv = new CsvReader(reader, config))
                {
                    csv.Context.RegisterClassMap<PlayerCsvEntryMap>();
                    var records = csv.GetRecords<PlayerCsvEntry>().ToArray();

                    SetPlayerTeam team = new SetPlayerTeam(teamId, teamName, world, records);

                    return team;
                }
            }
        }

    }
}
