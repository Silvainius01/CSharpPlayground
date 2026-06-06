using CommandEngine.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Morrowind
{
    class SkillManager : TypeManager<Skill, SkillManager>
    {
        public static List<Skill> SkillList { get; private set; } = new List<Skill>();
        public static Dictionary<string, Skill> SkillDict { get; private set; } = new Dictionary<string, Skill>();
        public static Dictionary<string, List<Skill>> SkillsByAttribute { get; private set; } = new Dictionary<string, List<Skill>>();
        public static Dictionary<string, List<Skill>> SkillsBySpecialization { get; private set; } = new Dictionary<string, List<Skill>>();

        protected override string GetDataPath()
            => $"{Directory.GetCurrentDirectory()}\\Data\\Skills.json";

        protected override void AddTypeEntry(Skill item)
        {
            if (!SkillDict.ContainsKey(item.Name))
            {
                SkillList.Add(item);
                SkillDict.Add(item.Name, item);

                if (!SkillsByAttribute.ContainsKey(item.GoverningAttribute))
                    SkillsByAttribute.Add(item.GoverningAttribute, new List<Skill>());
                SkillsByAttribute[item.GoverningAttribute].Add(item);

                if (!SkillsBySpecialization.ContainsKey(item.Specialization))
                    SkillsBySpecialization.Add(item.Specialization, new List<Skill>());
                SkillsBySpecialization[item.Specialization].Add(item);
            }
        }

        protected override List<Skill> GetDefaultTypesInternal()
        {
            List<Skill> skills = new List<Skill>()
            {
                new Skill("Acrobatics")
                { 
                    GoverningAttribute = "Strength", 
                    Specialization = "Stealth",
                },
                new Skill("Alchemy")
                {
                    GoverningAttribute = "Intelligence",
                    Specialization = "Magic",
                },
                new Skill("Alteration")
                {
                    GoverningAttribute = "Willpower",
                    Specialization = "Magic",
                },
                new Skill("Armorer")
                {
                    GoverningAttribute = "Strength", 
                    Specialization = "Combat",
                },
                new Skill("Athletics")
                { 
                    GoverningAttribute = "Speed",
                    Specialization = "Combat",
                },
                new Skill("Axe")
                { 
                    GoverningAttribute = "Strength", 
                    Specialization = "Combat",
                },
                new Skill("Block")
                { 
                    GoverningAttribute = "Agility", 
                    Specialization = "Combat",
                },
                new Skill("Blunt Weapon")
                {  
                    GoverningAttribute = "Strength", 
                    Specialization = "Combat",
                },
                new Skill("Conjuration")
                {
                    GoverningAttribute = "Intelligence",
                    Specialization = "Magic",
                },
                new Skill("Destruction")
                {
                    GoverningAttribute = "Willpower",
                    Specialization = "Magic",
                },
                new Skill("Enchant")
                {
                    GoverningAttribute = "Intelligence",
                    Specialization = "Magic",
                },
                new Skill("Hand to Hand")
                {  
                    GoverningAttribute = "Speed", 
                    Specialization = "Stealth",
                },
                new Skill("Heavy Armor")
                { 
                    GoverningAttribute = "Endurance", 
                    Specialization = "Combat",
                },
                new Skill("Illusion")
                {
                    GoverningAttribute = "Personality",
                    Specialization = "Magic",
                },
                new Skill("Light Armor")
                { 
                    GoverningAttribute = "Agility", 
                    Specialization = "Stealth",
                },
                new Skill("Long Blade")
                {  
                    GoverningAttribute = "Strength", 
                    Specialization = "Combat",
                },
                new Skill("Marksman")
                {  
                    GoverningAttribute = "Agility", 
                    Specialization = "Stealth",
                },
                new Skill("Medium Armor")
                { 
                    GoverningAttribute = "Endurance", 
                    Specialization = "Combat",
                },
                new Skill("Mercantile")
                {
                    GoverningAttribute = "Personality",
                    Specialization = "Stealth",
                },
                new Skill("Mysticism")
                {
                    GoverningAttribute = "Willpower",
                    Specialization = "Magic",
                },
                new Skill("Restoration")
                {
                    GoverningAttribute = "Willpower",
                    Specialization = "Magic",
                },
                new Skill("Security")
                {
                    GoverningAttribute = "Intelligence",
                    Specialization = "Stealth",
                },
                new Skill("Short Blade")
                {  
                    GoverningAttribute = "Speed",
                    Specialization = "Stealth",
                },
                new Skill("Sneak")
                {
                    GoverningAttribute = "Agility",
                    Specialization = "Stealth",
                },
                new Skill("Spear")
                {  
                    GoverningAttribute = "Endurance",
                    Specialization = "Combat",
                },
                new Skill("Speechcraft")
                {
                    GoverningAttribute = "Personality",
                    Specialization = "Stealth",
                },
                new Skill("Unarmored")
                {
                    GoverningAttribute = "Speed",
                    Specialization = "Magic",
                }
            };

            return skills;
        }
    }
}
