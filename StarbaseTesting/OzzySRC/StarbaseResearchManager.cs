using GameEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace StarbaseTesting
{
    class StarbaseResearchManager
    {
       public static string NodeDataFileDirectory = $"{OzzySrc.JsonDirectory}\\ResearchNodes.json";
       public static HashSet<StarbaseResearchNode> allResearchNodes = new HashSet<StarbaseResearchNode>();
       public static Dictionary<string, StarbaseResearchNode> NodesByName = new Dictionary<string, StarbaseResearchNode>();

        public StarbaseResearchManager() { }

        StarbaseResearchNode ParseNode(List<string> args)
        {
            // addNode "name" -r [recipe] ... -c {[resource] [value]} ...

            if (args.Count < 3)
            {
                ConsoleExt.WriteErrorLine("Must provide node name, and must contain at least one recipe.");
                return null;
            }

            List<string> researchCosts = new List<string>();
            StarbaseResearchNode node = new StarbaseResearchNode();

            node.Name = args[0];

            // Parse switches, and add arguments to their respective lists.
            int state = 0;
            for (int i = 1; i < args.Count; ++i)
            {
                switch (args[i])
                {
                    case "-r": state = 1; continue;
                    case "-c": state = 2; continue;
                    case "-d": state = 3; continue;
                }

                switch (state)
                {
                    case 1: node.Recipes.Add(args[i]); break; // Assume recipe is valid for now
                    case 2: researchCosts.Add(args[i]); break; 
                    case 3: node.Dependencies.Add(args[i]); break; // Assume dependencies are valid for now
                    default:
                        ConsoleExt.WriteWarningLine($"Argument '{args[i]}' is not a valid switch or an argument following one, and was skipped.");
                        break;
                }
            }

            if (researchCosts.Count % 2 != 0)
            {
                ConsoleExt.WriteErrorLine($"Invalid research costs; Costs must come in pairs.");
                return null;
            }

            // Add valid resource pairs, assume all resource names are correct for now.
            for (int i = 0; i < researchCosts.Count - 1; ++i)
            {
                string name = researchCosts[i];
                string cost = researchCosts[++i];

                if (StarbaseResourceManager.TryGetResource(name, out var resource) && resource.Type == StarbaseResourceType.Research)
                {
                    // Parse as float for command consistency.
                    if (float.TryParse(cost, out float rCost))
                        node.ResearchCosts.Add(resource.Name, (int)rCost);
                    else ConsoleExt.WriteWarningLine($"Invalid cost value '{cost}' for resource '{name}");
                }
                else ConsoleExt.WriteWarningLine($"Invalid resource '{name}'");
            }

            return node;
        }

        public bool AddSerializedNode(StarbaseResearchNode serializedNode)
        {
            if (serializedNode != null)
            {
                NodesByName.Add(serializedNode.Name, serializedNode);
                Console.WriteLine($"Added research node '{serializedNode.Name}'");
                return true;
            }
            
            ConsoleExt.WriteErrorLine($"Cannot add null research node");
            return false;
        }

        public void AddResearchNode(List<string> args)
        {
            AddSerializedNode(ParseNode(args));
        }

        public void SaveResearchNodes(List<string> args)
        {
            JObject jsonObject = new JObject();
            JArray jResearchNodes = new JArray();

            foreach (var r in NodesByName.Values)
                jResearchNodes.Add(JToken.FromObject(r));

            jsonObject["objects"] = jResearchNodes;

            StreamWriter writer = new StreamWriter(NodeDataFileDirectory);
            writer.Write(JsonConvert.SerializeObject(jsonObject));
            writer.Close();
        }

        public void LoadResearchNodes(List<string> args)
        {
            if(!File.Exists(NodeDataFileDirectory))
            {
                ConsoleExt.WriteWarningLine("Recipe data file does not exist!");
                return;
            }

            Console.Write("Loading crafting recipes...");

            StreamReader reader = new StreamReader(NodeDataFileDirectory);
            string json = reader.ReadToEnd();
            reader.Close();

            var jObject = JsonConvert.DeserializeObject<JObject>(json);
            var serializer = JsonSerializer.CreateDefault();
            foreach (var obj in jObject["objects"])
            {
                var r = (StarbaseResearchNode)serializer.Deserialize(new JTokenReader(obj), typeof(StarbaseResearchNode));
                AddSerializedNode(r);
            }

            Console.WriteLine("Done");
        }

        public void UpdateResearchNode(List<string> args)
        {
            if(args.Count < 1 || NodesByName.ContainsKey(args[0]))
            {
                ConsoleExt.WriteErrorLine("Must enter an existing research node to update.");
                return;
            }

            bool clearCosts = false;
            bool clearRecipes = false;
            bool clearDependencies = false;

            for(int i = 1; i < args.Count; ++i)
            {
                switch(args[i])
                {
                    case "-rclr":
                        clearRecipes = true;
                        args.RemoveAt(i--);
                        break;
                    case "-cclr":
                        clearCosts = true;
                        args.RemoveAt(i--);
                        break;
                    case "-dclr":
                        clearDependencies = true;
                        args.RemoveAt(i--);
                        break;
                }
            }

            var updatedNode = ParseNode(args);
            var currentNode = NodesByName[args[0]];
            currentNode.UpdateValues(updatedNode, clearRecipes, clearCosts, clearDependencies);
        }

        public void ValidateResearchNodes(List<string> args)
        {
            foreach(var node in NodesByName.Values)
            {
                foreach (var dep in node.Dependencies)
                    if (!NodesByName.ContainsKey(dep))
                        ConsoleExt.WriteWarningLine($"Node '{node.Name}' contains unknown dependency '{dep}'");
            }
        }
    }
}
