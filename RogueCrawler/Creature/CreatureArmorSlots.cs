using CommandEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RogueCrawler
{
    class CreatureArmorSlots
    {
        public float TotalWeight { get => armorSlots.Sum(kvp => kvp.Value.Weight); }
        public float ArmorRating { get => GetTotalArmorRating(); }
        public float ArmorCoverage { get => GetTotalArmorCoverage(); }

        Dictionary<ArmorSlotType, ItemArmor> armorSlots = new Dictionary<ArmorSlotType, ItemArmor>();

        /// <returns>The item previously in the slot, if any.</returns>
        public ItemArmor EquipItem(ItemArmor item)
        {
            ItemArmor rv = null;

            if(IsSlotOccupied(item.SlotType))
                rv = armorSlots[item.SlotType];

            armorSlots[item.SlotType] = item;
            return rv;
        }

        public bool IsSlotOccupied(ArmorSlotType slot)
            => armorSlots.ContainsKey(slot);
        public float GetSlotArmorRating(ArmorSlotType slot)
            => IsSlotOccupied(slot) ? armorSlots[slot].GetArmorRating() : 0f;
        public float GetSlotArmorCoverage(ArmorSlotType slot)
            => IsSlotOccupied(slot) ? armorSlots[slot].GetArmorCoverage() : 0f;


        float GetTotalArmorRating()
        {
            float totalRating = 0f;
            foreach (var slot in EnumExt<ArmorSlotType>.Values)
                totalRating += GetSlotArmorRating(slot);
            return totalRating;
        }
        
        float GetTotalArmorCoverage()
        {
            float totalCoverage = 0f;
            foreach (var slot in EnumExt<ArmorSlotType>.Values)
                totalCoverage += GetSlotArmorCoverage(slot);
            return totalCoverage;
        }
    }
}