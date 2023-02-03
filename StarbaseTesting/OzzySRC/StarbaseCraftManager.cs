using CommandEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net;
using System.Linq;

namespace StarbaseTesting
{
    class StarbaseCraftManager
    {
        static int _nextIndex = 0;
        static int NextIndex
        {
            get
            {
                while (RecipesByIndex.ContainsKey(_nextIndex))
                    ++_nextIndex;
                return _nextIndex;
            }
        }
        public static readonly string RecipeDataFileDirectory = $"{OzzySrc.JsonDirectory}\\RecipeData.json";
        public static readonly string OzzySrcRecipeDataFileDirectory = $"{OzzySrc.OzzySRCProjectDirectory}\\RecipeData.json";

        public static Dictionary<int, StarbaseCraftRecipe> RecipesByIndex = new Dictionary<int, StarbaseCraftRecipe>();
        public static Dictionary<string, StarbaseCraftRecipe> RecipesByName = new Dictionary<string, StarbaseCraftRecipe>();

        public static bool RecipeExists(string name) => RecipesByName.ContainsKey(name);

        public static bool RecipeHasNode(string recipe) => StarbaseResearchManager.RecipesOnNodes.Contains(recipe);

        static bool AddRecipe(StarbaseCraftRecipe recipe)
        {
            if (RecipesByName.ContainsKey(recipe.Name))
            {
                ConsoleExt.WriteLine($"Recipe for '{recipe.Name}' has already been added.", ConsoleColor.Yellow);
                return false;
            }

            if(recipe.Index < 0)
            {
                recipe.Index = NextIndex;
            }

            RecipesByIndex.Add(recipe.Index, recipe);
            RecipesByName.Add(recipe.Name, recipe);
            //Console.WriteLine($"Added recipe for {recipe.Name}.");
            return true;
        }
        static StarbaseCraftRecipe ParseRecipe(List<string> args)
        {
            if (args.Count < 3 || args.Count % 2 != 1)
            {
                ConsoleExt.WriteLine(
                    "Must contain at least 3 args [name] [resource] [count] AND all resource must be paired.",
                    ConsoleColor.Red);
                return null;
            }

            StarbaseCraftRecipe recipe = new StarbaseCraftRecipe();

            // Parse the recipe
            recipe.Name = args[0];
            for (int i = 1; i < args.Count - 1; ++i)
            {
                string n = args[i];
                string a = args[++i];
                bool validValue = float.TryParse(a, out float amt);

                if (!validValue)
                {
                    ConsoleExt.WriteLine($"Value '{a}' is not a valid number, skipping entry '{n}'", ConsoleColor.Yellow);
                    continue;
                }

                switch (n)
                {
                    case "ct":
                        recipe.CraftingTime = amt; continue;
                    case "sv":
                        recipe.VendorPrice = amt; continue;
                }

                // Get resource name
                if (StarbaseResourceManager.AllResources.TryGetValue(n, out StarbaseResource r))
                {
                    switch (r.Type)
                    {
                        case StarbaseResourceType.Ore:
                            recipe.Materials.Add(r.Name, amt); break;
                        case StarbaseResourceType.Research:
                            recipe.Research.Add(r.Name, (int)amt); break;
                    }
                }
                else
                {
                    ConsoleExt.WriteLine($"'{n}' is not a valid resource.", ConsoleColor.Yellow);
                    continue;
                }
            }

            return recipe;
        }

        static bool ArgsContainValidRecipe(List<string> args)
        {
            return args.Count > 0 && (
                (int.TryParse(args[0], out int index) && RecipesByIndex.ContainsKey(index)) ||
                RecipesByName.ContainsKey(args[0])
            );
        }

        static StarbaseCraftRecipe GetRecipeFromNameOrIndex(string str)
        {
            if (int.TryParse(str, out int index) && RecipesByIndex.ContainsKey(index))
                return RecipesByIndex[index];
            if (RecipesByName.ContainsKey(str))
                return RecipesByName[str];
            return null;
        }

        #region Commands
        public static void AddOzzyRecipeFormat(List<string> args)
        {
            var recipe = ParseRecipe(args);
            if (recipe != null)
            {
                AddRecipe(recipe);
                Console.WriteLine($"Added recipe [{recipe.Index}] '{recipe.Name}'.");
            }
        }

        public static void SaveOzzyRecipeFormat(List<string> args)
        {
            JObject jsonObject = new JObject();
            JArray jRecipes = new JArray();

            foreach (var r in RecipesByName.Values)
                jRecipes.Add(JToken.FromObject(r));

            jsonObject["recipes"] = jRecipes;

            StreamWriter writer = new StreamWriter(RecipeDataFileDirectory);
            writer.Write(JsonConvert.SerializeObject(jsonObject));
            writer.Close();
            ConsoleExt.WriteLine($"Saved recipes to '{RecipeDataFileDirectory}'", ConsoleColor.Green);
        }

