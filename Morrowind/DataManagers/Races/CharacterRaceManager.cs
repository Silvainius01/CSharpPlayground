using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandEngine.Interfaces;

namespace Morrowind
{
    class CharacterRaceManager : TypeManager<CharacterRace, CharacterRaceManager>
    {
        public static List<CharacterRace> RaceList { get; private set; } = new List<CharacterRace>();
        public static Dictionary<string, CharacterRace> RaceDict { get; private set; } = new Dictionary<string, CharacterRace>();
        public static Dictionary<string, List<CharacterRace>> RacesBySkillBonus { get; private set; } = new Dictionary<string, List<CharacterRace>>();
        public static Dictionary<string, List<CharacterRace>> RacesByMaleAttributeBonus { get; private set; } = new Dictionary<string, List<CharacterRace>>();
        public static Dictionary<string, List<CharacterRace>> RacesByFemaleAttributeBonus { get; private set; } = new Dictionary<string, List<CharacterRace>>();

        protected override string GetDataPath()
            => $"{Directory.GetCurrentDirectory()}\\Data\\Races.json";

        protected override void AddTypeEntry(CharacterRace item)
        {
            if (!RaceDict.ContainsKey(item.Name))
            {
                RaceList.Add(item);
                RaceDict.Add(item.Name, item);

                foreach (var skillBonus in item.SkillBonuses)
                {
                    if (!RacesBySkillBonus.ContainsKey(skillBonus.Key))
                        RacesBySkillBonus[skillBonus.Key] = new List<CharacterRace>();
                    RacesBySkillBonus[skillBonus.Key].Add(item);
                }

                foreach(var attrBonus in item.AttributeBonusesMale)
                {
                    if (!RacesByMaleAttributeBonus.ContainsKey(attrBonus.Key))
                        RacesByMaleAttributeBonus[attrBonus.Key] = new List<CharacterRace>();
                    RacesByMaleAttributeBonus[attrBonus.Key].Add(item);
                }

                foreach (var attrBonus in item.AttributeBonusesFemale)
                {
                    if (!RacesByFemaleAttributeBonus.ContainsKey(attrBonus.Key))
                        RacesByFemaleAttributeBonus[attrBonus.Key] = new List<CharacterRace>();
                    RacesByFemaleAttributeBonus[attrBonus.Key].Add(item);
                }
            }
        }

