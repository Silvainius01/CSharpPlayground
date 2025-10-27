using CommandEngine;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace RogueCrawler
{
    class CreatureArmorSlots : IInspectable, ISerializable<SerializedArmorSlots, CreatureArmorSlots>
    {
        public float TotalWeight { get => _armorSlots.Sum(kvp => kvp.Value.Weight); }
        public float ArmorRating { get => GetTotalArmorRating(); }
        public float ArmorCoverage { get => GetTotalArmorCoverage(); }

        public ReadOnlyDictionary<ArmorSlotType, ItemArmor> ArmorSlots;

        Dictionary<ArmorSlotType, ItemArmor> _armorSlots = new Dictionary<ArmorSlotType, ItemArmor>();

        public CreatureArmorSlots()
        {
            ArmorSlots = new ReadOnlyDictionary<ArmorSlotType, ItemArmor>(_armorSlots);
        }

        /// <returns>The item previously in the slot, if any.</returns>
        public ItemArmor EquipItem(ItemArmor item)
        {
            ItemArmor rv = null;

            if(IsSlotOccupied(item.SlotType))
                rv = _armorSlots[item.SlotType];

            _armorSlots[item.SlotType] = item;
            return rv;
        }

        public bool IsSlotOccupied(ArmorSlotType slot)
            => _armorSlots.ContainsKey(slot);
        public float GetSlotArmorRating(ArmorSlotType slot)
            => IsSlotOccupied(slot) ? _armorSlots[slot].GetArmorRating() : 0f;
        public float GetSlotArmorCoverage(ArmorSlotType slot)
            => IsSlotOccupied(slot) ? _armorSlots[slot].GetArmorCoverage() : 0f;


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

        public string BriefString()
        {
            return $"Armor Rating: {ArmorRating} | Coverage: {ArmorCoverage}";
        }
        public string InspectString(string prefix, int tabCount)
        {
            SmartStringBuilder sb = new SmartStringBuilder();

            if (prefix is null)
                prefix = "Armor Slots:";

            sb.Append(tabCount, prefix);

            ++tabCount;
            foreach(var slotType in EnumExt<ArmorSlotType>.Values)
            {
                if(IsSlotOccupied(slotType))
                {
                    sb.NewlineAppend(_armorSlots[slotType].InspectString(slotType.ToString(), tabCount));
                }
            }
            --tabCount;

            return sb.ToString();
        }
        public string DebugString(string prefix, int tabCount)
        {
            throw new NotImplementedException();
        }

        public SerializedArmorSlots GetSerializable()
        {
            SerializedArmorSlots serialized = new SerializedArmorSlots();
            foreach (var slot in _armorSlots)
                serialized.ArmorSlots.Add(slot.Key, (SerializedArmor)slot.Value.GetSerializable());
            return serialized;
        }
    }

    class SerializedArmorSlots : ISerialized<CreatureArmorSlots>
    {
        public Dictionary<ArmorSlotType, SerializedArmor> ArmorSlots { get; set; } = new Dictionary<ArmorSlotType, SerializedArmor>();

        public CreatureArmorSlots GetDeserialized()
        {
            CreatureArmorSlots armorSlots = new CreatureArmorSlots();

            foreach (var slot in ArmorSlots)
                armorSlots.EquipItem(DungeonGenerator.ArmorGenerator.FromSerialized(slot.Value));

            return armorSlots;
        }
    }
}