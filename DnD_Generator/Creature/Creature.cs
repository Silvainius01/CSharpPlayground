using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using GameEngine;

namespace DnD_Generator
{
    
    class Creature : IInspectable, IDungeonObject
    {
        public int ID { get; set; }
        public float HitPoints { get; set; }
        public int ArmorClass { get; set; }
        public int Level { get; set; } = 1;
        public string Name { get; set; }

        public ItemWeapon PrimaryWeapon { get; set; }
        public ItemWeapon SecondaryWeapon { get; set; }
        public DungeonChest<IItem> Inventory { get; set; } = DungeonChestGenerator.GetEmptyChest();
        public CreatureArmorSlots ArmorSlots;

        public DungeonRoom CurrentRoom { get; set; }
        public CreatureAttributes Attributes { get; set; }
        public CreatureProfeciencies Profeciencies { get; set; }

        public float MaxHitPoints
        { 
            get => DungeonCrawlerSettings.MinCreatureHitPoints + Attributes[AttributeType.CON] * DungeonCrawlerSettings.HitPointsPerConstitution;
        }
        public float Damage { get => PrimaryWeapon != null ? PrimaryWeapon.GetCreatureDamage(Attributes) : Attributes[AttributeType.STR]; }

        public Creature() { }

        public virtual string BriefString()
        {
            return $"[{ID}] {Name} ({Level}) | HP: {HitPoints} | DMG: {Damage}";
        }
        public virtual string InspectString(string prefix, int tabCount)
        {
            SmartStringBuilder builder = new SmartStringBuilder(DungeonCrawlerSettings.TabString);;

            if (prefix == string.Empty)
                prefix = $"[{ID}] {Name} (Lv.{Level}):";

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
                prefix = $"Creature Stats for {Name}:";

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
            return $"[{ID}] {Name}";
        }
    }
}
