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
    class OzzySrc
    {
        public static string JsonDirectory = $"{Directory.GetCurrentDirectory()}\\TextDocs\\Json";
        
        public CommandModule srcCommands = new CommandModule("Enter SRC Command");

        StarbaseCraftManager craftManager;
        StarbaseResearchManager researchManager;

        public OzzySrc()
        {
            StarbaseResourceManager.Init();

            if (!Directory.Exists(JsonDirectory))
                Directory.CreateDirectory(JsonDirectory);

            // Initialize crafting data and commands
            craftManager = new StarbaseCraftManager();
            srcCommands.Add("addRecipe", craftManager.AddOzzyRecipeFormat);
            srcCommands.Add("saveRecipes", craftManager.SaveOzzyRecipeFormat);
            srcCommands.Add("loadRecipes", craftManager.LoadOzzyRecipeFormat);
            srcCommands.Add("updateRecipe", craftManager.UpdateOzzyRecipeFormat);

            researchManager = new StarbaseResearchManager();
            srcCommands.Add("addNode", researchManager.AddResearchNode);
            srcCommands.Add("saveNodes", researchManager.SaveResearchNodes);
            srcCommands.Add("loadNodes", researchManager.LoadResearchNodes);
            srcCommands.Add("updateNode", researchManager.UpdateResearchNode);
            srcCommands.Add("validateNodes", researchManager.ValidateResearchNodes);
        }
    }
}
