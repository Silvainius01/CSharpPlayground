using System;
using System.Collections.Generic;
using System.Threading;
using Newtonsoft.Json;

namespace PlanetSide.Websocket
{
    public class CommSmashReporter : ReportServer
    {
        string world;
        CommSmashLeaderboard leaderboard;
        List<PlanetSideTeam> activeTeams;
        List<LeaderboardRequest> leaderboardRequests;
        Dictionary<string, PlanetStats> statsDict;

        PeriodicTimer leaderboardTimer = new PeriodicTimer(TimeSpan.FromSeconds(5));


        public CommSmashReporter(string port, string world) : base(port, ServerType.Publisher)
        {
            this.world = world;
            activeTeams = new List<PlanetSideTeam>();
            statsDict = new Dictionary<string, PlanetStats>();

            leaderboardRequests = new List<LeaderboardRequest>()
            {
                new LeaderboardRequest() { Name = "leaderboard-kills-infantry", GetStat = stats => stats.Kills },
                new LeaderboardRequest() { Name = "leaderboard-kills-vehilce", GetStat= stats => stats.VehicleKills },
                new LeaderboardRequest() { Name = "leaderboard-kills-air", GetStat= stats => stats.AirKills },
                new LeaderboardRequest() { Name = "leaderboard-kills-max", GetStat= stats => stats.GetExp(29).NumEvents },
                new LeaderboardRequest() { Name = "leaderboard-revives", GetStat = stats => stats.GetExp(7).NumEvents },
                new LeaderboardRequest() { Name = "leaderboard-resupplies", GetStat = stats =>
                {
                    int count = 0;
                    foreach(var id in ExperienceTable.ResupplyIds)
                        count += stats.GetExp(id).NumEvents;
                    return count;
                }},
            };
        }

        protected override bool OnServerStart()
        {
            Tracker.PopulateTables();

            var teamOne = new FactionTeam("New Conglomerate", 2, world);
            var teamTwo = new FactionTeam("Terran Republic", 3, world);

            activeTeams.Add(teamOne);
            activeTeams.Add(teamTwo);
            leaderboard = new CommSmashLeaderboard(teamOne, teamTwo);

            foreach (var team in activeTeams)
            {
                team.StartStream();
            }

            return true;
        }

        List<ServerReport> _reportList = new List<ServerReport>();
        protected override IEnumerable<ServerReport> GenerateReports()
        {
            _reportList.Clear();
            _reportList.Add(GetCommsSmashReport());

            int numPlayers = activeTeams[0].TeamPlayers.Count + activeTeams[1].TeamPlayers.Count;

            if (numPlayers >= 10)
                foreach (var request in leaderboardRequests)
                {
                    leaderboard.GenerateLeaderboard(request);
                    var board = leaderboard.Boards[request.Name];
                    _reportList.Add(new ServerReport()
                    {
                        Data = JsonConvert.SerializeObject(board),
                        Topic = request.Name
                    });
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
    }
}
