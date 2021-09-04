using GameEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarbaseTesting
{
    class StarbaseCraftRecipe
    {
        public string Name { get; set; }
        public float CraftingTime { get; set; }
        public Dictionary<string, float> Materials { get; set; } = new Dictionary<string, float>();
        public Dictionary<string, int> Research { get; set; } = new Dictionary<string, int>();
        public int Favourite { get; set; } = 0;
        public int Hide { get; set; } = 0;
        public float VendorPrice { get; set; } = 0;

        /// <summary>
        /// Updates the values of this recipe with info from a new one. Values < 0 will be ignored
        /// </summary>
        /// <param name="removeOmittedResources">Remove material cost and research values that do not exist in the new recipe data</param>
        public void UpdateValues(StarbaseCraftRecipe newValues, bool removeOmittedResources)
        {
            if (Name != newValues.Name)
                throw new ArgumentException("Recipes are not the same item.");

            Hide = newValues.Hide >= 0 ? newValues.Hide : Hide;
            Favourite = newValues.Favourite >= 0 ? newValues.Favourite : Favourite;
            VendorPrice = newValues.VendorPrice >= 0 ? newValues.VendorPrice : VendorPrice;
            CraftingTime = newValues.CraftingTime >= 0 ? newValues.CraftingTime : CraftingTime;

            if (removeOmittedResources)
            {
                // Add omitted materials and research so they can be removed
                foreach (var kvp in Materials)
                    if (!newValues.Materials.ContainsKey(kvp.Key))
                        newValues.Materials.Add(kvp.Key, 0);
                foreach (var kvp in Research)
                    if (!newValues.Research.ContainsKey(kvp.Key))
                        newValues.Research.Add(kvp.Key, 0);
            }

            // Update the new material and research values.
            foreach (var kvp in newValues.Materials)
            {
                if (Materials.ContainsKey(kvp.Key))
                {
                    if (kvp.Value == 0)
                        Materials.Remove(kvp.Key);
                    else Materials[kvp.Key] = kvp.Value;
                }
                else if (kvp.Value > 0)
                    Materials.Add(kvp);
            }

            foreach (var kvp in newValues.Research)
            {
                if (Research.ContainsKey(kvp.Key))
                {
                    if (kvp.Value == 0)
                        Research.Remove(kvp.Key);
                    else Research[kvp.Key] = kvp.Value;
                }
                else if (kvp.Value > 0)
                    Research.Add(kvp);
            }
        }
    }
}
