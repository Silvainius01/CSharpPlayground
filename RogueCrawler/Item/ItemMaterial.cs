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
        public string Name { get; set; }
        public float ValueModifier { get; set; }
        public float WeightModifier { get; set; }
        public float QualityModifier { get; set; }
        public float DamageModifier { get; set; }

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
                DamageModifier = DamageModifier
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

        public ItemMaterial GetDeserialized()
        {
            return new ItemMaterial()
            {
                Name = Name,
                ValueModifier = ValueModifier,
                WeightModifier = WeightModifier,
                QualityModifier = QualityModifier,
                DamageModifier = DamageModifier
            };
        }
    }
}
