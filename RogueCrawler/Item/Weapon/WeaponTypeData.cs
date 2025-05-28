using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace RogueCrawler
{
    class WeaponTypeData
    {
        public string WeaponType { get; set; }
        public AttributeType MajorAttribute { get; set; }
        public AttributeType MinorAttribute { get; set; }
        public AttributeType DamageAttribute { get; set; }
        public ItemWeaponHandedness Handedness { get; set; }

        public int BaseDamage { get; set; }
        public int BaseValue { get; set; }
        public float LargeWeaponDamageMult { get; set; } = 2;
        public float LargeWeaponWeightMult { get; set; } = 3;
        public string[] OneHandedWeaponNames { get; set; }
        public string[] TwoHandedWeaponNames { get; set; }
    }
}
