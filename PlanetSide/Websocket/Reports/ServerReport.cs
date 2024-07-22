using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanetSide.Websocket
{
    public struct ServerReport
    {
        public string Topic { get; set; }
        public string Data { get; set; }
    }
}
