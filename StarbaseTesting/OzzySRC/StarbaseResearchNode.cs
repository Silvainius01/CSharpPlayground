using GameEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace StarbaseTesting
{
    class StarbaseResearchNode
    {
        public string Name { get; set; } = string.Empty;
        public string Tree { get; set; } = string.Empty;
        public bool IsOwned { get; set; } = false;
        public HashSet<string> Recipes { get; set; } = new HashSet<string>();
        public HashSet<string> Dependencies { get; set; } = new HashSet<string>();
        public Dictionary<string, int> ResearchCosts { get; set; } = new Dictionary<string, int>();

        public StarbaseResearchNode() { }
        public StarbaseResearchNode(StarbaseResearchNode newValues) 
        {
            Name = newValues.Name;
            UpdateValues(newValues);
        }

        public bool UpdateValues(StarbaseResearchNode newValues)
        {
            if(newValues.Name != Name)
            {
                ConsoleExt.WriteErrorLine($"Cannot update node '{Name}' with node '{newValues.Name}'");
                return false;
            }

            IsOwned = newValues.IsOwned;
            Tree = newValues.Tree;

            foreach (var r in newValues.Recipes)
                Recipes.Add(r);
            foreach (var costKvp in newValues.ResearchCosts)
                if(!ResearchCosts.ContainsKey(costKvp.Key))
                    ResearchCosts.Add(costKvp);
            foreach (var d in newValues.Dependencies)
                Dependencies.Add(d);
            return true;
        }

        public ColorStringBuilder DebugColorString()
        {
            int tabCount = 0;
            ColorStringBuilder colorBuilder = new ColorStringBuilder("  ");

            colorBuilder.Append(tabCount, $"Research Node info:");

            ++tabCount;
            colorBuilder.NewlineAppend(tabCount, $"Name: {Name}");
            colorBuilder.NewlineAppend(tabCount, $"Tree: {Tree}");
            colorBuilder.NewlineAppend(tabCount, $"IsOwned: {IsOwned}");

            colorBuilder.NewlineAppend(tabCount, $"Recipes:", ConsoleColor.Gray);
            if(Recipes.Any())
            {
                ++tabCount;
                foreach(var str in Recipes)
                {
                    if(!StarbaseCraftManager.RecipeExists(str))
                        colorBuilder.AppendNewline(tabCount, $"{str} - Unknown recipe", ConsoleColor.Yellow);
                    else colorBuilder.AppendNewline(tabCount, str, ConsoleColor.Gray);
                }
                --tabCount;
            }
            else colorBuilder.Append(" --- NONE ---", ConsoleColor.Yellow);

            colorBuilder.NewlineAppend(tabCount, $"Cost:", ConsoleColor.Gray);
            if (ResearchCosts.Any())
            {
                ++tabCount;
                foreach (var kvp in ResearchCosts)
                {
                    if (!StarbaseResourceManager.TryGetResource(kvp.Key, out var resource))
                    {
                        if (resource.Type != StarbaseResourceType.Research)
                            colorBuilder.AppendNewline(tabCount, $"{kvp.Key} - Not a research type", ConsoleColor.Yellow);
                        else colorBuilder.AppendNewline(tabCount, $"{kvp.Key}: {kvp.Value}", ConsoleColor.Gray);
                    }
                    else colorBuilder.AppendNewline(tabCount, $"{kvp.Key} - Unknown resource", ConsoleColor.Red);
                }
                --tabCount;
            }
            else colorBuilder.Append(" --- NONE ---", ConsoleColor.Red);

            colorBuilder.NewlineAppend(tabCount, $"Dependencies:", ConsoleColor.Gray);
            if (Dependencies.Any())
            {
                ++tabCount;
                foreach (string parent in Dependencies)
                {
                    if (StarbaseResearchManager.NodeExists(parent))
                    {
                        if (!StarbaseResearchManager.NodeHasChildren(parent, out bool hasEntry))
                            colorBuilder.NewlineAppend(tabCount, $"{parent} - Dependency has no children", ConsoleColor.Red);
                        else if (!StarbaseResearchManager.GetChildNodes(parent).Contains(Name))
                            colorBuilder.NewlineAppend(tabCount, $"{parent} - Not a child of dependency", ConsoleColor.Red);
                        else colorBuilder.NewlineAppend(tabCount, parent, ConsoleColor.Gray);
                    }
                    else colorBuilder.NewlineAppend(tabCount, $"{parent} - Unknown Node", ConsoleColor.Red);
                }
                --tabCount;
            }
            else colorBuilder.Append(" --- NONE ---", ConsoleColor.Yellow);

            colorBuilder.NewlineAppend(tabCount, $"Children:", ConsoleColor.Gray);
            var children = StarbaseResearchManager.GetChildNodes(Name);
            if(children.Any())
            {
                ++tabCount;
                foreach (string childName in children)
                {
                    if (StarbaseResearchManager.NodeExists(childName))
                    {
                        if (!StarbaseResearchManager.NodeHasDependencies(childName) || !StarbaseResearchManager.NodeDependsOn(childName, Name))
                            colorBuilder.NewlineAppend(tabCount, $"{childName} - Child has no dependency", ConsoleColor.Red);
                        else colorBuilder.NewlineAppend(tabCount, childName, ConsoleColor.Gray);
                    }
                    else colorBuilder.NewlineAppend(tabCount, $"{childName} - Unknown Node", ConsoleColor.Red);
                }
                --tabCount;
            }
            else colorBuilder.Append(" --- NONE ---", ConsoleColor.Yellow);
            --tabCount;

            return colorBuilder;
        }
    }
}
