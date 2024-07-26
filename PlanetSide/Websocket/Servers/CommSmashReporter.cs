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

        Dictionary<string, PlanetStats> statsDict;
        CancellationTokenSource ctLeaderboardLoop;

        public CommSmashReporter(string port, string world) : base(port, world)
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
                 new FactionTeam("New Conglomerate", 2, world),
                 new FactionTeam("Terran Republic", 3, world)
            };
        }

        ServerReport GetCommsSmashReport()
        {
            var csReport = new CommSmashReport()
            {
                kills_net_t1 = activeTeams[0].TeamStats.Kills,
                kills_net_t2 = activeTeams[1].TeamStats.Kills,

                kills_vehicle_t1 = activeTeams[0].TeamStats.VehicleKills,
                kills_vehicle_t2 = activeTeams[1].TeamStats.VehicleKills,

                kills_air_t1 = activeTeams[0].TeamStats.AirKills,
                kills_air_t2 = activeTeams[1].TeamStats.AirKills,

                revives_t1 = activeTeams[0].TeamStats.GetExp(7).NumEvents,
                revives_t2 = activeTeams[1].TeamStats.GetExp(7).NumEvents,

                captures_t1 = 0,
                captures_t2 = 0,

                defenses_t1 = 0,
                defenses_t2 = 0
            };

            return new ServerReport()
            {
                Topic = "net_stats",
                Data = JsonConvert.SerializeObject(csReport)
            };
        }
    }
}
