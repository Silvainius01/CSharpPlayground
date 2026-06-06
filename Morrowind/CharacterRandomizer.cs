using CommandEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Morrowind
{
    class CharacterRandomizer
    {
        static int MaxWeaponSkills = 2;
        static List<string> WeaponSkills = new List<string>()
        {
            "Axe",
            "Blunt Weapon",
            // "Destruction",
            "Hand to Hand",
            "Long Blade",
            "Marksman",
            "Short Blade",
            "Spear",
        };

        static int MaxArmorSkills = 1;
        static List<string> ArmorSkills = new List<string>()
        {
            "Heavy Armor",
            "Light Armor",
            "Medium Armor",
            "Unarmored",
        };

        static int MaxLockSkills = 1;
        static List<string> LockSkills = new List<string>()
        {
            //"Alteration",
            "Security"
        };

        static Dictionary<string, ConsoleColor> SpecColors = new Dictionary<string, ConsoleColor>()
        {
            { "Combat", ConsoleColor.Blue },
            { "Magic", ConsoleColor.Magenta},
            { "Stealth", ConsoleColor.Yellow },
        };
        static Dictionary<string, string> ShortAttributes = new Dictionary<string, string>()
        {
            { "Strength", "STR" },
            { "Intelligence", "INT"},
            { "Willpower", "WIL" },
            { "Agility", "AGI" },
            { "Speed", "SPD" },
            { "Endurance", "END" },
            { "Personality", "PRS" },
            { "Luck", "LCK" },
        };

        static List<Skill> GetCharacterSkills(List<string> attributes, int numSkills)
        {
            int numLocks = 0;
            int numArmors = 0;
            int numWeapons = 0;
            List<Skill> charSkills = new List<Skill>(10);
            List<Skill> attrSkills = new List<Skill>(SkillManager.SkillList.Count);
            List<Skill> otherSkills = new List<Skill>(SkillManager.SkillList.Count);

            // Split skills into primary and secondary pools
            foreach (string attr in SkillManager.SkillsByAttribute.Keys)
            {
                if (attributes.Contains(attr))
                    attrSkills.AddRange(SkillManager.SkillsByAttribute[attr]);
                else otherSkills.AddRange(SkillManager.SkillsByAttribute[attr]);
            }

            attrSkills.Shuffle();
            otherSkills.Shuffle();

            // Keep 10 random skills from attr pool.
            // Add the rest to the other pool in the order they appeared.
            //while (attrSkills.Count > numSkills)
            //{
            //    otherSkills.Insert(0, attrSkills.Last());
            //    attrSkills.RemoveAt(attrSkills.Count - 1);
            //}

            // Skill validity util.
            bool IsValidSkill(Skill skill)
            {
                return // If we are not at the limit for the category OR the category doesnt contain this skill
                    (numLocks < MaxLockSkills || !LockSkills.Contains(skill.Name)) &&
                    (numArmors < MaxArmorSkills || !ArmorSkills.Contains(skill.Name)) &&
                    (numWeapons < MaxWeaponSkills || !WeaponSkills.Contains(skill.Name));
            }
            bool TryAddSkill(Skill skill)
            {
                if (!IsValidSkill(skill))
                    return false;

                // Record what kind of skill it is
                if (LockSkills.Contains(skill.Name))
                    ++numLocks;
                else if (ArmorSkills.Contains(skill.Name))
                    ++numArmors;
                else if (WeaponSkills.Contains(skill.Name))
                    ++numWeapons;

                charSkills.Add(skill);
                return true;
            }

            // Add skills to final list from primary pool first.
            for (int i = 0; i < numSkills; ++i)
            {
                if (!TryAddSkill(attrSkills[i]))
                    ConsoleExt.WriteWarningLine($"Failed to add '{attrSkills[i].Name}'");
            }

            // Fill any gaps from the remaining skills
            for(int i = charSkills.Count; i < numSkills; ++i)
            {
                if (!TryAddSkill(otherSkills[i]))
                    ConsoleExt.WriteWarningLine($"Failed to add '{otherSkills[i].Name}'");
                else Console.WriteLine($"Added '{otherSkills[i].Name}'");
            }


            //for (int i = 0; i < attrSkills.Count; ++i)
            //{
            //    Skill skill = attrSkills[i];

            //    // Swap invalid skills for the first valid one in the pool
            //    if(!IsValidSkill(skill))
            //    {
            //        int j = GetValidSkillIndex();
            //        attrSkills[i] = otherSkills[j];

            //        ConsoleExt.WriteWarningLine($"Swapping {skill.Name} for {otherSkills[j].Name}");

            //        // Add the invalid skill to the end of the general pool so it doesnt get queried over and over.
            //        otherSkills.RemoveAt(j);
            //        otherSkills.Add(skill);

            //        skill = attrSkills[i];
            //    }

            //    // Increment category counts if any apply
            //    if (LockSkills.Contains(skill.Name))
            //        ++numLocks;
            //    else if (ArmorSkills.Contains(skill.Name))
            //        ++numArmors;
            //    else if (WeaponSkills.Contains(skill.Name))
            //        ++numWeapons;
            //}

            return charSkills;
        }

        public static void GenerateCharacter(int numAttributes = 3, int numSkills = 10)
        {
            using (ManagedColorBuilder mcb = new ManagedColorBuilder("CharacterRandomizer", string.Empty, "  "))
            {
                float attrWeight = 1.0f / 5.0f; // How much an attribute bonus of 1 is worth
                float skillWeight = 1.0f / 5.0f; // How much a skill bonus of 1 is worth

                ConsoleColor defaultColor = ConsoleColor.Gray;
                ColorStringBuilder builder = mcb.Builder;


                // Select random attributes
                List<string> attributes = SkillManager.SkillsByAttribute.Keys.ToList();
                attributes.Shuffle();
                attributes.RemoveRange(numAttributes, attributes.Count - numAttributes);

                // Select random skills from the selected attributes
                List<Skill> charSkills = GetCharacterSkills(attributes, numSkills);

                // Build a weighted distrubtion of races based on how well they compliment selected skills
                float totalScores = 0.0f;
                Dictionary<string, float> raceScores = new Dictionary<string, float>();
                foreach (CharacterRace race in CharacterRaceManager.RaceList)
                {
                    float skillScore = 0;
                    float totalScoreMale = 0;
                    float totalScoreFemale = 0;

                    foreach (var kvp in race.SkillBonuses)
                        if (charSkills.Contains(s => s.Name == kvp.Key))
                            skillScore += kvp.Value * skillWeight;
                    foreach (var kvp in race.AttributeBonusesMale)
                        if (attributes.Contains(kvp.Key))
                            totalScoreMale += kvp.Value * attrWeight;
                    foreach (var kvp in race.AttributeBonusesFemale)
                        if (attributes.Contains(kvp.Key))
                            totalScoreFemale += kvp.Value * attrWeight;

                    totalScoreMale = skillScore + totalScoreMale;
                    totalScoreFemale = skillScore + totalScoreFemale;

                    raceScores.Add($"Male {race.Name}", totalScoreMale);
                    raceScores.Add($"Female {race.Name}", totalScoreFemale);

                    // Dont add negative scores to the totals, we ignore all options <= 0
                    totalScores += Math.Max(totalScoreMale, 0.0f) + Math.Max(totalScoreFemale, 0.0f);
                }

                builder.Append("\nAttributes: ");
                foreach (string attr in attributes)
                    builder.NewlineAppend(1, $"{charSkills.Count(s => s.GoverningAttribute == attr)} x {attr}");

                builder.NewlineAppend("\nMajor Skills: ");
                for (int i = 0; i < numSkills; ++i)
                {
                    if (i == numSkills / 2)
                        builder.NewlineAppend("Minor Skills: ", defaultColor);

                    builder.NewlineAppend(1, $"{ShortAttributes[charSkills[i].GoverningAttribute]} | ", defaultColor);
                    builder.Append(charSkills[i].Name, SpecColors[charSkills[i].Specialization]);
                }

                builder.NewlineAppend("\nRace Chances:", defaultColor);
                var sortedScores = raceScores.ToList();
                sortedScores.Sort((kvp1, kvp2) => -kvp1.Value.CompareTo(kvp2.Value));
                foreach (var kvp in sortedScores)
                {
                    float chance = Math.Max(kvp.Value, 0.0f) / totalScores;
                    string tabs = kvp.Key.Length > 12 ? "\t" : "\t\t";
                    builder.NewlineAppend(1, $"{kvp.Key}:{tabs}{chance.ToString("P")} ({kvp.Value})");
                }

                builder.WriteLine(true);
            }
        }
    }
}
