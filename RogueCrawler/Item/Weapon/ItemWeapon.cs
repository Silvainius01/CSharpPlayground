using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandEngine;

namespace RogueCrawler
{
    class ItemWeapon : IItem
    {
        public int ID { get; set; }
        public int Level
        {
            get => 1;
            set { throw new InvalidOperationException("Cannot directly set the level of a weapon."); }
        }

        public int BaseValue { get; set; }
        public float Weight { get; set; }
        public float Quality { get; set; }
        public ItemMaterial Material { get; set; }

        public float Condition { get; set; } = 1;
        public float MaxCondition { get; set; } = 1;

        public string ItemName { get; set; } // Display Name
        public string ObjectName { get; set; } // Weapon Skill
        public string WeaponType { get; set; } // General Skill
        
        public bool IsLargeWeapon { get; set; }
        public float BaseDamage { get; set; }

        public AttributeType MajorAttribute { get; set; }
        public AttributeType MinorAttribute { get; set; }
        public CrawlerAttributeSet AttributeRequirements { get; set; }

        public float GetWeaponDamage() 
            => Mathc.Truncate(BaseDamage * Material.DamageModifier * MathF.Log2(Quality + 1), 1);
        public float GetWeaponDamage(Creature wielder)
        {
            float damage = BaseDamage
                + wielder.GetAttribute(MajorAttribute) / 2.0f
                + wielder.GetAttribute(MinorAttribute) / 4.0f;
            damage *= Material.DamageModifier * MathF.Log2(Quality + 1);
            return Mathc.Truncate(damage, 1);
        }

        public float GetFatigueCost() => Weight * 2;

        public float GetRawValue() =>
            BaseValue * (IsLargeWeapon ? 1 : 2) * Quality * Material.ValueModifier * (Condition/MaxCondition);
        public int GetValue() => (int)Math.Max(GetRawValue(), 1);

        public WeaponTypeData GetWeaponData()
            => WeaponTypeManager.WeaponTypes[WeaponType];

        public string BriefString()
        {
            return $"[{ID}] {ItemName} | DMG: {GetWeaponDamage().ToString("n1")} | V: {GetValue()} | W: {Weight.ToString("n1")}";
        }
        public string InspectString(string prefix, int tabCount)
        {
            ColorStringBuilder builder = new ColorStringBuilder(DungeonCrawlerSettings.TabString);

            if (prefix == string.Empty)
                prefix = $"Weapon stats for [{ID}] {ItemName}:";

            builder.Append(tabCount, prefix);
            tabCount++;
            builder.NewlineAppend(tabCount, $"Type: {WeaponType}, {ObjectName}");
            builder.NewlineAppend(tabCount, $"Damage: {GetWeaponDamage().ToString("n1")}");
            builder.NewlineAppend(tabCount, $"Value: {GetValue()}");
            builder.NewlineAppend(tabCount, $"Quality: {Quality.ToString("n1")}");
            builder.NewlineAppend(tabCount, $"Weight: {Weight.ToString("n1")}");
            builder.NewlineAppend(tabCount, $"Material: {Material.Name}");
            tabCount--;

            return builder.ToString();
        }
        public string DebugString(string prefix, int tabCount)
        {
            ColorStringBuilder builder = new ColorStringBuilder(DungeonCrawlerSettings.TabString); ;

            if (prefix == string.Empty)
                prefix = $"Weapon stats for [{ID}] {ItemName}:";

            builder.Append(tabCount, prefix);
            tabCount++;
            builder.NewlineAppend(tabCount, $"ID: {ID}");
            builder.NewlineAppend(tabCount, $"Type: {WeaponType},{ObjectName}");
            builder.NewlineAppend(tabCount, $"Damage: {GetWeaponDamage()}");
            builder.NewlineAppend(tabCount, $"Value: {GetValue()} ({GetRawValue()})");
            builder.NewlineAppend(tabCount, $"Quality: {Quality}");
            builder.NewlineAppend(tabCount, $"Weight: {Weight}");
            builder.NewlineAppend(tabCount, $"Two Handed: {IsLargeWeapon}");
            builder.NewlineAppend(tabCount, $"Material: {Material.Name}");

            // Debug Specific
            builder.NewlineAppend(tabCount, $"Base Damage: {BaseDamage}", ConsoleColor.Cyan);
            tabCount--;

            return builder.ToString();
        }
        public string PlayerWeaponString(string prefix, int tabCount)
        {
            SmartStringBuilder builder = new SmartStringBuilder(DungeonCrawlerSettings.TabString); ;

            if (prefix == string.Empty)
                prefix = $"Weapon stats for [{ID}] {ObjectName}:";

            builder.Append(tabCount, prefix);
            tabCount++;
            builder.NewlineAppend(tabCount, $"Type: {WeaponType}");
            builder.NewlineAppend(tabCount, $"Damage: {GetWeaponDamage()}");
            builder.NewlineAppend(tabCount, $"Value: {GetValue()}");
            builder.NewlineAppend(tabCount, $"Weight: {Weight}");
            tabCount--;

            return builder.ToString();
        }

        public override string ToString()
        {
            return $"[{ID}] {ObjectName}";
        }

        public SerializedItem GetSerializable()
        {
            SerializedWeapon s = new SerializedWeapon()
            {
                BaseValue = BaseValue,
                Quality = Quality,
                Weight = Weight,
                BaseDamage = BaseDamage,
                ItemName = ItemName,
                ObjectName = ObjectName,
                MaterialName = Material.Name,
                Condition = Condition,
                MaxCondition = MaxCondition,

                WeaponType = WeaponType,
                IsLargeWeapon = IsLargeWeapon,
            };

            return s;
        }
    }

    class SerializedWeapon : SerializedItem
    {
        public float BaseDamage { get; set; }
        public bool IsLargeWeapon { get; set; }
        public string WeaponType { get; set; }

        public float Condition { get; set; } = 1;
        public float MaxCondition { get; set; } = 1;

        public override IItem GetDeserialized()
        {
            return DungeonGenerator.WeaponGenerator.FromSerialized(this);
        }
    }
}
