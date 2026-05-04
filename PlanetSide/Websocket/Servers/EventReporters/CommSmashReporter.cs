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
        public CommSmashReporter(string port, string world, int zone=-1) : base(port, world, zone)
        {

        }

        protected override List<LeaderboardRequest> GenerateLeaderboardRequests()
        {
            return new List<LeaderboardRequest>()
            {
                LeaderboardRequest.Kills("leaderboard-kills-infantry", 10),
                LeaderboardRequest.VehicleKills("leaderboard-kills-vehicle", 10),
                LeaderboardRequest.AirKills("leaderboard-kills-air", 10),
                LeaderboardRequest.Revives("leaderboard-revives", 10),
                LeaderboardRequest.TeamKills("leaderboard-team-kills", 10),
                LeaderboardRequest.WeaponKills("leaderboard-kills-weapon", 10),
                new LeaderboardRequest()
                {
                    Name = "leaderboard-kills-max",
                    LeaderboardType = LeaderboardType.Player,
                    BoardSize = 10,
                    GetStat = stats => stats.GetExp(ExperienceTable.KillMAX).NumEvents,
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
                }
            };
        }
        protected override List<PlanetSideTeam> GenerateTeams()
        {
            return new List<PlanetSideTeam>() {
                 new FactionTeam("CommSmash11_TeamOne_TR", 3, world),
                 new FactionTeam("CommSmash11_TeamTwo_NC", 2, world)
            };
        }

        protected override ServerReport GenerateReport()
        {
            return GetCommsSmashReport();
        }
        ServerReport GetCommsSmashReport()
        {
            var csReport = new CommSmashTeamReport()
            {
                TeamOneStats = activeTeams[0].TeamStats,
                TeamTwoStats = activeTeams[1].TeamStats,
            };

            return new ServerReport()
            {
                Topic = "net_stats",
                Data = JsonConvert.SerializeObject(csReport)
            };
        }
    }
}
