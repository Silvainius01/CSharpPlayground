using GameEngine;
using Microsoft.SqlServer.Server;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace StarbaseTesting
{
    partial class StarbaseResearchManager
    {
        public static string NodeDataFileDirectory = $"{OzzySrc.JsonDirectory}\\ResearchNodes.json";
        public static Dictionary<string, StarbaseResearchNode> NodesByName = new Dictionary<string, StarbaseResearchNode>();

        public static HashSet<string> NodeTreeNames = new HashSet<string>();
        public static HashSet<string> RecipesOnNodes = new HashSet<string>();

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
                    case "-l": state = 4; continue;
                }

                switch (state)
                {
                    case 1: node.Recipes.Add(args[i]); break; // Assume recipe is valid for now
                    case 2: researchCosts.Add(args[i]); break;
                    case 3: node.Dependencies.Add(args[i]); break; // Assume dependencies are valid for now
                    case 4: node.Children.Add(args[i]); break;
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

        static void AddDependency(StarbaseResearchNode node, string dep, bool warn = true)
        {
            node.Dependencies.Add(dep);
            // Add node to it's parent's child list
            if (TryGetNode(dep, out var parentNode))
                parentNode.Children.Add(node.Name);
            else if (warn) ConsoleExt.WriteWarningLine($"Added unknown dependency '{dep}' to node '{node.Name}'");
        }
        static void AddChild(StarbaseResearchNode node, string child, bool warn = true)
        {
            node.Children.Add(child);
            // Add node to it's parent's child list
            if (TryGetNode(child, out var childNode))
                childNode.Dependencies.Add(node.Name);
            else if (warn) ConsoleExt.WriteWarningLine($"Added unknown child '{child}' to node '{node.Name}'");
        }
        
        static void SyncDependencies(StarbaseResearchNode node)
        {
            foreach (var dep in node.Dependencies)
                AddDependency(node, dep);
        }

        static void RemoveDependency(StarbaseResearchNode node, string dep)
        {
            // Clear the node from its parent's child list
            node.Dependencies.Remove(dep);
            if (TryGetNode(dep, out var parentNode))
                parentNode.Children.Remove(node.Name);
        }
        static void RemoveDependencies(StarbaseResearchNode node)
        {
            // Clear our dependencies
            foreach (var dep in node.Dependencies.ToArray())
                RemoveDependency(node, dep);
            node.Dependencies.Clear();
        }

        static void RemoveChild(StarbaseResearchNode node, string child)
        {
            node.Children.Remove(child);
            if (TryGetNode(child, out var childNode))
                childNode.Dependencies.Remove(node.Name);
        }
        static void RemoveChildren(StarbaseResearchNode node)
        {
            // Remove ourselves as a dependency for all children
            foreach (var childName in node.Children.ToArray())
                RemoveChild(node, childName);
            node.Children.Clear();
        }

        static bool AddResearchNode(StarbaseResearchNode node, bool warn = true)
        {
            if (node != null)
            {
                if (node.Tree == null)
                    node.Tree = string.Empty;

                if (node.Tree != string.Empty)
                    NodeTreeNames.Add(node.Tree);

                NodesByName.Add(node.Name, node);

                foreach (var dep in node.Dependencies)
                    AddDependency(node, dep, warn);
                foreach (var child in node.Children)
                    AddChild(node, child, warn);
                foreach (var recipe in node.Recipes)
                    RecipesOnNodes.Add(recipe);

                Console.WriteLine($"Added research node '{node.Name}'");
                return true;
            }

            ConsoleExt.WriteErrorLine($"Cannot add null research node");
            return false;
        }
        
        static void RemoveResearchNode(StarbaseResearchNode node)
        {
            if (node != null)
            {
                RemoveChildren(node);
                RemoveDependencies(node);
                NodesByName.Remove(node.Name);
            }
        }
        
        public static HashSet<string> GetChildNodes(string name)
        {
            if (TryGetNode(name, out var node))
                return node.Children;
            return new HashSet<string>();
        }
        public static HashSet<string> GetAllChildNodes(string name)
        {
            if(TryGetNode(name, out var node))
                return GetAllChildNodes(node);
            return new HashSet<string>();
        }
        public static HashSet<string> GetAllChildNodes(StarbaseResearchNode node)
        {
            Queue<string> childQueue = new Queue<string>(node.Children);
            HashSet<string> addedChildren = new HashSet<string>();

            while (childQueue.Any())
            {
                string childName = childQueue.Dequeue();

                if (addedChildren.Contains(childName))
                    continue;
                addedChildren.Add(childName);

                if (TryGetNode(childName, out var childNode))
                    childQueue.EnqueueAll(childNode.Children);
            }

            return addedChildren;
        }
        
        public static HashSet<string> GetNodeDependencies(string node)
        {
            if (NodeExists(node))
                return GetAllNodeDependencies(NodesByName[node]);
            return new HashSet<string>();
        }
        public static HashSet<string> GetAllNodeDependencies(string name)
        {
            if (TryGetNode(name, out var node))
                return GetAllNodeDependencies(node);
            return new HashSet<string>();
        }
        public static HashSet<string> GetAllNodeDependencies(StarbaseResearchNode node)
        {
            Queue<string> parentQueue = new Queue<string>(node.Dependencies);
            HashSet<string> addedParents = new HashSet<string>();

            while (parentQueue.Any())
            {
                string parentName = parentQueue.Dequeue();

                if (addedParents.Contains(parentName))
                    continue;
                addedParents.Add(parentName);

                if (TryGetNode(parentName, out var parentNode))
                    parentQueue.EnqueueAll(parentNode.Dependencies);
            }

            return addedParents;
        }
        
        public static bool NodeExists(string name) => NodesByName.ContainsKey(name);
        public static bool TryGetNode(string name, out StarbaseResearchNode node) => NodesByName.TryGetValue(name, out node);

        public static bool NodeHasChildren(string name) => NodeExists(name) && NodesByName[name].Children.Count > 0;
        public static bool NodeChildOf(string node, string parentName) => TryGetNode(parentName, out var parentNode) && parentNode.Children.Contains(node);

        public static bool NodeHasDependencies(string name) => NodeExists(name) && NodesByName[name].Dependencies.Count > 0;
        public static bool NodeDependsOn(string nodeName, string parentName) => TryGetNode(nodeName, out var node) && node.Dependencies.Contains(parentName);

        public static bool NodeTreeExists(string tree) => NodeTreeNames.Contains(tree);

        #region Commands
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
                AddResearchNode(r, false);
            }

            foreach (var node in NodesByName.Values)
                SyncDependencies(node);
        }

        public static void UpdateResearchNode(List<string> args)
        {
            if (args.Count < 1 || !NodesByName.ContainsKey(args[0]))
            {
                ConsoleExt.WriteErrorLine("Must enter an existing research node to update.");
                return;
            }

            int state = 0;
            bool printDebug = false;
            StarbaseResearchNode update = null;
            StarbaseResearchNode current = NodesByName[args[0]];
            string newName = current.Name;

            for (int i = 1; i < args.Count; ++i)
            {
                if (args[i].Length > 0 && args[i][0] == '-')
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
                        RemoveDependencies(current);
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

                    case "-p":
                        state = 0;
                        printDebug = true;
                        break;
                }

                switch (state)
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

            if (args.Count > 0)
            {
                update = ParseNode(args);
                if (update == null)
                {
                    ConsoleExt.WriteErrorLine("Cannot update with null node.");
                    return;
                }
                if (update.Tree != null && update.Tree != string.Empty)
                    update.Tree = current.Tree;
                current.UpdateValues(update);
            }

            var newNode = new StarbaseResearchNode(current);
            newNode.Name = newName;
            newNode.Tree = current.Tree;

            RemoveResearchNode(current);
            AddResearchNode(newNode);
            SyncDependencies(newNode);

            current = newNode;
            Console.WriteLine($"Updated node '{current.Name}'");

            if (printDebug)
                current.DebugColorString().WriteLine(true);
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
            foreach (var childName in GetAllChildNodes(node))
            {
                if (TryGetNode(childName, out var childNode))
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

            if(!startNode.Children.Any())
            {
                Console.WriteLine($"Node '{startNode.Name}' does not have any children.");
                return;
            }

            int tabCount = 0;
            Queue<string> childQueue = new Queue<string>(startNode.Children);
            HashSet<string> addedNodes = new HashSet<string>();
            addedNodes.Add(startNode.Name);

            ColorStringBuilder colorBuilder = new ColorStringBuilder();
            colorBuilder.TabString = "  ";
            colorBuilder.AppendNewline(tabCount, $"Children of node '{startNode.Name}':");

            ++tabCount;
            while (childQueue.Any())
            {
                string childName = childQueue.Dequeue();

                if (addedNodes.Contains(childName))
                    continue;
                addedNodes.Add(childName);

                if (!TryGetNode(childName, out var childNode))
                {
                    colorBuilder.AppendNewline(tabCount, $"{childName} - Unknown research node", ConsoleColor.Yellow);
                    continue;
                }
                else if (!childNode.Dependencies.Overlaps(addedNodes))
                    colorBuilder.AppendNewline(tabCount, $"{childName} - Added before / no dependencies", ConsoleColor.Yellow);
                else colorBuilder.AppendNewline(tabCount, childName, ConsoleColor.Gray);
                childQueue.EnqueueAll(childNode.Children);
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
            HashSet<string> addedNodes = new HashSet<string>();
            addedNodes.Add(startNode.Name);

            ColorStringBuilder colorBuilder = new ColorStringBuilder();
            colorBuilder.TabString = "  ";
            colorBuilder.AppendNewline(tabCount, $"Dependencies of node '{startNode.Name}':");

            ++tabCount;
            while (parentQueue.Any())
            {
                string parentName = parentQueue.Dequeue();

                if (addedNodes.Contains(parentName))
                    continue;
                addedNodes.Add(parentName);

                if (!NodesByName.TryGetValue(parentName, out var parentNode))
                {
                    colorBuilder.AppendNewline(tabCount, $"{parentName} - Unknown research node", ConsoleColor.Yellow);
                    continue;
                }
                else if (!parentNode.Children.Overlaps(addedNodes))
                    colorBuilder.AppendNewline(tabCount, $"{parentName} - Added before / no children", ConsoleColor.Yellow);
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
            NodeValidator validator = new NodeValidator(args);
            string tree = validator.Tree;

            if(tree != string.Empty && !NodeTreeExists(tree))
            {
                ConsoleExt.WriteErrorLine($"Cannot filter by tree '{tree}' -- it doesn't exist.");
                return;
            }

            // Filter by tree if the tree option was used.
            var nodeEnumerable = tree != string.Empty
                ? NodesByName.Values.Where(n => n.Tree == tree)
                : NodesByName.Values;

            validator.ValidateNodes(nodeEnumerable);
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
        #endregion
    }
}
