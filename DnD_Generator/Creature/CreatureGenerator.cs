using DieRoller;
using System;
using System.Collections.Generic;
using System.Text;

namespace DnD_Generator
{
    public static class CreatureGenerator
    {
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

        public static Creature GenerateGoblin(bool rollHitpoints)
        {
            CreatureAttributes attributes = new CreatureAttributes()
            {
                Strength = 8,
                Dexterity = 14,
                Consitution = 10,
                Intelligence = 10,
                Wisdom = 8,
                Charisma = 8
            };

            StatRoll hp = new StatRoll(2, 6, 0);
            return new Creature()
            {
                Name = "Goblin",
                HitPoints = rollHitpoints ? hp.Roll() : (int)hp.GetAverage(),
                ArmorClass = 15,
                Attributes = attributes,
                Profeciencies = new CreatureProfeciencies()
            };
        }

        public static Creature GenerateGoblinBoss(bool rollHitpoints)
        {
            CreatureAttributes attributes = new CreatureAttributes()
            {
                Strength = 8,
                Dexterity = 14,
                Consitution = 10,
                Intelligence = 10,
                Wisdom = 8,
                Charisma = 8
            };

            StatRoll hp = new StatRoll(6, 6, 0);
            return new Creature()
            {
                Name = "Goblin Boss",
                HitPoints = rollHitpoints ? hp.Roll() : (int)hp.GetAverage(),
                ArmorClass = 17,
                Attributes = attributes,
                Profeciencies = new CreatureProfeciencies()
            };
        }
    }
}
