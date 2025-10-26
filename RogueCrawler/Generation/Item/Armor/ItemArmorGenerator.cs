using CommandEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueCrawler
{
    class ItemArmorGenerator : BaseDungeonObjectGenerator<ItemArmor, ItemArmorGenerationParameters>
    {
        public override ItemArmor Generate(ItemArmorGenerationParameters aParams)
        {
            aParams.Validate();

            string armorClass = aParams.PossibleArmorClasses.Any()
                ? aParams.PossibleArmorClasses.RandomItem()
                : ArmorTypeManager.ArmorByClass.Keys.RandomItem();
            ArmorSlotType armorSlot = aParams.PossibleArmorSlots.Any()
                ? aParams.PossibleArmorSlots.RandomItem()
                : EnumExt<ArmorSlotType>.RandomValue;
            ArmorTypeData data = ArmorTypeManager.ArmorByClass[armorClass]
                .Where(type => type.ArmorSlot == armorSlot).RandomItem();

            ItemArmor armor = new ItemArmor()
            {
                ID = NextId,
                BaseValue = data.BaseValue,
                Weight = CommandEngine.Random.NextInt(aParams.WeightRange, true) / 10.0f,
                Quality = GetQuality(aParams),
                Material = GetRandomMaterial(data),
                ItemName = data.ArmorType,
                ObjectName = data.ArmorClass,

                Condition = 1,
                MaxCondition = 1,

                SlotType = armorSlot,
                BaseArmorRating = data.BaseArmorRating,
            };

            return armor;
        }

        public ItemArmor GenerateUnarmoredSlot(ArmorSlotType slot)
        {
            string slotName = EnumExt<ArmorSlotType>.GetName(slot);
            return new ItemArmor()
            {
                ID = -1,
                BaseValue = 0,
                Weight = 0,
                Quality = 1,
                Material = MaterialTypeManager.Materials["Leather"],
                ItemName = $"Bare {slotName}",
                ObjectName = $"Bare{slotName}",
                ArmorClass = "Unarmored",
                BaseArmorRating = 0,
                SlotType = slot
            };
        }

        public ItemArmor FromSerialized(SerializedArmor serialized)
        {
            ItemArmor armor = new ItemArmor()
            {
                ID = NextId,
                ItemName = serialized.ItemName,
                ObjectName = serialized.ObjectName,
                Weight = serialized.Weight,
                Quality = serialized.Quality,
                BaseValue = serialized.BaseValue,
                Material = MaterialTypeManager.GetMaterialFromName(serialized.MaterialName),
                Condition = serialized.Condition,
                MaxCondition = serialized.MaxCondition,

                SlotType= serialized.SlotType,
                ArmorClass = serialized.ArmorClass,
                BaseArmorRating = serialized.BaseArmorRating,
            };
            return armor;
        }

        private ItemMaterial GetRandomMaterial(ArmorTypeData armorData)
        {
            return MaterialTypeManager.Materials.Values
                .Where(m => armorData.AllowedMaterials.Contains(m.Name) || (armorData.AllowAnyMetal && m.IsMetallic))
                .RandomItem();
        }
    }
}
