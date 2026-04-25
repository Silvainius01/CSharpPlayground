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
using System.Reflection.Metadata.Ecma335;
using System.Collections.ObjectModel;
using System.Threading;

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

    class Creature : IColorInspectable, IDungeonObject, ISerializable<SerializedCreature, Creature>
    {
        public int ID { get; set; }
        public string ObjectName { get; set; }
        public int Level { get; set; } = 1;

        public bool IsAlive => Health.Value > 0;

        public CreatureStat Health { get => Stats[0]; }
        public CreatureStat Fatigue { get => Stats[1]; }
        public CreatureStat Mana { get => Stats[2]; }
        public CreatureStat CombatSpeed { get => Stats[3]; }

        public DungeonRoom CurrentRoom { get; set; }
        public ItemWeapon PrimaryWeapon { get; set; }
        public ItemWeapon SecondaryWeapon { get; set; }
        public DungeonChest<IItem> Inventory { get; set; } = DungeonChestGenerator.GetEmptyChest();
        public CreatureArmorSlots Armor;

        public List<CreatureStat> Stats { get; private set; }
        public ReadOnlyDictionary<DamageType, float> Resistances { get; private set; }
        public CrawlerAttributeSet MaxAttributes { get; private set; }
        public CrawlerAttributeSet Afflictions { get; private set; }
        public CreatureProficiencies Proficiencies { get; set; }

        private ItemWeapon UnarmedWeapon;
        private Dictionary<DamageType, float> _resistances = new Dictionary<DamageType, float>();

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
                c.GetAttribute(AttributeType.DEX) * 2.0f +
                c.GetAttribute(AttributeType.CHA)) / 10.0f;

            Afflictions = new CrawlerAttributeSet(0);
            MaxAttributes = new CrawlerAttributeSet(0);
            Proficiencies = new CreatureProficiencies();

            Stats = new List<CreatureStat>()
            {
                new CreatureStat(this, maxHealthFunc, AttributeType.STR, AttributeType.CON),
                new CreatureStat(this, maxFatigueFunc, AttributeType.DEX, AttributeType.CON),
                new CreatureStat(this, maxManaFunc, AttributeType.INT, AttributeType.WIS, AttributeType.CHA),
                new CreatureStat(this, combatSpeedFunc, AttributeType.DEX, AttributeType.CHA)
            };

            Armor = new CreatureArmorSlots();
            Resistances = new ReadOnlyDictionary<DamageType, float>(_resistances);
            UnarmedWeapon = DungeonGenerator.WeaponGenerator.GenerateUnarmed(this);
        }

        public int GetAttribute(AttributeType attr)
        {
            return MaxAttributes[attr] + Afflictions[attr];
        }
        public float GetAttributePercent(AttributeType attr, float mult = 1.0f)
        {
            return GetAttribute(attr) / (float)MaxAttributes[attr];
        }

        public void HealAllStatsAndAfflictions()
        {
            Afflictions.SetAttributes(0);

            UpdateStats();
            foreach (var stat in Stats)
                stat.SetPercent(1);
        }

        public ItemWeapon GetCombatWeapon()
        {
            return PrimaryWeapon is not null
                ? PrimaryWeapon
                : UnarmedWeapon;
        }
        public float GetCombatDamage()
        {
            return GetCombatDamage(GetCombatWeapon());
        }
        public float GetCombatDamage(ItemWeapon weapon)
        {
            float damage = weapon.GetWeaponDamage()
                + GetAttribute(weapon.MajorAttribute) / 2.0f
                + GetAttribute(weapon.MinorAttribute) / 4.0f;
            float skillBonus = 1 + CreatureSkillUtility.GetWeaponSkillBonus(weapon, Proficiencies);

            return damage * skillBonus;
        }
        public DamageType GetDamageType()
        {
            return DamageType.Physical;
        }
        public float GetCombatHitChance()
        {
            ItemWeapon weapon = GetCombatWeapon();

            float chance = 0.01f;
            chance *= Proficiencies.GetSkillLevel(weapon.WeaponType) / 2 + Proficiencies.GetSkillLevel(weapon.ObjectName);
            chance *= GetAttributePercent(weapon.MajorAttribute) + (GetAttributePercent(weapon.MinorAttribute) / 2);
            chance *= 0.5f + Fatigue.Percent;
            return chance;
        }
        public float GetCombatEvasion()
        {
            float chance = 0.01f;
            chance *= (Proficiencies.GetSkillLevel(CreatureSkill.Evasion) / 4 * 3) + (Proficiencies.GetSkillLevel(CreatureSkill.Unarmored) / 4);
            chance *= GetAttributePercent(AttributeType.DEX) + (GetAttributePercent(AttributeType.WIS) / 2);
            chance *= 0.25f + Fatigue.Percent;
            return chance;
        }
        public float GetAttackFatigueCost()
        {
            ItemWeapon weapon = GetCombatWeapon();
            float attrDiv =
                GetAttributePercent(weapon.MajorAttribute) * 1.5f +
                GetAttributePercent(weapon.MinorAttribute) * 0.5f;
            return weapon.GetFatigueCost() / MathF.Max(attrDiv, 0.001f);
        }

        public bool CanEquipWeapon(ItemWeapon weapon)
        {
            return true;
        }

        public float GetArmorRating() => Armor.GetTotalArmorRating(this);

        void UpdateStats()
        {
            foreach (var stat in Stats)
                stat.Update();
            UpdateUnarmedWeapon();
        }
        void UpdateStats(AttributeType attr)
        {
            foreach (var stat in Stats)
                if (stat.LinkedAttributes.Contains(attr))
                    stat.Update();
            UpdateUnarmedWeapon();
        }
        void UpdateUnarmedWeapon()
        {
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

        public void AddResistance(DamageType damageType, float amount)
        {
            if (damageType == DamageType.True)
                throw new ArgumentException("Cannot add resistance to the True damage type.");

            if (!_resistances.ContainsKey(damageType))
                _resistances.Add(damageType, 0.0f);

            float r = Mathc.Clamp(_resistances[damageType] + amount, 0.0f, 1.0f);
            _resistances[damageType] = r;
        }
        public void SetResistance(DamageType damageType, float amount)
        {
            if (damageType == DamageType.True)
                throw new ArgumentException("Cannot set a resistance to the True damage type.");
            if (amount < 0.0f)
                throw new ArgumentException("Cannot set a negative resistance value.");

            amount = Mathc.Max(amount, 1.0f);
            if (!_resistances.ContainsKey(damageType))
                _resistances.Add(damageType, amount);
            else _resistances[damageType] = amount;
        }
        public float GetResistance(DamageType damageType)
        {
            if (_resistances.TryGetValue(damageType, out float r))
                return r;
            return 0.0f;
        }

        public virtual string BriefString()
        {
            StringBuilder builder = new StringBuilder(128);
            builder.Append($"[{ID}] {ObjectName} ({Level})");
            builder.Append($" | HP: {Health.Value.ToString("n1")}");
            builder.Append($" | AR: {GetArmorRating().ToString("n1")}");
            builder.Append($" | DMG: {GetCombatDamage().ToString("n1")}");
            builder.Append($" | SPD: {CombatSpeed.Value.ToString("n1")}");
            return builder.ToString();
        }
        public virtual string InspectString(string prefix, int tabCount)
        {
            SmartStringBuilder builder = new SmartStringBuilder(DungeonSettings.TabString); ;

            if (prefix == string.Empty)
                prefix = $"[{ID}] {ObjectName} (Lv.{Level}):";

            builder.Append(tabCount, prefix);

            tabCount++;
            builder.NewlineAppend(tabCount, $"HP: {Health.Value.ToString("n1")}");
            builder.NewlineAppend(tabCount, $"DMG: {GetCombatDamage().ToString("n1")}");
            builder.NewlineAppend(tabCount, $"Weapon:");
            builder.NewlineAppend(tabCount + 1, PrimaryWeapon.BriefString());
            builder.NewlineAppend(tabCount, "Armor: ");
            builder.NewlineAppend(tabCount + 1, Armor.BriefString());
            --tabCount;

            return builder.ToString();
        }
        public virtual string DebugString(string prefix, int tabCount)
        {
            SmartStringBuilder builder = new SmartStringBuilder(DungeonSettings.TabString); ;

            if (prefix == string.Empty)
                prefix = $"Creature Stats for {ObjectName}:";

            builder.Append(tabCount, prefix);
            tabCount++;
            builder.NewlineAppend(tabCount, $"ID: {ID}");
            builder.NewlineAppend(tabCount, $"HP: {Health.Value.ToString("n1")}/{Health.MaxValue.ToString("n1")}");
            builder.NewlineAppend(tabCount, $"Level: {Level}");
            builder.NewlineAppend(tabCount, $"Damage: {GetCombatDamage()}");
            builder.NewlineAppend(PrimaryWeapon.DebugString($"Weapon Stats:", tabCount));
            builder.NewlineAppend(MaxAttributes.DebugString("Atributes:", tabCount));
            builder.NewlineAppend(Proficiencies.DebugString("Skills:", tabCount));
            builder.NewlineAppend(Armor.DebugString("Armor:", tabCount));

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

        public ColorStringBuilder BriefColor(ConsoleColor initialColor = ConsoleColor.Gray)
        {
            throw new NotImplementedException();
        }
        public ColorStringBuilder InspectColor(string prefix, int tabCount, ConsoleColor initialColor = ConsoleColor.Gray)
        {
            throw new NotImplementedException();
        }
        public ColorStringBuilder DebugColor(string prefix, int tabCount, ConsoleColor initialColor = ConsoleColor.Gray)
        {
            throw new NotImplementedException();
        }

        #region Overloads
        public static bool operator ==(Creature a, Creature b) => a.ID == b.ID;
        public static bool operator !=(Creature a, Creature b) => a.ID != b.ID;
        #endregion
    }

    class SerializedCreature : ISerialized<Creature>
    {
        public string Name { get; set; }
        public int Level { get; set; }
        public List<float> CurrentStats { get; set; }
        public SerializedWeapon PrimaryWeapon { get; set; }
        public SerializedArmorSlots ArmorSlots { get; set; }
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
            Profeciencies = c.Proficiencies.GetSerializable();
            ArmorSlots = c.Armor.GetSerializable();

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
            c.PrimaryWeapon = DungeonGenerator.WeaponGenerator.FromSerialized(PrimaryWeapon);
            c.Armor = ArmorSlots.GetDeserialized();
            c.AddAttributePoints(Attributes.GetDeserialized());
            c.AddAffliction(Afflictions.GetDeserialized());
            c.Proficiencies = Profeciencies.GetDeserialized();

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
