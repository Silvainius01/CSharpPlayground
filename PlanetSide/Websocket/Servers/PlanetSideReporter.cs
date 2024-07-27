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
    public abstract class PlanetSideReporter : ReportServer
    {
        public int ZoneId { get; set; } = -1;
        public float LeaderboardRefresh { get; set; } = 10.0f;

        protected string world;
        protected EventLeaderboard leaderboard;
        protected List<PlanetSideTeam> activeTeams;
        protected List<LeaderboardRequest> leaderboardRequests;
        
        CancellationTokenSource ctLeaderboardLoop;

        public PlanetSideReporter(string port, string world, int zone) : base(port, ServerType.Publisher)
        {
            this.world = world;
            this.ZoneId = zone;
            activeTeams = GenerateTeams();
            leaderboardRequests = GenerateLeaderboardRequests();
            leaderboard = new EventLeaderboard(activeTeams.ToArray());
        }

        protected override bool OnServerStart()
        {
            Tracker.PopulateTables();

            ctLeaderboardLoop = new CancellationTokenSource();

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
            int numPlayers = activeTeams[0].TeamPlayers.Count + activeTeams[1].TeamPlayers.Count;

            if (numPlayers >= 10)
            {
                Console.WriteLine($"Leaderboard Queue: {_leaderboardReports.Count}");
                while (_leaderboardReports.Count > 0)
                {
                    if (_leaderboardReports.TryDequeue(out var report))
                        _reportList.Add(report);
                }
            }

            return _reportList;
        }

        private async Task LeaderboardCalcLoop(CancellationToken ct)
        {
            float waitTime = LeaderboardRefresh / leaderboardRequests.Count;
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

        protected abstract List<PlanetSideTeam> GenerateTeams();
        protected abstract List<LeaderboardRequest> GenerateLeaderboardRequests();
    }
}
