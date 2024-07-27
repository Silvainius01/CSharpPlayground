using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Websocket.Client.Logging;

namespace PlanetSide.Websocket
{
    public class CommSmashReporter : PlanetSideReporter
    {
        Action saveAction;
        Dictionary<string, PlanetStats> statsDict;
        CancellationTokenSource ctLeaderboardLoop;

        public CommSmashReporter(string port, string world, int zone=-1) : base(port, world, zone)
        {

        }

        protected override List<LeaderboardRequest> GenerateLeaderboardRequests()
        {
            return new List<LeaderboardRequest>()
            {
                new LeaderboardRequest()
                {
                    Name = "leaderboard-kills-infantry",
                    LeaderboardType = LeaderboardType.Player,
                    BoardSize = 10,
                    GetStat = stats => stats.Kills
                },
                new LeaderboardRequest()
                {
                    Name = "leaderboard-kills-vehicle",
                    LeaderboardType = LeaderboardType.Player,
                    BoardSize = 10,
                    GetStat = stats => stats.VehicleKills
                },
                new LeaderboardRequest()
                {
                    Name = "leaderboard-kills-air",
                    LeaderboardType = LeaderboardType.Player,
                    BoardSize = 10,
                    GetStat = stats => stats.AirKills
                },
                new LeaderboardRequest()
                {
                    Name = "leaderboard-kills-max",
                    LeaderboardType = LeaderboardType.Player,
                    BoardSize = 10,
                    GetStat = stats => stats.GetExp(ExperienceTable.KillMAX).NumEvents,
                },
                new LeaderboardRequest()
                {
                    Name = "leaderboard-revives",
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
                    Name = "leaderboard-resupplies",
                    LeaderboardType = LeaderboardType.Player,
                    BoardSize = 10,
                    GetStat = stats =>
                    {
                        int count = 0;
                        foreach(var id in ExperienceTable.ResupplyIds)
                            count += stats.GetExp(id).NumEvents;
                        return count;
                    }
                },
                new LeaderboardRequest()
                {
                    Name = "leaderboard-repair-vehicle",
                    LeaderboardType = LeaderboardType.Player,
                    BoardSize = 10,
                    GetStat = stats =>
                    {
                        float count = 0;
                        foreach(var id in ExperienceTable.VehicleRepairIds)
                            count += stats.GetExp(id).CumulativeScore;
                        return count;
                    }
                },
                new LeaderboardRequest()
                {
                    Name = "leaderboard-repair-max",
                    LeaderboardType = LeaderboardType.Player,
                    BoardSize = 10,
                    GetStat = stats =>
                    {
                        float count = 0;
                        foreach(var id in ExperienceTable.MaxRepairIds)
                            count += stats.GetExp(id).CumulativeScore;
                        return count;
                    }
                },
                new LeaderboardRequest()
                {
                    Name = "leaderboard-weapons",
                    LeaderboardType = LeaderboardType.Weapon,
                    BoardSize = 10,
                    GetStat = stats => stats.Kills
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
            return base.GenerateReports().Append(GetCommsSmashReport());
        }

        protected override List<PlanetSideTeam> GenerateTeams()
        {
            return new List<PlanetSideTeam>() {
                 new FactionTeam("CommSmash11_TeamOne_TR", 3, world),
                 new FactionTeam("CommSmash11_TeamTwo_NC", 2, world)
            };
        }

        ServerReport GetCommsSmashReport()
        {
            var csReport = new CommSmashTeamReport()
            {
                TeamOneStats = activeTeams[0].TeamStats,
                TeamTwoStats = activeTeams[1].TeamStats,
            };

            // Start a task to save teams.
            if (saveAction is null)
            {
                saveAction = () =>
                {
                    foreach (var team in activeTeams)
                        team.SaveStats();
                    saveAction = null;
                };
                Task.Run(saveAction);
            }

            return new ServerReport()
            {
                Topic = "net_stats",
                Data = JsonConvert.SerializeObject(csReport)
            };
        }

    }
}
