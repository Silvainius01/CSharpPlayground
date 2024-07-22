using System;
using System.Collections.Generic;
using System.Threading;
using Newtonsoft.Json;

namespace PlanetSide.Websocket
{
    public class CommSmashReporter : ReportServer
    {
        string world;
        List<PlanetSideTeam> activeTeams;
        Dictionary<string, PlanetStats> statsDict;

        PeriodicTimer leaderboardTimer = new PeriodicTimer(TimeSpan.FromSeconds(5));


        public CommSmashReporter(string port, string world) : base(port, ServerType.Publisher) 
        {
            this.world = world;
            activeTeams = new List<PlanetSideTeam>();
            statsDict = new Dictionary<string, PlanetStats>();
        }

        protected override bool OnServerStart()
        {
            Tracker.PopulateTables();

            //activeTeams.Add(new FactionTeam("Vanu Sovereignty", "1", "17"));
            activeTeams.Add(new FactionTeam("New Conglomerate", "2", world));
            activeTeams.Add(new FactionTeam("Terran Republic", "3", world));

            foreach (var team in activeTeams)
            {
                team.StartStream();
            }

            return true;
        }

        protected override ServerReport[] GenerateReports()
        {
            return new []
            {
                GetCommsSmashReport()
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
