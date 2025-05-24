using System;
using System.Collections.Generic;
using System.Text;

namespace RogueCrawler
{
    interface IItem : IInspectable, IDungeonObject, ISerializable<SerializedItem, IItem>
    {
        int Level { get; set; }
        int BaseValue { get; set; }
        float Weight { get; set; }
        float Quality { get; set; }
        string ItemName { get; set; }
        ItemMaterial Material { get; set; }

        int GetValue();
        float GetRawValue();
    }

    abstract class SerializedItem : ISerialized<IItem>
    {
        public int Count { get; set; }
        public int BaseValue { get; set; }
        public float Weight { get; set; }
        public float Quality { get; set; }
        public string ItemName { get; set; }
        public string ObjectName { get; set; }
        public string MaterialName { get; set; }

        public abstract IItem GetDeserialized();
    }
}
