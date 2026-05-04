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
using System.ComponentModel;

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
        protected CommandEngine.Timer roundTimer;
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
            roundTimer = new CommandEngine.Timer(TimeSpan.FromSeconds(RoundLength).TotalSeconds);

            serverCommands.Add(new ConsoleCommand("startRound", StartRoundCommand));
            serverCommands.Add(new ConsoleCommand("endRound", EndRoundCommand));
            serverCommands.Add(new ConsoleCommand("pauseRound", PauseRoundCommand));
            serverCommands.Add(new ConsoleCommand("resumeRound", StartRoundCommand));
            serverCommands.Add(new ConsoleCommand("saveStats", SaveStatsCommand));
            serverCommands.Add(new ConsoleCommand("setRoundLength", SetRoundLengthCommand));
        }

        protected override void OnInitialize()
        {
            Tracker.PopulateTables();
            activeTeams = GenerateTeams();
            leaderboardRequests = GenerateLeaderboardRequests();
        }
        protected override void OnServerStart()
        {
            // Start calculating leaderboards in the back ground.
            leaderboard = new EventLeaderboard(activeTeams.ToArray());
            ctLeaderboardLoop = new CancellationTokenSource();
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
            if (RoundStarted)
            {
                Logger.LogWarning("Round already started");
                return;
            }

            RoundPaused = false;
            RoundStarted = true;
            roundTimer.Activate(RoundLength);

            foreach (var team in activeTeams)
                team.ResumeStream();

            ctUpdate = new CancellationTokenSource();
            Task.Run(() => RoundUpdater(ctUpdate.Token));

            Console.WriteLine($"Round Started with {RoundLength / 60} minutes");
        }
        public void PauseRound()
        {
            RoundPaused = true;
            roundTimer.Deactivate();
            foreach (var team in activeTeams)
            {
                team.PauseStream();
            }

            double timeLeft = roundTimer.timeLeft;
            int minutes = (int)(roundTimer.timeLeft / 60);
            Console.WriteLine($"Round Paused. Time left: {minutes}:{(int)(timeLeft - minutes * 60)}");
        }
        public void ResumeRound()
        {
            if (!roundTimer.Resume())
            {
                Logger.LogError("Round timer failed to resume.");
                return;
            }

            RoundPaused = false;
            foreach (var team in activeTeams)
                team.ResumeStream();

            double timeLeft = roundTimer.timeLeft;
            int minutes = (int)(roundTimer.timeLeft / 60);
            Console.WriteLine($"Round Resumed. Time left: {minutes}:{(int)(timeLeft - minutes * 60)}");
        }
        public void EndRound()
        {
            if (!RoundStarted)
            {
                Logger.LogWarning("Round already over.");
                return;
            }

            RoundPaused = false;
            RoundStarted = false;
            ctUpdate.Cancel();
            ctLeaderboardLoop.Cancel();
            roundTimer.Deactivate(true);
            foreach (var team in activeTeams)
                team.PauseStream();
            Console.WriteLine("Round Over!");
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
            if (RoundStarted)
            {
                Logger.LogWarning("Cannot edit round length after it has started.");
                return;
            }
            RoundLength = minutes * 60;
        }

        protected override IEnumerable<ServerReport> GenerateReports()
        {
            int numPlayers = 0;
            foreach (var team in activeTeams)
                numPlayers += team.TeamPlayers.Count;

            _reportList.Clear();

            if (_leaderboardReports.Count > 0)
            {
                Console.WriteLine($"Leaderboard Queue: {_leaderboardReports.Count}");
                while (_leaderboardReports.Count > 0)
                {
                    if (_leaderboardReports.TryDequeue(out var report))
                        _reportList.Add(report);
                }
            }

            _reportList.Add(GenerateReport());

            return _reportList;
        }

        protected async Task RoundUpdater(CancellationToken ct)
        {
            int warnState = 0;
            int currentMinute = (int)(RoundLength / 60.0);
            PeriodicTimer taskTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(100));
            lastTime = DateTime.Now;

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
                    else if (currentMinute * 60 - roundTimer.timeLeft >= 60)
                    {
                        currentMinute = (int)Math.Ceiling(roundTimer.timeLeft / 60);
                        Console.WriteLine($"Round Time Left: {currentMinute} minutes");
                    }
                    else if (currentMinute == 1)
                    {
                        switch (warnState)
                        {
                            case 0:
                                ++warnState;
                                Console.WriteLine("60 seconds left!");
                                break;
                            case 1:
                                if (roundTimer.timeLeft <= 30)
                                {
                                    ++warnState;
                                    Console.WriteLine("30 seconds left!");
                                }
                                break;
                            case 2:
                                if (roundTimer.timeLeft <= 10)
                                {
                                    ++warnState;
                                    Console.WriteLine("10 seconds left!");
                                }
                                break;
                        }
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
                int numPlayers = 0;
                foreach (var team in activeTeams)
                    numPlayers += team.TeamPlayers.Count;

                foreach (var request in leaderboardRequests)
                {
                    if (numPlayers < request.BoardSize)
                        continue;

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
        protected abstract ServerReport GenerateReport();

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
