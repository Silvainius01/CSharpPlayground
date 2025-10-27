using CommandEngine;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;
using static RogueCrawler.DungeonCrawlerSettings;

namespace RogueCrawler
{
    
    class ItemArmorGenerationPresets
    {
        public static readonly Vector2Int LightWeight = new Vector2Int(MinWeaponWeight, LowWeaponWeight);
        public static readonly Vector2Int MidWeight = new Vector2Int(LowWeaponWeight, MidWeaponWeight);
        public static readonly Vector2Int HeavyWeight = new Vector2Int(MidWeaponWeight, MaxWeaponWeight);
        public static readonly Vector2Int AnyWeight = new Vector2Int(MinWeaponWeight, MaxWeaponWeight);

        public static ItemArmorGenerationParameters LowQualityWeaponChestItem
        {
            get => new ItemArmorGenerationParameters(QualityLevel.Low, DungeonGenerator.GetRandomQuality())
            {
                WeightRange = AnyWeight,
                LargeWeaponProbability = GetRandomLargeProb()
            };
        }
        public static ItemArmorGenerationParameters NormalQualityWeaponChestItem
        {
            get => new ItemArmorGenerationParameters(QualityLevel.Normal, DungeonGenerator.GetRandomQuality())
            {
                // QualityRange = MidQuality,
                WeightRange = AnyWeight,
                LargeWeaponProbability = GetRandomLargeProb()
            };
        }
        public static ItemArmorGenerationParameters SuperiorQualityWeaponChestItem
        {
            get => new ItemArmorGenerationParameters(QualityLevel.Superior, DungeonGenerator.GetRandomQuality())
            {
                // QualityRange = HighQuality,
                WeightRange = AnyWeight,
                LargeWeaponProbability = GetRandomLargeProb()
            };
        }
        public static ItemArmorGenerationParameters RandomWeaponItem
        {
            get => new ItemArmorGenerationParameters(DungeonGenerator.GetRandomQuality)
            {
                // QualityRange = AnyQuality,
                WeightRange = AnyWeight,
                LargeWeaponProbability = MidLargeRate
            };
        }
        
        public static ItemArmorGenerationParameters GetParamsForChest(int level, QualityLevel quality)
        {
            return new ItemArmorGenerationParameters(quality, DungeonGenerator.GetRandomQuality())
            {
                CreatureLevel = level,
                PossibleArmorClasses = new List<string>()
                {
                    DungeonConstants.ArmorClassLight,
                    DungeonConstants.ArmorClassMedium,
                    DungeonConstants.ArmorClassHeavy,
                },
                PossibleArmorSlots = new List<ArmorSlotType>(EnumExt<ArmorSlotType>.Values)
            };
        }
        public static ItemArmorGenerationParameters GetParamsForCreature(Creature creature, QualityLevel weaponQuality, QualityLevel weightQuality)
        {
            return new ItemArmorGenerationParameters(weaponQuality, DungeonGenerator.GetRandomQuality())
            {
                CreatureLevel = creature.Level,
                PossibleArmorClasses = new List<string>()
                {
                    DungeonConstants.ArmorClassLight,
                    DungeonConstants.ArmorClassMedium,
                    DungeonConstants.ArmorClassHeavy,
                },
                PossibleArmorSlots = new List<ArmorSlotType>(EnumExt<ArmorSlotType>.Values)
            };
        }
    }
}

