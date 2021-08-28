using GameEngine;
using System;
using System.Text;
using System.Collections.Generic;

namespace DnD_Generator
{
    class WeaponTypeData
    {
        public WeaponType Type { get; set; }
        public AttributeType PrimaryAttribute { get; set; }
        public AttributeType SecondaryAttribute { get; set; }
        public AttributeType ToHitAttribute { get; set; }
        public AttributeType DamageAttribute { get; set; }

        public int BaseDamage { get; set; }
        public int PrimaryAttributeBaseReq { get; set; }
        public int SecondaryAttributeBaseReq { get; set; }
        public float LargeWeaponDamageMult { get; set; } = 2;
        public float LargeWeaponWeightMult { get; set; } = 3;
    }

    class ItemWeaponGenerator : BaseDungeonObjectGenerator<ItemWeapon, ItemWeaponGenerationParameters>
    {
        public static Dictionary<WeaponType, WeaponTypeData> WeaponTypeData = new Dictionary<WeaponType, WeaponTypeData>()
        {
            [WeaponType.Axe] = new WeaponTypeData()
            {
                Type = WeaponType.Axe,
                PrimaryAttribute = AttributeType.STR,
                SecondaryAttribute = AttributeType.DEX,
                ToHitAttribute = AttributeType.DEX,
                DamageAttribute = AttributeType.STR,
                PrimaryAttributeBaseReq = 4,
                SecondaryAttributeBaseReq = 2,
                BaseDamage = 1,
                LargeWeaponDamageMult = 2.5f,
                LargeWeaponWeightMult = 3f
            },
            [WeaponType.Blade] = new WeaponTypeData()
            {
                Type = WeaponType.Blade,
                PrimaryAttribute = AttributeType.DEX,
                SecondaryAttribute = AttributeType.STR,
                ToHitAttribute = AttributeType.DEX,
                DamageAttribute = AttributeType.DEX,
                PrimaryAttributeBaseReq = 4,
                SecondaryAttributeBaseReq = 2,
                BaseDamage = 1,
                LargeWeaponDamageMult = 2,
                LargeWeaponWeightMult = 3
            },
            [WeaponType.Blunt] = new WeaponTypeData()
            {
                Type = WeaponType.Blunt,
                PrimaryAttribute = AttributeType.STR,
                SecondaryAttribute = AttributeType.CON,
                ToHitAttribute = AttributeType.DEX,
                DamageAttribute = AttributeType.STR,
                PrimaryAttributeBaseReq = 4,
                SecondaryAttributeBaseReq = 2,
                BaseDamage = 1,
                LargeWeaponDamageMult = 3,
                LargeWeaponWeightMult = 4,
            },
            [WeaponType.Ranged] = new WeaponTypeData()
            {
                Type = WeaponType.Ranged,
                PrimaryAttribute = AttributeType.DEX,
                SecondaryAttribute = AttributeType.STR,
                ToHitAttribute = AttributeType.STR,
                DamageAttribute = AttributeType.DEX,
                PrimaryAttributeBaseReq = 4,
                SecondaryAttributeBaseReq = 2,
                BaseDamage = 2,
                LargeWeaponDamageMult = 2,
                LargeWeaponWeightMult = 1.5f
            },
        };

        public override ItemWeapon Generate(ItemWeaponGenerationParameters wParams)
        {
            wParams.Validate();

            var weaponType = GetRandomWeaponType(wParams.PossibleWeaponTypes);

            ItemWeapon weapon = new ItemWeapon()
            {
                ID = NextId,
                Weight = Mathc.Random.NextInt(wParams.WeightRange, true) / 10.0f,
                IsLargeWeapon = Mathc.Random.NextInt(100) < wParams.LargeWeaponProbability,
                WeaponType = weaponType,
                DamageBonusAttribute = WeaponTypeData[weaponType].DamageAttribute,
                HitBonusAttribute = WeaponTypeData[weaponType].ToHitAttribute,
                BaseDamage = WeaponTypeData[weaponType].BaseDamage
            };
            weapon.Quality = GetQuality(wParams, weapon);
            weapon.AttributeRequirements = GenerateAttributeRequirements(weapon);
            weapon.Name = GetName(weapon);

            if (weapon.IsLargeWeapon)
            {
                weapon.BaseDamage *= WeaponTypeData[weaponType].LargeWeaponDamageMult;
                weapon.Weight *= WeaponTypeData[weaponType].LargeWeaponWeightMult;
            }
            weapon.Value = weapon.GetValue();

            return weapon;
        }

        public WeaponType GetRandomWeaponType(List<WeaponType> types = null) 
            => types != null && types.Count > 0
                ? (types.RandomItem())
                : EnumExt<WeaponType>.RandomValue;

        CreatureAttributes GenerateAttributeRequirements(ItemWeapon weapon)
        {
            var weaponTypeData = WeaponTypeData[weapon.WeaponType];
            CreatureAttributes req = new CreatureAttributes();

            req[AttributeType.STR] = GetStrengthReq(weapon.Weight);
            req[weaponTypeData.PrimaryAttribute] += (int)Math.Ceiling(weapon.Quality * GetPrimaryStatReq(weapon));
            req[weaponTypeData.SecondaryAttribute] += (int)Math.Ceiling(weapon.Quality * GetSecondaryStatReq(weapon));

            return req;
        }

        string GetName(ItemWeapon weapon)
        {
            StringBuilder builder = new StringBuilder();

            if (weapon.IsLargeWeapon)
                builder.Append("Large");
            if (weapon.Quality <= 0.0f)
                builder.Append("Broken");
            builder.Append(weapon.WeaponType);
            return builder.ToString();
        }

        int GetStrengthReq(float weight)
            => (int)Math.Ceiling(weight / 5.0);
        
        /// <summary>Get the stat requirement for Quality 1.0 weapon</summary>
        int GetPrimaryStatReq(ItemWeapon weapon)
            => WeaponTypeData[weapon].PrimaryAttributeBaseReq;
        
        /// <summary>Get the stat requirement for Quality 1.0 weapon</summary>
        int GetSecondaryStatReq(ItemWeapon weapon)
            => WeaponTypeData[weapon].SecondaryAttributeBaseReq;
        
        float GetMaxQuality(int level, ItemWeapon weapon)
        {
            if (level <= 0)
                return 0;

            int maxAttrPoints = (level * DungeonCrawlerSettings.AttributePointsPerLevel) - GetStrengthReq(weapon.Weight);
            float pointsPerQual = GetPrimaryStatReq(weapon) + GetSecondaryStatReq(weapon);
            return (maxAttrPoints / pointsPerQual).Truncate(1);    
        }

        float GetQuality(ItemWeaponGenerationParameters wParams, ItemWeapon weapon)
        {
            if (wParams.GenerateRelative)
            {
                var levelRange = DungeonGenerator.GetRelativeLootRange(wParams.CreatureLevel);
                int level = (int)Mathc.Random.GetMarsagliaBetween(levelRange, DungeonGenerator.GetQualityBias(wParams.QualityBias));
                if (wParams.CapToCreatureLevel && level > wParams.CreatureLevel)
                    level = wParams.CreatureLevel;
                return GetMaxQuality(level, weapon);
            }
            return GetMaxQuality(wParams.CreatureLevel, weapon);
        }
    }
}