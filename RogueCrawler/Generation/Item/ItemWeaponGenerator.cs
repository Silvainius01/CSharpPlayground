using CommandEngine;
using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using RogueCrawler.Item.Weapon;
using System.Runtime.Serialization;

namespace RogueCrawler
{
    class WeaponTypeData
    {
        public string WeaponType { get; set; }
        public AttributeType MajorAttribute { get; set; }
        public AttributeType MinorAttribute { get; set; }
        public AttributeType DamageAttribute { get; set; }
        public ItemWeaponHandedness Handedness { get; set; }

        public int BaseDamage { get; set; }
        public int PrimaryAttributeBaseReq { get; set; }
        public int SecondaryAttributeBaseReq { get; set; }
        public float LargeWeaponDamageMult { get; set; } = 2;
        public float LargeWeaponWeightMult { get; set; } = 3;
        public string[] OneHandedWeaponNames { get; set; }
        public string[] TwoHandedWeaponNames { get; set; }
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

            WeaponTypeData weaponTypeData = WeaponTypes[weaponType];
            ItemWeapon weapon = new ItemWeapon()
            {
                ID = NextId,
                Weight = CommandEngine.Random.NextInt(wParams.WeightRange, true) / 10.0f,
                IsLargeWeapon = IsLargeWeapon(weaponTypeData, wParams),
                WeaponType = weaponType,
                MajorAttribute = weaponTypeData.MajorAttribute,
                MinorAttribute = weaponTypeData.MinorAttribute,
                BaseDamage = weaponTypeData.BaseDamage,
                Material = MaterialTypeManager.Materials.Values.RandomItem()
            };
            weapon.Quality = GetQuality(wParams, weapon);
            weapon.ObjectName = GetWeaponName(weaponTypeData, weapon.IsLargeWeapon);
            weapon.ItemName = GetDisplayName(weapon);

            if (weapon.IsLargeWeapon)
            {
                weapon.BaseDamage *= weaponTypeData.LargeWeaponDamageMult;
                weapon.Weight *= weaponTypeData.LargeWeaponWeightMult;
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
                ItemName = serialized.ItemName,
                ObjectName = serialized.ObjectName,
                Weight = serialized.Weight,
                IsLargeWeapon = serialized.IsLargeWeapon,
                WeaponType = weaponType,
                Quality = serialized.Quality,
                BaseDamage = serialized.BaseDamage,
                MinorAttribute = WeaponTypes[weaponType].MinorAttribute,
                MajorAttribute = WeaponTypes[weaponType].MajorAttribute,
                Material = MaterialTypeManager.GetMaterialFromName(serialized.MaterialName)
            };

            weapon.AttributeRequirements = GenerateAttributeRequirements(weapon);
            weapon.Value = weapon.GetValue();

            return weapon;
        }

        public ItemWeapon GenerateUnarmed(Creature c)
        {
            return new ItemWeapon()
            {
                ID = -1,
                BaseDamage = 1,
                Weight = 1.0f,
                Quality = 1.0f,
                Value = 0,
                ObjectName = "Unarmed",
                WeaponType = "Blunt",
                ItemName = "Bare Fists",
                IsLargeWeapon = false,
                MajorAttribute = AttributeType.DEX,
                MinorAttribute = AttributeType.STR,
                AttributeRequirements = new CrawlerAttributeSet(0),
                Material = MaterialTypeManager.DefaultMaterial
            };
        }

        CrawlerAttributeSet GenerateAttributeRequirements(ItemWeapon weapon)
        {
            var weaponTypeData = WeaponTypes[weapon.WeaponType];
            CrawlerAttributeSet req = new CrawlerAttributeSet();

            req[AttributeType.STR] = GetStrengthReq(weapon.Weight);
            req[weaponTypeData.MajorAttribute] += (int)Math.Ceiling(weapon.Quality * GetPrimaryStatReq(weapon));
            req[weaponTypeData.MinorAttribute] += (int)Math.Ceiling(weapon.Quality * GetSecondaryStatReq(weapon));

            return req;
        }

        string GetWeaponName(WeaponTypeData typeData, bool isLarge)
        {
            return isLarge
                ? typeData.TwoHandedWeaponNames.RandomItem()
                : typeData.OneHandedWeaponNames.RandomItem();
        }
        string GetDisplayName(ItemWeapon weapon)
        {
            StringBuilder builder = new StringBuilder();

            string QualityPrefix(float quality) => quality switch
            {
                 < 0 => "Broken",
                 < 1 => "PoorQuality",
                 < 2 => string.Empty,
                 < 3 => "HighQuality",
                 < 4 => "Renowned",
                >= 4 => "Legendary",
                _ => "Anomalous"
            };

            //if (weapon.IsLargeWeapon)
            //    builder.Append("Large");
            builder.Append(QualityPrefix(weapon.Quality));
            builder.Append(weapon.Material.Name);
            builder.Append(weapon.ObjectName);
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
                int level = (int)(
                    CommandEngine.Random.GetMarsagliaBetween(levelRange) +
                    DungeonGenerator.GetLevelBias(wParams.QualityBias));
                if (wParams.CapToCreatureLevel && level > wParams.CreatureLevel)
                    level = wParams.CreatureLevel;
                return GetMaxQuality(level, weapon);
            }
            return GetMaxQuality(wParams.CreatureLevel, weapon);
        }

        bool IsLargeWeapon(WeaponTypeData weaponType, ItemWeaponGenerationParameters wParams)
        {
            return 
                weaponType.Handedness != ItemWeaponHandedness.One && (
                weaponType.Handedness == ItemWeaponHandedness.Two ||
                CommandEngine.Random.NextInt(100) < wParams.LargeWeaponProbability);
        }
    }
}