﻿using CommandEngine;
using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace RogueCrawler
{
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
                BaseValue = weaponTypeData.BaseValue,
                Material = wParams.Material,
                Quality = GetQuality(wParams),
                AttributeRequirements = new CrawlerAttributeSet()
            };

            weapon.AttributeRequirements.SetAttribute(AttributeType.STR, (int)Math.Ceiling(weapon.Weight / DungeonCrawlerSettings.WeaponWeightPerStr));
            weapon.ObjectName = GetWeaponName(weaponTypeData, weapon.IsLargeWeapon);
            weapon.ItemName = GetDisplayName(weapon);

            if (weapon.IsLargeWeapon)
            {
                weapon.BaseDamage *= weaponTypeData.LargeWeaponDamageMult;
                weapon.Weight *= weaponTypeData.LargeWeaponWeightMult;
            }

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
                BaseValue = serialized.BaseValue,
                MinorAttribute = WeaponTypes[weaponType].MinorAttribute,
                MajorAttribute = WeaponTypes[weaponType].MajorAttribute,
                Material = MaterialTypeManager.GetMaterialFromName(serialized.MaterialName)
            };

            return weapon;
        }

        public ItemWeapon GenerateUnarmed(Creature c)
        {
            return new ItemWeapon()
            {
                ID = -1,
                BaseDamage = c.GetAttribute(AttributeType.STR),
                Weight = 1.0f,
                Quality = 1.0f,
                BaseValue = 0,
                ObjectName = "Unarmed",
                WeaponType = "Blunt",
                ItemName = "Bare Fists",
                IsLargeWeapon = false,
                MajorAttribute = AttributeType.DEX,
                MinorAttribute = AttributeType.STR,
                Material = MaterialTypeManager.DefaultMaterial
            };
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
                <= 0 => "Broken",       // Unusable
                 < 1 => "Rusty",        // Less than base (0-1)
                 < 3 => string.Empty,   // Base+ Damage (1-3) 
                 < 7 => "Superior",     // Double+ (3-7)
                 < 15 => "Exalted",    // Triple+ (7-15)
                >= 15 => "Legendary",   // Quadruple+ (15+)
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
        
        float GetQuality(ItemWeaponGenerationParameters wParams)
            => wParams.QualityOverride < 0.0f
                ? DungeonGenerator.GetItemQuality(wParams.Quality, wParams.QualityBias)
                : wParams.QualityOverride;

        bool IsLargeWeapon(WeaponTypeData weaponType, ItemWeaponGenerationParameters wParams)
        {
            return 
                weaponType.Handedness != ItemWeaponHandedness.One && (
                weaponType.Handedness == ItemWeaponHandedness.Two ||
                CommandEngine.Random.NextInt(100) < wParams.LargeWeaponProbability);
        }
    }
}