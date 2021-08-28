using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameEngine;

namespace DnD_Generator
{
    
    class ItemWeapon : IItem
    {
        public int ID { get; set; }
        public float BaseDamage { get; set; }
        public float Weight { get; set; }
        public float Quality { get; set; }
        public int Value { get ; set; }
        public string Name { get; set; }
        public WeaponType WeaponType { get; set; }
        public bool IsLargeWeapon { get; set; }

        public AttributeType HitBonusAttribute { get; set; }
        public AttributeType DamageBonusAttribute { get; set; }
        public CreatureAttributes AttributeRequirements { get; set; }

        public static implicit operator WeaponType(ItemWeapon w) => w.WeaponType;

        public int GetLevel()
            => AttributeRequirements.Level;
        public float GetCreatureDamage(CreatureAttributes attributes) 
        {
            return GetWeaponDamage() + attributes[DamageBonusAttribute];
        }
        public float GetWeaponDamage()
        {
            return BaseDamage * Quality + GetLevel();
        }
        public float GetRawValue() =>
            ((IsLargeWeapon ? 1 : 2) * AttributeRequirements.Level * Quality) + AttributeRequirements.TotalScore;
        public int GetValue() => (int)Math.Max(GetRawValue(), 1);

        public bool CanEquip(CreatureAttributes attributes)
        {
            foreach (KeyValuePair<AttributeType, int> kvp in AttributeRequirements)
                if (attributes[kvp.Key] < kvp.Value)
                    return false;
            return true;
        }

        public WeaponTypeData GetWeaponData()
            => ItemWeaponGenerator.WeaponTypeData[WeaponType];

        public string BriefString()
        {
            return $"[{ID}] Lv.{GetLevel()} {Name} | DMG: {GetWeaponDamage()} | V: {Value} | W: {Weight}";
        }
        public string InspectString(string prefix, int tabCount)
        {
            SmartStringBuilder builder = new SmartStringBuilder(DungeonCrawlerSettings.TabString);;

            if (prefix == string.Empty)
                prefix = $"Weapon stats for [{ID}] {Name}:";

            builder.Append(tabCount, prefix);
            tabCount++;
            builder.NewlineAppend(tabCount, $"Type: Lv.{GetLevel()} {WeaponType}");
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
                builder.NewlineAppend(tabCount, $"Type: Lv.{AttributeRequirements.Level} {WeaponType}");
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
            builder.NewlineAppend(tabCount, $"Type: Lv.{GetLevel()} {WeaponType}");
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
    }
}
