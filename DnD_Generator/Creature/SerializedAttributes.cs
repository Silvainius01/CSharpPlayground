using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using CommandEngine;
using System.Net.Http.Headers;

namespace DnD_Generator
{
    class SerializedAttributes : ISerialized<CrawlerAttributeSet>
    {
        public Dictionary<AttributeType, int> Attributes = new Dictionary<AttributeType, int>();

        public SerializedAttributes() { }
        public SerializedAttributes(CrawlerAttributeSet attributes)
        {
            foreach (var attr in EnumExt<AttributeType>.Values)
                if (attributes[attr] > 0)
                    Attributes.Add(attr, attributes[attr]);
        }

        public CrawlerAttributeSet GetDeserialized()
        {
            return new CrawlerAttributeSet(attr =>
                Attributes.ContainsKey(attr) ? Attributes[attr] : 0
            );
        }
    }
}
