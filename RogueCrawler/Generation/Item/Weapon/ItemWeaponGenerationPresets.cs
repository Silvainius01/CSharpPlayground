using System;
using System.Collections.Generic;
using System.Text;
using CommandEngine;
using static RogueCrawler.DungeonSettings;

namespace RogueCrawler
{
    
    class ItemWeaponGenerationPresets
    {
        public static readonly Vector2Int LightWeight = new Vector2Int(MinWeaponWeight, LowWeaponWeight);
        public static readonly Vector2Int MidWeight = new Vector2Int(LowWeaponWeight, MidWeaponWeight);
        public static readonly Vector2Int HeavyWeight = new Vector2Int(MidWeaponWeight, MaxWeaponWeight);
        public static readonly Vector2Int AnyWeight = new Vector2Int(MinWeaponWeight, MaxWeaponWeight);
        public static Vector2Int GetWeightRange(QualityLevel weightRange)
        {
            switch (weightRange)
            {
                case QualityLevel.Low: return LightWeight;
                case QualityLevel.Normal: return MidWeight;
                case QualityLevel.Superior: return HeavyWeight;
            }
            return AnyWeight;
        }
        public static Vector2Int GetWeightRangeOrHigher(QualityLevel weightRange)
        {
            int rInt = CommandEngine.Random.NextInt((int)weightRange, EnumExt<QualityLevel>.Count);
            return GetWeightRange((QualityLevel)rInt);
        }
        public static Vector2Int GetWeightRangeOrLower(QualityLevel weightRange)
        {
            int rInt = CommandEngine.Random.NextInt(0, (int)weightRange);
            return GetWeightRange((QualityLevel)rInt);
        }

        public static readonly int NoLargeRate = 0;
        public static readonly int LowLargeRate = 25;
        public static readonly int MidLargeRate = 50;
        public static readonly int HighLargeRate = 75;
        public static readonly int AllLargeRate = 100;
        public static int GetRandomLargeProb() => CommandEngine.Random.NextInt(100, true);
        public static int GetLargeProbRange(ItemWeaponLargeRate largeRate)
        {
            switch (largeRate)
            {
                case ItemWeaponLargeRate.None: return NoLargeRate;
                case ItemWeaponLargeRate.Low: return LowLargeRate;
                case ItemWeaponLargeRate.Mid: return MidLargeRate;
                case ItemWeaponLargeRate.High: return HighLargeRate;
            }
            return AllLargeRate;
        }
        public static int GetLargeProbRangeOrHigher(ItemWeaponLargeRate largeRate)
        {
            int rInt = CommandEngine.Random.NextInt((int)largeRate, (int)ItemWeaponLargeRate.High);
            return GetLargeProbRange((ItemWeaponLargeRate)rInt);
        }
        public static int GetLargeProbRangeOrLower(ItemWeaponLargeRate largeRate)
        {
            int rInt = CommandEngine.Random.NextInt((int)ItemWeaponLargeRate.Low, (int)largeRate);
            return GetLargeProbRange((ItemWeaponLargeRate)rInt);
        }

        public static ItemWeaponGenerationParameters StartWeaponItem
        {
            get => new ItemWeaponGenerationParameters(QualityLevel.Normal)
            {
                CreatureLevel = 1,
                QualityOverride = 1.0f,
                WeightRange = new Vector2Int(25, 25),
                LargeWeaponProbability = ItemWeaponGenerationPresets.NoLargeRate,
                Material = MaterialTypeManager.DefaultMaterial,
            };
        }
        public static ItemWeaponGenerationParameters BrokenWeaponItem
        {
            get => new ItemWeaponGenerationParameters()
            {
                WeightRange = new Vector2Int(25, 50),
                QualityOverride = 0.0f,
                LargeWeaponProbability = ItemWeaponGenerationPresets.MidLargeRate,
            };
        }
        public static ItemWeaponGenerationParameters LowQualityWeaponChestItem
        {
            get => new ItemWeaponGenerationParameters(QualityLevel.Low, DungeonGenerator.GetRandomQuality())
            {
                WeightRange = AnyWeight,
                LargeWeaponProbability = GetRandomLargeProb()
            };
        }
        public static ItemWeaponGenerationParameters NormalQualityWeaponChestItem
        {
            get => new ItemWeaponGenerationParameters(QualityLevel.Normal, DungeonGenerator.GetRandomQuality())
            {
                // QualityRange = MidQuality,
                WeightRange = AnyWeight,
                LargeWeaponProbability = GetRandomLargeProb()
            };
        }
        public static ItemWeaponGenerationParameters SuperiorQualityWeaponChestItem
        {
            get => new ItemWeaponGenerationParameters(QualityLevel.Superior, DungeonGenerator.GetRandomQuality())
            {
                // QualityRange = HighQuality,
                WeightRange = AnyWeight,
                LargeWeaponProbability = GetRandomLargeProb()
            };
        }
        public static ItemWeaponGenerationParameters RandomWeaponItem
        {
            get => new ItemWeaponGenerationParameters(DungeonGenerator.GetRandomQuality)
            {
                // QualityRange = AnyQuality,
                WeightRange = AnyWeight,
                LargeWeaponProbability = MidLargeRate
            };
        }
        
        public static ItemWeaponGenerationParameters GetParamsForChest(int level, QualityLevel weaponQuality, QualityLevel weightQuality)
        {
            return new ItemWeaponGenerationParameters(weaponQuality, DungeonGenerator.GetRandomQuality())
            {
                CreatureLevel = level,
                WeightRange = GetWeightRange(weightQuality),
                LargeWeaponProbability = MidLargeRate
            };
        }
        public static ItemWeaponGenerationParameters GetParamsForCreature(Creature creature, QualityLevel weaponQuality, QualityLevel weightQuality)
        {
            return new ItemWeaponGenerationParameters(weaponQuality, DungeonGenerator.GetRandomQuality())
            {
                CreatureLevel = creature.Level,
                WeightRange = GetWeightRange(weightQuality),
                LargeWeaponProbability = MidLargeRate
            };
        }
        public static ItemWeaponGenerationParameters GenerateWeaponAtLevel(int level)
        {
            return new ItemWeaponGenerationParameters(QualityLevel.Normal)
            {
                CreatureLevel = level,
                WeightRange = AnyWeight,
                LargeWeaponProbability = MidLargeRate
            };
        }
    }
}
