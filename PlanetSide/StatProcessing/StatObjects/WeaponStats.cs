using PlanetSide.StatProcessing.StatObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanetSide
{
    public class WeaponStats : IStatObject<WeaponData>
    {
        public WeaponData Data { get; set; }
        public PlanetStats Stats { get; set; }
    }
}
