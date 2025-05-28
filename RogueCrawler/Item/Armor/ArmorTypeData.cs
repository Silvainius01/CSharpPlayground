using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace RogueCrawler
{
    class ArmorTypeData
    {
        public string ArmorClass { get; set; }
        public AttributeType MajorAttribute { get; set; }
        public AttributeType MinorAttribute { get; set; }
        public ItemWeaponHandedness Handedness { get; set; }

        public int BaseArmorRating { get; set; } = 1;
        public int BaseValue { get; set; } = 1;
        public int PrimaryAttributeBaseReq { get; set; }
        public int SecondaryAttributeBaseReq { get; set; }
        public float LargeWeaponDamageMult { get; set; } = 2;
        public float LargeWeaponWeightMult { get; set; } = 3;
    }
}
