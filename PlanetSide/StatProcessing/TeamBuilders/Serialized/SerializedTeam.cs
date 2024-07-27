using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanetSide
{
    public class SerializedTeam
    {
        public int TeamSize { get; private set; }
        public int WorldId { get; private set; }
        public int ZoneId { get; protected set; }
        public int FactionId { get; private set; }
        public string TeamName { get; private set; }
        public PlanetStats TeamStats { get; private set; }
        public Dictionary<int, WeaponStats> TeamWeapons { get; private set; }
        public Dictionary<string, PlayerStats> TeamPlayers { get; private set; }
    }
}
