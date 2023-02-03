using CommandEngine;
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
    enum ClearSwitchValue { None, Unknown, All }

    class OzzySrc
    {
        public const string OzzySRCProjectDirectory = "D:\\Programming\\StarbaseGubs\\OzzySRC";
        public static readonly string JsonDirectory = $"{Directory.GetCurrentDirectory()}\\TextDocs\\Json";
        
        public CommandModule srcCommands = new CommandModule(
            defaultPrompt: "\nEnter SRC Command",
            invalidMsg: "Not an SRC command."
            );

        StarbaseResearchManager researchManager;

        public OzzySrc()
        {
            StarbaseResourceManager.LoadResources();
            StarbaseCraftManager.LoadOzzyRecipeFormat(null);
            StarbaseResearchManager.LoadResearchNodes(null);
            StarbaseResearchManager.ValidateResearchNodes(new List<string>() { "-c", "-t" });

            if (!Directory.Exists(JsonDirectory))
                Directory.CreateDirectory(JsonDirectory);

            // Initialize crafting data and commands
            srcCommands.Add("addRecipe", StarbaseCraftManager.AddOzzyRecipeFormat);
            srcCommands.Add("saveRecipes", StarbaseCraftManager.SaveOzzyRecipeFormat);
            srcCommands.Add("loadRecipes", StarbaseCraftManager.LoadOzzyRecipeFormat);
            srcCommands.Add("updateRecipe", StarbaseCraftManager.UpdateOzzyRecipeFormat);
            srcCommands.Add("validateRecipes", StarbaseCraftManager.ValidateRecipes);
            srcCommands.Add("printRecipe", StarbaseCraftManager.PrintRecipe);
            srcCommands.Add("removeRecipe", StarbaseCraftManager.RemoveRecipe);
            srcCommands.Add("copyRecipes", StarbaseCraftManager.CopyRecipesToSRC);
            srcCommands.Add("findRecipes", StarbaseCraftManager.FindRecipes);

            // Initialize research node data and commands
            srcCommands.Add("addNode", StarbaseResearchManager.AddResearchNode);
            srcCommands.Add("saveNodes", StarbaseResearchManager.SaveResearchNodes);
            srcCommands.Add("loadNodes", StarbaseResearchManager.LoadResearchNodes);
            srcCommands.Add("updateNode", StarbaseResearchManager.UpdateResearchNode);
            srcCommands.Add("validateNodes", StarbaseResearchManager.ValidateResearchNodes);
            srcCommands.Add("childNodes", StarbaseResearchManager.PrintResearchNodeChildren);
            srcCommands.Add("parentNodes", StarbaseResearchManager.PrintResearchNodeDependencies);
            srcCommands.Add("setNodeTree", StarbaseResearchManager.SetResearchNodeTree);
            srcCommands.Add("debugNode", StarbaseResearchManager.ViewResearchNode);
            srcCommands.Add("printTree", StarbaseResearchManager.GetNodeTreeDepth);
            // updateNode "Birght Blues" -n "Bright Blues"
            // "Rocket Launcher -d" -n "Rocket Launcher" -d "Grenade Launcher" -c r 50000 b 75000 p 25000
        }
    }
}
