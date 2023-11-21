
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace AlbionHelper
{
    public class AlbionItem
    {
        public static Dictionary<string, AlbionItem> itemsByString = new Dictionary<string, AlbionItem>();

        public class CraftingData
        {
            public float NutritionCost => parentItem.itemValue * 0.1125f;
            public float CraftingCapacityCost => 0;
            public AlbionBuilding craftingStation;
            public Dictionary<string, int> recipe = new Dictionary<string, int>();
            
            AlbionItem parentItem;
        }
        public class MarketData
        {
            public int royalMarketAverage;
        }

        public string name;
        public int itemValue;
        public CraftingData craftingData;
        public MarketData marketData;
    }

    public class AlbionFoodItem : AlbionItem
    {
        public int nutritionValue;
    }

    public class AlbionBuilding
    {
        public string name;
        public int tier;
        public int craftingCapacity;
        public int nutritionCapacity;
        public AlbionFoodItem favoriteFood;
    }

    public class AlbionCalc
    {
        public static float CalcTotalReturnRate(float displayedRate)
        {
            float numCrafts = 100000;
            float stack = numCrafts;
            int totalCrafted = 0;
            while (stack > 0)
            {
                totalCrafted += (int)stack;
                stack = MathF.Floor(stack * displayedRate);
            }

            return totalCrafted / numCrafts;
        }

        public static int CalcProfitPerItem(AlbionItem product, float stationReturnRate, int stationFee, int taxRate)
        {            
            //float silverPerNutrition = 0.0f;

            //if (stationFee > 0)
            //{
            //    silverPerNutrition = product.craftingData.NutritionCost / 100 * stationFee;
            //}
            //else
            //{
            //    AlbionFoodItem stationFood = product.craftingData.craftingStation.favoriteFood;
            //    silverPerNutrition = (stationFood.nutritionValue * 2) / stationFood.marketData.royalMarketAverage;
            //    silverPerNutrition *= product.craftingData.NutritionCost;
            //}

            int totalSilverPerCraft = 0; //(int)silverPerNutrition;
            
            // Just adding the raw material costs
            foreach (var kvp in product.craftingData.recipe)
            {
                AlbionItem recipeItem = AlbionItem.itemsByString[kvp.Key];
                totalSilverPerCraft += recipeItem.marketData.royalMarketAverage;
            }

            // Account for resources return bonus and market tax
            int profitAdjust = (int)(
                product.marketData.royalMarketAverage *
                CalcTotalReturnRate(stationReturnRate) * 
                (1 - taxRate));

            return profitAdjust - totalSilverPerCraft;
        }
    }
}