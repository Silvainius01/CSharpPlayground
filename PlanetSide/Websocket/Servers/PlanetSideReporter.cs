using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Websocket.Client.Logging;
using CommandEngine;
using System.Linq;

namespace PlanetSide.Websocket
{
    public abstract class PlanetSideReporter : ReportServer
    {
        public int ZoneId { get; set; } = -1;
        public float LeaderboardRefresh { get; set; } = 10.0f;

        protected double RoundLength { get; set; } = 15 * 60;
        protected bool RoundPaused { get; set; }
        protected bool RoundStarted { get; set; }

        protected DateTime lastTime = DateTime.Now;
        protected CommandEngine.Timer roundTimer = new CommandEngine.Timer(TimeSpan.FromMinutes(10).TotalSeconds);
        protected CancellationTokenSource ctUpdate = new CancellationTokenSource();

        protected string world;
        protected EventLeaderboard leaderboard;
        protected List<PlanetSideTeam> activeTeams;
        protected List<LeaderboardRequest> leaderboardRequests;

        List<ServerReport> _reportList = new List<ServerReport>();
        ConcurrentQueue<ServerReport> _leaderboardReports = new ConcurrentQueue<ServerReport>();
        CancellationTokenSource ctLeaderboardLoop;

        public PlanetSideReporter(string port, string world, int zone) : base(port, ServerType.Publisher)
        {
            this.world = world;
            this.ZoneId = zone;
            
            serverCommands.Add(new ConsoleCommand("startRound", StartRoundCommand));
            serverCommands.Add(new ConsoleCommand("endRound", EndRoundCommand));
            serverCommands.Add(new ConsoleCommand("pauseRound", PauseRoundCommand));
            serverCommands.Add(new ConsoleCommand("resumeRound", StartRoundCommand));
            serverCommands.Add(new ConsoleCommand("saveStats", SaveStatsCommand));
            serverCommands.Add(new ConsoleCommand("setRoundLength", SetRoundLengthCommand));
        }

        protected override void OnInitialize() 
        {
            leaderboardRequests = GenerateLeaderboardRequests();
        }
        protected override void OnServerStart()
        {
            Tracker.PopulateTables();
            activeTeams = GenerateTeams();
            leaderboard = new EventLeaderboard(activeTeams.ToArray());
            ctLeaderboardLoop = new CancellationTokenSource();

            // Start calculating leaderboards in the back ground.
            Task.Run(() => LeaderboardCalcLoop(ctLeaderboardLoop.Token));

            foreach (var team in activeTeams)
                team.StartStream();

            PauseRound();
        }
        protected override void OnServerPause()
        {
            PauseRound();
        }
        protected override void OnServerStop()
        {
            SaveStats(true);
        }

        public void StartRound()
        {
            RoundPaused = false;
            roundTimer.Activate(RoundLength);

            foreach (var team in activeTeams)
                team.UnPauseStream();

            ctUpdate = new CancellationTokenSource();
            Task.Run(() => RoundUpdater(ctUpdate.Token));
        }
        public void PauseRound()
        {
            RoundPaused = true;
            roundTimer.Deactivate();
            foreach (var team in activeTeams)
            {
                team.PauseStream();
            }
        }
        public void EndRound()
        {
            ctUpdate.Cancel();
            roundTimer.Deactivate();
            foreach (var team in activeTeams)
            {
                team.PauseStream();
            }
        }

        public void SaveStats(bool printFull)
        {
            foreach (var team in activeTeams)
                if (printFull)
                    team.SaveFullStats();
                else team.SaveStats();
        }

        public void SetRoundLength(int minutes)
        {
            RoundLength = minutes * 60;
        }

        protected override IEnumerable<ServerReport> GenerateReports()
        {
            int numPlayers = activeTeams[0].TeamPlayers.Count + activeTeams[1].TeamPlayers.Count;

            _reportList.Clear();

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

        protected async Task RoundUpdater(CancellationToken ct)
        {
            lastTime = DateTime.Now;
            PeriodicTimer taskTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(100));

            while (!ct.IsCancellationRequested)
            {
                if (!RoundPaused)
                {
                    double dt = (DateTime.Now - lastTime).TotalSeconds;
                    lastTime = DateTime.Now;

                    if (roundTimer.Update(dt))
                    {
                        EndRound();
                    }
                }
                await taskTimer.WaitForNextTickAsync(ct);
            }
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

        private void StartRoundCommand(List<string> args)
        {
            if (args.Any() && int.TryParse(args[0], out int minutes))
                SetRoundLength(minutes);
            StartRound();
        }
        private void EndRoundCommand(List<string> args)
            => EndRound();
        private void PauseRoundCommand(List<string> args)
            => PauseRound();
        private void SaveStatsCommand(List<string> args)
        {
            bool printFull = false;
            if (args.Any())
            {
                string lowerArg = args[0].ToLower();
                printFull = (lowerArg == "full" || lowerArg == "f");
            }

            foreach (var team in activeTeams)
                if (printFull)
                    team.SaveFullStats();
                else team.SaveStats();
        }
        private void SetRoundLengthCommand(List<string> args)
        {
            if (args.Any() && int.TryParse(args[0], out int minutes))
                SetRoundLength(minutes);
            else ConsoleExt.WriteErrorLine("Usage: setRoundLength <minutes>");
        }
    }
}
