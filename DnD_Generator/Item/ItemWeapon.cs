using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandEngine;

namespace DnD_Generator
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
        public int Value { get ; set; }
        public string Name { get; set; }
        public string WeaponType { get; set; }
        public bool IsLargeWeapon { get; set; }

        public AttributeType HitBonusAttribute { get; set; }
        public AttributeType DamageBonusAttribute { get; set; }
        public CrawlerAttributeSet AttributeRequirements { get; set; }

        public float GetCreatureDamage(CrawlerAttributeSet attributes) 
        {
            return GetWeaponDamage() + attributes[DamageBonusAttribute];
        }
        public float GetWeaponDamage()
        {
            return BaseDamage * Quality + Level;
        }
        public float GetRawValue() =>
            ((IsLargeWeapon ? 1 : 2) * Level * Quality) + AttributeRequirements.TotalScore;
        public int GetValue() => (int)Math.Max(GetRawValue(), 1);

        public bool CanEquip(CrawlerAttributeSet attributes)
        {
            foreach (KeyValuePair<AttributeType, int> kvp in AttributeRequirements)
                if (attributes[kvp.Key] < kvp.Value)
                    return false;
            return true;
        }

        public WeaponTypeData GetWeaponData()
            => WeaponTypeManager.WeaponTypes[WeaponType];

        public string BriefString()
        {
            return $"[{ID}] Lv.{Level} {Name} | DMG: {GetWeaponDamage()} | V: {Value} | W: {Weight}";
        }
        public string InspectString(string prefix, int tabCount)
        {
            SmartStringBuilder builder = new SmartStringBuilder(DungeonCrawlerSettings.TabString);;

            if (prefix == string.Empty)
                prefix = $"Weapon stats for [{ID}] {Name}:";

            builder.Append(tabCount, prefix);
            tabCount++;
            builder.NewlineAppend(tabCount, $"Type: Lv.{Level} {WeaponType}");
            builder.NewlineAppend(tabCount, $"Damage: {GetWeaponDamage()}");
            builder.NewlineAppend(tabCount, $"Value: {GetValue()}");
            builder.NewlineAppend(tabCount, $"Weight: {Weight}");
            builder.NewlineAppend(AttributeRequirements.InspectString("Requirements:", tabCount));
            tabCount--;
            
            return builder.ToString();
        }
        public string DebugString(string prefix, int tabCount)
        {
            SmartStringBuilder builder = new SmartStringBuilder(DungeonCrawlerSettings.TabString);;

            if (prefix == string.Empty)
                prefix = $"Weapon stats for {Name}:";

            builder.Append(tabCount, prefix);
            tabCount++;
                builder.NewlineAppend(tabCount, $"ID: {ID}");
                builder.NewlineAppend(tabCount, $"Type: Lv.{AttributeRequirements.CreatureLevel} {WeaponType}");
                builder.NewlineAppend(tabCount, $"Base Damage: {BaseDamage}");
                builder.NewlineAppend(tabCount, $"Quality: {Quality}");
                builder.NewlineAppend(tabCount, $"Weight: {Weight}");
                builder.NewlineAppend(tabCount, $"Two Handed: {IsLargeWeapon}");
                builder.NewlineAppend(tabCount, $"Min Expected Damage: {GetWeaponDamage()}");
                builder.NewlineAppend(tabCount, $"Value: {GetValue()} ({GetRawValue()})");
                builder.NewlineAppend(tabCount, AttributeRequirements.InspectString("Requirements:", tabCount));
            tabCount--;

            return builder.ToString();
        }
        public string PlayerWeaponString(string prefix, int tabCount)
        {
            SmartStringBuilder builder = new SmartStringBuilder(DungeonCrawlerSettings.TabString);;

            if (prefix == string.Empty)
                prefix = $"Weapon stats for [{ID}] {Name}:";

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
            return $"[{ID}] {Name}";
        }

        public SerializedItem GetSerializable()
        {
            SerializedWeapon s = new SerializedWeapon()
            {
                Name = Name,
                Value = Value,
                Quality = Quality,
                Weight = Weight,
                BaseDamage = BaseDamage,
                WeaponType = WeaponType,
                IsLargeWeapon = IsLargeWeapon
            };

            return s;
        }
    }

    class SerializedWeapon : SerializedItem
    {
        public float BaseDamage { get; set; }
        public string WeaponType { get; set; }
        public bool IsLargeWeapon { get; set; }

        public override IItem GetDeserialized()
        {
            return DungeonGenerator.GenerateWeaponFromSerialized(this);
        }
    }
}
