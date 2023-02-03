using System;
using System.Collections.Generic;
using System.Text;

namespace DnD_Generator
{
    interface ISerializable<TSerialized>
    {
        public TSerialized GetSerializable();
    }

    interface ISerialized<TBase>
    {
        public TBase GetDeserialized();
    }
}
