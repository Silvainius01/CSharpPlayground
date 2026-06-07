using CommandEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Morrowind
{
    class RandomCharacter
    {
        public string Race { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public string Birthsign { get; set; } = string.Empty;
        public List<string> Factions { get; set; } = new();
        public List<string> FavoredAttributes { get; set; } = new();
        public List<Skill> MajorSkills { get; set; } = new();
        public List<Skill> MinorSkills { get; set; } = new();
    }

    class RandomizerOptions
    {
        public int NumSkills { get; set; } = 10;
        public int NumAttributes { get; set; } = 3;

        public int MaxWeaponSkills { get; set; } = 2;
        public int MaxArmorSkills { get; set; } = 1;
        public int MaxLockSkills { get; set; } = 1;

        public float RaceSkillWeight { get; set; } = 1 / 5.0f;
        public float RaceAttributeWeight { get; set; } = 1 / 5.0f;

        public float FactionSkillWeight { get; set; } = 1.0f;
        public float FactionAttributeWeight { get; set; } = 0.5f;

        public float MajorSkillSpecWeight { get; set; } = 1.0f;
        public float MinorSkillSpecWeight { get; set; } = 1.5f;

        public float MinimumRaceScore { get; set; } = 0.0f;
        public float MinimumFactionScore { get; set; } = 3.0f;

        /// <summary> Allows factions pairs that require specific quest completion orders and/or game knowledge to complete. </summary>
        public bool AllowAdvancedFactionPairs { get; set; } = false;

        /// <summary> If enabled, at least one skill that can open locks will be included. </summary>
        public bool ForceLockpickingSkill { get; private set; } = false;
        /// <summary> If enabled, at least one weapon skill will be included. </summary>
        public bool ForceWeaponSkill { get; private set; } = true;
        /// <summary> If enabled, at least one armor skill will be included. </summary>
        public bool ForceArmorSkill { get; private set; } = true;
        /// <summary> If enabled, at least one skill goverened by favored attributes is excluded. </summary>
        public bool ForceTrainableAttributes { get; private set; } = true;

        /// <summary> If enabled, Unarmored will be considered an armor skill </summary>
        public bool UnarmoredCountsAsArmor { get; private set; } = true;
        /// <summary> If enabled, Marksman will be considered a weapon skill </summary>
        public bool MarksmanCountsAsWeapon { get; private set; } = true;
        /// <summary> If enabled, Destruction will be considered a weapon skill </summary>
        public bool DestructionCountsAsWeapon { get; private set; } = false;

        public List<string> LockSkills = new List<string>()
        {
            SkillManager.SkillNameAlteration,
            SkillManager.SkillNameSecurity
        };
        public List<string> ArmorSkills = new List<string>()
        {
            SkillManager.SkillNameHeavyArmor,
            SkillManager.SkillNameLightArmor,
            SkillManager.SkillNameMediumArmor,
        };
        public List<string> WeaponSkills = new List<string>()
        {
            SkillManager.SkillNameAxe,
            SkillManager.SkillNameBluntWeapon,
            SkillManager.SkillNameHandToHand,
            SkillManager.SkillNameLongBlade,
            SkillManager.SkillNameShortBlade,
            SkillManager.SkillNameSpear,
        };

        public RandomizerOptions(
            bool unarmoredIsArmor = true,
            bool marksmanIsWeapon = true,
            bool destructionIsWeapon = false)
        {
            UnarmoredCountsAsArmor = unarmoredIsArmor;
            MarksmanCountsAsWeapon = marksmanIsWeapon;
            DestructionCountsAsWeapon = destructionIsWeapon;

            if (UnarmoredCountsAsArmor)
                WeaponSkills.Add(SkillManager.SkillNameUnarmored);
            if (MarksmanCountsAsWeapon)
                WeaponSkills.Add(SkillManager.SkillNameUnarmored);
            if (DestructionCountsAsWeapon)
                WeaponSkills.Add(SkillManager.SkillNameUnarmored);
        }
    }


    class CharacterRandomizer
    {
        static Dictionary<string, ConsoleColor> SpecColors = new Dictionary<string, ConsoleColor>()
        {
            { "Combat", ConsoleColor.Blue },
            { "Magic", ConsoleColor.Magenta},
            { "Stealth", ConsoleColor.Yellow },
        };
        static Dictionary<string, string> ShortAttributes = new Dictionary<string, string>()
        {
            { CharacterAttribute.Strength, "STR" },
            { CharacterAttribute.Intelligence, "INT"},
            { CharacterAttribute.Willpower, "WIL" },
            { CharacterAttribute.Agility, "AGI" },
            { CharacterAttribute.Speed, "SPD" },
            { CharacterAttribute.Endurance, "END" },
            { CharacterAttribute.Personality, "PRS" },
            { CharacterAttribute.Luck, "LCK" },
        };

        public static RandomCharacter GenerateCharacter(RandomizerOptions options, bool debug = false)
        {
            List<string> attributes = GetCharacterAttributes(options.NumAttributes);
            List<Skill> charSkills = GetCharacterSkills(attributes, options);
            List<(string race, float score)> raceDistro = GetCharacterRaceDistro(attributes, charSkills, options);
            string specialization = GetSpecializaton(charSkills, options);
            string selectedRace = SelectRandomEntry(raceDistro);

            List<(string faction, float score)> factionPrimaryDistro = GetCharacterFactionDistro(attributes, charSkills, options);
            string factionPrimary = SelectRandomEntry(factionPrimaryDistro, out int pIndex);

            List<(string faction, float score)> factionSecondaryDistro = GetCharacterSecondaryFactionDistro(factionPrimaryDistro, pIndex, factionPrimary, options);
            string factionSecondary = SelectRandomEntry(factionSecondaryDistro);

            factionSecondary = SelectRandomEntry(factionSecondaryDistro);

            using (ManagedColorBuilder mcb = new ManagedColorBuilder("CharacterRandomizer", string.Empty, "  "))
            {
                ConsoleColor defaultColor = ConsoleColor.Gray;
                ColorStringBuilder builder = mcb.Builder;

                builder.Append("\nAttributes: ");
                foreach (string attr in attributes)
                    builder.NewlineAppend(1, $"{charSkills.Count(s => s.GoverningAttribute == attr)} x {attr}");

                builder.NewlineAppend("\nMajor Skills: ");
                for (int i = 0; i < options.NumSkills; ++i)
                {
                    if (i == options.NumSkills / 2)
                        builder.NewlineAppend("Minor Skills: ", defaultColor);

                    builder.NewlineAppend(1, $"{ShortAttributes[charSkills[i].GoverningAttribute]} | ", defaultColor);
                    builder.Append(charSkills[i].Name, SpecColors[charSkills[i].Specialization]);
                }

                builder.NewlineAppend("Specialization:");
                builder.NewlineAppend(1, specialization);

                if (debug)
                {
                    void BuildDistroString(List<(string entry, float score)> distro, string selected)
                    {
                        float totalScore = distro.Aggregate(0.0f, (total, tuple) => total + Math.Max(tuple.score, 0.0f));
                        foreach (var tuple in distro)
                        {
                            string tabs = tuple.entry.Length > 12 ? "\t" : "\t\t";
                            float chance = Math.Max(tuple.score, 0.0f) / totalScore;
                            ConsoleColor c = tuple.entry == selected ? ConsoleColor.Yellow : defaultColor;
                            builder.NewlineAppend(1, $"{tuple.entry}:{tabs}{chance.ToString("P")} ({tuple.score})", c);
                        }
                    }

                    builder.NewlineAppend("\nRace Chances:", defaultColor);
                    BuildDistroString(raceDistro, selectedRace);

                    builder.NewlineAppend("\nPrimary Faction Chances:", defaultColor);
                    BuildDistroString(factionPrimaryDistro, factionPrimary);

                    builder.NewlineAppend("\nSecondary Faction Chances:", defaultColor);
                    BuildDistroString(factionSecondaryDistro, factionSecondary);
                }
                else
                {
                    builder.NewlineAppend($"\nRace:", defaultColor);
                    builder.NewlineAppend(1, selectedRace);

                    builder.NewlineAppend($"\nPrimary Faction:", defaultColor);
                    builder.NewlineAppend(1, factionPrimary);

                    builder.NewlineAppend($"\nSecondary Faction:", defaultColor);
                    builder.NewlineAppend(1, factionSecondary);
                }

                builder.WriteLine(true);
            }

            int l = options.NumSkills / 2;
            return new RandomCharacter()
            {
                Race = selectedRace,
                FavoredAttributes = attributes,
                MajorSkills = charSkills.Slice(0, l),
                MinorSkills = charSkills.Slice(l, l),
            };
        }

        static string GetSpecializaton(List<Skill> skills, RandomizerOptions options)
        {
            float most = 0;
            string spec = "Magic";
            Dictionary<string, float> specCount = new();
            for (int i = 0; i < skills.Count; i++)
            {
                Skill skill = skills[i];
                if (!specCount.ContainsKey(skill.Specialization))
                    specCount.Add(skill.Specialization, 0);

                // Specialization scores depend on whether they are in your major or minor slots
                specCount[skill.Specialization] += i < options.NumSkills / 2
                    ? options.MajorSkillSpecWeight
                    : options.MinorSkillSpecWeight;

                if (specCount[skill.Specialization] > most)
                    spec = skill.Specialization;
            }

            return spec;
        }

        static List<string> GetCharacterAttributes(int numAttributes)
        {
            // Select random attributes
            List<string> attributes = SkillManager.SkillsByAttribute.Keys.ToList();
            attributes.Shuffle();
            attributes.RemoveRange(numAttributes, attributes.Count - numAttributes);
            return attributes;
        }

        static List<Skill> GetCharacterSkills(List<string> attributes, RandomizerOptions options)
        {
            int numLocks = 0;
            int numArmors = 0;
            int numWeapons = 0;
            List<Skill> charSkills = new List<Skill>(10);
            List<Skill> attrSkills = new List<Skill>(16);
            List<Skill> otherSkills = new List<Skill>(16);
            Dictionary<string, int> attrCount = attributes.ToDictionary(s => s, s => 0);

            // Split skills into primary and secondary pools
            foreach (string attr in SkillManager.SkillsByAttribute.Keys)
            {
                if (attributes.Contains(attr))
                    attrSkills.AddRange(SkillManager.SkillsByAttribute[attr]);
                else otherSkills.AddRange(SkillManager.SkillsByAttribute[attr]);
            }

            attrSkills.Shuffle();
            otherSkills.Shuffle();

            // Skill validity util.
            bool AtCategoryCapacity(Skill skill)
            {
                return
                    !(numLocks < options.MaxLockSkills || !options.LockSkills.Contains(skill.Name)) &&
                    !(numArmors < options.MaxArmorSkills || !options.ArmorSkills.Contains(skill.Name)) &&
                    !(numWeapons < options.MaxWeaponSkills || !options.WeaponSkills.Contains(skill.Name));
            }
            bool AtAttributeCapacity(Skill skill)
            {
                return
                    options.ForceTrainableAttributes &&
                    attrCount.TryGetValue(skill.GoverningAttribute, out int count) &&
                    count >= SkillManager.SkillsByAttribute[skill.GoverningAttribute].Count - 1;
            }
            bool IsValidSkill(Skill skill)
            {
                if (AtCategoryCapacity(skill))
                {
                    ConsoleExt.WriteWarningLine($"Skipped {skill.Name}: category limit reached");
                    return false;
                }
                if (AtAttributeCapacity(skill))
                {
                    ConsoleExt.WriteWarningLine($"Skipped {skill.Name}: attribute limit reached");
                    return false;
                }
                return true;
            }
            void AddSkill(Skill skill, int index = -1)
            {
                // Record what kind of skill it is
                if (options.LockSkills.Contains(skill.Name))
                    ++numLocks;
                else if (options.ArmorSkills.Contains(skill.Name))
                    ++numArmors;
                else if (options.WeaponSkills.Contains(skill.Name))
                    ++numWeapons;

                if (attrCount.ContainsKey(skill.GoverningAttribute))
                    ++attrCount[skill.GoverningAttribute];

                if (index >= 0 && index < charSkills.Count)
                    charSkills.Insert(index, skill);
                else charSkills.Add(skill);
            }

            // Add skills to final list from primary pool first.
            Console.WriteLine("\nAdding primary skills");
            for (int i = 0; i < attrSkills.Count && charSkills.Count < options.NumSkills; ++i)
            {
                var skill = attrSkills[i];
                if (IsValidSkill(skill))
                {
                    AddSkill(skill);
                    Console.WriteLine($"Adding {skill.Name}");
                }
            }

            // Exit early if we get lucky
            if (charSkills.Count == options.NumSkills)
                return charSkills;

            int missingLocks = options.ForceLockpickingSkill ? Math.Max(1 - numLocks, 0) : 0;
            int missingArmors = options.ForceArmorSkill ? Math.Max(1 - numArmors, 0) : 0;
            int missingWeapons = options.ForceWeaponSkill ? Math.Max(1 - numWeapons, 0) : 0;
            int mReqs = missingLocks + missingArmors + missingWeapons;

            if(charSkills.Count > options.NumSkills - mReqs)
                Console.WriteLine("\nTrimming primary skills");
            while (charSkills.Count > options.NumSkills - mReqs)
            {
                var skill = charSkills.Last();
                charSkills.RemoveAt(charSkills.LastIndex());
                ConsoleExt.WriteWarningLine($"Removed {skill.Name}: need room for required skills");
            }

            bool IsRequired(Skill skill)
            {
                return
                    (options.ForceLockpickingSkill && missingLocks == 0) ||
                    (options.ForceArmorSkill && missingArmors == 0) ||
                    (options.ForceWeaponSkill && missingWeapons == 0);
            }
            bool IsMissing(Skill skill)
            {
                return
                    (missingLocks > 0 && options.LockSkills.Contains(skill.Name)) ||
                    (missingArmors > 0 && options.ArmorSkills.Contains(skill.Name)) ||
                    (missingWeapons > 0 && options.WeaponSkills.Contains(skill.Name));
            }
            bool AddIfMissing(Skill skill, string source)
            {
                if (missingLocks > 0 && options.LockSkills.Contains(skill.Name))
                {
                    AddSkill(skill);
                    --missingLocks;
                    ConsoleExt.WriteWarningLine($"Added {skill.Name} from {source}: required lock skill");
                    return true;
                }
                else if (missingArmors > 0 && options.ArmorSkills.Contains(skill.Name))
                {
                    AddSkill(skill);
                    --missingArmors;
                    ConsoleExt.WriteWarningLine($"Added {skill.Name} from {source}: required armor skill");
                    return true;
                }
                else if (missingWeapons > 0 && options.WeaponSkills.Contains(skill.Name))
                {
                    AddSkill(skill);
                    --missingWeapons;
                    ConsoleExt.WriteWarningLine($"Added {skill.Name} from {source}: required weapon skill");
                    return true;
                }
                return false;
            }
            void RemoveSkill(Skill skill, int index)
            {
                if (options.LockSkills.Contains(skill.Name))
                {
                    --numLocks;
                    ++missingLocks;
                }
                else if (options.ArmorSkills.Contains(skill.Name))
                {
                    --numArmors;
                    ++missingArmors;
                }
                else if (options.WeaponSkills.Contains(skill.Name))
                {
                    --numWeapons;
                    ++missingWeapons;
                }

                --attrCount[skill.GoverningAttribute];
                charSkills.RemoveAt(index);
            }

            // Fill any missing requirements. Start with unused primary skills
            Console.WriteLine("\nFilling missing from primary skills");
            for (int i = attrSkills.Count - 1; i >= 0; --i)
            {
                Skill skill = attrSkills[i];
                if (charSkills.Contains(s => s.Name == skill.Name))
                    continue;
                if (IsMissing(skill))
                {
                    // If the missing skill is of an attribute at capacity, swap it
                    if (AtAttributeCapacity(skill))
                    {
                        for (int j = charSkills.Count - 1; j >= 0; --j)
                        {
                            var cSkill = charSkills[j];
                            if (cSkill.GoverningAttribute == skill.GoverningAttribute && !IsRequired(cSkill))
                            {
                                ConsoleExt.WriteWarningLine($"Swapping {skill.Name} for {cSkill.Name}");
                                RemoveSkill(cSkill, j);
                                AddSkill(skill, j);
                                break;
                            }
                        }
                        continue;
                    }
                    else // Otherwise, just add it
                    {
                        AddSkill(skill);
                        Console.WriteLine($"Adding {skill.Name} from primary: was missing");
                    }
                }
            }

            // Scan over secondary skills for anything missing
            for (int i = 0; i < otherSkills.Count && charSkills.Count < options.NumSkills; i++)
            {
                Skill skill = otherSkills[i];
                AddIfMissing(skill, "secondary");
            }

            // Fill any gaps from the remaining skills
            for (int i = 0; i < otherSkills.Count && charSkills.Count < options.NumSkills; ++i)
            {
                var skill = otherSkills[i];
                if (IsValidSkill(skill))
                {
                    AddSkill(skill);
                    Console.WriteLine($"Adding {skill.Name} from secondary: gap filling");
                }
            }

            return charSkills;
        }

        static List<(string race, float score)> GetCharacterRaceDistro(List<string> attributes, List<Skill> skills, RandomizerOptions options)
        {
            // Build a weighted distrubtion of races based on how well they compliment selected skills
            float totalScores = 0.0f;
            List<(string race, float score)> raceScores = new();
            foreach (CharacterRace race in CharacterRaceManager.RaceList)
            {
                float skillScore = 0;
                float totalScoreMale = 0;
                float totalScoreFemale = 0;

                foreach (var kvp in race.SkillBonuses)
                    if (skills.Contains(s => s.Name == kvp.Key))
                        skillScore += kvp.Value * options.RaceSkillWeight;
                foreach (var kvp in race.AttributeBonusesMale)
                    if (attributes.Contains(kvp.Key))
                        totalScoreMale += kvp.Value * options.RaceAttributeWeight;
                foreach (var kvp in race.AttributeBonusesFemale)
                    if (attributes.Contains(kvp.Key))
                        totalScoreFemale += kvp.Value * options.RaceAttributeWeight;

                totalScoreMale = skillScore + totalScoreMale;
                totalScoreFemale = skillScore + totalScoreFemale;

                raceScores.Add(($"Male {race.Name}", totalScoreMale));
                raceScores.Add(($"Female {race.Name}", totalScoreFemale));

                // Scores < 0 are not added to total and are ignored
                totalScores += Math.Max(totalScoreMale, 0.0f) + Math.Max(totalScoreFemale, 0.0f);
            }

            // Sort scores by descending order
            raceScores.Sort((t1, t2) => -t1.score.CompareTo(t2.score));
            return raceScores;
        }

        static List<(string faction, float score)> GetCharacterFactionDistro(List<string> attributes, List<Skill> skills, RandomizerOptions options)
        {
            // Build a weighted distrubtion of races based on how well they compliment selected skills
            float totalScores = 0.0f;
            List<(string faction, float score)> factionScores = new();
            foreach (Faction faction in FactionManager.FactionList)
            {
                float factionScore = 0;

                if (!faction.IsJoinable || faction.MainQuestRequired)
                    continue;

                foreach (string skill in faction.FavoredSkills)
                    if (skills.Contains(s => s.Name == skill))
                        factionScore += 1.0f * options.FactionSkillWeight;
                foreach (string attr in faction.FavoredAttributes)
                    if (attributes.Contains(attr))
                        factionScore += 1.0f * options.FactionAttributeWeight;

                if (factionScore < options.MinimumFactionScore)
                {
                    //ConsoleExt.WriteWarningLine($"Faction {faction.Name} excluded. Score was {factionScore}");
                    factionScore = -factionScore;
                }

                // Scores < 0 are not added to total and are ignored
                factionScores.Add((faction.Name, factionScore));
                totalScores += Math.Max(factionScore, 0.0f);
            }

            // Sort scores by descending order
            factionScores.Sort((t1, t2) => -t1.score.CompareTo(t2.score));
            return factionScores;
        }
        static List<(string faction, float score)> GetCharacterSecondaryFactionDistro(List<(string faction, float score)> primaryDistro, int pIndex, string primary, RandomizerOptions options)
        {
            List<(string faction, float score)> factionSecondaryDistro = new(primaryDistro);
            Faction pFaction = FactionManager.FactionDict[primary];

            factionSecondaryDistro.RemoveAt(pIndex);
            for (int i = 0; i < factionSecondaryDistro.Count; ++i)
            {
                var tuple = factionSecondaryDistro[i];
                if (pFaction.IncompatibleFactions.Contains(tuple.faction))
                {
                    factionSecondaryDistro.RemoveAt(i--);
                    ConsoleExt.WriteWarningLine($"Removed {tuple.faction}: Incompatible");
                }
                else if (pFaction.AdvancedPairings.Contains(tuple.faction) && !options.AllowAdvancedFactionPairs)
                {
                    factionSecondaryDistro.RemoveAt(i--);
                    ConsoleExt.WriteWarningLine($"Removed {tuple.faction}: Requires advanced pairings");
                }
            }

            return factionSecondaryDistro;
        }

        static string SelectRandomEntry(List<(string entry, float score)> distribution)
            => SelectRandomEntry(distribution, out int swallow);
        static string SelectRandomEntry(List<(string entry, float score)> distribution, out int index)
        {
            float chance = 0.0f;
            float totalScore = distribution.Aggregate(0.0f, (total, tuple) => total + Math.Max(tuple.score, 0.0f));
            double r = CommandEngine.Random.NormalDouble;
            string selectedRace = string.Empty;
            for (index = 0; index < distribution.Count; index++)
            {
                chance += distribution[index].score / totalScore;
                if (r < chance)
                {
                    selectedRace = distribution[index].entry;
                    break;
                }
            }

            return selectedRace;
        }
        static void TestDistribution(List<(string entry, float score)> distribution)
        {
            int totalPulls = 100000;
            Dictionary<int, int> pullsPerIndex = new Dictionary<int, int>();

            for (int i = 0; i < distribution.Count; ++i)
                pullsPerIndex.Add(i, 0);

            for (int i = 0; i < totalPulls; ++i)
            {
                string selected = SelectRandomEntry(distribution, out int index);
                pullsPerIndex[index] += 1;
            }

            using (ManagedColorBuilder mcb = new ManagedColorBuilder("DistroTesting", string.Empty, "  "))
            {
                ConsoleColor defaultColor = ConsoleColor.Gray;
                ColorStringBuilder builder = mcb.Builder;

                int actualPulls = 0;
                int expectedPulls = 0;
                float totalChance = 0.0f;
                float totalScore = distribution.Aggregate(0.0f, (total, tuple) => total + Math.Max(tuple.score, 0.0f));
                builder.NewlineAppend("Chances:", defaultColor);
                for (int i = 0; i < distribution.Count; i++)
                {
                    var tuple = distribution[i];

                    if (pullsPerIndex[i] <= 0 && tuple.score <= 0)
                        continue;

                    string tabs = tuple.entry.Length > 12 ? "\t" : "\t\t";
                    float expected = Math.Max(tuple.score, 0.0f) / totalScore;
                    float actual = pullsPerIndex[i] / (float)totalPulls;
                    int ePulls = (int)(expected * totalPulls);

                    expectedPulls += ePulls;
                    actualPulls += pullsPerIndex[i];
                    totalChance += expected;

                    builder.NewlineAppend(1, $"{tuple.entry}:{tabs}E: {expected.ToString("P")} -> A: {actual.ToString("P")} ({pullsPerIndex[i]}/{ePulls})", defaultColor);

                    if (expected != actual)
                    {
                        float diff = Math.Abs(expected - actual);
                        bool higher = actual >= expected;
                        builder.Append($" {(higher ? '+' : '-')}{diff.ToString("P")}", higher ? ConsoleColor.Green : ConsoleColor.Red);
                    }
                }
                builder.NewlineAppend(1, $"Pulls: E {expectedPulls}  A {actualPulls}", defaultColor);
                builder.NewlineAppend(1, $"Totals: S {totalScore}  C {totalChance.ToString("P")}", defaultColor);
                builder.WriteLine(true);
            }
        }
    }
}
