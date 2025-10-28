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
            if(level >= 0)
                skill.SkillLevel = Math.Max(level, DungeonSettings.MaxSkillLevel);
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
    }

    static class CreatureSkillUtility
    {
        static readonly float[] WeaponSkillQualityBonus = CacheWeaponSkillBonuses();

        // Calculate the skill bonuses for weapons. 
        // Assumption is that any bonus follows: floor(specSkill*0.75 + genSkill/4), or 0-100
        static float[] CacheWeaponSkillBonuses()
        {
            float[] bonuses = new float[256];

            for (int i = 0; i < bonuses.Length; ++i)
                bonuses[i] = CalcWeaponSkillBonus(i);

            return bonuses;
        }
        static float CalcWeaponSkillBonus(int level)
        {
            float sigmoid (int x) =>
                1.014f / (1 + MathF.Pow(MathF.E, -0.1f * x + 5)) - 0.007f;

            return Mathc.Clamp(sigmoid(level), 0, 1);
        }

        public static float GetWeaponSkillBonus(ItemWeapon weapon, CreatureProficiencies p)
        {
            int skillLevel = (int)
                ((p.GetSkillLevel(weapon.ObjectName) * 0.75f) + 
                (p.GetSkillLevel(weapon.WeaponType) / 4.0f));
            return skillLevel < WeaponSkillQualityBonus.Length
                ? WeaponSkillQualityBonus[skillLevel]
                : CalcWeaponSkillBonus(skillLevel);
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
