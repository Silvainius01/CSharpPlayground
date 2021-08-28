using System;
using System.Collections.Generic;
using System.Text;

namespace DnD_Generator
{
    class CreatureArmorSlots
    {
        public List<ItemArmor> EquippedRings = new List<ItemArmor>();
        public Dictionary<ItemArmorSlotType, ItemArmor> EquippedArmor = new Dictionary<ItemArmorSlotType, ItemArmor>();

        public bool EquipArmor(ItemArmor armor)
        {
            return false;
        }
    }
}
