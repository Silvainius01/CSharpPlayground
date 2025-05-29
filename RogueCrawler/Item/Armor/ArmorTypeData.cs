using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace RogueCrawler
{
    class ArmorTypeData
    {
        public string ArmorType { get; set; }
        public string ArmorClass { get; set; }
        public ArmorSlotType ArmorSlot { get; set; }

        public int BaseValue { get; set; } = 1;
        public int BaseArmorRating { get; set; } = 1;

        public bool AllowAnyMetal { get; set; } = false;
        public string[] AllowedMaterials { get; set; }
    }
}
