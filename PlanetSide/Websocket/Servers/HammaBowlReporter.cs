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
    public class HammaBowlReporter : PlanetSideReporter
    {
        public HammaBowlReporter(string port, string world, int zone) : base(port, world, zone)
        {

        }

        protected override List<LeaderboardRequest> GenerateLeaderboardRequests()
        {
            throw new NotImplementedException();
        }

        protected override List<PlanetSideTeam> GenerateTeams()
        {
            throw new NotImplementedException();
        }
    }
}
