using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using CommandEngine;
using RogueCrawler;

namespace RogueCrawler
{
    class Creature : IInspectable, IDungeonObject, ISerializable<SerializedCreature, Creature>
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int ArmorClass { get; set; }
        public int Level { get; set; } = 1;

        public float HitPoints { get; set; }
        public float FatiguePoints { get; set; }
        public float ManaPoints { get; set; }

        public float MaxHitPoints
        {
            get =>
                GetAttribute(AttributeType.STR) * 2.5f +
                (GetAttribute(AttributeType.CON) + 1) * 5;
        }
        public float MaxFatiguePoints
        {
            get =>
                GetAttribute(AttributeType.CON) * 2.5f +
                (GetAttribute(AttributeType.DEX) + 1) * 5;
        }
        public float MaxManaPoints
        {
            get =>
                (GetAttribute(AttributeType.INT) + GetAttribute(AttributeType.CHA)) * 2 +
                (GetAttribute(AttributeType.WIS) + 1) * 5;
        }

        public float HealthPercent => HitPoints / MaxHitPoints;
        public float FatiguePercent => FatiguePoints / MaxFatiguePoints;
        public float ManaPercent => ManaPoints / MaxManaPoints;

        public ItemWeapon PrimaryWeapon { get; set; }
        public ItemWeapon SecondaryWeapon { get; set; }
        public DungeonChest<IItem> Inventory { get; set; } = DungeonChestGenerator.GetEmptyChest();
        public CreatureArmorSlots ArmorSlots;

        public DungeonRoom CurrentRoom { get; set; }
        public CrawlerAttributeSet MaxAttributes { get; set; }
        public CrawlerAttributeSet Afflictions { get; set; }
        public CreatureProfeciencies Profeciencies { get; set; }

        public Creature() { }

        public int GetAttribute(AttributeType attr)
        {
            return MaxAttributes[attr] + Afflictions[attr];
        }
        public float GetAttributePercent(AttributeType attr)
        {
            return GetAttribute(attr) / (float)MaxAttributes[attr];
        }

        public float GetHitChance()
        {
            float chance = 0.01f;
            chance *= Profeciencies.GetSkillLevel(PrimaryWeapon.WeaponType) / 2 + Profeciencies.GetSkillLevel(PrimaryWeapon.Name);
            chance *= GetAttributePercent(PrimaryWeapon.MajorAttribute) + (GetAttributePercent(PrimaryWeapon.MinorAttribute) / 2);
            chance *= 0.5f + FatiguePercent;
            return chance;
        }

        public float GetCreatureDamage()
        {
            return PrimaryWeapon is not null
                ? PrimaryWeapon.GetWeaponDamage() + GetAttribute(PrimaryWeapon.MinorAttribute)
                : GetAttribute(AttributeType.STR);
        }

        public bool CanEquipWeapon(ItemWeapon weapon)
        {
            foreach (KeyValuePair<AttributeType, int> kvp in weapon.AttributeRequirements)
                if (GetAttribute(kvp.Key) < kvp.Value)
                    return false;
            return true;
        }

        public virtual string BriefString()
        {
            return $"[{ID}] {Name} ({Level}) | HP: {HitPoints} | DMG: {GetCreatureDamage()}";
        }
        public virtual string InspectString(string prefix, int tabCount)
        {
            SmartStringBuilder builder = new SmartStringBuilder(DungeonCrawlerSettings.TabString); ;

            if (prefix == string.Empty)
                prefix = $"[{ID}] {Name} (Lv.{Level}):";

            builder.Append(tabCount, prefix);

            tabCount++;
            builder.NewlineAppend(tabCount, $"HP: {HitPoints}");
            builder.NewlineAppend(tabCount, $"DMG: {GetCreatureDamage()}");
            builder.NewlineAppend(tabCount, $"Weapon:");
            builder.NewlineAppend(tabCount + 1, PrimaryWeapon.BriefString());
            --tabCount;

            return builder.ToString();
        }
        public virtual string DebugString(string prefix, int tabCount)
        {
            SmartStringBuilder builder = new SmartStringBuilder(DungeonCrawlerSettings.TabString); ;

            if (prefix == string.Empty)
                prefix = $"Creature Stats for {Name}:";

            builder.Append(tabCount, prefix);
            tabCount++;
            builder.NewlineAppend(tabCount, $"ID: {ID}");
            builder.NewlineAppend(tabCount, $"HP: {HitPoints}/{MaxHitPoints}");
            builder.NewlineAppend(tabCount, $"Level: {Level}");
            builder.NewlineAppend(tabCount, $"Damage: {GetCreatureDamage()}");
            builder.NewlineAppend(MaxAttributes.DebugString("Atributes:", tabCount));
            builder.NewlineAppend(PrimaryWeapon.DebugString($"Weapon Stats:", tabCount));

            return builder.ToString();
        }
        public override string ToString()
        {
            return $"[{ID}] {Name}";
        }

        public virtual SerializedCreature GetSerializable()
        {
            return new SerializedCreature(this);
        }
    }

    class SerializedCreature : ISerialized<Creature>
    {
        public string Name { get; set; }
        public int Level { get; set; }
        public float HitPoints { get; set; }
        public float FatiguePoints { get; set; }
        public float ManaPoints { get; set; }
        public SerializedWeapon PrimaryWeapon { get; set; }
        public SerializedAttributes Attributes { get; set; }
        public SerializedAttributes Afflictions { get; set; }
        public Dictionary<Type, List<object>> InventoryItems { get; set; } = new Dictionary<Type, List<object>>();

        public SerializedCreature() { }
        public SerializedCreature(Creature c)
        {
            Name = c.Name;
            Level = c.Level;
            HitPoints = c.HitPoints;
            FatiguePoints = c.FatiguePoints;
            ManaPoints = c.ManaPoints;
            PrimaryWeapon = (SerializedWeapon)c.PrimaryWeapon.GetSerializable();
            Attributes = c.MaxAttributes.GetSerializable();
            Afflictions = c.Afflictions.GetSerializable();

            foreach (var kvp in c.Inventory.Items)
            {
                var item = kvp.Value.Item;
                SerializedItem sItem = item.GetSerializable();
                Type tItem = sItem.GetType();
                sItem.Count = kvp.Value.Count;

                if (!InventoryItems.ContainsKey(tItem))
                    InventoryItems.Add(tItem, new List<object>());
                InventoryItems[tItem].Add(sItem);
            }
        }

        public virtual Creature GetDeserialized()
        {
            return new Creature() { };
        }
    }
}
