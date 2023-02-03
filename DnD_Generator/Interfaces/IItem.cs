using System;
using System.Collections.Generic;
using System.Text;

namespace DnD_Generator
{
    interface IItem : IInspectable, IDungeonObject, ISerializable<SerializedItem>
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
