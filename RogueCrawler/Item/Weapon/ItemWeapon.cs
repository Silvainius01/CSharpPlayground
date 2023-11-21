using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandEngine;
using RogueCrawler.Item.Weapon;

namespace RogueCrawler
{
    class ItemWeapon : IItem
    {
        public int ID { get; set; }
        public int Level
        {
            get => AttributeRequirements.WeaponLevel;
            set { throw new InvalidOperationException("Cannot directly set the level of a weapon."); }
        }
        public float BaseDamage { get; set; }
        public float Weight { get; set; }
        public float Quality { get; set; }
        public int Value { get; set; }
        public bool IsLargeWeapon { get; set; }

        public string ItemName { get; set; } // Display Name
        public string ObjectName { get; set; } // Weapon Skill
        public string WeaponType { get; set; } // General Skill

        public ItemMaterial Material { get; set; }

        public AttributeType MajorAttribute { get; set; }
        public AttributeType MinorAttribute { get; set; }
        public AttributeType DamageAttribute { get; set; }
        public CrawlerAttributeSet AttributeRequirements { get; set; }

        public float GetWeaponDamage()
        {
            float damage = BaseDamage
                + AttributeRequirements.GetAttribute(MajorAttribute) / 2
                + AttributeRequirements.GetAttribute(MinorAttribute) / 4;
            return damage * Material.DamageModifier * Quality;
        }

        public float GetFatigueCost() => Weight * 2;

        public float GetRawValue() =>
            ((IsLargeWeapon ? 1 : 2) * Level * Quality) + AttributeRequirements.TotalScore;
        public int GetValue() => (int)Math.Max(GetRawValue(), 1);

        public WeaponTypeData GetWeaponData()
            => WeaponTypeManager.WeaponTypes[WeaponType];

        public string BriefString()
        {
            return $"[{ID}] Lv.{Level} {ItemName} | DMG: {GetWeaponDamage()} | V: {Value} | W: {Weight}";
        }
        public string InspectString(string prefix, int tabCount)
        {
            ColorStringBuilder builder = new ColorStringBuilder(DungeonCrawlerSettings.TabString);

            if (prefix == string.Empty)
                prefix = $"Weapon stats for [{ID}] {ItemName}:";

            builder.Append(tabCount, prefix);
            tabCount++;
            builder.NewlineAppend(tabCount, $"Type: Lv.{Level} {WeaponType}, {ObjectName}");
            builder.NewlineAppend(tabCount, $"Damage: {GetWeaponDamage()}");
            builder.NewlineAppend(tabCount, $"Value: {GetValue()}");
            builder.NewlineAppend(tabCount, $"Quality: {Quality}");
            builder.NewlineAppend(tabCount, $"Weight: {Weight}");
            builder.NewlineAppend(tabCount, $"Material: {Material.Name}");
            builder.NewlineAppend(AttributeRequirements.InspectString("Requirements:", tabCount));
            tabCount--;

            return builder.ToString();
        }
        public string DebugString(string prefix, int tabCount)
        {
            SmartStringBuilder builder = new SmartStringBuilder(DungeonCrawlerSettings.TabString); ;

            if (prefix == string.Empty)
                prefix = $"Weapon stats for {ItemName}:";

            builder.Append(tabCount, prefix);
            tabCount++;
            builder.NewlineAppend(tabCount, $"ID: {ID}");
            builder.NewlineAppend(tabCount, $"Type: Lv.{Level} {WeaponType},{ObjectName}");
            builder.NewlineAppend(tabCount, $"Base Damage: {BaseDamage}");
            builder.NewlineAppend(tabCount, $"Quality: {Quality}");
            builder.NewlineAppend(tabCount, $"Weight: {Weight}");
            builder.NewlineAppend(tabCount, $"Two Handed: {IsLargeWeapon}");
            builder.NewlineAppend(tabCount, $"Min Expected Damage: {GetWeaponDamage()}");
            builder.NewlineAppend(tabCount, $"Value: {GetValue()} ({GetRawValue()})");
            builder.NewlineAppend(tabCount, $"Material: {Material.Name}");
            builder.NewlineAppend(tabCount, AttributeRequirements.InspectString("Requirements:", tabCount));
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
            builder.NewlineAppend(tabCount, $"Type: Lv.{Level} {WeaponType}");
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
                Value = Value,
                Quality = Quality,
                Weight = Weight,
                BaseDamage = BaseDamage,
                ItemName = ItemName,
                ObjectName = ObjectName,
                WeaponType = WeaponType,
                IsLargeWeapon = IsLargeWeapon,
                MaterialName = Material.Name
            };

            return s;
        }
    }

    class SerializedWeapon : SerializedItem
    {
        public float BaseDamage { get; set; }
        public bool IsLargeWeapon { get; set; }
        public string WeaponType { get; set; }

        public override IItem GetDeserialized()
        {
            return DungeonGenerator.GenerateWeaponFromSerialized(this);
        }
    }
}
