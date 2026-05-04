using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using CommandEngine;
using System.Linq;

namespace PlanetSide.Websocket
{
    public class KothReporter : PlanetSideReporter
    {
        public KothReporter(string port, string world, int zone) : base(port, world, zone)
        {
        }

        protected override List<PlanetSideTeam> GenerateTeams()
        {
            var teams = new List<PlanetSideTeam>()
            {
                new FactionTeam("VS", 1, world, this.ZoneId),
                new FactionTeam("NC", 2, world, this.ZoneId),
                new FactionTeam("TR", 3, world, this.ZoneId),
            };

            return teams;
        }
        protected override List<LeaderboardRequest> GenerateLeaderboardRequests()
        {
            return new List<LeaderboardRequest>()
            {
                LeaderboardRequest.Kills("koth-leaderboard-kills", 10),
                LeaderboardRequest.Revives("koth-leaderboard-revives", 10),
                LeaderboardRequest.TeamKills("koth-leaderboard-team-kills", 10)
            };
        }
        protected override IEnumerable<ServerReport> GenerateReports()
        {
            return base.GenerateReports().Append(GenerateKothReport());
        }

        private ServerReport GenerateKothReport()
        {
            var report = new KothReport()
            {
                VS_Stats = activeTeams[0].TeamStats,
                NC_Stats = activeTeams[1].TeamStats,
                TR_Stats = activeTeams[2].TeamStats,
            };

            return new ServerReport()
            {
                Topic = "koth_stats",
                Data = JsonConvert.SerializeObject(report)
            };
        }
    }
}
