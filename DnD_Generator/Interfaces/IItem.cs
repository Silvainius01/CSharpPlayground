using System;
using System.Collections.Generic;
using System.Text;

namespace RogueCrawler
{
    interface IItem : IInspectable, IDungeonObject, ISerializable<SerializedItem, IItem>
    {
        int Level { get; set; }
        int Value { get; set; }
        float Weight { get; set; }
        float Quality { get; set; }

        int GetValue();
        float GetRawValue();
    }

    abstract class SerializedItem : ISerialized<IItem>
    {
        public string Name { get; set; }
        public int Value { get; set; }
        public float Weight { get; set; }
        public float Quality { get; set; }
        public int Count { get; set; }

        public abstract IItem GetDeserialized();
    }
}
