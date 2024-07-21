using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using PlanetSide.Websocket;

namespace PlanetSide.WebsocketServer
{
    public class Server
    {
        static List<PlanetSideTeam> activeTeams = new List<PlanetSideTeam>();

        public static void StartPublishingServer()
        {
            StartTeamTracking();
            var statsDict = new Dictionary<string, PlanetStats>();

            using (var publisher = new PublisherSocket())
            {
                publisher.Bind("tcp://*:56854");

                int i = 0;

                while (true)
                {
                    string data = JsonConvert.SerializeObject(GenerateReport());

                    publisher
                        .SendMoreFrame("net_stats") // Topic
                        .SendFrame(data); // Message

                    Console.WriteLine(data);
                    Thread.Sleep(1000);
                }
            }
        }

        static void StartTeamTracking()
        {
            var handler = Tracker.Handler;

            Tracker.PopulateTables(handler);
            //activeTeams.Add(new FactionTeam("Vanu Sovereignty", "1", "17", handler));
            activeTeams.Add(new FactionTeam("New Conglomerate", "2", "10", handler));
            activeTeams.Add(new FactionTeam("Terran Republic", "3", "10", handler));

            foreach (var team in activeTeams)
            {
                team.StartStream();
            }
        }

        static CommSmashReport GenerateReport()
        {
            return new CommSmashReport()
            {
                kills_net_t1 = activeTeams[0].TeamStats.Kills,
                kills_net_t2 = activeTeams[1].TeamStats.Kills,

                kills_vehicle_t1 = activeTeams[0].TeamStats.VehicleKills,
                kills_vehicle_t2 = activeTeams[1].TeamStats.VehicleKills,

                kills_air_t1 = activeTeams[0].TeamStats.TeamKills,
                kills_air_t2 = activeTeams[1].TeamStats.TeamKills,

                revives_t1 = activeTeams[0].TeamStats.GetExp(7).NumEvents,
                revives_t2 = activeTeams[1].TeamStats.GetExp(7).NumEvents,

                captures_t1 = activeTeams[0].TeamStats.GetExp(272).NumEvents,
                captures_t2 = activeTeams[1].TeamStats.GetExp(272).NumEvents,

                defenses_t1 = activeTeams[0].TeamStats.GetExp(6).NumEvents,
                defenses_t2 = activeTeams[1].TeamStats.GetExp(6).NumEvents
            };
        }
    }
}
