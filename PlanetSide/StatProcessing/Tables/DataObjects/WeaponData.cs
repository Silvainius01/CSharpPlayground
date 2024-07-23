using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanetSide
{
    public struct WeaponData : ITeamDataObject
    {
        /// <summary> The ItemId of the weapon. This is the ID you typically want to be using, as the IDs received in events use it. </summary>
        public int Id { get; set; }
        public int TeamId { get; set; }
        public int WeaponId { get; set; }
        public int FactionId { get; set; }
        public string Name { get; set; }
        public bool IsVehicleWeapon { get; set; }

        public override string ToString()
            => $"[{Id}][{WeaponId}] {Name}";
    }
}