        protected override List<CharacterRace> GetDefaultTypesInternal()
        {
            List<CharacterRace> races = new List<CharacterRace>()
            {
                new CharacterRace("Argonian")
                {
                    SkillBonuses = new Dictionary<string, int>()
                    {
                        { "Alchemy", 5 },
                        { "Athletics", 15 },
                        { "Illusion", 5 },
                        { "Medium Armor", 5 },
                        { "Mysticism", 5 },
                        { "Spear", 5 },
                        { "Unarmored", 5 },
                    },
                    AttributeBonusesMale = new Dictionary<string, int>()
                    {
                        { "Willpower", -10 },
                        { "Agility", 10 },
                        { "Speed", 10 },
                        { "Endurance", -10 },
                        { "Personality", -10 },
                    },
                    AttributeBonusesFemale = new Dictionary<string, int>()
                    {
                        { "Intelligence", 10 },
                        { "Endurance", -10 },
                        { "Personality", -10 },
                    }
                },
                new CharacterRace("Breton")
                {
                    SkillBonuses = new Dictionary<string, int>()
                    {
                        
                    },
                    AttributeBonusesMale = new Dictionary<string, int>()
                    {
                        { "Intelligence",   10 },
                        { "Willpower",      10 },
                        { "Agility",        -10 },
                        { "Speed",          -10 },
                        { "Endurance",      -10 },
                    },
                    AttributeBonusesFemale = new Dictionary<string, int>()
                    {
                        { "Strength",       -10 },
                        { "Intelligence",   10 },
                        { "Willpower",      10 },
                        { "Agility",        -10 },
                        { "Endurance",      -10 },
                    }
                },
                new CharacterRace("Dark Elf")
                {
                    SkillBonuses = new Dictionary<string, int>()
                    {
                        { "Atheltics", 5},
                        { "Destruction", 10},
                        { "Light Armor", 5},
                        { "Long Blade", 5},
                        { "Marksman", 5},
                        { "Mysticism", 5},
                        { "Short Blade", 10},
                    },
                    AttributeBonusesMale = new Dictionary<string, int>()
                    {
                        { "Willpower",      -10 },
                        { "Speed",          10 },
                        { "Personality",    -10 },
                    },
                    AttributeBonusesFemale = new Dictionary<string, int>()
                    {
                        { "Willpower",      -10 },
                        { "Speed",          10 },
                        { "Endurance",      -10 },
                    }
                },
                new CharacterRace("High Elf")
                {
                    SkillBonuses = new Dictionary<string, int>()
                    {
                        { "Alchemy", 10 },
                        { "Alteration", 5 },
                        { "Conjuration", 5 },
                        { "Destruction", 10 },
                        { "Enchant", 10 },
                        { "Illusion", 5 },
                    },
                    AttributeBonusesMale = new Dictionary<string, int>()
                    {
                        { "Strength",       -10 },
                        { "Intelligence",   10 },
                        { "Speed",          -10 },
                    },
                    AttributeBonusesFemale = new Dictionary<string, int>()
                    {
                        { "Strength",       -10 },
                        { "Intelligence",   10 },
                        { "Endurance",      -10 },
                    }
                },
                new CharacterRace("Imperial")
                {
                    SkillBonuses = new Dictionary<string, int>()
                    {
                        { "Blunt Weapon", 5 },
                        { "Hand to Hand", 5 },
                        { "Light Armor", 5 },
                        { "Long Blade", 10 },
                        { "Mercantile", 10 },
                        { "Speechcraft", 10 },
                    },
                    AttributeBonusesMale = new Dictionary<string, int>()
                    {
                        { "Willpower",      -10 },
                        { "Agility",        -10 },
                        { "Personality",    10 },
                    },
                    AttributeBonusesFemale = new Dictionary<string, int>()
                    {
                        { "Agility",        -10 },
                        { "Speed",          -10 },
                        { "Personality",    10 },
                    }
                },
                new CharacterRace("Khajiit")
                {
                    SkillBonuses = new Dictionary<string, int>()
                    {
                        { "Acrobatics", 15 },
                        { "Athletics", 5 },
                        { "Hand to Hand", 5 },
                        { "Light Armor", 5 },
                        { "Security", 5 },
                        { "Short Blade", 5 },
                        { "Sneak", 5 },
                    },
                    AttributeBonusesMale = new Dictionary<string, int>()
                    {
                        { "Willpower",      -10 },
                        { "Agility",        10 },
                        { "Endurance",      -10 },
                    },
                    AttributeBonusesFemale = new Dictionary<string, int>()
                    {
                        { "Strength",       -10 },
                        { "Willpower",      -10 },
                        { "Agility",        10 },
                    }
                },
                new CharacterRace("Nord")
                {
                    SkillBonuses = new Dictionary<string, int>()
                    {
                        { "Axe", 10 },
                        { "Blunt Weapon", 10 },
                        { "Heavy Armor", 5 },
                        { "Long Blade", 5 },
                        { "Medium Armor", 10 },
                        { "Spear", 5 },
                    },
                    AttributeBonusesMale = new Dictionary<string, int>()
                    {
                        { "Strength",       10 },
                        { "Intelligence",   -10 },
                        { "Agility",        -10 },
                        { "Endurance",      10 },
                        { "Personality",    -10 },
                    },
                    AttributeBonusesFemale = new Dictionary<string, int>()
                    {
                        { "Strength",       10 },
                        { "Intelligence",   -10 },
                        { "Willpower",      10 },
                        { "Agility",        -10 },
                        { "Personality",    -10 },
                    }
                },
                new CharacterRace("Orc")
                {
                    SkillBonuses = new Dictionary<string, int>()
                    {
                        { "Armorer", 10 },
                        { "Axe", 5 },
                        { "Block", 10 },
                        { "Heavy Armor", 10 },
                        { "Medium Armor", 10 },
                    },
                    AttributeBonusesMale = new Dictionary<string, int>()
                    {
                        { "Strength",       5 },
                        { "Intelligence",   -10 },
                        { "Willpower",      10 },
                        { "Agility",        -5 },
                        { "Speed",          -10 },
                        { "Endurance",      10 },
                        { "Personality",    -10 },
                    },
                    AttributeBonusesFemale = new Dictionary<string, int>()
                    {
                        { "Strength",       5 },
                        { "Willpower",      5 },
                        { "Agility",        -5 },
                        { "Speed",          -10 },
                        { "Endurance",      10 },
                        { "Personality",    -15 },
                    }
                },
                new CharacterRace("Redguard")
                {
                    SkillBonuses = new Dictionary<string, int>()
                    {
                        { "Athletics", 5 },
                        { "Axe", 5 },
                        { "Blunt Weapon", 5 },
                        { "Heavy Armor", 5 },
                        { "Long Blade", 15 },
                        { "Medium Armor", 5 },
                        { "Short Blade", 5 },
                    },
                    AttributeBonusesMale = new Dictionary<string, int>()
                    {
                        { "Strength",       10 },
                        { "Intelligence",   -10 },
                        { "Willpower",      -10 },
                        { "Endurance",      10 },
                        { "Personality",    -10 },
                    },
                    AttributeBonusesFemale = new Dictionary<string, int>()
                    {
                        { "Intelligence",   -10 },
                        { "Willpower",      -10 },
                        { "Endurance",      10 },
                    }
                },
                new CharacterRace("Wood Elf")
                {
                    SkillBonuses = new Dictionary<string, int>()
                    {
                        { "Acrobatics", 5 },
                        { "Alchemy", 5 },
                        { "Light Armor", 10 },
                        { "Marksman", 15 },
                        { "Sneak", 10 },
                    },
                    AttributeBonusesMale = new Dictionary<string, int>()
                    {
                        { "Strength",       -10 },
                        { "Willpower",      -10 },
                        { "Agility",        10 },
                        { "Speed",          10 },
                        { "Endurance",      -10 },
                    },
                    AttributeBonusesFemale = new Dictionary<string, int>()
                    {
                        { "Strength",       -10 },
                        { "Willpower",      -10 },
                        { "Agility",        10 },
                        { "Speed",          10 },
                        { "Endurance",      -10 },
                    }
                },
            };

            return races;
        }
    }
}
