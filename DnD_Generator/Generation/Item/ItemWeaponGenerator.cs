using CommandEngine;
using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace DnD_Generator
{
    class WeaponTypeData
    {
        public string WeaponType { get; set; }
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
        static Dictionary<string, WeaponTypeData> WeaponTypes => WeaponTypeManager.WeaponTypes;

        public override ItemWeapon Generate(ItemWeaponGenerationParameters wParams)
        {
            wParams.Validate();

            string weaponType = null;

            if (!wParams.PossibleWeaponTypes.Any())
                weaponType = WeaponTypeManager.RandomType;
            else weaponType = wParams.PossibleWeaponTypes.RandomItem();

            ItemWeapon weapon = new ItemWeapon()
            {
                ID = NextId,
                Weight = Mathc.Random.NextInt(wParams.WeightRange, true) / 10.0f,
                IsLargeWeapon = Mathc.Random.NextInt(100) < wParams.LargeWeaponProbability,
                WeaponType = weaponType,
                DamageBonusAttribute = WeaponTypes[weaponType].DamageAttribute,
                HitBonusAttribute = WeaponTypes[weaponType].ToHitAttribute,
                BaseDamage = WeaponTypes[weaponType].BaseDamage
            };
            weapon.Quality = GetQuality(wParams, weapon);
            weapon.Name = GetName(weapon);

            if (weapon.IsLargeWeapon)
            {
                weapon.BaseDamage *= WeaponTypes[weaponType].LargeWeaponDamageMult;
                weapon.Weight *= WeaponTypes[weaponType].LargeWeaponWeightMult;
            }

            weapon.AttributeRequirements = GenerateAttributeRequirements(weapon);
            weapon.Value = weapon.GetValue();

            return weapon;
        }

        public ItemWeapon FromSerializable(SerializedWeapon serialized)
        {
            var weaponType = serialized.WeaponType;
            ItemWeapon weapon = new ItemWeapon()
            {
                ID = NextId,
                Name = serialized.Name,
                Weight = serialized.Weight,
                IsLargeWeapon = serialized.IsLargeWeapon,
                WeaponType = weaponType,
                Quality = serialized.Quality,
                BaseDamage = serialized.BaseDamage,
                DamageBonusAttribute = WeaponTypes[weaponType].DamageAttribute,
                HitBonusAttribute = WeaponTypes[weaponType].ToHitAttribute,
            };

            weapon.AttributeRequirements = GenerateAttributeRequirements(weapon);
            weapon.Value = weapon.GetValue();

            return weapon;
        }

        CrawlerAttributeSet GenerateAttributeRequirements(ItemWeapon weapon)
        {
            var weaponTypeData = WeaponTypes[weapon.WeaponType];
            CrawlerAttributeSet req = new CrawlerAttributeSet();

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
            => WeaponTypes[weapon.WeaponType].PrimaryAttributeBaseReq;
        
        /// <summary>Get the stat requirement for Quality 1.0 weapon</summary>
        int GetSecondaryStatReq(ItemWeapon weapon)
            => WeaponTypes[weapon.WeaponType].SecondaryAttributeBaseReq;
        
        float GetMaxQuality(int level, ItemWeapon weapon)
        {
            if (level <= 0)
                return 0;

            int maxAttrPoints = (level * DungeonCrawlerSettings.AttributePointsPerWeaponLevel) - GetStrengthReq(weapon.Weight);
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