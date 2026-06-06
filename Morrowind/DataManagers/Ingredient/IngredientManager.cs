using CommandEngine;
using CommandEngine.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Morrowind.Data
{
    class IngredientManager : TypeManager<Ingredient, IngredientManager>
    {
        public static List<Ingredient> IngredientList { get; private set; } = new List<Ingredient>();
        public static Dictionary<string, Ingredient> IngredientDict { get; private set; } = new Dictionary<string, Ingredient>();

        protected override void AddTypeEntry(Ingredient item)
        {
            if (!IngredientDict.ContainsKey(item.Name))
            {
                IngredientList.Add(item);
                IngredientDict.Add(item.Name, item);
            }
            else ConsoleExt.WriteWarningLine($"Ingredient with name {item.Name} already exists.");
        }

        protected override string GetDataPath()
            => $"{Directory.GetCurrentDirectory()}\\Data\\Ingredients.json";

        protected override List<Ingredient> GetDefaultTypesInternal()
        {
            if (_IsLoaded)
                return IngredientList;

            List<Ingredient> ingredients = new List<Ingredient>(128)
            {
                new Ingredient("Alit Hide", "Morrowind")
                {
                    Value = 5,
                    Weight = 1,
                    Effects = new List<string>(4)
                    {
                        "Drain Intelligence",
                        "Resist Poison",
                        "Telekinesis",
                        "Detect Animal"
                    }
                },
                new Ingredient("Ampoule Pod", "Morrowind")
                {
                    Value = 2,
                    Weight = 0.1f,
                    Effects = new List<string>(4)
                    {
                        "Water Walking",
                        "Paralyze",
                        "Detect Animal",
                        "Drain Willpower",
                    }
                },
                new Ingredient("Ash Salts", "Morrowind")
                {
                    Value = 25,
                    Weight = 0.1f,
                    Effects = new List<string>(4)
                    {
                        "Drain Agility",
                        "Resist Magicka",
                        "Cure Blight Disease",
                        "Resist Magicka",
                    }
                },
                new Ingredient("Ash Yam", "Morrowind")
                {
                    Value = 1,
                    Weight = 0.5f,
                    Effects = new List<string>(4)
                    {
                        "Fortify Intelligence",
                        "Fortify Strength",
                        "Resist Common Disease",
                        "Detect Key",
                    }
                },
                new Ingredient("Bittergreen Petals", "Morrowind")
                {
                    Value = 5,
                    Weight = 0.1f,
                    Effects = new List<string>(4)
                    {
                        "Restore Intelligence",
                        "Invisibility",
                        "Drain Endurance",
                        "Drain Magicka",
                    }
                },
                new Ingredient("Black Anther", "Morrowind")
                {
                    Value = 2,
                    Weight = 0.1f,
                    Effects = new List<string>(4)
                    {
                        "Drain Agility",
                        "Resist Fire",
                        "Drain Endurance",
                        "Light",
                    }
                },
                new Ingredient("Black Lichen", "Morrowind")
                {
                    Value = 2,
                    Weight = 0.1f,
                    Effects = new List<string>(4)
                    {
                        "Drain Strength",
                        "Resist Frost",
                        "Drain Speed",
                        "Cure Poison",
                    }
                },
                new Ingredient("Bloat", "Morrowind")
                {
                    Value = 5,
                    Weight = 0.1f,
                    Effects = new List<string>(4)
                    {
                        "Drain Magicka",
                        "Fortify Intelligence",
                        "Fortify Willpower",
                        "Detect Animal",
                    }
                },
                new Ingredient("Bonemeal", "Morrowind")
                {
                    Value = 2,
                    Weight = 0.2f,
                    Effects = new List<string>(4)
                    {
                        "Restore Agility",
                        "Telekinesis",
                        "Drain Fatigue",
                        "Drain Personality",
                    }
                },
                new Ingredient("Bread", "Morrowind")
                {
                    Value = 1,
                    Weight = 0.1f,
                    Effects = new List<string>(4)
                    {
                        "Restore Fatigue",
                    }
                },
                new Ingredient("Bungler's Bane", "Morrowind")
                {
                    Value = 1,
                    Weight = 0.5f,
                    Effects = new List<string>(4)
                    {
                        "Drain Speed",
                        "Drain Endurance",
                        "Dispel",
                        "Drain Strength",
                    }
                },
                new Ingredient("Chokeweed", "Morrowind")
                {
                    Value = 1,
                    Weight = 0.1f,
                    Effects = new List<string>(4)
                    {
                        "Drain Luck",
                        "Restore Fatigue",
                        "Cure Common Disease",
                        "Drain Willpower",
                    }
                },
                new Ingredient("Coda Flower", "Morrowind")
                {
                    Value = 23,
                    Weight = 0.1f,
                    Effects = new List<string>(4)
                    {
                        "Drain Personality",
                        "Levitate",
                        "Drain Intelligence",
                        "Drain Health",
                    }
                },
                new Ingredient("Comberry", "Morrowind")
                {
                    Value = 2,
                    Weight = 0.1f,
                    Effects = new List<string>(4)
                    {
                        "Drain Fatigue",
                        "Restore Magicka",
                        "Fire Shield",
                        "Reflect",
                    }
                },
                new Ingredient("Corkbulb Root", "Morrowind")
                {
                    Value = 5,
                    Weight = 0.1f,
                    Effects = new List<string>(4)
                    {
                        "Cure Paralyzation",
                        "Restore Health",
                        "Lightning Shield",
                        "Fortify Luck",
                    }
                },
                new Ingredient("Corprus Weepings", "Morrowind")
                {
                    Value = 50,
                    Weight = 0.1f,
                    Effects = new List<string>(4)
                    {
                        "Drain Fatigue",
                        "Fortify Luck",
                        "Drain Willpower",
                        "Restore Health",
                    }
                },
                new Ingredient("Crab Meat", "Morrowind")
                {
                    Value = 1,
                    Weight = 0.5f,
                    Effects = new List<string>(4)
                    {
                        "Restore Fatigue",
                        "Resist Shock",
                        "Lightning Shield",
                        "Restore Luck",
                    }
                },
                new Ingredient("Daedra Skin", "Morrowind")
                {
                    Value = 200,
                    Weight = 0.2f,
                    Effects = new List<string>(4)
                    {
                        "Fortify Strength",
                        "Cure Common Disease",
                        "Paralyze",
                        "Swift Swim",
                    }
                },
                new Ingredient("Daedra's Heart", "Morrowind")
                {
                    Value = 200,
                    Weight = 1.0f,
                    Effects = new List<string>(4)
                    {
                        "Restore Magicka",
                        "Fortify Endurance",
                        "Drain Agility",
                        "Night Eye",
                    }
                },
                new Ingredient("Diamond", "Morrowind")
                {
                    Value = 250,
                    Weight = 0.1f,
                    Effects = new List<string>(4)
                    {
                        "Drain Agility",
                        "Invisibility",
                        "Reflect",
                        "Detect Key",
                    }
                },
                new Ingredient("Dreugh Wax", "Morrowind")
                {
                    Value = 100,
                    Weight = 0.2f,
                    Effects = new List<string>(4)
                    {
                        "Fortify Strength",
                        "Restore Strength",
                        "Drain Luck",
                        "Drain Willpower",
                    }
                },
                new Ingredient("Ectoplasm", "Morrowind")
                {
                    Value = 10,
                    Weight = 0.1f,
                    Effects = new List<string>(4)
                    {
                        "Fortify Agility",
                        "Detect Animal",
                        "Drain Strength",
                        "Drain Health",
                    }
                },
                new Ingredient("Emerald", "Morrowind")
                {
                    Value = 150,
                    Weight = 0.2f,
                    Effects = new List<string>(4)
                    {
                        "Fortify Magicka",
                        "Restore Health",
                        "Drain Agility",
                        "Drain Endurance",
                    }
                },
                new Ingredient("Fire Petal", "Morrowind")
                {
                    Value = 2,
                    Weight = 0.1f,
                    Effects = new List<string>(4)
                    {
                        "Resist Fire",
                        "Drain Health",
                        "Spell Absorption",
                        "Paralyze",
                    }
                },
                new Ingredient("Fire Salts", "Morrowind")
                {
                    Value = 100,
                    Weight = 0.1f,
                    Effects = new List<string>(4)
                    {
                        "Drain Health",
                        "Fortify Agility",
                        "Resist Frost",
                        "Fire Shield",
                    }
                },
                new Ingredient("Frost Salts", "Morrowind")
                {
                    Value = 75,
                    Weight = 0.1f,
                    Effects = new List<string>(4)
                    {
                        "Drain Speed",
                        "Restore Magicka",
                        "Frost Shield",
                        "Resist Fire",
                    }
                },
                new Ingredient("Ghoul Heart", "Morrowind")
                {
                    Value = 150,
                    Weight = 0.5f,
                    Effects = new List<string>(4)
                    {
                        "Paralyze",
                        "Cure Poison",
                        "Fortify Attack",
                    }
                },
                new Ingredient("Gold Kanet", "Morrowind")
                {
                    Value = 5,
                    Weight = 0.1f,
                    Effects = new List<string>(4)
                    {
                        "Drain Health",
                        "Burden",
                        "Drain Luck",
                        "Restore Strength",
                    }
                },
                new Ingredient("Gravedust", "Morrowind")
                {
                    Value = 1,
                    Weight = 0.1f,
                    Effects = new List<string>(4)
                    {
                        "Drain Intelligence",
                        "Cure Common Disease",
                        "Drain Magicka",
                        "Restore Endurance",
                    }
                },
                new Ingredient("Green Lichen", "Morrowind")
                {
                    Value = 1,
                    Weight = 0.1f,
                    Effects = new List<string>(4)
                    {
                        "Fortify Personality",
                        "Cure Common Disease",
                        "Drain Strength",
                        "Drain Health",
                    }
                },
                new Ingredient("Guar Hide", "Morrowind")
                {
                    Value = 5,
                    Weight = 1,
                    Effects = new List<string>(4)
                    {
                        "Drain Fatigue",
                        "Fortify Endurance",
                        "Restore Personality",
                        "Fortify Luck",
                    }
                },
                new Ingredient("Hackle-Lo Leaf", "Morrowind")
                {
                    Value = 30,
                    Weight = 0.1f,
                    Effects = new List<string>(4)
                    {
                        "Restore Fatigue",
                        "Paralyze",
                        "Water Breathing",
                        "Restore Luck",
                    }
                },
                new Ingredient("Heather", "Morrowind")
                {
                    Value = 1,
                    Weight = 0.1f,
                    Effects = new List<string>(4)
                    {
                        "Restore Personality",
                        "Feather",
                        "Drain Speed",
                        "Drain Personality",
                    }
                },
                new Ingredient("Hound Meat", "Morrowind")
                {
                    Value = 2,
                    Weight = 1,
                    Effects = new List<string>(4)
                    {
                        "Resotre Fatigue",
                        "Fortify Fatigue",
                        "Reflect",
                        "Detect Enchantment",
                    }
                },
                new Ingredient("Hypha Facia", "Morrowind")
                {
                    Value = 1,
                    Weight = 0.1f,
                    Effects = new List<string>(4)
                    {
                        "Drain Luck",
                        "Drain Agility",
                        "Drain Fatigue",
                        "Detect Enchantment",
                    }
                },
                new Ingredient("Kagouti Hide", "Morrowind")
                {
                    Value = 2,
                    Weight = 1,
                    Effects = new List<string>(4)
                    {
                        "Drain Fatigue",
                        "Fortify Speed",
                        "Resist Common Disease",
                        "Night Eye",
                    }
                },
                new Ingredient("Kresh Fiber", "Morrowind")
                {
                    Value = 1,
                    Weight = 0.1f,
                    Effects = new List<string>(4)
                    {
                        "Restore Luck",
                        "Fortify Personality",
                        "Drain Magicka",
                        "Drain Speed",
                    }
                },
                new Ingredient("Kwama Cuttle", "Morrowind")
                {
                    Value = 2,
                    Weight = 0.1f,
                    Effects = new List<string>(4)
                    {
                        "Resist Poison",
                        "Drain Fatigue",
                        "Water Walking",
                        "Water Breathing",
                    }
                },
                new Ingredient("Large Kwama Egg", "Morrowind")
                {
                    Value = 2,
                    Weight = 2,
                    Effects = new List<string>(4)
                    {
                        "Restore Fatigue",
                        "Paralyze",
                        "Frost Shield",
                        "Fortify Health",
                    }
                },
                new Ingredient("Luminous Russula", "Morrowind")
                {
                    Value = 1,
                    Weight = 0.2f,
                    Effects = new List<string>(4)
                    {
                        "Water Breathing",
                        "Drain Fatigue",
                        "Poison",
                    }
                },
                new Ingredient("Marshmerrow", "Morrowind")
                {
                    Value = 1,
                    Weight = 0.1f,
                    Effects = new List<string>(4)
                    {
                        "Restore Health",
                        "Detect Enchantment",
                        "Drain Willpower",
                        "Drain Fatigue",
                    }
                },
                new Ingredient("Moon Sugar", "Morrowind")
                {
                    Value = 50,
                    Weight = 0.1f,
                    Effects = new List<string>(4)
                    {
                        "Fortify Speed",
                        "Dispel",
                        "Drain Endurance",
                        "Drain Luck",
                    }
                },
                new Ingredient("Muck", "Morrowind")
                {
                    Value = 1,
                    Weight = 0.1f,
                    Effects = new List<string>(4)
                    {
                        "Drain Intelligence",
                        "Detect Key",
                        "Drain Personality",
                        "Cure Common Disease",
                    }
                },
                new Ingredient("Netch Leather", "Morrowind")
                {
                    Value = 1,
                    Weight = 1,
                    Effects = new List<string>(4)
                    {
                        "Fortify Endurance",
                        "Fortify Intelligence",
                        "Drain Personality",
                        "Cure Paralyzation",
                    }
                },
                new Ingredient("Pearl", "Morrowind")
                {
                    Value = 100,
                    Weight = 0.1f,
                    Effects = new List<string>(4)
                    {
                        "Drain Agility",
                        "Dispel",
                        "Water Breathing",
                        "Resist Common Disease",
                    }
                },
                new Ingredient("Racer Plumes", "Morrowind")
                {
                    Value = 20,
                    Weight = 0.1f,
                    Effects = new List<string>(4)
                    {
                        "Drain Willpower",
                        "Levitate",
                    }
                },
                new Ingredient("Rat Meat", "Morrowind")
                {
                    Value = 1,
                    Weight = 1,
                    Effects = new List<string>(4)
                    {
                        "Drain Magicka",
                        "Paralyze",
                        "Cure Poison",
                        "Resist Poison",
                    }
                },
                new Ingredient("Raw Ebony", "Morrowind")
                {
                    Value = 200,
                    Weight = 10,
                    Effects = new List<string>(4)
                    {
                        "Drain Agility",
                        "Cure Poison",
                        "Frost Shield",
                        "Restore Speed",
                    }
                },
                new Ingredient("Raw Glass", "Morrowind")
                {
                    Value = 200,
                    Weight = 2,
                    Effects = new List<string>(4)
                    {
                        "Drain Intelligence",
                        "Drain Strength",
                        "Drain Speed",
                        "Fire Shield",
                    }
                },
                new Ingredient("Red Lichen", "Morrowind")
                {
                    Value = 25,
                    Weight = 0.1f,
                    Effects = new List<string>(4)
                    {
                        "Drain Speed",
                        "Light",
                        "Cure Common Disease",
                        "Drain Magicka",
                    }
                },
                new Ingredient("Resin", "Morrowind")
                {
                    Value = 10,
                    Weight = 0.1f,
                    Effects = new List<string>(4)
                    {
                        "Restore Health",
                        "Restore Speed",
                        "Burden",
                        "Resist Common Disease",
                    }
                },
                new Ingredient("Roobrush", "Morrowind")
                {
                    Value = 1,
                    Weight = 0.1f,
                    Effects = new List<string>(4)
                    {
                        "Drain Willpower",
                        "Fortify Agility",
                        "Drain Health",
                        "Cure Poison",
                    }
                },
                new Ingredient("Ruby", "Morrowind")
                {
                    Value = 200,
                    Weight = 0.2f,
                    Effects = new List<string>(4)
                    {
                        "Drain Health",
                        "Feather",
                        "Restore Intelligence",
                        "Drain Agility",
                    }
                },
                new Ingredient("Saltrice", "Morrowind")
                {
                    Value = 1,
                    Weight = 0.1f,
                    Effects = new List<string>(4)
                    {
                        "Restore Fatigue",
                        "Fortify Magicka",
                        "Drain Strength",
                        "Restore Health",
                    }
                },
                new Ingredient("Scales", "Morrowind")
                {
                    Value = 2,
                    Weight = 0.2f,
                    Effects = new List<string>(4)
                    {
                        "Drain Personality",
                        "Water Walking",
                        "Restore Endurance",
                        "Swift Swim",
                    }
                },
                new Ingredient("Scamp Skin", "Morrowind")
                {
                    Value = 10,
                    Weight = 0.1f,
                    Effects = new List<string>(4)
                    {
                        "Drain Magicka",
                        "Cure Paralyzation",
                        "Restore Personality",
                        "Restore Strength",
                    }
                },
                new Ingredient("Scathecraw", "Morrowind")
                {
                    Value = 2,
                    Weight = 0.1f,
                    Effects = new List<string>(4)
                    {
                        "Drain Strength",
                        "Cure Poison",
                        "Drain Health",
                        "Restore Willpower",
                    }
                },
                new Ingredient("Scrap Metal", "Morrowind")
                {
                    Value = 20,
                    Weight = 10,
                    Effects = new List<string>(4)
                    {
                        "Drain Health",
                        "Lightning Shield",
                        "Resist Shock",
                        "Restore Intelligence",
                    }
                },
                new Ingredient("Scrib Jelly", "Morrowind")
                {
                    Value = 10,
                    Weight = 0.1f,
                    Effects = new List<string>(4)
                    {
                        "Fortify Willpower",
                        "Cure Poison",
                        "Cure Blight Disease",
                        "Restore Willpower",
                    }
                },
                new Ingredient("Scrib Jerky", "Morrowind")
                {
                    Value = 5,
                    Weight = 0.2f,
                    Effects = new List<string>(4)
                    {
                        "Restore Fatigue",
                        "Fortify Fatigue",
                        "Burden",
                        "Swift Swim",
                    }
                },
                new Ingredient("Scuttle", "Morrowind")
                {
                    Value = 10,
                    Weight = 0.1f,
                    Effects = new List<string>(4)
                    {
                        "Restore Fatigue",
                        "Fortify Fatigue",
                        "Feather",
                        "Telekinesis",
                    }
                },
                new Ingredient("Shalk Resin", "Morrowind")
                {
                    Value = 50,
                    Weight = 0.1f,
                    Effects = new List<string>(4)
                    {
                        "Drain Fatigue",
                        "Fortify Health",
                        "Drain Personality",
                        "Fortify Speed",
                    }
                },
                new Ingredient("Sload Soap", "Morrowind")
                {
                    Value = 50,
                    Weight = 0.1f,
                    Effects = new List<string>(4)
                    {
                        "Drain Personality",
                        "Fortify Agility",
                        "Fire Shield",
                        "Restore Agility",
                    }
                },
                new Ingredient("Small Kwama Egg", "Morrowind")
                {
                    Value = 1,
                    Weight = 0.5f,
                    Effects = new List<string>(4)
                    {
                        "Restore Fatigue",
                    }
                },
                new Ingredient("Spore Pod", "Morrowind")
                {
                    Value = 1,
                    Weight = 0.1f,
                    Effects = new List<string>(4)
                    {
                        "Drain Strength",
                        "Drain Fatigue",
                        "Detect Key",
                        "Paralyze",
                    }
                },
                new Ingredient("Stoneflower Petals", "Morrowind")
                {
                    Value = 1,
                    Weight = 0.1f,
                    Effects = new List<string>(4)
                    {
                        "Restore Strength",
                        "Fortify Magicka",
                        "Drain Luck",
                        "Fortify Personality",
                    }
                },
                new Ingredient("Trama Root", "Morrowind")
                {
                    Value = 10,
                    Weight = 0.1f,
                    Effects = new List<string>(4)
                    {
                        "Restore Willpower",
                        "Levitate",
                        "Drain Magicka",
                        "Drain Speed",
                    }
                },
                new Ingredient("Vampire Dust", "Morrowind")
                {
                    Value = 500,
                    Weight = 0.1f,
                    Effects = new List<string>(4)
                    {
                        "Fortify Health",
                        "Fortify Strength",
                        "Spell Absorption",
                        "Vampirism",
                    }
                },
                new Ingredient("Violet Coprinus", "Morrowind")
                {
                    Value = 1,
                    Weight = 0.5f,
                    Effects = new List<string>(4)
                    {
                        "Water Walking",
                        "Drain Fatigue",
                        "Poison",
                    }
                },
                new Ingredient("Void Salts", "Morrowind")
                {
                    Value = 100,
                    Weight = 0.1f,
                    Effects = new List<string>(4)
                    {
                        "Restore Magicka",
                        "Spell Absorption",
                        "Paralyze",
                        "Drain Endurance",
                    }
                },
                new Ingredient("Wickwheat", "Morrowind")
                {
                    Value = 1,
                    Weight = 0.1f,
                    Effects = new List<string>(4)
                    {
                        "Restore Health",
                        "Fortify Willpower",
                        "Paralyze",
                        "Damage Intelligence",
                    }
                },
                new Ingredient("Willow Anther", "Morrowind")
                {
                    Value = 10,
                    Weight = 0.1f,
                    Effects = new List<string>(4)
                    {
                        "Drain Personality",
                        "Frost Shield",
                        "Cure Common Disease",
                        "Cure Paralyzation",
                    }
                },

                new Ingredient("Large Corprusmeat Hunk", "Morrowind")
                {
                    Value = 0,
                    Weight = 1,
                    Effects = new List<string>(4)
                    {
                        "Drain Fatigue",
                        "Drain Health",
                        "Drain Magicka",
                    }
                },
                new Ingredient("Large Wrapped Corprusmeat", "Morrowind")
                {
                    Value = 0,
                    Weight = 1,
                    Effects = new List<string>(4)
                    {
                        "Drain Fatigue",
                        "Drain Health",
                        "Drain Magicka",
                    }
                },
                new Ingredient("Medium Corprusmeat Hunk", "Morrowind")
                {
                    Value = 0,
                    Weight = 0.5f,
                    Effects = new List<string>(4)
                    {
                        "Drain Fatigue",
                        "Drain Health",
                        "Drain Magicka",
                    }
                },
                new Ingredient("Medium Wrapped Corprusmeat", "Morrowind")
                {
                    Value = 0,
                    Weight = 0.5f,
                    Effects = new List<string>(4)
                    {
                        "Drain Fatigue",
                        "Drain Health",
                        "Drain Magicka",
                    }
                },
                new Ingredient("Small Corprusmeat Hunk", "Morrowind")
                {
                    Value = 0,
                    Weight = 0.2f,
                    Effects = new List<string>(4)
                    {
                        "Drain Fatigue",
                        "Drain Health",
                        "Drain Magicka",
                    }
                },
                new Ingredient("Small Wrapped Corprusmeat", "Morrowind")
                {
                    Value = 0,
                    Weight = 0.2f,
                    Effects = new List<string>(4)
                    {
                        "Drain Fatigue",
                        "Drain Health",
                        "Drain Magicka",
                    }
                },
                new Ingredient("Wrapped Corpusmeat Hunk", "Morrowind")
                {
                    Value = 0,
                    Weight = 0.2f,
                    Effects = new List<string>(4)
                    {
                        "Fatigue",
                        "Drain Health",
                        "Drain Magicka",
                    }
                },

                new Ingredient("Human Flesh", "Morrowind")
                {
                    Value = 1,
                    Weight = 1.0f,
                    Effects = new List<string>(4)
                    {
                        "Fortify Health",
                        "Drain Intelligence",
                        "Drain Personality",
                    }
                },
                new Ingredient("Poison", "Morrowind")
                {
                    Value = 0,
                    Weight = 0,
                    Effects = new List<string>(4)
                    {
                        "Weakness to Poison",
                        "Damage Health",
                        "Damage Fatigue",
                        "Poison",
                    }
                },

                new Ingredient("Bear Pelt", "Bloodmoon")
                {
                    Value = 2,
                    Weight = 1,
                    Effects = new List<string>(4)
                    {
                        "Drain Fatigue",
                        "Fortify Strength",
                        "Resist Common Disease",
                        "Night Eye",
                    }
                },
                new Ingredient("Bristleback Leather", "Bloodmoon")
                {
                    Value = 2,
                    Weight = 1,
                    Effects = new List<string>(4)
                    {
                        "Blind",
                        "Frost Damage",
                        "Resist Frost",
                        "Recall",
                    }
                },
                new Ingredient("Grahl Eyeball", "Bloodmoon")
                {
                    Value = 15,
                    Weight = 1,
                    Effects = new List<string>(4)
                    {
                        "Resist Frost",
                        "Night Eye",
                        "Drain Magicka",
                        "Fortify Strength",
                    }
                },
                new Ingredient("Gravetar", "Bloodmoon")
                {
                    Value = 5,
                    Weight = 0.1f,
                    Effects = new List<string>(4)
                    {
                        "Resist Frost",
                        "Drain Health",
                        "Fortify Fatigue",
                        "Drain Luck",
                    }
                },
                new Ingredient("Heartwood", "Bloodmoon")
                {
                    Value = 200,
                    Weight = 1,
                    Effects = new List<string>(4)
                    {
                        "Restore Magicka",
                        "Fortify Agility",
                        "Drain Strength",
                        "Weakness to Fire",
                    }
                },
                new Ingredient("Holly Berries", "Bloodmoon")
                {
                    Value = 5,
                    Weight = 0.1f,
                    Effects = new List<string>(4)
                    {
                        "Resist Frost",
                        "Frost Shield",
                        "Frost Damage",
                        "Weakness to Fire",
                    }
                },
                new Ingredient("Horker Tusk", "Bloodmoon")
                {
                    Value = 5,
                    Weight = 0.1f,
                    Effects = new List<string>(4)
                    {
                        "Drain Alteration",
                        "Fortify Intelligence",
                        "Fortify Maximum Magicka",
                        "Detect Animal",
                    }
                },
                new Ingredient("Raw Stahlrim", "Bloodmoon")
                {
                    Value = 300,
                    Weight = 5,
                    Effects = new List<string>(4)
                    {
                        "Resist Frost",
                        "Frost Damage",
                        "Paralyze",
                        "Restore Health",
                    }
                },
                new Ingredient("Ripened Belladonna Berries", "Bloodmoon")
                {
                    Value = 5,
                    Weight = 0.1f,
                    Effects = new List<string>(4)
                    {
                        "Resist Magicka",
                        "Restore Magicka",
                        "Fortify Magicka",
                        "Drain Magicka",
                    }
                },
                new Ingredient("Snow Bear Pelt", "Bloodmoon")
                {
                    Value = 2,
                    Weight = 1,
                    Effects = new List<string>(4)
                    {
                        "Drain Fatigue",
                        "Fortify Speed",
                        "Resist Common Disease",
                        "Night Eye",
                    }
                },
                new Ingredient("Snow Wolf Pelt", "Bloodmoon")
                {
                    Value = 2,
                    Weight = 1,
                    Effects = new List<string>(4)
                    {
                        "Drain Fatigue",
                        "Fortify Speed",
                        "Resist Common Disease",
                        "Night Eye",
                    }
                },
                new Ingredient("Unripened Belladonna Berries", "Bloodmoon")
                {
                    Value = 5,
                    Weight = 0.1f,
                    Effects = new List<string>(4)
                    {
                        "Resist Magicka",
                        "Restore Magicka",
                        "Fortify Magicka",
                        "Drain Magicka",
                    }
                },
                new Ingredient("Wolf Pelt", "Bloodmoon")
                {
                    Value = 2,
                    Weight = 1,
                    Effects = new List<string>(4)
                    {
                        "Drain Fatigue",
                        "Fortify Speed",
                        "Resist Common Disease",
                        "Night Eye",
                    }
                },
                new Ingredient("Wolfsbane Petals", "Bloodmoon")
                {
                    Value = 1,
                    Weight = 0.1f,
                    Effects = new List<string>(4)
                    {
                        "Restore Intelligence",
                        "Invisibility",
                        "Drain Endurance",
                        "Drain Magicka",
                    }
                },


                new Ingredient("Adamantium Ore", "Tribunal")
                {
                    Value = 200,
                    Weight = 50,
                    Effects = new List<string>(4)
                    {
                        "Burden",
                        "Restore Magicka",
                        "Poison",
                        "Reflect",
                    }
                },
                new Ingredient("Durzog Meat", "Tribunal")
                {
                    Value = 7,
                    Weight = 2,
                    Effects = new List<string>(4)
                    {
                        "Fortify Agility",
                        "Fortify Strength",
                        "Blind",
                        "Damage Magicka",
                    }
                },
                new Ingredient("Golden Sedge Flowers", "Tribunal")
                {
                    Value = 1,
                    Weight = 1,
                    Effects = new List<string>(4)
                    {
                        "Drain Magicka",
                        "Fortify Strength",
                        "Fortify Attack",
                        "Swift Swim",
                    }
                },
                new Ingredient("Horn Lily Bulb", "Tribunal")
                {
                    Value = 1,
                    Weight = 1,
                    Effects = new List<string>(4)
                    {
                        "Resist Paralysis",
                        "Drain Health",
                        "Restore Strength",
                        "Restore Endurance",
                    }
                },
                new Ingredient("Lloramor Spines", "Tribunal")
                {
                    Value = 1,
                    Weight = 1,
                    Effects = new List<string>(4)
                    {
                        "Spell Absorption",
                        "Invisibility",
                        "Poison",
                        "Detect Enchantment",
                    }
                },
                new Ingredient("Meadow Rye", "Tribunal")
                {
                    Value = 1,
                    Weight = 1,
                    Effects = new List<string>(4)
                    {
                        "Fortify Speed",
                        "Damage Health",
                        "Restore Speed",
                        "Drain Speed",
                    }
                },
                new Ingredient("Nirthfly Stalks", "Tribunal")
                {
                    Value = 1,
                    Weight = 1,
                    Effects = new List<string>(4)
                    {
                        "Damage Health",
                        "Fortify Speed",
                        "Restore Speed",
                        "Drain Speed",
                    }
                },
                new Ingredient("Noble Sedge Flowers", "Tribunal")
                {
                    Value = 1,
                    Weight = 1,
                    Effects = new List<string>(4)
                    {
                        "Damage Health",
                        "Restore Agility",
                        "Poison",
                        "Fortify Agility",
                    }
                },
                new Ingredient("Scrib Cabbage", "Tribunal")
                {
                    Value = 1,
                    Weight = 1,
                    Effects = new List<string>(4)
                    {
                        "Drain Intelligence",
                        "Damage Health",
                        "Restore Agility",
                        "Fortify Agility",
                    }
                },
                new Ingredient("Sweetpulp", "Tribunal")
                {
                    Value = 1,
                    Weight = 1,
                    Effects = new List<string>(4)
                    {
                        "Paralyze",
                        "Resist Paralysis",
                        "Drain Magicka",
                        "Restore Endurance",
                    }
                },
                new Ingredient("Timsa-Come-By Flowers", "Tribunal")
                {
                    Value = 1,
                    Weight = 1,
                    Effects = new List<string>(4)
                    {
                        "Dispel",
                        "Resist Paralysis",
                        "Drain Magicka",
                        "Restore Endurance",
                    }
                },
            };

            return ingredients;
        }
    }
}
