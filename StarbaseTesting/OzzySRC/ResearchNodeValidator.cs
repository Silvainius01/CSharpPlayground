using CommandEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarbaseTesting
{
    partial class StarbaseResearchManager
    {
        class NodeValidator
        {
            public bool WarnEmptyRecipes { get; set; }
            public bool WarnEmptyDependencies { get; set; }
            public bool WarnEmptyTree { get; set; }
            public bool WarnEmptyCost { get; set; }
            public bool WarnEmptyChildren { get; set; }
            public string Tree { get; set; }
            public ColorStringBuilder sb { get; set; }

            public NodeValidator(List<string> args)
            {
                sb = new ColorStringBuilder("  ");

                WarnEmptyRecipes = false;
                WarnEmptyDependencies = false;
                WarnEmptyTree = false;
                WarnEmptyCost = false;
                WarnEmptyChildren = false;
                Tree = string.Empty;

                int parseState = 0;
                foreach (var arg in args)
                {
                    switch (arg)
                    {
                        case "-r":
                            WarnEmptyRecipes = true; break;
                        case "-d":
                            WarnEmptyDependencies = true; break;
                        case "-t":
                            WarnEmptyTree = true; break;
                        case "-c":
                            WarnEmptyCost = true; break;
                        case "-l":
                            WarnEmptyChildren = true; break;
                        case "-tree":
                            parseState = 1; break;
                        default:
                            if (parseState == 1)
                                Tree = arg;
                            parseState = 0;
                            break;
                    }
                }
            }

            public void ValidateNode(int tabCount, StarbaseResearchNode node)
            {
                sb.AppendNewline(tabCount, $"Errors in node '{node.Name}':", ConsoleColor.Gray);
                int length = sb.TotalLength();

                ++tabCount;
                if (WarnEmptyTree && (node.Tree == null || node.Tree == string.Empty))
                    sb.AppendNewline(tabCount, "Is not part of a research tree.", ConsoleColor.Yellow);

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
                else if (WarnEmptyCost)
                    sb.AppendNewline(tabCount, $"Contains no research cost.", ConsoleColor.Red);

                if (node.Recipes.Any())
                {
                    foreach (var recipe in node.Recipes)
                        if (!StarbaseCraftManager.RecipeExists(recipe))
                            sb.AppendNewline(tabCount, $"Contains unknown recipe '{recipe}'.", ConsoleColor.Yellow);
                        else if (!StarbaseCraftManager.RecipeHasNode(recipe))
                            sb.AppendNewline(tabCount, $"Recipe '{recipe}' not recorded as added.", ConsoleColor.Red);
                }
                else if (WarnEmptyRecipes)
                    sb.AppendNewline(tabCount, $"Contains no recipes.", ConsoleColor.Yellow);

                if (node.Dependencies.Any())
                {
                    foreach (var dep in node.Dependencies)
                        if (!NodeExists(dep))
                            sb.AppendNewline(tabCount, $"Contains unknown dependency '{dep}'.", ConsoleColor.Yellow);
                        else if (!NodeChildOf(node.Name, dep))
                            sb.AppendNewline(tabCount, $"Not a child of '{dep}'.", ConsoleColor.Yellow);
                }
                else if (WarnEmptyDependencies)
                    sb.AppendNewline(tabCount, $"Contains no dependencies.", ConsoleColor.Yellow);

                if (node.Children.Any())
                {
                    foreach (var child in node.Children)
                        if (!NodeExists(child))
                            sb.AppendNewline(tabCount, $"Contains unknown child '{child}'.", ConsoleColor.Yellow);
                        else if (!NodeDependsOn(child, node.Name))
                            sb.AppendNewline(tabCount, $"Child '{child}' is not dependent.", ConsoleColor.Yellow);
                }
                else if (WarnEmptyChildren)
                    sb.AppendNewline(tabCount, $"Contains no children.", ConsoleColor.Yellow);

                --tabCount;

                if (length < sb.TotalLength())
                    sb.Write(false);
                sb.Clear();
            }

            public void ValidateNodes(IEnumerable<StarbaseResearchNode> nodes)
            {
                int tabCount = 0;
                foreach (var node in nodes)
                    ValidateNode(tabCount, node);
            }
        }
    }
}