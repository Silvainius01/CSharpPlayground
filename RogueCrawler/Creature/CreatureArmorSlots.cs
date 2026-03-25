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
        public static readonly int TotalSlots = EnumExt<ArmorSlotType>.Count;
        public static readonly float UnarmoredRatingModifier = 100.0f / DungeonSettings.MaxUnarmoredSkillRating;

        public float TotalWeight { get => _armorSlots.Sum(kvp => kvp.Value.Weight); }
        public float ArmorCoverage { get => GetTotalArmorCoverage(); }
        public float BaseArmorRating { get => GetTotalBaseArmorRating(); }

        public ReadOnlyDictionary<ArmorSlotType, ItemArmor> ArmorSlots;

        Dictionary<ArmorSlotType, ItemArmor> _armorSlots = new Dictionary<ArmorSlotType, ItemArmor>();

        public CreatureArmorSlots()
        {
            ArmorSlots = new ReadOnlyDictionary<ArmorSlotType, ItemArmor>(_armorSlots);
        }

        /// <returns> The item previously in the slot, if any. </returns>
        public ItemArmor EquipItem(ItemArmor item)
        {
            ItemArmor rv = null;

            if (IsSlotOccupied(item.SlotType))
            {
                rv = _armorSlots[item.SlotType];
            }

            _armorSlots[item.SlotType] = item;
            return rv;
        }

        public bool IsSlotOccupied(ArmorSlotType slot)
            => _armorSlots.ContainsKey(slot) && _armorSlots[slot].ArmorClass != DungeonConstants.ArmorClassUnarmored;
        public float GetSlotArmorCoverage(ArmorSlotType slot)
            => IsSlotOccupied(slot) ? _armorSlots[slot].GetArmorCoverage() : 0f;
        public float GetSlotBaseArmorRating(ArmorSlotType slot)
            => IsSlotOccupied(slot) ? _armorSlots[slot].GetArmorRating() : 0f;
        public float GetSlotArmorRating(ArmorSlotType slot, Creature c)
        { 
            if(_armorSlots.ContainsKey(slot))
            {
                return GetArmorRatingOf(_armorSlots[slot], c.Proficiencies);
            }
            throw new Exception("Armor slots object does not contain an entry");
        }
        public float GetArmorRatingOf(ItemArmor armor, CreatureProficiencies wearer)
        {
            if (armor.ArmorClass == DungeonConstants.ArmorClassUnarmored)
            {
                // Note: since skill determines both the base rating and the skill bonus, the final maximum rating is 1.25x the number in settings.
                // Assuming no enchantments.
                float rating = wearer.GetSkillLevel(armor.ArmorClass) / UnarmoredRatingModifier;
                return rating * CreatureSkillUtility.GetArmorSkillBonus(armor, wearer);
            }
            return armor.GetArmorRating() * CreatureSkillUtility.GetArmorSkillBonus(armor, wearer);
        }

        float GetTotalBaseArmorRating()
        {
            float totalRating = 0f;
            foreach (var slot in EnumExt<ArmorSlotType>.Values)
                totalRating += GetSlotBaseArmorRating(slot);
            return totalRating;
        }
        public float GetTotalArmorRating(Creature c)
        {
            float totalRating = 0f;
            foreach (var slot in EnumExt<ArmorSlotType>.Values)
                totalRating += GetSlotArmorRating(slot, c);
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
            return $"Armor Rating: {BaseArmorRating} | Coverage: {ArmorCoverage}";
        }
        public string InspectString(string prefix, int tabCount)
        {
            SmartStringBuilder sb = new SmartStringBuilder();

            if (prefix is null)
                prefix = "Armor Slots:";

            sb.Append(tabCount, prefix);

            ++tabCount;
            foreach (var slotType in EnumExt<ArmorSlotType>.Values)
            {
                if (IsSlotOccupied(slotType))
                {
                    string slotPrefix = $"{slotType}, {_armorSlots[slotType].ItemName}";
                    sb.NewlineAppend(_armorSlots[slotType].InspectString(slotPrefix, tabCount));
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