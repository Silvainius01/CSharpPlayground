using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using CommandEngine;

namespace DnD_Generator
{
    class Creature : IInspectable, IDungeonObject, ISerializable<SerializedCreature>
    {
        public int ID { get; set; }
        public float HitPoints { get; set; }
        public int ArmorClass { get; set; }
        public int Level { get; set; } = 1;
        public string WeaponName { get; set; }

        public ItemWeapon PrimaryWeapon { get; set; }
        public ItemWeapon SecondaryWeapon { get; set; }
        public DungeonChest<IItem> Inventory { get; set; } = DungeonChestGenerator.GetEmptyChest();
        public CreatureArmorSlots ArmorSlots;

        public DungeonRoom CurrentRoom { get; set; }
        public CrawlerAttributeSet Attributes { get; set; }
        public CreatureProfeciencies Profeciencies { get; set; }

        public float MaxHitPoints
        { 
            get => DungeonCrawlerSettings.MinCreatureHitPoints + Attributes[AttributeType.CON] * DungeonCrawlerSettings.HitPointsPerConstitution;
        }
        public float Damage { get => PrimaryWeapon != null ? PrimaryWeapon.GetCreatureDamage(Attributes) : Attributes[AttributeType.STR]; }

        public Creature() { }

        public virtual string BriefString()
        {
            return $"[{ID}] {WeaponName} ({Level}) | HP: {HitPoints} | DMG: {Damage}";
        }
        public virtual string InspectString(string prefix, int tabCount)
        {
            SmartStringBuilder builder = new SmartStringBuilder(DungeonCrawlerSettings.TabString);;

            if (prefix == string.Empty)
                prefix = $"[{ID}] {WeaponName} (Lv.{Level}):";

            builder.Append(tabCount, prefix);

            tabCount++;
            builder.NewlineAppend(tabCount, $"HP: {HitPoints}");
            builder.NewlineAppend(tabCount, $"DMG: {Damage}");
            builder.NewlineAppend(tabCount, $"Weapon:");
            builder.NewlineAppend(tabCount + 1, PrimaryWeapon.BriefString());
            --tabCount;

            return builder.ToString();
        }
        public virtual string DebugString(string prefix, int tabCount)
        {
            SmartStringBuilder builder = new SmartStringBuilder(DungeonCrawlerSettings.TabString);;

            if(prefix == string.Empty)
                prefix = $"Creature Stats for {WeaponName}:";

            builder.Append(tabCount, prefix);
            tabCount++;
            builder.NewlineAppend(tabCount, $"ID: {ID}");
            builder.NewlineAppend(tabCount, $"HP: {HitPoints}/{MaxHitPoints}");
            builder.NewlineAppend(tabCount, $"Level: {Level}");
            builder.NewlineAppend(tabCount, $"Damage: {Damage}");
            builder.NewlineAppend(Attributes.DebugString("Atributes:", tabCount));
            builder.NewlineAppend(PrimaryWeapon.DebugString($"Weapon Stats:", tabCount));

            return builder.ToString();
        }
        public override string ToString()
        {
            return $"[{ID}] {WeaponName}";
        }

        public virtual SerializedCreature GetSerializable()
        {
            return new SerializedCreature(this);
        }
    }

    class SerializedCreature : ISerialized<Creature>
    {
        public string Name { get; set; }
        public float HitPoints { get; set; }
        public int Level { get; set; }
        public SerializedWeapon PrimaryWeapon { get; set; }
        public SerializedAttributes Attributes { get; set; }
        public Dictionary<Type, List<object>> InventoryItems { get; set; } = new Dictionary<Type, List<object>>();

        public SerializedCreature() { }
        public SerializedCreature(Creature c)
        {
            Name = c.WeaponName;
            HitPoints = c.HitPoints;
            Level = c.Level;
            PrimaryWeapon = (SerializedWeapon)c.PrimaryWeapon.GetSerializable();
            Attributes = c.Attributes.GetSerializable();

            foreach(var kvp in c.Inventory.Items)
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
