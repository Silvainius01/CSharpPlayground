using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CommandEngine;

namespace RogueCrawler
{
    class CreatureSkill
    {
        public int SkillLevel { get; set; }
        public float SkillProgress { get; set; }
        public string SkillName { get; set; }

        public override string ToString()
        {
            return $"{SkillName}: {SkillLevel} [{SkillProgress}]";
        }
    }

    class CreatureProfeciencies : IInspectable
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
            SmartStringBuilder builder = new SmartStringBuilder(DungeonCrawlerSettings.TabString); ;

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
            SmartStringBuilder builder = new SmartStringBuilder(DungeonCrawlerSettings.TabString); ;

            if (prefix == string.Empty)
                prefix = "Attributes:";
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
            skill.SkillLevel = level;
            skill.SkillProgress = progress;
        }
        public void SetSkillLevel(string skillName, int level) => SetSkill(skillName, level, 0);
        public void SetSkillProgress(string skillName, float progress) => SetSkill(skillName, 0, progress);

        public void AddSkill(string skillName, int level, float progress)
        {
            var skill = GetSkill(skillName);
            skill.SkillLevel += level;
            skill.SkillProgress += progress;
        }
        public void AddSkillLevel(string skillName, int level) => AddSkill(skillName, level, 0);
        public void AddSkillProgress(string skillName, float progress) => AddSkill(skillName, 0, progress);
    }
}
