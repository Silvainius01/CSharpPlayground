using GameEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace StarbaseTesting
{
    class StarbaseCraftManager
    {
        public static string RecipeDataFileDirectory = $"{OzzySrc.JsonDirectory}\\Recipes.json";

        public static Dictionary<string, StarbaseCraftRecipe> RecipesByName = new Dictionary<string, StarbaseCraftRecipe>();

        public static bool RecipeExists(string name) => RecipesByName.ContainsKey(name);

        #region Commands
        public static void AddOzzyRecipeFormat(List<string> args)
        {
            AddRecipe(ParseRecipe(args));
        }

        public static void SaveOzzyRecipeFormat(List<string> args)
        {
            JObject jsonObject = new JObject();
            JArray jRecipes = new JArray();

            foreach (var r in RecipesByName.Values)
                jRecipes.Add(JToken.FromObject(r));

            jsonObject["objects"] = jRecipes;

            StreamWriter writer = new StreamWriter(RecipeDataFileDirectory);
            writer.Write(JsonConvert.SerializeObject(jsonObject));
            writer.Close();
        }

        public static void LoadOzzyRecipeFormat(List<string> args)
        {
            if(!File.Exists(RecipeDataFileDirectory))
            {
                ConsoleExt.WriteWarningLine("Recipe data file does not exist!");
                return;
            }

            StreamReader reader = new StreamReader(RecipeDataFileDirectory);
            string json = reader.ReadToEnd();
            reader.Close();

            var jObject = JsonConvert.DeserializeObject<JObject>(json);
            var serializer = JsonSerializer.CreateDefault();
            foreach (var obj in jObject["objects"])
            {
                var r = (StarbaseCraftRecipe)serializer.Deserialize(new JTokenReader(obj), typeof(StarbaseCraftRecipe));
                AddRecipe(r);
            }
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
                current.UpdateValues(update, removeOmittedResources);
            }

            if (newName.Length > 0)
            {
                RecipesByName.Remove(current.Name);
                current.Name = newName;
                AddRecipe(current);
                Console.WriteLine($"Updated recipe to '{current.Name}'");
            }
            else Console.WriteLine($"Updated recipe '{current.Name}'");
        }
        #endregion

        static bool AddRecipe(StarbaseCraftRecipe recipe)
        {
            if (RecipesByName.ContainsKey(recipe.Name))
            {
                ConsoleExt.WriteLine($"Recipe for '{recipe.Name}' has already been added.", ConsoleColor.Yellow);
                return false;
            }

            RecipesByName.Add(recipe.Name, recipe);
            Console.WriteLine($"Added recipe for {recipe.Name}.");
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
    }
}
