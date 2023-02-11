using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.CompilerServices;

namespace RogueCrawler
{
    interface ISerializable<TSerialized, TBase> where TSerialized : ISerialized<TBase>
    {
        public TSerialized GetSerializable();
    }

    interface ISerialized<TBase>
    {
        public TBase GetDeserialized();
    }
}
