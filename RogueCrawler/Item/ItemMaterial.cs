using CommandEngine;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueCrawler
{
    class ItemMaterial : IInspectable, ISerializable<SerializedItemMaterial, ItemMaterial>
    {
        public string Name { get; set; } = "Anomolous";
        public float ValueModifier { get; set; } = 1.0f;
        public float WeightModifier { get; set; } = 1.0f;
        public float QualityModifier { get; set; } = 1.0f;
        public float DamageModifier { get; set; } = 1.0f;
        public float ArmorModifier { get; set; } = 1.0f;

        public bool IsMetallic { get; set; } = true;
        public bool IsWeaponMaterial { get; set; } = true;
        public bool IsArmorMaterial { get; set; } = true;

        public string BriefString()
        {
            throw new NotImplementedException();
        }
        public string DebugString(string prefix, int tabCount)
        {
            throw new NotImplementedException();
        }
        public string InspectString(string prefix, int tabCount)
        {
            throw new NotImplementedException();
        }

        public SerializedItemMaterial GetSerializable()
        {
            return new SerializedItemMaterial()
            {
                Name = Name,
                ValueModifier = ValueModifier,
                WeightModifier = WeightModifier,
                QualityModifier = QualityModifier,
                DamageModifier = DamageModifier,
                ArmorModifier = ArmorModifier,
                IsWeaponMaterial = IsWeaponMaterial,
                IsArmorMaterial = IsArmorMaterial,
                IsMetallic = IsMetallic,
            };
        }
    }

    class SerializedItemMaterial : ISerialized<ItemMaterial>
    {
        public string Name { get; set; }
        public float ValueModifier { get; set; }
        public float WeightModifier { get; set; }
        public float QualityModifier { get; set; }
        public float DamageModifier { get; set; }
        public float ArmorModifier { get; set; }

        public bool IsMetallic { get; set; } = true;
        public bool IsWeaponMaterial { get; set; } = false;
        public bool IsArmorMaterial { get; set; } = false;

        public ItemMaterial GetDeserialized()
        {
            return new ItemMaterial()
            {
                Name = Name,
                ValueModifier = ValueModifier,
                WeightModifier = WeightModifier,
                QualityModifier = QualityModifier,
                DamageModifier = DamageModifier,
                ArmorModifier = ArmorModifier,
                IsWeaponMaterial = IsWeaponMaterial,
                IsArmorMaterial = IsArmorMaterial,
                IsMetallic = IsMetallic
            };
        }
    }
}
