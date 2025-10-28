using CommandEngine;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace RogueCrawler
{
    class ItemArmor : IItem
    {
        public int ID { get; set; }
        public int Level
        {
            get => 1;
            set { throw new InvalidOperationException("Cannot directly set the level of armor."); }
        }

        public int BaseValue { get; set; }
        public float Weight { get; set; }
        public float Quality { get; set; }
        public ItemMaterial Material { get; set; }

        public string ItemName { get; set; } // Display Name
        public string ObjectName { get; set; } // Armor Type
        public string ArmorClass { get; set; } // Armor Skill

        public int BaseArmorRating { get; set; } = 1;
        public float Condition { get; set; } = 1;
        public float MaxCondition { get; set; } = 1;

        public ArmorSlotType SlotType { get; set; }

        public float GetArmorRating()
        {
            // Base armor rating adds to 10 with full set.
            float slotModifier = SlotType switch
            {
                ArmorSlotType.Head => 2f,
                ArmorSlotType.Chest => 3f,
                ArmorSlotType.Arm => 1f,
                ArmorSlotType.Hand => 1f,
                ArmorSlotType.Waist => 2f,
                ArmorSlotType.Foot => 1f,
                _ => 1.0f
            };

            return MathF.Ceiling(
                BaseArmorRating
                * Material.ArmorRatingModifier
                * MathF.Log2(Quality + 1)
                * slotModifier
            );
        }
        public float GetArmorCoverage()
        {
            // Must add to 1f
            float slotModifier = SlotType switch
            {
                ArmorSlotType.Head => 0.05f,
                ArmorSlotType.Chest => 0.35f,
                ArmorSlotType.Arm => 0.10f,
                ArmorSlotType.Hand => 0.05f,
                ArmorSlotType.Waist => 0.35f,
                ArmorSlotType.Foot => 0.10f,
                _ => 1.0f
            };
            return slotModifier * ArmorClassModifier() * Material.ArmorCoverageModifier;
        }

        public float GetRawValue() =>
            BaseValue * Quality * Material.ValueModifier * (Condition / MaxCondition);
        public int GetValue() => (int)Math.Max(GetRawValue(), 1);

        public string BriefString()
        {
            return $"[{ID}] {ItemName} | " +
                $"R: {GetArmorRating().ToString("n1")} | " +
                $"C: {GetArmorCoverage().ToString("n1")} | " +
                $"V: {GetValue()} | " +
                $"W: {Weight.ToString("n1")}";
        }
        public string InspectString(string prefix, int tabCount)
        {
            SmartStringBuilder builder = new SmartStringBuilder(DungeonSettings.TabString);

            if (prefix == string.Empty)
                prefix = $"Armor stats for [{ID}] {ItemName}:";

            builder.Append(tabCount, prefix);
            tabCount++;
            builder.NewlineAppend(tabCount, $"Type: {SlotType}, {ObjectName}, {ArmorClass}");
            builder.NewlineAppend(tabCount, $"Rating: {GetArmorRating().ToString("n1")}");
            builder.NewlineAppend(tabCount, $"Coverage: {GetArmorCoverage().ToString("n1")}");
            builder.NewlineAppend(tabCount, $"Value: {GetValue()}");
            builder.NewlineAppend(tabCount, $"Quality: {Quality.ToString("n1")}");
            builder.NewlineAppend(tabCount, $"Weight: {Weight.ToString("n1")}");
            builder.NewlineAppend(tabCount, $"Material: {Material.Name}");
            tabCount--;

            return builder.ToString();
        }
        public string DebugString(string prefix, int tabCount)
        {
            SmartStringBuilder builder = new SmartStringBuilder(DungeonSettings.TabString);

            if (prefix == string.Empty)
                prefix = $"Armor stats for [{ID}] {ItemName}:";

            builder.Append(tabCount, prefix);
            tabCount++;
            builder.NewlineAppend(tabCount, $"Type: {SlotType}, {ObjectName}");
            builder.NewlineAppend(tabCount, $"Rating: {GetArmorRating()}");
            builder.NewlineAppend(tabCount, $"Coverage: {GetArmorCoverage()}");
            builder.NewlineAppend(tabCount, $"Value: {GetValue()}");
            builder.NewlineAppend(tabCount, $"Quality: {Quality}");
            builder.NewlineAppend(tabCount, $"Weight: {Weight}");
            builder.NewlineAppend(tabCount, $"Material: {Material.Name}");

            // Debug Specific
            builder.NewlineAppend(tabCount, $"BaseAR: {BaseArmorRating}");
            tabCount--;

            return builder.ToString();
        }

        float ArmorClassModifier()
        {
            float acm = 1.0f;
            switch (ArmorClass)
            {
                case DungeonConstants.ArmorClassUnarmored: acm = 0.0f; break;
                case DungeonConstants.ArmorClassClothing: acm = 0.1f; break;
                case DungeonConstants.ArmorClassLight: acm = 0.33f; break;
                case DungeonConstants.ArmorClassMedium: acm = 0.66f; break;
                case DungeonConstants.ArmorClassHeavy: acm = 1.0f; break;
                default:
                    ConsoleExt.WriteWarning($"Unknown armor class '{ArmorClass}'");
                    break;
            }
            return acm;
        }

        public SerializedItem GetSerializable()
        {
            SerializedArmor s = new SerializedArmor()
            {
                BaseValue = BaseValue,
                Quality = Quality,
                Weight = Weight,
                ItemName = ItemName,
                ObjectName = ObjectName,
                MaterialName = Material.Name,
                Condition = Condition,
                MaxCondition = MaxCondition,

                SlotType = SlotType,
                ArmorClass = ArmorClass,
                BaseArmorRating = BaseArmorRating,
            };

            return s;
        }
    }

    class SerializedArmor : SerializedItem
    {
        public string ArmorClass { get; set; }

        public int BaseArmorRating { get; set; }
        public float Condition { get; set; }
        public float MaxCondition { get; set; }

        public ArmorSlotType SlotType { get; set; }

        public override IItem GetDeserialized()
        {
            return DungeonGenerator.ArmorGenerator.FromSerialized(this);
        }
    }
}
