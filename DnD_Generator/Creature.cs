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

        public CreatureAttributes Attributes { get; set; }
        public CreatureProfeciencies Profeciencies { get; set; }

        public static Creature Generate(string name, StatRoll hitPoints, int armorClass, CreatureAttributes attributes, CreatureProfeciencies profeciencies)
        {
            return new Creature()
            {
                Name = name,
                HitPoints = hitPoints.Roll(),
                ArmorClass = armorClass,
                Attributes = attributes,
                Profeciencies = profeciencies
            };
        }
    }
}
