using System;
using System.Collections.Generic;
using System.Text;
using GameEngine;

namespace DnD_Generator
{
    public enum ItemWeaponWeightRange { Light, Mid, Heavy, Any }
    public enum ItemWeaponQualityRange { Low, Mid, High, Any }

    public class ItemWeaponGenerationPresets
    {
        public static readonly Vector2Int LightWeight = new Vector2Int(25, 50);
        public static readonly Vector2Int MidWeight = new Vector2Int(50, 75);
        public static readonly Vector2Int HeavyWeight = new Vector2Int(75, 100);
        public static readonly Vector2Int AnyWeight = new Vector2Int(25, 100);
        public static Vector2Int GetWeightRange(ItemWeaponWeightRange weightRange)
        {
            switch(weightRange)
            {
                case ItemWeaponWeightRange.Light: return LightWeight;
                case ItemWeaponWeightRange.Mid: return MidWeight;
                case ItemWeaponWeightRange.Heavy: return HeavyWeight;
            }
            return AnyWeight;
        }
        public static Vector2Int GetWeightRangeOrHigher(ItemWeaponWeightRange weightRange)
        {
            int rInt = Mathc.Random.NextInt((int)weightRange, (int)ItemWeaponWeightRange.Any);
            return GetWeightRange((ItemWeaponWeightRange)rInt);
        }
        public static Vector2Int GetWeightRangeOrLower(ItemWeaponWeightRange weightRange)
        {
            int rInt = Mathc.Random.NextInt(0, (int)weightRange);
            return GetWeightRange((ItemWeaponWeightRange)rInt);
        }

        public static readonly Vector2Int LowQuality = new Vector2Int(25, 100);
        public static readonly Vector2Int MidQuality = new Vector2Int(25, 100);
        public static readonly Vector2Int HighQuality = new Vector2Int(25, 100);
        public static readonly Vector2Int AnyQuality = new Vector2Int(25, 100);
        public static Vector2Int GetQualityRange(ItemWeaponQualityRange QualityRange)
        {
            switch (QualityRange)
            {
                case ItemWeaponQualityRange.Low: return LowQuality;
                case ItemWeaponQualityRange.Mid: return MidQuality;
                case ItemWeaponQualityRange.High: return HighQuality;
            }
            return AnyQuality;
        }
        public static Vector2Int GetQualityRangeOrHigher(ItemWeaponQualityRange QualityRange)
        {
            int rInt = Mathc.Random.NextInt((int)QualityRange, (int)ItemWeaponQualityRange.Any);
            return GetQualityRange((ItemWeaponQualityRange)rInt);
        }
        public static Vector2Int GetQualityRangeOrLower(ItemWeaponQualityRange QualityRange)
        {
            int rInt = Mathc.Random.NextInt(0, (int)QualityRange);
            return GetQualityRange((ItemWeaponQualityRange)rInt);
        }

        public static readonly int NoLargeRate = 0;
        public static readonly int LowLargeRate = 25;
        public static readonly int MidLargeRate = 50;
        public static readonly int HighLargeRate = 75;
        public static readonly int AllLargeRate = 100;
    }
}
