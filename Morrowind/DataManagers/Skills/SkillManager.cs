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

        #region Skill Names
        public const string SkillNameAcrobatics      = "Acrobatics";
        public const string SkillNameAlchemy         = "Alchemy";
        public const string SkillNameAlteration      = "Alteration";
        public const string SkillNameArmorer         = "Armorer";
        public const string SkillNameAthletics       = "Athletics";
        public const string SkillNameAxe             = "Axe";
        public const string SkillNameBlock           = "Block";
        public const string SkillNameBluntWeapon     = "Blunt Weapon";
        public const string SkillNameConjuration     = "Conjuration";
        public const string SkillNameDestruction     = "Destruction";
        public const string SkillNameEnchant         = "Enchant";
        public const string SkillNameHandToHand      = "Hand to Hand";
        public const string SkillNameHeavyArmor      = "Heavy Armor";
        public const string SkillNameIllusion        = "Illusion";
        public const string SkillNameLightArmor      = "Light Armor";
        public const string SkillNameLongBlade       = "Long Blade";
        public const string SkillNameMarksman        = "Marksman";
        public const string SkillNameMediumArmor     = "Medium Armor";
        public const string SkillNameMercantile      = "Mercantile";
        public const string SkillNameMysticism       = "Mysticism";
        public const string SkillNameRestoration     = "Restoration";
        public const string SkillNameSecurity        = "Security";
        public const string SkillNameShortBlade      = "Short Blade";
        public const string SkillNameSneak           = "Sneak";
        public const string SkillNameSpear           = "Spear";
        public const string SkillNameSpeechcraft     = "Speechcraft";
        public const string SkillNameUnarmored       = "Unarmored";
        #endregion

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
                new Skill(SkillNameAcrobatics)
                { 
                    GoverningAttribute = CharacterAttribute.Strength, 
                    Specialization = "Stealth",
                },
                new Skill(SkillNameAlchemy)
                {
                    GoverningAttribute = CharacterAttribute.Intelligence,
                    Specialization = "Magic",
                },
                new Skill(SkillNameAlteration)
                {
                    GoverningAttribute = CharacterAttribute.Willpower,
                    Specialization = "Magic",
                },
                new Skill(SkillNameArmorer)
                {
                    GoverningAttribute = CharacterAttribute.Strength, 
                    Specialization = "Combat",
                },
                new Skill(SkillNameAthletics)
                { 
                    GoverningAttribute = CharacterAttribute.Speed,
                    Specialization = "Combat",
                },
                new Skill(SkillNameAxe)
                { 
                    GoverningAttribute = CharacterAttribute.Strength, 
                    Specialization = "Combat",
                },
                new Skill(SkillNameBlock)
                { 
                    GoverningAttribute = CharacterAttribute.Agility, 
                    Specialization = "Combat",
                },
                new Skill(SkillNameBluntWeapon)
                {  
                    GoverningAttribute = CharacterAttribute.Strength, 
                    Specialization = "Combat",
                },
                new Skill(SkillNameConjuration)
                {
                    GoverningAttribute = CharacterAttribute.Intelligence,
                    Specialization = "Magic",
                },
                new Skill(SkillNameDestruction)
                {
                    GoverningAttribute = CharacterAttribute.Willpower,
                    Specialization = "Magic",
                },
                new Skill(SkillNameEnchant)
                {
                    GoverningAttribute = CharacterAttribute.Intelligence,
                    Specialization = "Magic",
                },
                new Skill(SkillNameHandToHand)
                {  
                    GoverningAttribute = CharacterAttribute.Speed, 
                    Specialization = "Stealth",
                },
                new Skill(SkillNameHeavyArmor)
                { 
                    GoverningAttribute = CharacterAttribute.Endurance, 
                    Specialization = "Combat",
                },
                new Skill(SkillNameIllusion)
                {
                    GoverningAttribute = CharacterAttribute.Personality,
                    Specialization = "Magic",
                },
                new Skill(SkillNameLightArmor)
                { 
                    GoverningAttribute = CharacterAttribute.Agility, 
                    Specialization = "Stealth",
                },
                new Skill(SkillNameLongBlade)
                {  
                    GoverningAttribute = CharacterAttribute.Strength, 
                    Specialization = "Combat",
                },
                new Skill(SkillNameMarksman)
                {  
                    GoverningAttribute = CharacterAttribute.Agility, 
                    Specialization = "Stealth",
                },
                new Skill(SkillNameMediumArmor)
                { 
                    GoverningAttribute = CharacterAttribute.Endurance, 
                    Specialization = "Combat",
                },
                new Skill(SkillNameMercantile)
                {
                    GoverningAttribute = CharacterAttribute.Personality,
                    Specialization = "Stealth",
                },
                new Skill(SkillNameMysticism)
                {
                    GoverningAttribute = CharacterAttribute.Willpower,
                    Specialization = "Magic",
                },
                new Skill(SkillNameRestoration)
                {
                    GoverningAttribute = CharacterAttribute.Willpower,
                    Specialization = "Magic",
                },
                new Skill(SkillNameSecurity)
                {
                    GoverningAttribute = CharacterAttribute.Intelligence,
                    Specialization = "Stealth",
                },
                new Skill(SkillNameShortBlade)
                {  
                    GoverningAttribute = CharacterAttribute.Speed,
                    Specialization = "Stealth",
                },
                new Skill(SkillNameSneak)
                {
                    GoverningAttribute = CharacterAttribute.Agility,
                    Specialization = "Stealth",
                },
                new Skill(SkillNameSpear)
                {  
                    GoverningAttribute = CharacterAttribute.Endurance,
                    Specialization = "Combat",
                },
                new Skill(SkillNameSpeechcraft)
                {
                    GoverningAttribute = CharacterAttribute.Personality,
                    Specialization = "Stealth",
                },
                new Skill(SkillNameUnarmored)
                {
                    GoverningAttribute = CharacterAttribute.Speed,
                    Specialization = "Magic",
                }
            };

            return skills;
        }
    }
}
