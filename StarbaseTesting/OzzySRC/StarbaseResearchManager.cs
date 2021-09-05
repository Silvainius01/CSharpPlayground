using GameEngine;
using Microsoft.SqlServer.Server;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace StarbaseTesting
{
    class StarbaseResearchManager
    {
        public static string NodeDataFileDirectory = $"{OzzySrc.JsonDirectory}\\ResearchNodes.json";
        public static Dictionary<string, StarbaseResearchNode> NodesByName = new Dictionary<string, StarbaseResearchNode>();
        
        // We dont want this serialized, so might as well store it here.
        public static Dictionary<string, HashSet<string>> NodeChildren = new Dictionary<string, HashSet<string>>();

        public StarbaseResearchManager() { }

        static StarbaseResearchNode ParseNode(List<string> args)
        {
            // addNode "name" -r [recipe] ... -c {[resource] [value]} ...

            if (args.Count < 3)
            {
                ConsoleExt.WriteErrorLine("Must provide node name, and must contain at least one recipe, cost, or dependency.");
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
                    if (!node.ResearchCosts.ContainsKey(resource.Name) && float.TryParse(cost, out float rCost))
                        node.ResearchCosts.Add(resource.Name, (int)rCost);
                    else ConsoleExt.WriteWarningLine($"Invalid cost value '{cost}' for resource '{name}");
                }
                else ConsoleExt.WriteWarningLine($"Invalid resource '{name}'");
            }

            return node;
        }

        static void AddDependency(StarbaseResearchNode node, string dep)
        {
            if (!NodeChildren.ContainsKey(dep))
                NodeChildren.Add(dep, new HashSet<string>());

            node.Dependencies.Add(dep);
            NodeChildren[dep].Add(node.Name);
        }
        static void RemoveDependency(StarbaseResearchNode node, string dep)
        {
            // Clear the node from its parent's child list
            if (NodeChildren.ContainsKey(dep))
                NodeChildren[dep].Remove(node.Name);
            node.Dependencies.Remove(dep);
        }
        static void RemoveAllDependencies(StarbaseResearchNode node, bool removeChildren)
        {
            // Remove ourselves as a dependency for all children
            if (removeChildren && NodeChildren.ContainsKey(node.Name))
                foreach (var child in NodeChildren[node.Name].ToArray())
                    if (NodesByName.TryGetValue(child, out var childNode))
                        RemoveDependency(childNode, node.Name);

            // Clear our dependencies
            foreach (var dep in node.Dependencies.ToArray())
                RemoveDependency(node, dep);
            node.Dependencies.Clear();
        }
        static void RemoveResearchNode(StarbaseResearchNode node)
        {
            if (node != null)
            {
                NodesByName.Remove(node.Name);
                RemoveAllDependencies(node, true);
                NodeChildren.Remove(node.Name);
            }
        }
        static bool AddResearchNode(StarbaseResearchNode node)
        {
            if (node != null)
            {
                if (node.Tree == null)
                    node.Tree = string.Empty;

                NodesByName.Add(node.Name, node);

                if (!NodeChildren.ContainsKey(node.Name))
                    NodeChildren.Add(node.Name, new HashSet<string>());

                foreach (var dep in node.Dependencies)
                    AddDependency(node, dep);

                Console.WriteLine($"Added research node '{node.Name}'");
                return true;
            }

            ConsoleExt.WriteErrorLine($"Cannot add null research node");
            return false;
        }
        public static HashSet<string> GetAllChildNodes(string node)
        {
            if(NodeExists(node) && NodeHasChildren(node))
                return GetChildNodes(NodesByName[node]);
            return new HashSet<string>();
        }
        public static HashSet<string> GetAllChildNodes(StarbaseResearchNode node)
        {
            Queue<string> childQueue = new Queue<string>(NodeChildren[node.Name]);
            HashSet<string> addedChildren = new HashSet<string>();

            while (childQueue.Any())
            {
                string childName = childQueue.Dequeue();

                if (addedChildren.Contains(childName))
                    continue;
                addedChildren.Add(childName);

                if (NodeChildren.ContainsKey(childName))
                    childQueue.EnqueueAll(NodeChildren[childName]);
            }

            return addedChildren;
        }
        public static HashSet<string> GetChildNodes(string node)
        {
            if (NodeExists(node) && NodeHasChildren(node))
                return NodeChildren[node];
            return new HashSet<string>();
        }
        public static HashSet<string> GetChildNodes(StarbaseResearchNode node)
        {
            if (NodeExists(node.Name) && NodeHasChildren(node.Name))
                return NodeChildren[node.Name];
            return new HashSet<string>();
        }
        public static HashSet<string> GetNodeDependencies(string node)
        {
            if (NodeExists(node))
                return GetNodeDependencies(NodesByName[node]);
            return new HashSet<string>();
        }
        public static HashSet<string> GetNodeDependencies(StarbaseResearchNode node)
        {
            Queue<string> parentQueue = new Queue<string>(node.Dependencies);
            HashSet<string> addedParents = new HashSet<string>();

            while (parentQueue.Any())
            {
                string parentName = parentQueue.Dequeue();

                if (addedParents.Contains(parentName))
                    continue;
                addedParents.Add(parentName);

                if (NodesByName.TryGetValue(parentName, out var parentNode))
                    parentQueue.EnqueueAll(parentNode.Dependencies);
            }

            return addedParents;
        }
        public static bool NodeExists(string name) => NodesByName.ContainsKey(name);
        public static bool NodeHasChildren(string name) => NodeChildren.ContainsKey(name) && NodeChildren[name].Count > 0;
        public static bool NodeHasChildren(string name, out bool hasEntry)
        {
            if(NodeChildren.ContainsKey(name))
            {
                hasEntry = true;
                return NodeChildren[name].Count > 0;
            }
            hasEntry = false;
            return false;
        }
        public static bool NodeHasDependencies(string name) => NodeExists(name) && NodesByName[name].Dependencies.Count > 0;
        public static bool NodeDependsOn(string node, string name) => NodeHasDependencies(node) && NodesByName[node].Dependencies.Contains(name);

        public static void AddResearchNode(List<string> args)
        {
            AddResearchNode(ParseNode(args));
        }

        public static void SaveResearchNodes(List<string> args)
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

        public static void LoadResearchNodes(List<string> args)
        {
            if (!File.Exists(NodeDataFileDirectory))
            {
                ConsoleExt.WriteWarningLine("Recipe data file does not exist!");
                return;
            }

            StreamReader reader = new StreamReader(NodeDataFileDirectory);
            string json = reader.ReadToEnd();
            reader.Close();

            var jObject = JsonConvert.DeserializeObject<JObject>(json);
            var serializer = JsonSerializer.CreateDefault();
            foreach (var obj in jObject["objects"])
            {
                var r = (StarbaseResearchNode)serializer.Deserialize(new JTokenReader(obj), typeof(StarbaseResearchNode));
                AddResearchNode(r);
            }
        }

        public static void UpdateResearchNode(List<string> args)
        {
            if (args.Count < 1 || !NodesByName.ContainsKey(args[0]))
            {
                ConsoleExt.WriteErrorLine("Must enter an existing research node to update.");
                return;
            }

            int state = 0;
            string newName = string.Empty;
            StarbaseResearchNode update = null;
            StarbaseResearchNode current = NodesByName[args[0]];

            for (int i = 1; i < args.Count; ++i)
            {
                if(args[i].Length > 0 && args[i][0] == '-')
                    state = -1;

                switch (args[i])
                {
                    case "-rclr":
                        state = 0;
                        current.Recipes.Clear();
                        break;
                    case "-rcln":
                        state = 0;
                        foreach (var recipe in current.Recipes.ToArray())
                            if (!StarbaseCraftManager.RecipeExists(recipe))
                            {
                                current.Recipes.Remove(recipe);
                                Console.WriteLine($"\tRemoved unknown recipe '{recipe}'.");
                            }
                        break;

                    case "-cclr":
                        state = 0;
                        current.ResearchCosts.Clear();
                        break;
                    case "-ccln":
                        state = 0;
                        foreach (var kvp in current.ResearchCosts.ToArray())
                        {
                            if (StarbaseResourceManager.TryGetResource(kvp.Key, out var resource))
                            {
                                if (resource.Type != StarbaseResourceType.Research)
                                {
                                    current.ResearchCosts.Remove(resource.Name);
                                    Console.WriteLine($"\tRemoved non-research resource '{resource}'.");
                                }
                            }
                            else
                            {
                                current.ResearchCosts.Remove(kvp.Key);
                                Console.WriteLine($"\tRemoved unknown resource '{resource}'.");
                            }
                        }
                        break;

                    case "-dclr":
                        state = 0;
                        RemoveAllDependencies(current, false);
                        break;
                    case "-dcln":
                        state = 0;
                        foreach (var dep in current.Dependencies.ToArray())
                            if (!NodesByName.ContainsKey(dep))
                            {
                                RemoveDependency(current, dep);
                                Console.WriteLine($"\tRemoved unknown dependency '{dep}'.");
                            }
                        break;
                    case "-dr":
                        state = 1;
                        args.RemoveAt(i);
                        break;

                    case "-n": // Rename the node, remove both the switch and the new name from the argument list.
                        state = 2;
                        args.RemoveAt(i);
                        break;
                }

                switch(state)
                {
                    case 0:
                        args.RemoveAt(i--);
                        break;
                    case 1:
                        RemoveDependency(current, args[i]);
                        args.RemoveAt(i--);
                        break;
                    case 2:
                        newName = args[i];
                        args.RemoveAt(i--);
                        break;
                    default: 
                        break;
                }
            }

            if (args.Count >= 3 || newName.Length == 0)
            {
                update = ParseNode(args);
                if(update == null)
                {
                    ConsoleExt.WriteErrorLine("Cannot update with null node.");
                    return;
                }
                current.UpdateValues(update);
            }

            if (newName.Length > 0)
            {
                var newNode = new StarbaseResearchNode(current);
                newNode.Name = newName;

                if (NodeChildren.ContainsKey(current.Name))
                    foreach (var childName in NodeChildren[current.Name])
                        if (NodesByName.TryGetValue(childName, out var childNode))
                            AddDependency(childNode, newName);

                RemoveResearchNode(current);
                AddResearchNode(newNode);
                Console.WriteLine($"Updated node to '{current.Name}'");
            }
            else Console.WriteLine($"Updated node '{current.Name}'");
        }

        public static void SetResearchNodeTree(List<string> args)
        {
            if(args.Count < 1 || !NodesByName.ContainsKey(args[0]))
            {
                ConsoleExt.WriteErrorLine("Must provide valid node.");
                return;
            }
            if(args.Count < 2)
            {
                ConsoleExt.WriteErrorLine("Must provide a tree name.");
                return;
            }

            if (!CommandManager.YesNoPrompt("  Note: setting the tree of this node will also set ALL SUBSEQUENT nodes to the same tree. Continue?", false))
                return;

            string tree = args[1];
            StarbaseResearchNode node = NodesByName[args[0]];
            node.Tree = tree;

            ColorStringBuilder colorBuilder = new ColorStringBuilder(
                "  ",
                $"Set node '{node.Name}' to tree '{tree}', with following children:\n",
                ConsoleColor.Gray);

            int tabCount = 1;
            foreach (var childName in GetChildNodes(node))
            {
                if (NodesByName.TryGetValue(childName, out var childNode))
                {
                    childNode.Tree = args[1];
                    colorBuilder.AppendNewline(tabCount, $"{childName}");
                }
            }
            colorBuilder.Write(true);
        }

        public static void PrintResearchNodeChildren(List<string> args)
        {
            if(args.Count < 1 || !NodesByName.ContainsKey(args[0]))
            {
                ConsoleExt.WriteErrorLine("Must provide valid node.");
                return;
            }

            StarbaseResearchNode startNode = NodesByName[args[0]];

            if(!NodeChildren[startNode.Name].Any())
            {
                Console.WriteLine($"Node '{startNode.Name}' does not have any children.");
                return;
            }

            int tabCount = 0;
            Queue<string> childQueue = new Queue<string>(NodeChildren[startNode.Name]);
            HashSet<string> addedChildren = new HashSet<string>();

            ColorStringBuilder colorBuilder = new ColorStringBuilder();
            colorBuilder.TabString = "  ";
            colorBuilder.AppendNewline(tabCount, $"Children of node '{startNode.Name}':");

            ++tabCount;
            while (childQueue.Any())
            {
                string childName = childQueue.Dequeue();

                if (addedChildren.Contains(childName))
                    continue;
                addedChildren.Add(childName);

                if (!NodesByName.TryGetValue(childName, out var childNode))
                    colorBuilder.AppendNewline(tabCount, $"{childName} - Unknown research node", ConsoleColor.Yellow);
                else if (!NodeChildren.ContainsKey(childName))
                    colorBuilder.AppendNewline(tabCount, $"{childName} - No entry in children dict", ConsoleColor.Yellow);
                else
                {
                    colorBuilder.AppendNewline(tabCount, childName, ConsoleColor.Gray);
                    childQueue.EnqueueAll(NodeChildren[childName]);
                }
            }
            --tabCount;

            colorBuilder.WriteLine(true);
        }

        public static void PrintResearchNodeDependencies(List<string> args)
        {
            if (args.Count < 1 || !NodesByName.ContainsKey(args[0]))
            {
                ConsoleExt.WriteErrorLine("Must provide valid node.");
                return;
            }

            StarbaseResearchNode startNode = NodesByName[args[0]];

            if (!startNode.Dependencies.Any())
            {
                Console.WriteLine($"Node '{startNode.Name}' does not have any dependencies.");
                return;
            }

            int tabCount = 0;
            Queue<string> parentQueue = new Queue<string>(startNode.Dependencies);
            HashSet<string> addedParents = new HashSet<string>();

            ColorStringBuilder colorBuilder = new ColorStringBuilder();
            colorBuilder.TabString = "  ";
            colorBuilder.AppendNewline(tabCount, $"Dependencies of node '{startNode.Name}':");

            ++tabCount;
            while (parentQueue.Any())
            {
                string parentName = parentQueue.Dequeue();

                if (addedParents.Contains(parentName))
                    continue;
                addedParents.Add(parentName);

                if (!NodesByName.TryGetValue(parentName, out var parentNode))
                    colorBuilder.AppendNewline(tabCount, $"{parentName} - Unknown research node", ConsoleColor.Yellow);
                else if (!NodeChildren.ContainsKey(parentName))
                    colorBuilder.AppendNewline(tabCount, $"{parentName} - No entry in children dict", ConsoleColor.Yellow);
                else
                {
                    colorBuilder.AppendNewline(tabCount, parentName, ConsoleColor.Gray);
                    parentQueue.EnqueueAll(parentNode.Dependencies);
                }
            }
            --tabCount;

            colorBuilder.WriteLine(true);
        }

        public static void ValidateResearchNodes(List<string> args)
        {
            int tabCount = 0;
            ColorStringBuilder sb = new ColorStringBuilder();
            sb.TabString = "  ";

            bool warnEmptyRecipes = false;
            bool warnEmptyDependencies = false;
            bool warnEmptyTree = false;
            bool warnEmptyCost = false;
            foreach(var arg in args)
            {
                switch(arg)
                {
                    case "-r":
                        warnEmptyRecipes = true; break;
                    case "-d":
                        warnEmptyDependencies = true; break;
                    case "-t":
                        warnEmptyTree = true; break;
                    case "-c":
                        warnEmptyCost = true; break;
                }
            }

            foreach (var node in NodesByName.Values)
            {
                sb.AppendNewline(tabCount, $"Errors in node '{node.Name}':", ConsoleColor.Yellow);
                int length = sb.TotalLength();

                ++tabCount;
                if (warnEmptyTree && (node.Tree == null || node.Tree == string.Empty))
                    sb.AppendNewline(tabCount, "Is not part of a research tree.");

                if (node.ResearchCosts.Any())
                {
                    foreach (var kvp in node.ResearchCosts)
                        if (StarbaseResourceManager.TryGetResource(kvp.Key, out var resource))
                        {
                            if (resource.Type != StarbaseResourceType.Research)
                                sb.AppendNewline(tabCount, $"Contains non-research resource '{resource}'.", ConsoleColor.Yellow);
                        }
                        else sb.AppendNewline(tabCount, $"Contains unknown resource '{resource}'.", ConsoleColor.Yellow);
                }
                else if (warnEmptyCost)
                    sb.AppendNewline(tabCount, $"Contains no research cost.", ConsoleColor.Red);

                if (node.Recipes.Any())
                {
                    foreach (var recipe in node.Recipes)
                        if (!StarbaseCraftManager.RecipeExists(recipe))
                            sb.AppendNewline(tabCount, $"Contains unknown recipe '{recipe}'.", ConsoleColor.Yellow);
                }
                else if(warnEmptyRecipes)
                    sb.AppendNewline(tabCount, $"Contains no recipes.", ConsoleColor.Yellow);

                if (node.Dependencies.Any())
                {
                    foreach (var dep in node.Dependencies)
                        if (!NodesByName.ContainsKey(dep))
                            sb.AppendNewline(tabCount, $"Contains unknown dependency '{dep}'.", ConsoleColor.Yellow);
                }
                else if(warnEmptyDependencies)
                    sb.AppendNewline(tabCount, $"Contains no dependencies.", ConsoleColor.Yellow);

                foreach (var child in NodeChildren[node.Name])
                    if(!NodesByName.ContainsKey(child))
                        sb.AppendNewline(tabCount, $"Contains unknown child '{child}'.", ConsoleColor.Yellow);
                --tabCount;

                if (length < sb.TotalLength())
                    sb.Write(false);
                sb.Clear();
            }
        }

        public static void ViewResearchNode(List<string> args)
        {
            if (args.Count < 1 || !NodesByName.ContainsKey(args[0]))
            {
                ConsoleExt.WriteErrorLine("Must provide valid node.");
                return;
            }

            StarbaseResearchNode node = NodesByName[args[0]];

            node.DebugColorString().WriteLine(true);
        }
    }
}
