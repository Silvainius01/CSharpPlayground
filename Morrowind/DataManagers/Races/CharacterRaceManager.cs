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
                        { SkillManager.SkillNameAlchemy, 5 },
                        { SkillManager.SkillNameAthletics, 15 },
                        { SkillManager.SkillNameIllusion, 5 },
                        { SkillManager.SkillNameMediumArmor, 5 },
                        { SkillManager.SkillNameMysticism, 5 },
                        { SkillManager.SkillNameSpear, 5 },
                        { SkillManager.SkillNameUnarmored, 5 },
                    },
                    AttributeBonusesMale = new Dictionary<string, int>()
                    {
                        { CharacterAttribute.Willpower, -10 },
                        { CharacterAttribute.Agility, 10 },
                        { CharacterAttribute.Speed, 10 },
                        { CharacterAttribute.Endurance, -10 },
                        { CharacterAttribute.Personality, -10 },
                    },
                    AttributeBonusesFemale = new Dictionary<string, int>()
                    {
                        { CharacterAttribute.Intelligence, 10 },
                        { CharacterAttribute.Endurance, -10 },
                        { CharacterAttribute.Personality, -10 },
                    }
                },
                new CharacterRace("Breton")
                {
                    SkillBonuses = new Dictionary<string, int>()
                    {
                        { SkillManager.SkillNameAlchemy, 5 },
                        { SkillManager.SkillNameAlteration, 5 },
                        { SkillManager.SkillNameConjuration, 10 },
                        { SkillManager.SkillNameIllusion, 5 },
                        { SkillManager.SkillNameMysticism, 10 },
                        { SkillManager.SkillNameRestoration, 10 },
                    },
                    AttributeBonusesMale = new Dictionary<string, int>()
                    {
                        { CharacterAttribute.Intelligence,   10 },
                        { CharacterAttribute.Willpower,      10 },
                        { CharacterAttribute.Agility,        -10 },
                        { CharacterAttribute.Speed,          -10 },
                        { CharacterAttribute.Endurance,      -10 },
                    },
                    AttributeBonusesFemale = new Dictionary<string, int>()
                    {
                        { CharacterAttribute.Strength,       -10 },
                        { CharacterAttribute.Intelligence,   10 },
                        { CharacterAttribute.Willpower,      10 },
                        { CharacterAttribute.Agility,        -10 },
                        { CharacterAttribute.Endurance,      -10 },
                    }
                },
                new CharacterRace("Dark Elf")
                {
                    SkillBonuses = new Dictionary<string, int>()
                    {
                        { "Atheltics", 5},
                        { SkillManager.SkillNameDestruction, 10},
                        { SkillManager.SkillNameLightArmor, 5},
                        { SkillManager.SkillNameLongBlade, 5},
                        { SkillManager.SkillNameMarksman, 5},
                        { SkillManager.SkillNameMysticism, 5},
                        { SkillManager.SkillNameShortBlade, 10},
                    },
                    AttributeBonusesMale = new Dictionary<string, int>()
                    {
                        { CharacterAttribute.Willpower,      -10 },
                        { CharacterAttribute.Speed,          10 },
                        { CharacterAttribute.Personality,    -10 },
                    },
                    AttributeBonusesFemale = new Dictionary<string, int>()
                    {
                        { CharacterAttribute.Willpower,      -10 },
                        { CharacterAttribute.Speed,          10 },
                        { CharacterAttribute.Endurance,      -10 },
                    }
                },
                new CharacterRace("High Elf")
                {
                    SkillBonuses = new Dictionary<string, int>()
                    {
                        { SkillManager.SkillNameAlchemy, 10 },
                        { SkillManager.SkillNameAlteration, 5 },
                        { SkillManager.SkillNameConjuration, 5 },
                        { SkillManager.SkillNameDestruction, 10 },
                        { SkillManager.SkillNameEnchant, 10 },
                        { SkillManager.SkillNameIllusion, 5 },
                    },
                    AttributeBonusesMale = new Dictionary<string, int>()
                    {
                        { CharacterAttribute.Strength,       -10 },
                        { CharacterAttribute.Intelligence,   10 },
                        { CharacterAttribute.Speed,          -10 },
                    },
                    AttributeBonusesFemale = new Dictionary<string, int>()
                    {
                        { CharacterAttribute.Strength,       -10 },
                        { CharacterAttribute.Intelligence,   10 },
                        { CharacterAttribute.Endurance,      -10 },
                    }
                },
                new CharacterRace("Imperial")
                {
                    SkillBonuses = new Dictionary<string, int>()
                    {
                        { SkillManager.SkillNameBluntWeapon, 5 },
                        { SkillManager.SkillNameHandToHand, 5 },
                        { SkillManager.SkillNameLightArmor, 5 },
                        { SkillManager.SkillNameLongBlade, 10 },
                        { SkillManager.SkillNameMercantile, 10 },
                        { SkillManager.SkillNameSpeechcraft, 10 },
                    },
                    AttributeBonusesMale = new Dictionary<string, int>()
                    {
                        { CharacterAttribute.Willpower,      -10 },
                        { CharacterAttribute.Agility,        -10 },
                        { CharacterAttribute.Personality,    10 },
                    },
                    AttributeBonusesFemale = new Dictionary<string, int>()
                    {
                        { CharacterAttribute.Agility,        -10 },
                        { CharacterAttribute.Speed,          -10 },
                        { CharacterAttribute.Personality,    10 },
                    }
                },
                new CharacterRace("Khajiit")
                {
                    SkillBonuses = new Dictionary<string, int>()
                    {
                        { SkillManager.SkillNameAcrobatics, 15 },
                        { SkillManager.SkillNameAthletics, 5 },
                        { SkillManager.SkillNameHandToHand, 5 },
                        { SkillManager.SkillNameLightArmor, 5 },
                        { SkillManager.SkillNameSecurity, 5 },
                        { SkillManager.SkillNameShortBlade, 5 },
                        { SkillManager.SkillNameSneak, 5 },
                    },
                    AttributeBonusesMale = new Dictionary<string, int>()
                    {
                        { CharacterAttribute.Willpower,      -10 },
                        { CharacterAttribute.Agility,        10 },
                        { CharacterAttribute.Endurance,      -10 },
                    },
                    AttributeBonusesFemale = new Dictionary<string, int>()
                    {
                        { CharacterAttribute.Strength,       -10 },
                        { CharacterAttribute.Willpower,      -10 },
                        { CharacterAttribute.Agility,        10 },
                    }
                },
                new CharacterRace("Nord")
                {
                    SkillBonuses = new Dictionary<string, int>()
                    {
                        { SkillManager.SkillNameAxe, 10 },
                        { SkillManager.SkillNameBluntWeapon, 10 },
                        { SkillManager.SkillNameHeavyArmor, 5 },
                        { SkillManager.SkillNameLongBlade, 5 },
                        { SkillManager.SkillNameMediumArmor, 10 },
                        { SkillManager.SkillNameSpear, 5 },
                    },
                    AttributeBonusesMale = new Dictionary<string, int>()
                    {
                        { CharacterAttribute.Strength,       10 },
                        { CharacterAttribute.Intelligence,   -10 },
                        { CharacterAttribute.Agility,        -10 },
                        { CharacterAttribute.Endurance,      10 },
                        { CharacterAttribute.Personality,    -10 },
                    },
                    AttributeBonusesFemale = new Dictionary<string, int>()
                    {
                        { CharacterAttribute.Strength,       10 },
                        { CharacterAttribute.Intelligence,   -10 },
                        { CharacterAttribute.Willpower,      10 },
                        { CharacterAttribute.Agility,        -10 },
                        { CharacterAttribute.Personality,    -10 },
                    }
                },
                new CharacterRace("Orc")
                {
                    SkillBonuses = new Dictionary<string, int>()
                    {
                        { SkillManager.SkillNameArmorer, 10 },
                        { SkillManager.SkillNameAxe, 5 },
                        { SkillManager.SkillNameBlock, 10 },
                        { SkillManager.SkillNameHeavyArmor, 10 },
                        { SkillManager.SkillNameMediumArmor, 10 },
                    },
                    AttributeBonusesMale = new Dictionary<string, int>()
                    {
                        { CharacterAttribute.Strength,       5 },
                        { CharacterAttribute.Intelligence,   -10 },
                        { CharacterAttribute.Willpower,      10 },
                        { CharacterAttribute.Agility,        -5 },
                        { CharacterAttribute.Speed,          -10 },
                        { CharacterAttribute.Endurance,      10 },
                        { CharacterAttribute.Personality,    -10 },
                    },
                    AttributeBonusesFemale = new Dictionary<string, int>()
                    {
                        { CharacterAttribute.Strength,       5 },
                        { CharacterAttribute.Willpower,      5 },
                        { CharacterAttribute.Agility,        -5 },
                        { CharacterAttribute.Speed,          -10 },
                        { CharacterAttribute.Endurance,      10 },
                        { CharacterAttribute.Personality,    -15 },
                    }
                },
                new CharacterRace("Redguard")
                {
                    SkillBonuses = new Dictionary<string, int>()
                    {
                        { SkillManager.SkillNameAthletics, 5 },
                        { SkillManager.SkillNameAxe, 5 },
                        { SkillManager.SkillNameBluntWeapon, 5 },
                        { SkillManager.SkillNameHeavyArmor, 5 },
                        { SkillManager.SkillNameLongBlade, 15 },
                        { SkillManager.SkillNameMediumArmor, 5 },
                        { SkillManager.SkillNameShortBlade, 5 },
                    },
                    AttributeBonusesMale = new Dictionary<string, int>()
                    {
                        { CharacterAttribute.Strength,       10 },
                        { CharacterAttribute.Intelligence,   -10 },
                        { CharacterAttribute.Willpower,      -10 },
                        { CharacterAttribute.Endurance,      10 },
                        { CharacterAttribute.Personality,    -10 },
                    },
                    AttributeBonusesFemale = new Dictionary<string, int>()
                    {
                        { CharacterAttribute.Intelligence,   -10 },
                        { CharacterAttribute.Willpower,      -10 },
                        { CharacterAttribute.Endurance,      10 },
                    }
                },
                new CharacterRace("Wood Elf")
                {
                    SkillBonuses = new Dictionary<string, int>()
                    {
                        { SkillManager.SkillNameAcrobatics, 5 },
                        { SkillManager.SkillNameAlchemy, 5 },
                        { SkillManager.SkillNameLightArmor, 10 },
                        { SkillManager.SkillNameMarksman, 15 },
                        { SkillManager.SkillNameSneak, 10 },
                    },
                    AttributeBonusesMale = new Dictionary<string, int>()
                    {
                        { CharacterAttribute.Strength,       -10 },
                        { CharacterAttribute.Willpower,      -10 },
                        { CharacterAttribute.Agility,        10 },
                        { CharacterAttribute.Speed,          10 },
                        { CharacterAttribute.Endurance,      -10 },
                    },
                    AttributeBonusesFemale = new Dictionary<string, int>()
                    {
                        { CharacterAttribute.Strength,       -10 },
                        { CharacterAttribute.Willpower,      -10 },
                        { CharacterAttribute.Agility,        10 },
                        { CharacterAttribute.Speed,          10 },
                        { CharacterAttribute.Endurance,      -10 },
                    }
                },
            };

            return races;
        }
    }
}
