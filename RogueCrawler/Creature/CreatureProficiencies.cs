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

        public int SkillLevel { get; set; }
        public float SkillProgress { get; set; }
        public string SkillName { get; set; }

        public override string ToString()
        {
            return $"{SkillName}: {SkillLevel} [{SkillProgress}]";
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
                if (skill.SkillLevel > 0 || skill.SkillProgress > 0)
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
                    SkillLevel = 0,
                    SkillProgress = 0,
                    SkillName = skillName
                });
            }
            return Skills[skillName];
        }
        public int GetSkillLevel(string skillName) => GetSkill(skillName).SkillLevel;
        public float GetSkillProgress(string skillName) => GetSkill(skillName).SkillProgress;

        public void SetSkill(string skillName, int level, float progress)
        {
            var skill = GetSkill(skillName);
            if (level >= 0)
                skill.SkillLevel = Math.Min(level, DungeonSettings.MaxSkillLevel);
            if (progress >= 0)
                skill.SkillProgress = progress;
        }
        public void SetSkillLevel(string skillName, int level) => SetSkill(skillName, level, -1);
        public void SetSkillProgress(string skillName, float progress) => SetSkill(skillName, -1, progress);

        public void AddSkill(string skillName, int level, float progress)
        {
            var skill = GetSkill(skillName);
            skill.SkillLevel = Math.Min(skill.SkillLevel + level, DungeonSettings.MaxSkillLevel);
            skill.SkillProgress += progress;
        }
        public void AddSkillLevel(string skillName, int level) => AddSkill(skillName, level, 0);
        public void AddSkillProgress(string skillName, float progress) => AddSkill(skillName, 0, progress);

        public SerializedProfeciencies GetSerializable()
        {
            return new SerializedProfeciencies()
            {
                Skills = this.Skills
            };
        }

        public ColorStringBuilder BriefColor(ConsoleColor initialColor = ConsoleColor.Gray)
        {
            throw new NotImplementedException();
        }

        public ColorStringBuilder InspectColor(string prefix, int tabCount, ConsoleColor initialColor = ConsoleColor.Gray)
        {
            throw new NotImplementedException();
        }

        public ColorStringBuilder DebugColor(string prefix, int tabCount, ConsoleColor initialColor = ConsoleColor.Gray)
        {
            throw new NotImplementedException();
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
                skills.SetSkill(skill.SkillName, skill.SkillLevel, skill.SkillProgress);

            return skills;
        }
    }
}
