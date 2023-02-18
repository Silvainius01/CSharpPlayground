using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using CommandEngine;
using RogueCrawler;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Xml.Linq;
using System.Numerics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RogueCrawler
{
    class CreatureStat
    {
        public float Value { get; private set; } = 1;
        public float MaxValue { get; private set; } = 1;
        public float Percent { get => Value / MaxValue; }
        public HashSet<AttributeType> LinkedAttributes { get; }

        Creature linkedCreature;
        Func<Creature, float> UpdateMaxValue { get; set; }

        public CreatureStat(Creature linkedCreature, Func<Creature, float> updateMaxValueFunc)
        {
            this.linkedCreature = linkedCreature;
            UpdateMaxValue = updateMaxValueFunc;
            LinkedAttributes = new HashSet<AttributeType>();
            Update();
        }
        public CreatureStat(Creature linkedCreature, Func<Creature, float> updateMaxValueFunc, params AttributeType[] linkedAttributes)
        {
            this.linkedCreature = linkedCreature;
            UpdateMaxValue = updateMaxValueFunc;
            LinkedAttributes = new HashSet<AttributeType>(linkedAttributes);
            Update();
        }

        public void Update()
        {
            float p = Percent;
            MaxValue = UpdateMaxValue(linkedCreature);
            Value = MaxValue * p;
        }

        public void SetValue(float amt) => Value = amt;
        public void AddValue(float amt) => Value = MathF.Max(0, MathF.Min(Value + amt, MaxValue));
        public void SetPercent(float percent) => Value = MaxValue * percent;
    }

    class Creature : IInspectable, IDungeonObject, ISerializable<SerializedCreature, Creature>
    {
        public int ID { get; set; }
        public string ObjectName { get; set; }
        public int ArmorClass { get; set; }
        public int Level { get; set; } = 1;

        public CreatureStat Health { get => Stats[0]; }
        public CreatureStat Fatigue { get => Stats[1]; }
        public CreatureStat Mana { get => Stats[2]; }
        public CreatureStat CombatSpeed { get=> Stats[3]; }
        public CreatureStat CombatEvasion { get => Stats[4]; }

        public DungeonRoom CurrentRoom { get; set; }
        public ItemWeapon PrimaryWeapon { get; set; }
        public ItemWeapon SecondaryWeapon { get; set; }
        public DungeonChest<IItem> Inventory { get; set; } = DungeonChestGenerator.GetEmptyChest();
        public CreatureArmorSlots ArmorSlots;

        public List<CreatureStat> Stats { get; private set; }
        public CrawlerAttributeSet MaxAttributes { get; private set; }
        public CrawlerAttributeSet Afflictions { get; private set; }
        public CreatureProfeciencies Profeciencies { get; set; }

        private ItemWeapon UnarmedWeapon;

        public Creature()
        {
            var maxHealthFunc = (Creature c) =>
                c.GetAttribute(AttributeType.STR) * 2.5f +
                (c.GetAttribute(AttributeType.CON) + 1) * 5;
            var maxFatigueFunc = (Creature c) =>
                c.GetAttribute(AttributeType.CON) * 2.5f +
                (c.GetAttribute(AttributeType.DEX) + 1) * 5;
            var maxManaFunc = (Creature c) =>
                (c.GetAttribute(AttributeType.INT) + c.GetAttribute(AttributeType.CHA)) * 2 +
                (c.GetAttribute(AttributeType.WIS) + 1) * 5.0f;
            var combatSpeedFunc = (Creature c) => 1.0f + (
                c.GetAttribute(AttributeType.DEX) * 2 +
                c.GetAttribute(AttributeType.CHA) / 10);
            

            Afflictions = new CrawlerAttributeSet(0);
            MaxAttributes = new CrawlerAttributeSet(0);
            Profeciencies = new CreatureProfeciencies();

            Stats = new List<CreatureStat>()
            {
                new CreatureStat(this, maxHealthFunc, AttributeType.STR, AttributeType.CON),
                new CreatureStat(this, maxFatigueFunc, AttributeType.DEX, AttributeType.CON),
                new CreatureStat(this, maxManaFunc, AttributeType.INT, AttributeType.WIS, AttributeType.CHA),
                new CreatureStat(this, combatSpeedFunc, AttributeType.DEX, AttributeType.CHA)
            };

            UnarmedWeapon = DungeonGenerator.GenerateUnarmedWeapon(this);
        }

        public int GetAttribute(AttributeType attr)
        {
            return MaxAttributes[attr] + Afflictions[attr];
        }
        public float GetAttributePercent(AttributeType attr)
        {
            return GetAttribute(attr) / (float)MaxAttributes[attr];
        }

        public float GetCombatDamage()
        {
            return PrimaryWeapon is not null
                ? PrimaryWeapon.GetWeaponDamage() + GetAttribute(PrimaryWeapon.MinorAttribute)
                : GetAttribute(AttributeType.STR);
        }
        public float GetCombatHitChance()
        {
            ItemWeapon weapon = PrimaryWeapon;
            if(PrimaryWeapon is null)
            {
                weapon = UnarmedWeapon;
                weapon.AttributeRequirements[AttributeType.STR] = GetAttribute(AttributeType.STR);
                weapon.AttributeRequirements[AttributeType.DEX] = GetAttribute(AttributeType.DEX);
                weapon.AttributeRequirements[AttributeType.WIS] = GetAttribute(AttributeType.WIS);
            }

            float chance = 0.01f;
            chance *= Profeciencies.GetSkillLevel(weapon.WeaponType) / 2 + Profeciencies.GetSkillLevel(weapon.ObjectName);
            chance *= GetAttributePercent(weapon.MajorAttribute) + (GetAttributePercent(weapon.MinorAttribute) / 2);
            chance *= 0.5f + Fatigue.Percent;
            return chance;
        }

        public bool CanEquipWeapon(ItemWeapon weapon)
        {
            foreach (KeyValuePair<AttributeType, int> kvp in weapon.AttributeRequirements)
                if (GetAttribute(kvp.Key) < kvp.Value)
                    return false;
            return true;
        }

        void UpdateStats()
        {
            foreach (var stat in Stats)
                stat.Update();
        }
        void UpdateStats(AttributeType attr)
        {
            foreach (var stat in Stats)
                if (stat.LinkedAttributes.Contains(attr))
                    stat.Update();
        }

        public void AddAttributePoints(AttributeType attr, int amount)
        {
            MaxAttributes[attr] += amount;
            UpdateStats(attr);
        }
        public void AddAttributePoints(CrawlerAttributeSet attributes)
        {
            foreach (var kvp in attributes)
                MaxAttributes[kvp.Key] += kvp.Value;
            UpdateStats();
        }

        public void AddAffliction(AttributeType attr, int amount)
        {
            Afflictions[attr] += amount;
            UpdateStats(attr);
        }
        public void AddAffliction(CrawlerAttributeSet afflictions)
        {
            foreach (var kvp in afflictions)
                Afflictions[kvp.Key] += kvp.Value;
            UpdateStats();
        }

        public virtual string BriefString()
        {
            return $"[{ID}] {ObjectName} ({Level}) | HP: {Health.Value} | DMG: {GetCombatDamage()}";
        }
        public virtual string InspectString(string prefix, int tabCount)
        {
            SmartStringBuilder builder = new SmartStringBuilder(DungeonCrawlerSettings.TabString); ;

            if (prefix == string.Empty)
                prefix = $"[{ID}] {ObjectName} (Lv.{Level}):";

            builder.Append(tabCount, prefix);

            tabCount++;
            builder.NewlineAppend(tabCount, $"HP: {Health.Value}");
            builder.NewlineAppend(tabCount, $"DMG: {GetCombatDamage()}");
            builder.NewlineAppend(tabCount, $"Weapon:");
            builder.NewlineAppend(tabCount + 1, PrimaryWeapon.BriefString());
            --tabCount;

            return builder.ToString();
        }
        public virtual string DebugString(string prefix, int tabCount)
        {
            SmartStringBuilder builder = new SmartStringBuilder(DungeonCrawlerSettings.TabString); ;

            if (prefix == string.Empty)
                prefix = $"Creature Stats for {ObjectName}:";

            builder.Append(tabCount, prefix);
            tabCount++;
            builder.NewlineAppend(tabCount, $"ID: {ID}");
            builder.NewlineAppend(tabCount, $"HP: {Health.Value}/{Health.MaxValue}");
            builder.NewlineAppend(tabCount, $"Level: {Level}");
            builder.NewlineAppend(tabCount, $"Damage: {GetCombatDamage()}");
            builder.NewlineAppend(MaxAttributes.DebugString("Atributes:", tabCount));
            builder.NewlineAppend(PrimaryWeapon.DebugString($"Weapon Stats:", tabCount));

            return builder.ToString();
        }
        public override string ToString()
        {
            return $"[{ID}] {ObjectName}";
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
        public List<float> CurrentStats { get; set; }
        public SerializedWeapon PrimaryWeapon { get; set; }
        public SerializedAttributes Attributes { get; set; }
        public SerializedAttributes Afflictions { get; set; }
        public SerializedProfeciencies Profeciencies { get; set; }
        public Dictionary<Type, List<object>> InventoryItems { get; set; } = new Dictionary<Type, List<object>>();

        public SerializedCreature() { }
        public SerializedCreature(Creature c)
        {
            Name = c.ObjectName;
            Level = c.Level;
            CurrentStats = new List<float>(c.Stats.Count);
            PrimaryWeapon = (SerializedWeapon)c.PrimaryWeapon.GetSerializable();
            Attributes = c.MaxAttributes.GetSerializable();
            Afflictions = c.Afflictions.GetSerializable();
            Profeciencies = c.Profeciencies.GetSerializable();

            for (int i = 0; i < c.Stats.Count; ++i)
                CurrentStats.Add(c.Stats[i].Value);

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
            Creature c = new Creature();
            DeserializeCreatureInto(c);
            return c;
        }
        protected void DeserializeCreatureInto(Creature c)
        {
            var serializer = JsonSerializer.CreateDefault();

            c.ObjectName = Name;
            c.Level = Level;
            c.PrimaryWeapon = DungeonGenerator.GenerateWeaponFromSerialized(PrimaryWeapon);
            c.AddAttributePoints(Attributes.GetDeserialized());
            c.AddAffliction(Afflictions.GetDeserialized());
            c.Profeciencies = Profeciencies.GetDeserialized();

            for (int i = 0; i < c.Stats.Count; ++i)
            {
                if (i < CurrentStats.Count)
                    c.Stats[i].SetValue(CurrentStats[i]);
                else c.Stats[i].SetPercent(1.0f);
            }

            foreach (var kvp in InventoryItems)
            {
                Type type = kvp.Key;
                foreach (JObject obj in kvp.Value)
                {
                    SerializedItem converted = (SerializedItem)serializer.Deserialize(new JTokenReader(obj), type);
                    c.Inventory.AddItem(converted.GetDeserialized());
                }
            }
        }
    }
}