        public static void CopyRecipesToSRC(List<string> args)
        {
            if(args.Count > 0)
            {
                switch(args[0])
                {
                    case "-s":
                    case "-save":
                        args.RemoveAt(0);
                        SaveOzzyRecipeFormat(args);
                        break;
                }
            }

            System.IO.File.Copy(RecipeDataFileDirectory, OzzySrcRecipeDataFileDirectory, true);
            ConsoleExt.WriteLine($"Copied recipes to '{OzzySrcRecipeDataFileDirectory}'", ConsoleColor.Green);
        }

        public static void LoadOzzyRecipeFormat(List<string> args)
        {
            if(!File.Exists(RecipeDataFileDirectory))
            {
                ConsoleExt.WriteWarningLine($"Cannot load '{RecipeDataFileDirectory}': Recipe data file does not exist!");
                return;
            }

            Console.Write("Loading Recipes...");

            StreamReader reader = new StreamReader(RecipeDataFileDirectory);
            string json = reader.ReadToEnd();
            reader.Close();

            var jObject = JsonConvert.DeserializeObject<JObject>(json);
            var serializer = JsonSerializer.CreateDefault();
            foreach (var obj in jObject["recipes"])
            {
                var r = (StarbaseCraftRecipe)serializer.Deserialize(new JTokenReader(obj), typeof(StarbaseCraftRecipe));
                AddRecipe(r);
            }

            Console.WriteLine("Done!");
            Console.WriteLine($"  Loaded {RecipesByName.Count} recipes.");
        }

        public static void UpdateOzzyRecipeFormat(List<string> args)
        {
            if (args.Count < 1 || !RecipesByName.ContainsKey(args[0]))
            {
                ConsoleExt.WriteErrorLine("Must specify a valid recipe to update.");
                return;
            }

            bool removeOmittedResources = false;
            string newName = string.Empty;

            for (int i = 1; i < args.Count; ++i)
            {
                switch (args[i])
                {
                    case "clr":
                        removeOmittedResources = true;
                        args.RemoveAt(i--);
                        break;
                    case "-n": // Rename the recipe, remove both the switch and the new name from the argument list.
                        if (i >= args.Count - 1)
                        {
                            ConsoleExt.WriteErrorLine("Must provide a new name after -n switch.");
                            return;
                        }
                        newName = args[i + 1];
                        args.RemoveAt(i + 1);
                        args.RemoveAt(i--);
                        break;
                }
            }

            StarbaseCraftRecipe update = null;
            StarbaseCraftRecipe current = RecipesByName[args[0]];

            if (args.Count >= 3 || newName.Length == 0)
            {
                update = ParseRecipe(args);
                if (update == null)
                    return;
                current.UpdateValues(update, removeOmittedResources);
            }

            if (newName.Length > 0)
            {
                RecipesByName.Remove(current.Name);
                current.Name = newName;
                AddRecipe(current);
                Console.WriteLine($"Updated recipe to [{current.Index}] '{current.Name}'");
            }
            else Console.WriteLine($"Updated recipe [{current.Index}] '{current.Name}'");
        }

        public static void ValidateRecipes(List<string> args)
        {
            int tabCount = 0;
            int errorCount = 0;
            int warningCount = 0;
            ColorStringBuilder sb = new ColorStringBuilder("  ");

            foreach(var recipe in RecipesByName.Values)
            {
                sb.Append(tabCount, $"Errors in recipe '{recipe.Name}':", ConsoleColor.Gray);
                int length = sb.TotalLength();

                ++tabCount;

                if (!RecipeHasNode(recipe.Name))
                {
                    ++errorCount;
                    sb.NewlineAppend(tabCount, $"Not on a research node", ConsoleColor.Red);
                }

                if(!recipe.Materials.Any())
                {
                    ++errorCount;
                    sb.NewlineAppend(tabCount, "Does not have a material cost", ConsoleColor.Red);
                }
                else
                {
                    foreach(var kvp in recipe.Materials)
                        if(kvp.Value <= 0)
                        {
                            ++warningCount;
                            sb.NewlineAppend(tabCount, $"Invalid {kvp.Key} cost of {kvp.Value}", ConsoleColor.Yellow);
                        }
                }
                

                if (!recipe.Research.Any())
                {
                    ++warningCount;
                    sb.NewlineAppend(tabCount, "Does not have a research output", ConsoleColor.Yellow);
                }
                else
                {
                    foreach (var kvp in recipe.Research)
                        if (kvp.Value <= 0)
                        {
                            ++warningCount;
                            sb.NewlineAppend(tabCount, $"Invalid {kvp.Key} output of {kvp.Value}", ConsoleColor.Yellow);
                        }
                }

                --tabCount;

                if (length < sb.TotalLength())
                    sb.WriteLine(false);
                sb.Clear();
            }

            sb.Append(tabCount, "Results: ", ConsoleColor.Gray);
            sb.Append($"{errorCount} errors ", ConsoleColor.Red);
            sb.Append($"{warningCount} warnings", ConsoleColor.Yellow);
            sb.WriteLine(true);
        }

