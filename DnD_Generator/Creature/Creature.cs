using System;
using System.Collections.Generic;
using System.Text;
using DieRoller;

namespace DnD_Generator
{
    public enum CreatureType { Humanoid, Goblinoid, Beast, Fey }
    public enum Allignment { LawfulGood, LawfulNeutral, LawfulEvil, NeutralGood, TrueNeutral, NeutralEvil, ChaoticGood, ChaoticNeutral, ChoaticEvil }

    public class Creature
    {
        public string Name { get; set; }
        public int HitPoints { get; set; }
        public int ArmorClass { get; set; }

        public ItemWeapon PrimaryWeapon { get; set; }
        public ItemWeapon SecondaryWeapon { get; set; }
        public Dictionary<IItem, int> Inventory { get; set; } = new Dictionary<IItem, int>();
        public CreatureArmorSlots ArmorSlots;

        public CreatureAttributes Attributes { get; set; }
        public CreatureProfeciencies Profeciencies { get; set; }
    }
}
