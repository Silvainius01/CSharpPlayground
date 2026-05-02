using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using CommandEngine;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Http.Headers;
using Newtonsoft.Json.Bson;
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

            // Pre-pause streams so they dont start before the game does.
            foreach (var team in teams)
                team.PauseStream();

            return teams;
        }
        protected override List<LeaderboardRequest> GenerateLeaderboardRequests()
        {
            return new List<LeaderboardRequest>()
            {
                new LeaderboardRequest()
                {
                    Name = "koth-leaderboard-kills",
                    LeaderboardType = LeaderboardType.Player,
                    BoardSize = 10,
                    GetStat = stats => stats.Kills
                },
                new LeaderboardRequest()
                {
                    Name = "koth-leaderboard-revives",
                    LeaderboardType = LeaderboardType.Player,
                    BoardSize = 10,
                    GetStat = stats =>
                    {
                        int count = 0;
                        foreach(var id in ExperienceTable.ReviveIds)
                            count += stats.GetExp(id).NumEvents;
                        return count;
                    }
                },
                new LeaderboardRequest()
                {
                    Name = "leaderboard-team-kills",
                    LeaderboardType = LeaderboardType.Player,
                    BoardSize = 10,
                    GetStat = stats => stats.TeamKills
                }
            };
        }
        protected override IEnumerable<ServerReport> GenerateReports()
        {
            //if (!roundTimer.hasFired)
            //{
            //    if (roundTimer.Update((DateTime.Now - lastTime).TotalSeconds))
            //    {
            //        foreach (var team in activeTeams)
            //            team.PauseStream();
            //    }
            //}

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
