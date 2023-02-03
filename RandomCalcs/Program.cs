using CommandEngine;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomCalcs
{
    class SeraphStaff
    {
        public string name;
        public int baseDamage;
        public float baseCritDamage;
        public float baseCritChance;
        public float baseDamageMult = 1.0f;
        public float baseSpeed;
    }

    class SeraphUpgrade
    {
        public string name;
        public float increasePerLevel;
        public float baseValue = 0.0f;
        Dictionary<int, int> levels = new Dictionary<int, int>();

        public void AddUpgradeLevel(int level)
        {
            if (levels.ContainsKey(level))
                ++levels[level];
            else levels.Add(level, 1);
        }
        public void AddUpgradeLevels(int level, int amount)
        {
            if (levels.ContainsKey(level))
                levels[level] += amount;
            else levels.Add(level, amount);
        }
        public float GetUpgradeValue(float tomeValue)
        {
            int tLevels = 0;
            float final = 0;
            foreach (var kvp in levels)
            {
                final += kvp.Key * kvp.Value * increasePerLevel * (kvp.Key == 1 ? tomeValue : 1.0f);
                tLevels += kvp.Key * kvp.Value;
            }
            return final + (tLevels > 0 ? baseValue : 0);
        }
    }

    class Program
    {
        public static SeraphUpgrade damageCard = new SeraphUpgrade()
        {
            name = "Damage",
            increasePerLevel = 2,
            baseValue = 0
        };
        public static SeraphUpgrade attackSpeedCard = new SeraphUpgrade()
        {
            name = "Attack Speed",
            increasePerLevel = .12f,
            baseValue = 1.0f
        };
        public static SeraphUpgrade critChanceCard = new SeraphUpgrade()
        {
            name = "Crit Chance",
            increasePerLevel = .05f,
            baseValue = 0.0f
        };
        public static SeraphUpgrade critDamageCard = new SeraphUpgrade()
        {
            name = "Crit Damage",
            increasePerLevel = .5f / 2,
            baseValue = 0
        };
        public static SeraphUpgrade tomeCard = new SeraphUpgrade()
        {
            name = "Tome",
            increasePerLevel = 0.35f / 3,
            baseValue = 1.0f
        };

        public static SeraphStaff wizardStaff = new SeraphStaff
        {
            name = "Wizard Staff",
            baseDamage = 4,
            baseDamageMult = 1.0f,
            baseCritDamage = 1.5f,
            baseCritChance = 0.0f,
            baseSpeed = 1.0f
        };
        public static SeraphStaff emeraldStaff = new SeraphStaff()
        {
            name = "Emerald Staff",
            baseDamage = 4,
            baseDamageMult = 0.5f,
            baseCritDamage = 1.5f,
            baseCritChance = 0.0f,
            baseSpeed = 2.0f
        };

        static CommandModule seraphCommands = new CommandModule("Enter Seraph Command");
        
        static void Main(string[] args)
        {
            seraphCommands.Add(new ConsoleCommand("dps", CalcDPS));

            while (true)
                seraphCommands.NextCommand(true);
        }

        static void CalcDPS(List<string> args)
        {
            if (args.Count < 1)
            {
                ConsoleExt.WriteErrorLine("Must provide at least one staff");
                return;
            }

            int tabCount = 0;
            SeraphStaff staff = null;
            SmartStringBuilder msg = new SmartStringBuilder("  ");

            switch(args[0])
            {
                case "emerald":
                    staff = emeraldStaff; break;
                default:
                    staff = wizardStaff; break;
            }

            bool AddLevels(SeraphUpgrade card, string levelStr, string amountStr)
            {
                if (int.TryParse(levelStr, out int level) && int.TryParse(amountStr, out int amount))
                {
                    card.AddUpgradeLevels(level, amount);
                    return true;
                }

                ConsoleExt.WriteWarningLine($"Failed to parse values for card '{card.name}'.");
                return false;
            }

            for(int i = 1; i < args.Count; ++i)
            {
                switch(args[i])
                {
                    case "-d":
                        if (args.Count - i <= 2)
                        {
                            i += 2;
                            ConsoleExt.WriteWarningLine("Skipped damage upgrade, not enough args.");
                            break;
                        }
                        AddLevels(damageCard, args[++i], args[++i]);
                        break;
                    case "-cc":
                        if (args.Count - i <= 2)
                        {
                            i += 2;
                            ConsoleExt.WriteWarningLine("Skipped crit chance upgrade, not enough args.");
                            break;
                        }
                        AddLevels(critChanceCard, args[++i], args[++i]);
                        break;
                    case "-cd":
                        if (args.Count - i <= 2)
                        {
                            i += 2;
                            ConsoleExt.WriteWarningLine("Skipped crit damage upgrade, not enough args.");
                            break;
                        }
                        AddLevels(critDamageCard, args[++i], args[++i]);
                        break;
                    case "-s":
                        if (args.Count - i <= 2)
                        {
                            i += 2;
                            ConsoleExt.WriteWarningLine("Skipped attack speed upgrade, not enough args.");
                            break;
                        }
                        AddLevels(attackSpeedCard, args[++i], args[++i]);
                        break;
                    case "-t":
                        if (args.Count - i <= 1)
                        {
                            i += 2;
                            ConsoleExt.WriteWarningLine("Skipped damage upgrade, not enough args.");
                            break;
                        }
                        AddLevels(damageCard, "3", args[++i]);
                        break;
                    default:
                        ConsoleExt.WriteWarningLine($"Unknown upgrade switch {args[i]}");
                        break;
                }
            }

            float tomeValue = Math.Max(1.0f, tomeCard.GetUpgradeValue(1.0f));
            float tDamage = staff.baseDamage         + damageCard.GetUpgradeValue(tomeValue);
            float tSpeed = staff.baseSpeed           + attackSpeedCard.GetUpgradeValue(tomeValue);
            float tCritChance = staff.baseCritChance + critChanceCard.GetUpgradeValue(tomeValue);
            float tCritDamage = staff.baseCritDamage + critDamageCard.GetUpgradeValue(tomeValue);

            float dps = tDamage * tSpeed * staff.baseDamageMult;
            float critDps = dps * tCritDamage * Math.Min(1.0f, tCritChance);

            msg.AppendNewline(tabCount, $"{staff.name} dps:");
            ++tabCount;
            msg.AppendNewline(tabCount, $"Base: {dps}");
            msg.AppendNewline(tabCount, $"Crit: {critDps}");
            msg.AppendNewline(tabCount, $"Upgrades:");
            ++tabCount;
            msg.AppendNewline(tabCount, $"    Damage: +{damageCard.GetUpgradeValue(tomeValue)}");
            msg.AppendNewline(tabCount, $"     Speed: +{attackSpeedCard.GetUpgradeValue(tomeValue)}");
            msg.AppendNewline(tabCount, $"CritChance: +{critChanceCard.GetUpgradeValue(tomeValue)}");
            msg.AppendNewline(tabCount, $"CritDamage: +{critDamageCard.GetUpgradeValue(tomeValue)}");
            msg.AppendNewline(tabCount, $"     Tomes: +{tomeCard.GetUpgradeValue(1.0f)}");
            --tabCount;
            --tabCount;

            Console.WriteLine(msg.ToString());
        }
    }
}
