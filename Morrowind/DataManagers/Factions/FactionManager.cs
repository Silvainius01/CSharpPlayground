using CommandEngine.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading.Tasks;

namespace Morrowind
{
    class FactionManager : TypeManager<Faction, FactionManager>
    {
        public static List<Faction> FactionList = new();
        public static Dictionary<string, Faction> FactionDict = new();

        protected override string GetDataPath()
            => $"{Directory.GetCurrentDirectory()}\\Data\\Factions.json";

        protected override void AddTypeEntry(Faction item)
        {
            if (!FactionDict.ContainsKey(item.Name))
            {
                FactionList.Add(item);
                FactionDict.Add(item.Name, item);
            }
        }

        protected override List<Faction> GetDefaultTypesInternal()
        {
            List<Faction> factions = new List<Faction>()
            {
                new Faction("House Hlaalu")
                {
                    IsJoinable = true,
                    FavoredAttributes = new List<string>() { CharacterAttribute.Speed, CharacterAttribute.Agility },
                    FavoredSkills = new List<string>()
                    {
                        SkillManager.SkillNameSpeechcraft,
                        SkillManager.SkillNameMercantile,
                        SkillManager.SkillNameMarksman,
                        SkillManager.SkillNameShortBlade,
                        SkillManager.SkillNameLightArmor,
                        SkillManager.SkillNameSecurity,
                    },
                    IncompatibleFactions = new List<string>(){ "House Redoran", "House Telvanni" }
                },
                new Faction("House Redoran")
                {
                    IsJoinable = true,
                    FavoredAttributes = new List<string>() { CharacterAttribute.Endurance, CharacterAttribute.Strength },
                    FavoredSkills = new List<string>()
                    {
                        SkillManager.SkillNameAthletics,
                        SkillManager.SkillNameSpear,
                        SkillManager.SkillNameLongBlade,
                        SkillManager.SkillNameHeavyArmor,
                        SkillManager.SkillNameMediumArmor,
                        SkillManager.SkillNameArmorer,
                    },
                    IncompatibleFactions = new List<string>(){ "House Hlaalu", "House Telvanni" }
                },
                new Faction("House Telvanni")
                {
                    IsJoinable = true,
                    FavoredAttributes = new List<string>() { CharacterAttribute.Willpower, CharacterAttribute.Intelligence },
                    FavoredSkills = new List<string>()
                    {
                        SkillManager.SkillNameMysticism,
                        SkillManager.SkillNameConjuration,
                        SkillManager.SkillNameIllusion,
                        SkillManager.SkillNameAlteration,
                        SkillManager.SkillNameDestruction,
                        SkillManager.SkillNameEnchant,
                    },
                    IncompatibleFactions = new List<string>(){ "House Hlaalu", "House Redoran" }
                },
                new Faction("Blades")
                {
                    IsJoinable = true,
                    MainQuestRequired = true,
                    FavoredAttributes = new List<string>() { CharacterAttribute.Intelligence, CharacterAttribute.Personality },
                    FavoredSkills = new List<string>()
                    {
                        SkillManager.SkillNameSpeechcraft,
                        SkillManager.SkillNameMarksman,
                        SkillManager.SkillNameLightArmor,
                        SkillManager.SkillNameSneak,
                        SkillManager.SkillNameRestoration,
                        SkillManager.SkillNameLongBlade,
                    }
                },
                new Faction("East Empire Company")
                {
                    IsJoinable = true,
                    FavoredAttributes = new List<string>() { CharacterAttribute.Personality, CharacterAttribute.Willpower },
                    FavoredSkills = new List<string>()
                    {
                        SkillManager.SkillNameSpeechcraft,
                        SkillManager.SkillNameMercantile,
                        SkillManager.SkillNameSecurity,
                        SkillManager.SkillNameLongBlade,
                        SkillManager.SkillNameMediumArmor,
                    }
                },
                new Faction("Fighters Guild")
                {
                    IsJoinable = true,
                    FavoredAttributes = new List<string>() { CharacterAttribute.Strength, CharacterAttribute.Endurance },
                    FavoredSkills = new List<string>()
                    {
                        SkillManager.SkillNameAxe,
                        SkillManager.SkillNameLongBlade,
                        SkillManager.SkillNameBluntWeapon,
                        SkillManager.SkillNameHeavyArmor,
                        SkillManager.SkillNameArmorer,
                        SkillManager.SkillNameBlock,
                    },
                    AdvancedPairings = new List<string>() { "Thieves Guild" }
                },
                new Faction("Imperial Cult")
                {
                    IsJoinable = true,
                    FavoredAttributes = new List<string>() { CharacterAttribute.Personality, CharacterAttribute.Willpower },
                    FavoredSkills = new List<string>()
                    {
                        SkillManager.SkillNameSpeechcraft,
                        SkillManager.SkillNameUnarmored,
                        SkillManager.SkillNameRestoration,
                        SkillManager.SkillNameMysticism,
                        SkillManager.SkillNameConjuration,
                        SkillManager.SkillNameEnchant,
                        SkillManager.SkillNameBluntWeapon,
                    }
                },
                new Faction("Imperial Legion")
                {
                    IsJoinable = true,
                    FavoredAttributes = new List<string>() { CharacterAttribute.Endurance, CharacterAttribute.Personality },
                    FavoredSkills = new List<string>()
                    {
                        SkillManager.SkillNameAthletics,
                        SkillManager.SkillNameSpear,
                        SkillManager.SkillNameLongBlade,
                        SkillManager.SkillNameBluntWeapon,
                        SkillManager.SkillNameHeavyArmor,
                        SkillManager.SkillNameBlock,
                    }
                },
                new Faction("Mages Guild")
                {
                    IsJoinable = true,
                    FavoredAttributes = new List<string>() { CharacterAttribute.Intelligence, CharacterAttribute.Willpower },
                    FavoredSkills = new List<string>()
                    {
                        SkillManager.SkillNameAlchemy,
                        SkillManager.SkillNameMysticism,
                        SkillManager.SkillNameIllusion,
                        SkillManager.SkillNameAlteration,
                        SkillManager.SkillNameDestruction,
                        SkillManager.SkillNameEnchant,
                    }
                },
                new Faction("Thieves Guild")
                {
                    IsJoinable = true,
                    FavoredAttributes = new List<string>() { CharacterAttribute.Agility, CharacterAttribute.Personality },
                    FavoredSkills = new List<string>()
                    {
                        SkillManager.SkillNameMarksman,
                        SkillManager.SkillNameShortBlade,
                        SkillManager.SkillNameLightArmor,
                        SkillManager.SkillNameAcrobatics,
                        SkillManager.SkillNameSneak,
                        SkillManager.SkillNameSecurity,
                    },
                    AdvancedPairings = new List<string>() { "Fighters Guild" }
                },
                new Faction("Ashlanders")
                {
                    IsJoinable = true,
                    MainQuestRequired = true,
                    FavoredAttributes = new List<string>() { CharacterAttribute.Agility, CharacterAttribute.Endurance },
                    FavoredSkills = new List<string>()
                    {
                        SkillManager.SkillNameAlteration,
                        SkillManager.SkillNameLightArmor,
                        SkillManager.SkillNameMarksman,
                        SkillManager.SkillNameMediumArmor,
                        SkillManager.SkillNameMysticism,
                        SkillManager.SkillNameSpear,
                    }
                },
                new Faction("Morag Tong")
                {
                    IsJoinable = true,
                    MainQuestRequired = true,
                    FavoredAttributes = new List<string>() { CharacterAttribute.Speed, CharacterAttribute.Agility },
                    FavoredSkills = new List<string>()
                    {
                        SkillManager.SkillNameAcrobatics,
                        SkillManager.SkillNameIllusion,
                        SkillManager.SkillNameMarksman,
                        SkillManager.SkillNameLightArmor,
                        SkillManager.SkillNameShortBlade,
                        SkillManager.SkillNameSneak,
                    }
                },
                new Faction("Tribunal Temple")
                {
                    IsJoinable = true,
                    MainQuestRequired = true,
                    FavoredAttributes = new List<string>() { CharacterAttribute.Intelligence, CharacterAttribute.Personality },
                    FavoredSkills = new List<string>()
                    {
                        SkillManager.SkillNameAlchemy,
                        SkillManager.SkillNameBluntWeapon,
                        SkillManager.SkillNameConjuration,
                        SkillManager.SkillNameMysticism,
                        SkillManager.SkillNameRestoration,
                        SkillManager.SkillNameUnarmored,
                    }
                },
            };

            return factions;
        }
    }
}
