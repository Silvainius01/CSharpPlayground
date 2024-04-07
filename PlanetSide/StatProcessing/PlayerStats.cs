using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DaybreakGames.Census.Stream;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Bson;
using System.Security.Cryptography.X509Certificates;

namespace PlanetSide
{
    public class PlayerStats
    {
        public Dictionary<int, CumulativeExperience> Experience = new Dictionary<int, CumulativeExperience>();
    }
}
