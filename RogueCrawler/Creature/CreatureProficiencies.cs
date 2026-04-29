using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Xml.Linq;
using CommandEngine;
using Newtonsoft.Json;

namespace RogueCrawler
{
    class CreatureSkill
    {
        #region Skill Name Constants
        public static string Evasion = "Evasion";
        public static string Unarmored = "Unarmored";
        #endregion

        public int Level { get; set; }
        public int Experience { get; set; }
        public string Name { get; set; }

        public int ExpNeeded => ExperienceNeeded(Level);
        public float Progress => (float)Experience / ExpNeeded;

        public override string ToString()
        {
            if (Level >= DungeonSettings.MaxSkillLevel)
                return $"{Name}: [{Level}]"; 
            return $"{Name}: {Level} [{Experience}/{ExpNeeded}] -> {(Progress * 100).ToString("n1")}%";
        }

        public int ExperienceNeeded(int level)
        {
            return 50 + 25 * (int)Math.Ceiling(Math.Pow(Level, 1.1521));
        }
    }

    class CreatureProficiencies : IInspectable, ISerializable<SerializedProfeciencies, CreatureProficiencies>
    {
        Dictionary<string, CreatureSkill> Skills = new Dictionary<string, CreatureSkill>();

        public CreatureSkill this[string key]
        {
            get => GetSkill(key);
        }

        public string BriefString()
        {
            return ToString();
        }
        public string DebugString(string prefix, int tabCount)
        {
            SmartStringBuilder builder = new SmartStringBuilder(DungeonSettings.TabString);

            if (prefix == string.Empty)
                prefix = "Skills:";
            builder.Append(tabCount, prefix);

            tabCount++;
            foreach (var skill in Skills.Values)
                builder.NewlineAppend(tabCount, skill.ToString());
            tabCount--;
            return builder.ToString();
        }
        public string InspectString(string prefix, int tabCount)
        {
            SmartStringBuilder builder = new SmartStringBuilder(DungeonSettings.TabString);

            if (prefix == string.Empty)
                prefix = "Skills:";
            builder.Append(tabCount, prefix);

            tabCount++;
            foreach (var skill in Skills.Values)
                if (skill.Level > 0 || skill.Experience > 0)
                    builder.NewlineAppend(tabCount, skill.ToString());
            tabCount--;
            return builder.ToString();
        }
        public override string ToString()
        {
            return Skills.ToString((kvp) => kvp.Value.ToString(), " ");
        }

        public CreatureSkill GetSkill(string skillName)
        {
            if (!Skills.ContainsKey(skillName))
            {
                Skills.Add(skillName, new CreatureSkill()
                {
                    Level = 0,
                    Experience = 0,
                    Name = skillName
                });
            }
            return Skills[skillName];
        }
        public int GetSkillLevel(string skillName) => GetSkill(skillName).Level;
        public float GetSkillProgress(string skillName) => GetSkill(skillName).Experience;

        public void SetSkill(string skillName, int level, int experience)
        {
            var skill = GetSkill(skillName);
            if (level >= 0)
                skill.Level = Math.Min(level, DungeonSettings.MaxSkillLevel);
            if (experience >= 0)
                skill.Experience = experience;
        }
        public void SetSkillLevel(string skillName, int level) => SetSkill(skillName, level, -1);
        public void SetSkillProgress(string skillName, int experience) => SetSkill(skillName, -1, experience);

        public void AddSkill(string skillName, int level, int experience)
        {
            var skill = GetSkill(skillName);

            if (experience > 0 && skill.Level < DungeonSettings.MaxSkillLevel)
            {
                int expNeeded = skill.ExperienceNeeded(skill.Level + level);
                skill.Experience += experience;
                while (skill.Experience >= expNeeded)
                {
                    ++level;
                    skill.Experience -= expNeeded;
                    expNeeded = skill.ExperienceNeeded(skill.Level + level);
                }
            }
            else skill.Experience = 0;

            skill.Level = Math.Min(skill.Level + level, DungeonSettings.MaxSkillLevel);
        }
        public void AddSkillLevel(string skillName, int level) => AddSkill(skillName, level, 0);
        public void AddSkillExperience(string skillName, int experience) => AddSkill(skillName, 0, experience);

        public SerializedProfeciencies GetSerializable()
        {
            Dictionary<string, CreatureSkill> savedSkills = new Dictionary<string, CreatureSkill>();

            // Dont bother saving skills with no levels or progress.
            // Also, we are copying skills here to avoid creating additional references.
            foreach (var skill in this.Skills.Values)
                if (skill.Level > 0 || skill.Experience > 0)
                    savedSkills.Add(skill.Name, new CreatureSkill()
                    {
                        Level = skill.Level,
                        Experience = skill.Experience,
                        Name = skill.Name
                    });

            return new SerializedProfeciencies()
            {
                Skills = savedSkills
            };
        }
    }

    static class CreatureSkillUtility
    {
        static readonly float[] BaseSkillQualityBonus = CacheBaseSkillBonuses();

        // Calculate the skill bonuses for weapons. 
        // Assumption is that any bonus follows: floor(specSkill*0.75 + genSkill/4), or 0-100
        static float[] CacheBaseSkillBonuses()
        {
            float[] bonuses = new float[256];

            for (int i = 0; i < bonuses.Length; ++i)
                bonuses[i] = GetBaseSkillBonus(i);

            return bonuses;
        }
        static float GetBaseSkillBonus(int level)
        {
            // 1.014 / (1 + e^-0.1*x+5) - 0.007
            float sigmoid(int x) =>
                1.014f / (1 + MathF.Pow(MathF.E, -0.1f * x + 5)) - 0.007f;

            return Mathc.Clamp(sigmoid(level), 0, 1);
        }

        public static float GetDefaultSkillBonus(int skillLevel)
        {
            return skillLevel < BaseSkillQualityBonus.Length
               ? BaseSkillQualityBonus[skillLevel]
               : GetBaseSkillBonus(skillLevel);
        }
        public static float GetDefaultSkillBonus(string skill, CreatureProficiencies p)
        {
            return GetDefaultSkillBonus(p.GetSkillLevel(skill));
        }
        public static float GetWeaponSkillBonus(ItemWeapon weapon, CreatureProficiencies p)
        {
            int skillLevel = (int)
                ((p.GetSkillLevel(weapon.ObjectName) * 0.75f) +
                (p.GetSkillLevel(weapon.WeaponType) / 4.0f));
            return GetDefaultSkillBonus(skillLevel);
        }
        public static float GetArmorSkillBonus(ItemArmor armor, CreatureProficiencies p)
        {
            return GetDefaultSkillBonus(p.GetSkillLevel(armor.ArmorClass)) + 0.25f;
        }
    }

    class SerializedProfeciencies : ISerialized<CreatureProficiencies>
    {
        public Dictionary<string, CreatureSkill> Skills { get; set; }

        public CreatureProficiencies GetDeserialized()
        {
            CreatureProficiencies skills = new CreatureProficiencies();

            foreach (var skill in Skills.Values)
                skills.SetSkill(skill.Name, skill.Level, skill.Experience);

            return skills;
        }
    }
}
