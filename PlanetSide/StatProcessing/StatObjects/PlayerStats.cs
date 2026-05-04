using PlanetSide.StatProcessing.StatObjects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanetSide
{
    public class PlayerStats : IStatObject<CharacterData>
    {
        public string Alias { get; set; } = string.Empty;
        public CharacterData Data { get; set; }
        public PlanetStats Stats { get; set; }
        public Dictionary<int, WeaponStats> WeaponStats = new Dictionary<int, WeaponStats>();
    }
}
