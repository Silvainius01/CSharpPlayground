using CommandEngine;
using CommandEngine.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Morrowind.Data
{
    class SpellEffectManager : TypeManager<SpellEffect, SpellEffectManager>
    {
        public static List<SpellEffect> SpellEffectList { get; private set; } = new();
        public static Dictionary<string, SpellEffect> SpellEffectDict { get; private set; } = new();

        protected override void AddTypeEntry(SpellEffect item)
        {
            if (!SpellEffectDict.ContainsKey(item.Name))
            {
                SpellEffectList.Add(item);
                SpellEffectDict.Add(item.Name, item);
            }
            else ConsoleExt.WriteWarningLine($"SpellEffect with name {item.Name} already exists.");
        }

        protected override string GetDataPath()
            => $"{Directory.GetCurrentDirectory()}\\Data\\SpellEffects.json";

        protected override List<SpellEffect> GetDefaultTypesInternal()
        {
            if (_IsLoaded)
                return SpellEffectList;

            // Damage attribute base = 8
            // Drain attribute base = 1
            // Restore attribute base = 8
            // Fortify attribute base = 1

            // Drain skill base = 1

            List<SpellEffect> SpellEffects = new List<SpellEffect>(128)
            {
                new SpellEffect("Blind") { BaseCost = 1, IsNegative = true },
                new SpellEffect("Burden") { BaseCost = 1, IsNegative = true },
                new SpellEffect("Cure Blight Disease") { BaseCost = 2000, IsNegative = false },
                new SpellEffect("Cure Common Disease") { BaseCost = 300, IsNegative = false },
                new SpellEffect("Cure Paralyzation") { BaseCost = 100, IsNegative = false },
                new SpellEffect("Cure Poison") { BaseCost = 100, IsNegative = false },
                new SpellEffect("Damage Health") { BaseCost = 8, IsNegative = true },
                new SpellEffect("Damage Fatigue") { BaseCost = 4, IsNegative = true },
                new SpellEffect("Damage Magicka") { BaseCost = 8, IsNegative = true },
                new SpellEffect("Damage Intelligence") { BaseCost = 8, IsNegative = true },
                new SpellEffect("Detect Animal") { BaseCost = 0.75f, IsNegative = false },
                new SpellEffect("Detect Enchantment") { BaseCost = 1, IsNegative = false },
                new SpellEffect("Detect Key") { BaseCost = 1, IsNegative = false },
                new SpellEffect("Dispel") { BaseCost = 5, IsNegative = false },
                new SpellEffect("Drain Health") { BaseCost = 4, IsNegative = true },
                new SpellEffect("Drain Fatigue") { BaseCost = 2, IsNegative = true },
                new SpellEffect("Drain Magicka") { BaseCost = 4, IsNegative = true },
                new SpellEffect("Drain Alteration") { BaseCost = 1, IsNegative = true },
                new SpellEffect("Drain Agility") { BaseCost = 1, IsNegative = true },
                new SpellEffect("Drain Endurance") { BaseCost = 1, IsNegative = true },
                new SpellEffect("Drain Intelligence") { BaseCost = 1, IsNegative = true },
                new SpellEffect("Drain Luck") { BaseCost = 1, IsNegative = true },
                new SpellEffect("Drain Personality") { BaseCost = 1, IsNegative = true },
                new SpellEffect("Drain Speed") { BaseCost = 1, IsNegative = true },
                new SpellEffect("Drain Strength") { BaseCost = 1, IsNegative = true },
                new SpellEffect("Drain Willpower") { BaseCost = 1, IsNegative = true },
                new SpellEffect("Feather") { BaseCost = 1, IsNegative = false },
                new SpellEffect("Fire Shield") { BaseCost = 3, IsNegative = false },
                new SpellEffect("Fortify Health") { BaseCost = 1, IsNegative = false },
                new SpellEffect("Fortify Fatigue") { BaseCost = 0.5f, IsNegative = false },
                new SpellEffect("Fortify Magicka") { BaseCost = 1, IsNegative = false },
                new SpellEffect("Fortify Maximum Magicka") {BaseCost = 4, IsNegative = false },
                new SpellEffect("Fortify Attack") { BaseCost = 1, IsNegative = false },
                new SpellEffect("Fortify Agility") { BaseCost = 1, IsNegative = false },
                new SpellEffect("Fortify Endurance") { BaseCost = 1, IsNegative = false },
                new SpellEffect("Fortify Intelligence") { BaseCost = 1, IsNegative = false },
                new SpellEffect("Fortify Luck") { BaseCost = 1, IsNegative = false },
                new SpellEffect("Fortify Personality") { BaseCost = 1, IsNegative = false },
                new SpellEffect("Fortify Speed") { BaseCost = 1, IsNegative = false },
                new SpellEffect("Fortify Strength") { BaseCost = 1, IsNegative = false },
                new SpellEffect("Fortify Willpower") { BaseCost = 1, IsNegative = false },
                new SpellEffect("Frost Damage") { BaseCost = 5, IsNegative = true },
                new SpellEffect("Frost Shield") { BaseCost = 3, IsNegative = true },
                new SpellEffect("Invisibility") { BaseCost = 20, IsNegative = false },
                new SpellEffect("Levitate") { BaseCost = 3, IsNegative = false },
                new SpellEffect("Light") { BaseCost = 0.2f, IsNegative = false },
                new SpellEffect("Lightning Shield") { BaseCost = 3, IsNegative = false },
                new SpellEffect("Night Eye") { BaseCost = 0.2f, IsNegative = false },
                new SpellEffect("Paralyze") { BaseCost = 40, IsNegative = true },
                new SpellEffect("Poison") { BaseCost = 9, IsNegative = true },
                new SpellEffect("Recall") { BaseCost = 350, IsNegative = false },
                new SpellEffect("Reflect") { BaseCost = 10, IsNegative = false },
                new SpellEffect("Resist Common Disease") { BaseCost = 2, IsNegative = false },
                new SpellEffect("Resist Fire") { BaseCost = 2, IsNegative = false },
                new SpellEffect("Resist Frost") { BaseCost = 2, IsNegative = false },
                new SpellEffect("Resist Poison") { BaseCost = 2, IsNegative = false },
                new SpellEffect("Resist Shock") { BaseCost = 2, IsNegative = false },
                new SpellEffect("Resist Magicka") { BaseCost = 2, IsNegative = false },
                new SpellEffect("Resist Paralysis") { BaseCost = 0.2f, IsNegative = false },
                new SpellEffect("Restore Health") { BaseCost = 5, IsNegative = false },
                new SpellEffect("Restore Fatigue") { BaseCost = 1, IsNegative = false },
                new SpellEffect("Restore Magicka") { BaseCost = 5, IsNegative = false },
                new SpellEffect("Restore Agility") { BaseCost = 1, IsNegative = false },
                new SpellEffect("Restore Endurance") { BaseCost = 1, IsNegative = false },
                new SpellEffect("Restore Intelligence") { BaseCost = 1, IsNegative = false },
                new SpellEffect("Restore Luck") { BaseCost = 1, IsNegative = false },
                new SpellEffect("Restore Personality") { BaseCost = 1, IsNegative = false },
                new SpellEffect("Restore Speed") { BaseCost = 1, IsNegative = false },
                new SpellEffect("Restore Strength") { BaseCost = 1, IsNegative = false },
                new SpellEffect("Restore Willpower") { BaseCost = 1, IsNegative = false },
                new SpellEffect("Spell Absorption") { BaseCost = 10, IsNegative = false },
                new SpellEffect("Swift Swim") { BaseCost = 2, IsNegative = false },
                new SpellEffect("Telekinesis") { BaseCost = 1, IsNegative = false },
                new SpellEffect("Vampirism") { BaseCost = 5, IsNegative = true },
                new SpellEffect("Water Breathing") { BaseCost = 3, IsNegative = false },
                new SpellEffect("Water Walking") { BaseCost = 3, IsNegative = false },
                new SpellEffect("Weakness to Fire") { BaseCost = 3, IsNegative = false },
                new SpellEffect("Weakness to Poison") { BaseCost = 3, IsNegative = false },
            };

            return SpellEffects;
        }
    }
}