        public static void PrintRecipe(List<string> args)
        {
            if(args.Count < 1)
            {
                ConsoleExt.WriteErrorLine("Must provice a recipe index, name, or switch argument.");
                return;
            }

            if(args[0] == "-a")
            {
                Console.WriteLine("All Defined Recipes:");
                foreach(var r in RecipesByName.Values)
                {
                    Console.WriteLine($"  [{r.Index}] '{r.Name}'");
                }
                return;
            }
            
            StarbaseCraftRecipe recipe = GetRecipeFromNameOrIndex(args[0]);

            if(recipe == null)
            {
                ConsoleExt.WriteErrorLine("Not a valid recipe.");
                return;
            }

            SmartStringBuilder msg = new SmartStringBuilder("  ");
            int tabCount = 1;
            msg.AppendNewline(tabCount, $"Name: {recipe.Name}");
            msg.AppendNewline(tabCount, $"Index: {recipe.Index}");
            msg.AppendNewline(tabCount, $"Cost:");
            ++tabCount;
            foreach(var kvp in recipe.Materials)
                msg.AppendNewline(tabCount, $"{kvp.Key}: {kvp.Value}");
            --tabCount;
            msg.AppendNewline(tabCount, $"Research:");
            ++tabCount;
            foreach (var kvp in recipe.Research)
                msg.AppendNewline(tabCount, $"{kvp.Key}: {kvp.Value}");
            --tabCount;
            msg.AppendNewline(tabCount, $"Crafting Time: {recipe.CraftingTime}");
            msg.AppendNewline(tabCount, $"Vendor Price: {recipe.VendorPrice}");
            Console.WriteLine(msg.ToString());
        }

        public static void RemoveRecipe(List<string> args)
        {
            StarbaseCraftRecipe recipe = GetRecipeFromNameOrIndex(args[0]);

            if (RecipesByName == null)
            {
                ConsoleExt.WriteErrorLine("Not a valid recipe.");
                return;
            }

            if (CommandManager.YesNoPrompt($"Are you sure you want to delete recipe [{recipe.Index}] '{recipe.Name}'?", false))
            {
                RecipesByName.Remove(recipe.Name);
                RecipesByIndex.Remove(recipe.Index);
                Console.WriteLine($"Removed Recipe [{recipe.Index}] '{recipe.Name}'");
            }
            else
            {
                Console.WriteLine($"Did NOT remove Recipe [{recipe.Index}] '{recipe.Name}'");
            }
        }

        public static void FindRecipes(List<string> args)
        {
            if (args.Count < 1)
                return;

            int tabCount = 0;
            SmartStringBuilder msg = new SmartStringBuilder("  ");
            msg.AppendNewline(tabCount, $"Recipe names containing '{args[0]}':");

            tabCount++;
            foreach (var kvp in RecipesByName)
            {
                if (kvp.Key.Contains(args[0]))
                {
                    msg.AppendNewline(tabCount, $"[{kvp.Value.Index}] {kvp.Value.Name}");
                }
            }
            tabCount--;

            Console.WriteLine(msg.ToString());
        }
        #endregion
    }
}

/*
 addRecipe "Plasma Cannon Barrel" Ark 7641.15 Cha 5130.83 Kut 3820.65 Vok 1910.25 r 758 y 505 b 1264
 addRecipe "Plasma Cannon Magazine" Aeg 460.64 Cha 209.38 Bas 167.52 r 92
 addRecipe "Plasma Cannon Magazine (Refill)" Ice 900 Vok 675 Kar 675 y 112 b 22
 addRecipe "Plasma Cannon Magazine (Full)" Ice 900 Vok 675 Kar 675 y 112 b 22 Aeg 460.64 Cha 209.38 Bas 167.52 r 92
 addRecipe "Plasma Cannon Structure" Kut 2823.75 Cha 2823.68 Vok 1269.05 Exo 855.6 Ark 684.53 b 642 p 426
 addRecipe "Plasma Cannon Structure Mirror" 
 */
