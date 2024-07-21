using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanetSide.Websocket
{
    public struct CommSmashReport
    {
        [JsonProperty("kills-net-t1")]
        public int kills_net_t1 { get; set; }
        [JsonProperty("kills-net-t2")]
        public int kills_net_t2 { get; set; }

        [JsonProperty("kills-vehicle-t1")]
        public int kills_vehicle_t1 { get; set; }
        [JsonProperty("kills-vehicle-t2")]
        public int kills_vehicle_t2 { get; set; }

        [JsonProperty("kills-air-t1")]
        public int kills_air_t1 { get; set; }
        [JsonProperty("kills-air-t2")]
        public int kills_air_t2 { get; set; }

        //public int deaths_net_t1 { get; set; }
        //public int deaths_net_t2 { get; set; }

        [JsonProperty("revives-t1")]
        public int revives_t1 { get; set; }
        [JsonProperty("revives-t2")]
        public int revives_t2 { get; set; }

        [JsonProperty("captures-t1")]
        public int captures_t1 { get; set; }
        [JsonProperty("captures-t2")]
        public int captures_t2 { get; set; }

        [JsonProperty("defenses-t1")]
        public int defenses_t1 { get; set; }
        [JsonProperty("defenses-t2")]
        public int defenses_t2 { get; set; }

        //public int online_t1 { get; set; }
        //public int online_t2 { get; set; }
    }
}
