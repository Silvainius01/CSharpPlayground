using System;
using System.Collections.Generic;
using System.Text;
using GameEngine;

namespace DnD_Generator
{
    public class ItemWeaponGenerationProperties
    {
        public Vector2Int WeightRange { get; set; }
        public Vector2Int QualityRange { get; set; }
        public int LargeWeaponProbability { get; set; } = 50;
        public List<WeaponType> PossibleWeaponTypes { get; set; } = new List<WeaponType>();
    }
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

    public class ItemWeaponGenerator
    {
        static int currId = 0;
        static WeaponType[] WeaponTypes = Mathc.GetEnumValues<WeaponType>();
        static Dictionary<WeaponType, WeaponTypeData> WeaponBaseReqs = new Dictionary<WeaponType, WeaponTypeData>()
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


        public static ItemWeapon GenerateWeapon(ItemWeaponGenerationProperties properties)
        {
            var weaponType = properties.PossibleWeaponTypes.Count > 0 
                ? Mathc.GetRandomItemFromList(properties.PossibleWeaponTypes)
                : Mathc.GetRandomItemFromEnumerable(WeaponTypes);

            ItemWeapon weapon = new ItemWeapon()
            {
                Name = weaponType.ToString(),
                Quality = Mathc.Random.NextInt(properties.QualityRange, true) / 10.0f,
                Weight = Mathc.Random.NextInt(properties.WeightRange, true) / 10.0f,
                IsLargeWeapon = Mathc.Random.NextInt(100) < properties.LargeWeaponProbability,
                WeaponType = weaponType,
                DamageBonusAttribute = WeaponBaseReqs[weaponType].DamageAttribute,
                HitBonusAttribute = WeaponBaseReqs[weaponType].ToHitAttribute,
                BaseDamage = WeaponBaseReqs[weaponType].BaseDamage
            };
            weapon.AttributeRequirements = GenerateAttributeRequirements(weapon);

            if (weapon.WeaponType != WeaponType.Ranged && weapon.IsLargeWeapon)
            {
                weapon.BaseDamage *= WeaponBaseReqs[weaponType].LargeWeaponDamageMult;
                weapon.Weight *= WeaponBaseReqs[weaponType].LargeWeaponWeightMult;
            }

            return weapon;
        }
        static CreatureAttributes GenerateAttributeRequirements(ItemWeapon weapon)
        {
            var weaponTypeData = WeaponBaseReqs[weapon.WeaponType];
            CreatureAttributes req = new CreatureAttributes();

            req.Strength = (int)(weapon.Weight / 5.0) + 1;

            req[weaponTypeData.PrimaryAttribute] += weaponTypeData.PrimaryAttributeBaseReq;
            req[weaponTypeData.SecondaryAttribute] += weaponTypeData.SecondaryAttributeBaseReq;

            req[weaponTypeData.PrimaryAttribute] += (int)(weapon.Quality * 2);
            req[weaponTypeData.SecondaryAttribute] += (int)(weapon.Quality * 1.5f);

            return req;
        }
    }
}
