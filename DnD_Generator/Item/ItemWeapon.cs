using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameEngine;

namespace DnD_Generator
{
    public enum WeaponType { Blade, Ranged, Axe, Blunt }

    public class ItemWeapon : IItem
    {
        public int ID { get; set; }
        public float BaseDamage { get; set; }
        public float Weight { get; set; }
        public float Quality { get; set; }
        public string Name { get; set; }
        public WeaponType WeaponType { get; set; }
        public bool IsLargeWeapon { get; set; }

        public AttributeType HitBonusAttribute { get; set; }
        public AttributeType DamageBonusAttribute { get; set; }
        public CreatureAttributes AttributeRequirements { get; set; }

        public string DebugString()
        {
            float valueRaw = ((IsLargeWeapon ? 1 : 2) + AttributeRequirements.Attributes.Values.Sum()) * Quality;
            int value = (int)(valueRaw) + 1;
            float expectedDamage = BaseDamage * Quality + AttributeRequirements[DamageBonusAttribute];
            StringBuilder builder = new StringBuilder($"Weapon Stats for {Name}:");

            builder.Append($"\n\tBase Damage: {BaseDamage}");
            builder.Append($"\n\tMin Expected Damage: {expectedDamage}");
            builder.Append($"\n\tQuality: {Quality}");
            builder.Append($"\n\tWeight: {Weight}");
            builder.Append($"\n\tTwo Handed: {IsLargeWeapon}");
            builder.Append($"\n\tValue: {value} ({valueRaw})");
            builder.Append($"\n\tRequirements: ");

            foreach(KeyValuePair<AttributeType, int> kvp in AttributeRequirements)
            {
                if (kvp.Value > 0)
                    builder.Append($"\n\t\t{kvp.Key}: {kvp.Value}");
            }

            return builder.ToString();
        }
    }
}
