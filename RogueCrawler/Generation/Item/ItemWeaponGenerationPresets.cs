using System;
using System.Collections.Generic;
using System.Text;
using CommandEngine;
using static RogueCrawler.DungeonCrawlerSettings;

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
                case QualityLevel.Mid: return MidWeight;
                case QualityLevel.High: return HeavyWeight;
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

        public static readonly Vector2Int LowQuality = new Vector2Int(AllowBrokenWeapons ? 0 : 1, LowWeaponQuality);
        public static readonly Vector2Int MidQuality = new Vector2Int(LowWeaponQuality, MidWeaponQuality);
        public static readonly Vector2Int HighQuality = new Vector2Int(MidWeaponQuality, MaxWeaponQuality);
        public static readonly Vector2Int AnyQuality = new Vector2Int(AllowBrokenWeapons ? 0 : 1, MaxWeaponQuality);
        public static Vector2Int GetQualityRange(QualityLevel QualityRange)
        {
            switch (QualityRange)
            {
                case QualityLevel.Low: return LowQuality;
                case QualityLevel.Mid: return MidQuality;
                case QualityLevel.High: return HighQuality;
            }
            return AnyQuality;
        }
        public static Vector2Int GetQualityRangeOrHigher(QualityLevel QualityRange)
        {
            int rInt = CommandEngine.Random.NextInt((int)QualityRange, EnumExt<QualityLevel>.Count);
            return GetQualityRange((QualityLevel)rInt);
        }
        public static Vector2Int GetQualityRangeOrLower(QualityLevel QualityRange)
        {
            int rInt = CommandEngine.Random.NextInt(0, (int)QualityRange);
            return GetQualityRange((QualityLevel)rInt);
        }
        public static float GetQualityBias(QualityLevel q)
        {
            float bias = 1.0f;
            switch(q)
            {
                case QualityLevel.Low: bias = LowQualityLootLevelBias; break;
                case QualityLevel.Mid: bias = MidQualityLootLevelBias; break;
                case QualityLevel.High: bias = HighQualityLootLevelBias; break;
            }
            return bias;
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
            get => new ItemWeaponGenerationParameters(QualityLevel.Mid)
            {
                CreatureLevel = 1,
                GenerateRelative = false,
                CapToCreatureLevel = true,
                WeightRange = new Vector2Int(25, 25),
                LargeWeaponProbability = ItemWeaponGenerationPresets.NoLargeRate,
            };
        }
        public static ItemWeaponGenerationParameters BrokenWeaponItem
        {
            get => new ItemWeaponGenerationParameters()
            {
                WeightRange = new Vector2Int(25, 50),
                // QualityRange = new Vector2Int(0, 0), // ~28% chance of <1.0 Quality starter
                LargeWeaponProbability = ItemWeaponGenerationPresets.MidLargeRate,
            };
        }
        public static ItemWeaponGenerationParameters LowQualityWeaponChestItem
        {
            get => new ItemWeaponGenerationParameters()
            {
                // QualityRange = LowQuality,
                WeightRange = AnyWeight,
                LargeWeaponProbability = GetRandomLargeProb()
            };
        }
        public static ItemWeaponGenerationParameters MidQualityWeaponChestItem
        {
            get => new ItemWeaponGenerationParameters()
            {
                // QualityRange = MidQuality,
                WeightRange = AnyWeight,
                LargeWeaponProbability = GetRandomLargeProb()
            };
        }
        public static ItemWeaponGenerationParameters HighQualityWeaponChestItem
        {
            get => new ItemWeaponGenerationParameters()
            {
                // QualityRange = HighQuality,
                WeightRange = AnyWeight,
                LargeWeaponProbability = GetRandomLargeProb()
            };
        }
        public static ItemWeaponGenerationParameters RandomWeaponItem
        {
            get => new ItemWeaponGenerationParameters()
            {
                // QualityRange = AnyQuality,
                WeightRange = AnyWeight,
                LargeWeaponProbability = MidLargeRate
            };
        }
        
        public static ItemWeaponGenerationParameters GetParamsForChest(int level, QualityLevel weaponQuality, QualityLevel weightQuality)
        {
            return new ItemWeaponGenerationParameters(weaponQuality)
            {
                CreatureLevel = level,
                GenerateRelative = true,
                CapToCreatureLevel = false,
                WeightRange = GetWeightRange(weightQuality),
                LargeWeaponProbability = MidLargeRate
            };
        }
        public static ItemWeaponGenerationParameters GetParamsForCreature(Creature creature, QualityLevel weaponQuality, QualityLevel weightQuality)
        {
            return new ItemWeaponGenerationParameters(weaponQuality)
            {
                CreatureLevel = creature.Level,
                GenerateRelative = true,
                CapToCreatureLevel = true,
                WeightRange = GetWeightRange(weightQuality),
                LargeWeaponProbability = MidLargeRate
            };
        }
        public static ItemWeaponGenerationParameters GenerateWeaponAtLevel(int level, bool generateRelative, bool capLevel)
        {
            return new ItemWeaponGenerationParameters(QualityLevel.Mid)
            {
                CreatureLevel = level,
                GenerateRelative = generateRelative,
                CapToCreatureLevel = false,
                WeightRange = AnyWeight,
                LargeWeaponProbability = MidLargeRate
            };
        }
    }
}
