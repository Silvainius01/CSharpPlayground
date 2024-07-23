using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanetSide
{
    public struct VehicleData : ITeamDataObject
    {
        public int Id { get; set; }
        public int TeamId { get; set; }
        public int FactionId { get; set; }
        public string Name { get; set; }
        public VehicleType Type { get; set; }

        public override string ToString()
            => $"[{Id}] {Name}";
    }
}
