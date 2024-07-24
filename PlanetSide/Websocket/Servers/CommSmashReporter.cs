using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Websocket.Client.Logging;

namespace PlanetSide.Websocket
{
    public class CommSmashReporter : ReportServer
    {
        string world;
        CommSmashLeaderboard leaderboard;
        List<PlanetSideTeam> activeTeams;
        List<LeaderboardRequest> leaderboardRequests;
        Dictionary<string, PlanetStats> statsDict;
        CancellationTokenSource ctLeaderboardLoop;


        public CommSmashReporter(string port, string world) : base(port, ServerType.Publisher)
        {
            this.world = world;
            activeTeams = new List<PlanetSideTeam>();
            statsDict = new Dictionary<string, PlanetStats>();

            leaderboardRequests = new List<LeaderboardRequest>()
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
                    Name = "leaderboard-kills-vehilce",
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

        protected override bool OnServerStart()
        {
            Tracker.PopulateTables();

            var teamOne = new FactionTeam("New Conglomerate", 2, world);
            var teamTwo = new FactionTeam("Terran Republic", 3, world);
            ctLeaderboardLoop = new CancellationTokenSource();

            activeTeams.Add(teamOne);
            activeTeams.Add(teamTwo);
            leaderboard = new CommSmashLeaderboard(teamOne, teamTwo);

            foreach (var team in activeTeams)
            {
                team.StartStream();
            }

            // Start calculating leaderboards in the back ground.
            Task.Run(() => LeaderboardCalcLoop(ctLeaderboardLoop.Token));

            return true;
        }

        List<ServerReport> _reportList = new List<ServerReport>();
        ConcurrentQueue<ServerReport> _leaderboardReports = new ConcurrentQueue<ServerReport>();
        protected override IEnumerable<ServerReport> GenerateReports()
        {
            _reportList.Clear();
            _reportList.Add(GetCommsSmashReport());

            int numPlayers = activeTeams[0].TeamPlayers.Count + activeTeams[1].TeamPlayers.Count;

            if (numPlayers >= 10)
                while(_leaderboardReports.Count > 0)
                {
                    if (_leaderboardReports.TryDequeue(out var report))
                        _reportList.Add(report);
                }

            return _reportList;
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

        private async Task LeaderboardCalcLoop(CancellationToken ct)
        {
            float waitTime = 5.0f / leaderboardRequests.Count;
            PeriodicTimer boardtimer = new PeriodicTimer(TimeSpan.FromSeconds(waitTime));

            while (!ct.IsCancellationRequested)
            {
                foreach (var request in leaderboardRequests)
                {
                    var board = leaderboard.CalculateLeaderboard(request);
                    _leaderboardReports.Enqueue(new ServerReport()
                    {
                        Data = JsonConvert.SerializeObject(board),
                        Topic = request.Name,
                    });
                    await boardtimer.WaitForNextTickAsync(ct);
                }
            }

            if (!ct.IsCancellationRequested)
                Logger.LogError("Leaderboard calculations routine exited!");
            Logger.LogDebug("Leaderboard calculation routine exited.");
        }
    }
}
