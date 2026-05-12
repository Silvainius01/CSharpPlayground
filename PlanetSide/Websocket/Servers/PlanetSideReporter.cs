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
using DaybreakGames.Census.Stream;
using System.Text.Json;

namespace PlanetSide.Websocket
{
    public abstract class PlanetSideReporter : ReportServer
    {
        public int ZoneId { get; protected set; } = -1;
        public int WorldId { get; protected set; }
        public float LeaderboardRefresh { get; protected set; } = 10.0f;

        // Round state data
        public double RoundLength { get; protected set; } = 15 * 60;
        public bool RoundPaused { get; private set; }
        public bool RoundStarted { get; private set; }

        protected DateTime lastTime = DateTime.Now;
        protected CommandEngine.Timer roundTimer;
        protected CancellationTokenSource ctRoundUpdate;

        protected EventLeaderboard leaderboard;
        protected List<PlanetSideTeam> activeTeams;
        protected List<LeaderboardRequest> leaderboardRequests;

        List<ServerReport> _reportList = new List<ServerReport>();
        ConcurrentQueue<ServerReport> _leaderboardReports = new ConcurrentQueue<ServerReport>();
        CancellationTokenSource ctLeaderboardLoop;

        public PlanetSideReporter(string port, int world, int zone) : base(port, ServerType.Publisher)
        {
            this.WorldId = world;
            this.ZoneId = zone;
            roundTimer = new CommandEngine.Timer(TimeSpan.FromSeconds(RoundLength).TotalSeconds);
            ctRoundUpdate = new CancellationTokenSource();


            serverCommands.Add(new ConsoleCommand("startRound", StartRoundCommand));
            serverCommands.Add(new ConsoleCommand("endRound", EndRoundCommand));
            serverCommands.Add(new ConsoleCommand("pauseRound", PauseRoundCommand));
            serverCommands.Add(new ConsoleCommand("resumeRound", ResumeRoundCommand));
            serverCommands.Add(new ConsoleCommand("saveStats", SaveStatsCommand));
            serverCommands.Add(new ConsoleCommand("setRoundLength", SetRoundLengthCommand));
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
                team.ResumeProcessing();

            if (ctRoundUpdate.IsCancellationRequested)
                ctRoundUpdate = new CancellationTokenSource();
            Task.Run(() => RoundUpdater(ctRoundUpdate.Token));

            Console.WriteLine($"Round Started with {RoundLength / 60} minutes");
        }
        public void PauseRound()
        {
            RoundPaused = true;
            roundTimer.Deactivate();
            foreach (var team in activeTeams)
            {
                team.PauseProcessing();
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
                team.ResumeProcessing();

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

            roundTimer.Deactivate(true);
            foreach (var team in activeTeams)
                team.PauseProcessing();

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

        public bool ProcessCensusEvent(SocketResponse response)
        {
            // Drop all events while paused or stopped.
            if (!RoundStarted || RoundPaused)
                return false;

            // Parse the event here so each team can focus on filtering instead of deserializing.
            ICensusEvent? censusEvent = Tracker.ProcessCensusEvent(response);

            // Give the event object to each team
            if (censusEvent is not null)
            {
                // Jank, but task that writes to disk only accepts the JsonElement rn
                bool eventIsValid = false;
                JsonElement payload = response.Message.RootElement.GetProperty("payload");

                // Send event to teams, and mark if it was accepted by any of them.
                foreach (var team in activeTeams)
                    eventIsValid |= team.ProcessCensusEvent(censusEvent, payload);

                // Send any accepted events to other systems that care. 
                if (eventIsValid)
                {
                    switch (censusEvent.EventType)
                    {
                        case CensusEventType.GainExperience:
                            var expEvent = (ExperiencePayload)censusEvent;
                            DamageTracker.AddAssist(expEvent);
                            DamageTracker.AddHeals(expEvent);
                            break;
                        case CensusEventType.Death:
                            var deathEvent = (DeathPayload)censusEvent;
                            DamageTracker.AddKill(deathEvent);
                            break;
                    }
                }
            }

            return false;
        }

        protected override void OnInitialize()
        {
            Tracker.PopulateTables();
            Tracker.SaveAllEvents = true;
            activeTeams = GenerateTeams();
            leaderboardRequests = GenerateLeaderboardRequests();
        }
        protected override void OnServerStart()
        {
            // Start calculating leaderboards in the back ground.
            leaderboard = new EventLeaderboard(activeTeams.ToArray());
            ctLeaderboardLoop = new CancellationTokenSource();
            Task.Run(() => LeaderboardCalcLoop(ctLeaderboardLoop.Token));

            CensusStreamSubscription subscription = new CensusStreamSubscription()
            {
                Characters = new List<string>(),
                Worlds = new List<string>(),
                EventNames = new List<string>(),
                LogicalAndCharactersWithWorlds = true
            };

            foreach (var team in activeTeams)
            {
                team.AddPlayers();
                subscription.Merge(team.GetStreamSubscription());
                team.StartProcessing();
            }

            // Pre-emptively pause the round. Prevents any events from leaking through.
            PauseRound();

            // Create the event stream
            var handler = Tracker.Handler;
            handler.GetOrAddSubscription("Reporter", subscription);
            handler.AddActionToSubscription("Reporter", ProcessCensusEvent);
            handler.ConnectClientAsync("Reporter").Wait();
        }
        protected override void OnServerPause()
        {
            PauseRound();
        }
        protected override void OnServerStop()
        {
            if (RoundStarted)
                EndRound();

            foreach (var team in activeTeams)
                team.StopProcessing();

            SaveStats(true);
        }

        protected override IEnumerable<ServerReport> GenerateReports()
        {
            int numPlayers = 0;
            foreach (var team in activeTeams)
                numPlayers += team.TeamPlayers.Count;

            _reportList.Clear();

            if (_leaderboardReports.Count > 0)
            {
                Logger.LogDebug($"Leaderboard Queue: {_leaderboardReports.Count}");
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
            using PeriodicTimer taskTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(100));
            lastTime = DateTime.Now;

            Logger.LogInformation("Began round timer routine");
            while (!ct.IsCancellationRequested && RoundStarted)
            {
                if (!RoundPaused)
                {
                    double dt = (DateTime.Now - lastTime).TotalSeconds;
                    lastTime = DateTime.Now;

                    if (roundTimer.Update(dt))
                    {
                        EndRound();
                    }
                    else if (currentMinute <= 2)
                    {
                        switch (warnState)
                        {
                            case 0:
                                if (roundTimer.timeLeft <= 60)
                                {
                                    ++warnState;
                                    Console.WriteLine("60 seconds left!");
                                }
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
                    else if (currentMinute * 60 - roundTimer.timeLeft >= 60)
                    {
                        currentMinute = (int)Math.Ceiling(roundTimer.timeLeft / 60);
                        Console.WriteLine($"Round Time Left: {currentMinute} minutes");
                    }
                }
                await taskTimer.WaitForNextTickAsync(ct);
            }

            if (ct.IsCancellationRequested)
                Logger.LogError("Round timer routine cancelled!");
            else Logger.LogInformation("Round timer routine exited.");
        }
        private async Task LeaderboardCalcLoop(CancellationToken ct)
        {
            float waitTime = LeaderboardRefresh / leaderboardRequests.Count;
            using PeriodicTimer boardtimer = new PeriodicTimer(TimeSpan.FromSeconds(waitTime));

            Logger.LogInformation("Began leaderboard processing");
            while (!ct.IsCancellationRequested && IsActive)
            {
                int numPlayers = 0;
                for (int i = 0; i < activeTeams.Count; i++)
                {
                    activeTeams[i].UpdateDamage(); // Shoving this here since it'll be consistent 
                    numPlayers += activeTeams[i].GetPlayerCount();
                }

                for (int i = 0; i < leaderboardRequests.Count; i++)
                {
                    LeaderboardRequest request = leaderboardRequests[i];
                    if (numPlayers >= request.BoardSize)
                    {
                        var board = leaderboard.CalculateLeaderboard(request);
                        _leaderboardReports.Enqueue(new ServerReport()
                        {
                            Data = JsonConvert.SerializeObject(board),
                            Topic = request.Name,
                        });
                    }
                    await boardtimer.WaitForNextTickAsync(ct);
                }
            }

            if (ct.IsCancellationRequested)
                Logger.LogError("Leaderboard calculations routine cancelled!");
            else Logger.LogInformation("Leaderboard calculation routine exited.");
        }

        protected abstract List<PlanetSideTeam> GenerateTeams();
        protected abstract List<LeaderboardRequest> GenerateLeaderboardRequests();
        protected abstract ServerReport GenerateReport();

        private void StartRoundCommand(List<string> args)
        {
            if (!IsActive)
            {
                Logger.LogWarning("Server is closed.");
                return;
            }
            if (RoundStarted)
            {
                Logger.LogWarning("Round already started.");
                return;
            }

            if (args.Any() && int.TryParse(args[0], out int minutes))
                SetRoundLength(minutes);
            StartRound();
        }
        private void EndRoundCommand(List<string> args)
        {
            if (!IsActive)
            {
                Logger.LogWarning("Server is clsoed.");
                return;
            }

            EndRound();
        }
        private void PauseRoundCommand(List<string> args)
        {
            if (!IsActive)
            {
                Logger.LogWarning("Server is clsoed.");
                return;
            }

            PauseRound();
        }
        private void ResumeRoundCommand(List<string> args)
        {
            if (!IsActive)
            {
                Logger.LogWarning("Server is clsoed.");
                return;
            }
            ResumeRound();
        }
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
