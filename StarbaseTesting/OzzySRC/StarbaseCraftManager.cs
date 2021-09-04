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

        public Dictionary<string, StarbaseCraftRecipe> RecipesByName = new Dictionary<string, StarbaseCraftRecipe>();

        public StarbaseCraftManager()
        {
            LoadOzzyRecipeFormat(null);
        }

        public bool AddRecipe(StarbaseCraftRecipe recipe)
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

        public StarbaseCraftRecipe ParseRecipe(List<string> args)
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

        public void AddOzzyRecipeFormat(List<string> args)
        {
            AddRecipe(ParseRecipe(args));
        }

        public void SaveOzzyRecipeFormat(List<string> args)
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

        public void LoadOzzyRecipeFormat(List<string> args)
        {
            if(!File.Exists(RecipeDataFileDirectory))
            {
                ConsoleExt.WriteWarningLine("Recipe data file does not exist!");
                return;
            }

            Console.Write("Loading crafting recipes...");

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

            Console.WriteLine("Done");
        }

        public void UpdateOzzyRecipeFormat(List<string> args)
        {
            if (args.Count < 1 || !RecipesByName.ContainsKey(args[0]))
            {
                ConsoleExt.WriteErrorLine("Must specify a valid recipe to update.");
                return;
            }

            bool removeOmittedResources = false;
            for (int i = 1; i < args.Count; ++i)
            {
                switch (args[i])
                {
                    case "clr":
                        removeOmittedResources = true;
                        args.RemoveAt(i--);
                        break;
                }
            }

            StarbaseCraftRecipe update = ParseRecipe(args);
            RecipesByName[update.Name].UpdateValues(update, removeOmittedResources);
            Console.WriteLine($"Updated recipe '{update.Name}'");
        }
    }
}
