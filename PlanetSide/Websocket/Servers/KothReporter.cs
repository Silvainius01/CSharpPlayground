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
        const double defaultRoundLength = 10 * 60;

        bool RoundPaused { get; set; }
        bool RoundStarted { get; set; }

        DateTime lastTime = DateTime.Now;
        CommandEngine.Timer roundTimer = new CommandEngine.Timer(TimeSpan.FromMinutes(10).TotalSeconds);
        CancellationTokenSource ctUpdate = new CancellationTokenSource();

        public KothReporter(string port, string world, int zone) : base(port, world, zone)
        {
            this.ZoneId = zone;
            RoundPaused = true;
        }

        public void StartRound(double lengthSeconds = defaultRoundLength)
        {
            RoundPaused = false;
            roundTimer.Activate(lengthSeconds);

            foreach (var team in activeTeams)
                team.UnPauseStream();
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
            roundTimer.Deactivate();
            foreach (var team in activeTeams)
            {
                team.PauseStream();
                team.ResetStats();
            }
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

        private async Task RoundUpdater(CancellationToken ct)
        {
            lastTime = DateTime.Now;

            while (!ct.IsCancellationRequested)
            {
                double dt = (DateTime.Now - lastTime).TotalSeconds;
                lastTime = DateTime.Now;

                if (roundTimer.Update(dt))
                {
                    foreach (var team in activeTeams)
                        team.PauseStream();
                }
            }
        }
    }
}
