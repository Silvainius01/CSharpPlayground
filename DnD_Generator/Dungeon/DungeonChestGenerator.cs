using System;
using System.Collections.Generic;
using System.Text;
using GameEngine;

namespace DnD_Generator
{
    public enum DungeonChestType { Weapon, Armor, Misc, Any }
    public enum DungeonChestQuality { Low, Mid, High, Any }
    public enum DungeonChestLootGrade { EarlyGame, MidGame, EndGame, Any}

    public class DungeonChestGenerationProperties
    {
        public DungeonChestType ChestType { get; set; }
        public DungeonChestQuality ChestQuality { get; set; }
        public DungeonChestLootGrade ChestLootGrade { get; set; }
        public Vector2Int ItemRange { get; set; }
    }

    class DungeonChestGenerator
    {
        public static DungeonChest GenerateChest(DungeonChestGenerationProperties properties)
        {
            DungeonChest chest = new DungeonChest();

            // Assume a wepaons chest for now
            PopulateWeaponsChest(chest, Mathc.Random.NextInt(properties.ItemRange));

            return chest;
        }

        static void PopulateWeaponsChest(DungeonChest chest, int numItems)
        {
            for(int i = 0; i < numItems; ++i)
            {
                ItemWeaponGenerationProperties weaponProperties = new ItemWeaponGenerationProperties()
                {
                    QualityRange = ItemWeaponGenerationPresets.AnyQuality,
                    WeightRange = ItemWeaponGenerationPresets.AnyWeight,
                    LargeWeaponProbability = ItemWeaponGenerationPresets.LowLargeRate
                };
                chest.Items.Add(ItemWeaponGenerator.GenerateWeapon(weaponProperties), 1);
            }
        }
    }
}
