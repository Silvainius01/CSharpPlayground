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
        public string Name { get; set; }
        public bool IsOwned { get; set; }
        public HashSet<string> Recipes { get; set; } = new HashSet<string>();
        public HashSet<string> Dependencies { get; set; } = new HashSet<string>();
        public Dictionary<string, int> ResearchCosts { get; set; } = new Dictionary<string, int>();

        public StarbaseResearchNode() { }

        public bool UpdateValues(StarbaseResearchNode newValues, bool clearRecipes, bool clearCosts, bool clearDependencies)
        {
            if(newValues.Name != Name)
            {
                ConsoleExt.WriteErrorLine($"Cannot update node '{Name}' with node '{newValues.Name}'");
                return false;
            }

            IsOwned = newValues.IsOwned;

            if (clearRecipes)
                Recipes.Clear();
            if (clearCosts)
                ResearchCosts.Clear();
            if (clearDependencies)
                Dependencies.Clear();

            foreach (var r in newValues.Recipes)
                Recipes.Add(r);
            foreach (var costKvp in newValues.ResearchCosts)
                ResearchCosts.Add(costKvp);
            foreach (var d in newValues.Dependencies)
                Dependencies.Add(d);
            return true;
        }
    }
}
