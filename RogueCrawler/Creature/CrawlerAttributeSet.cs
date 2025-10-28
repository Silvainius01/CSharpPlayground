using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CommandEngine;
using Newtonsoft.Json.Linq;

namespace RogueCrawler
{
    class CrawlerAttributeSet : IEnumerable<KeyValuePair<AttributeType, int>>, IInspectable, ISerializable<SerializedAttributes, CrawlerAttributeSet>
    {
        Dictionary<AttributeType, int> Attributes = new Dictionary<AttributeType, int>();

        private int totalScore = 0;

        public int this[AttributeType key]
        {
            get => GetAttribute(key);
            set => SetAttribute(key, value);
        }

        public int TotalScore
        {
            get => totalScore;
        }
        public int CreatureLevel => GetAttributeLevel(DungeonSettings.AttributePointsPerCreatureLevel);

        public CrawlerAttributeSet(int baseValue = 0)
        {
            for (int i = 0; i < EnumExt<AttributeType>.Count; ++i)
                Attributes.Add(EnumExt<AttributeType>.Values[i], 0);
        }
        public CrawlerAttributeSet(CrawlerAttributeSet otherAttributes) : this((attr) => otherAttributes[attr]) { }
        public CrawlerAttributeSet(Func<AttributeType, int> determineAttribute) : this() { SetAttributes(determineAttribute); }

        public void AddAttribute(AttributeType attr, int amt)
        {
            Attributes[attr] += amt;
            UpdateMetaData();
        }
        public void SetAttribute(AttributeType attr, int amt)
        {
            Attributes[attr] = amt;
            UpdateMetaData();
        }

        public void SetAttributes(int value)
        {
            foreach (AttributeType attr in EnumExt<AttributeType>.Values)
                Attributes[attr] = value;
            UpdateMetaData();
        }
        public void SetAttributes(Func<AttributeType, int> determineAttribute)
        {
            foreach (AttributeType attr in EnumExt<AttributeType>.Values)
                Attributes[attr] = determineAttribute(attr);
            UpdateMetaData();
        }
        public int GetAttribute(AttributeType attribute)
        {
            if (Attributes.ContainsKey(attribute))
                return Attributes[attribute];
            return 0;
        }

        private void UpdateMetaData()
        {
            totalScore = Attributes.Aggregate(0, (total, kvp) => total + kvp.Value);
        }

        public string BriefString()
        {
            return ToString();
        }
        public string InspectString(string prefix, int tabCount)
        {
            SmartStringBuilder builder = new SmartStringBuilder(DungeonSettings.TabString); ;

            if (prefix == string.Empty)
                prefix = "Attributes:";
            builder.Append(tabCount, prefix);

            tabCount++;
            foreach (var kvp in Attributes)
                if (kvp.Value > 0)
                    builder.NewlineAppend(tabCount, $"{kvp.Key}: {kvp.Value}");
            tabCount--;
            return builder.ToString();
        }
        public string DebugString(string prefix, int tabCount)
        {
            SmartStringBuilder builder = new SmartStringBuilder(DungeonSettings.TabString); ;

            if (prefix == string.Empty)
                prefix = "Attributes:";
            builder.Append(tabCount, prefix);

            tabCount++;
            foreach (var kvp in Attributes)
                builder.NewlineAppend(tabCount, $"{kvp.Key}: {kvp.Value}");
            tabCount--;
            return builder.ToString();
        }
        public override string ToString()
        {
            return Attributes.ToString((kvp) => $"{kvp.Key}:{kvp.Value}", " ");
        }

        public SerializedAttributes GetSerializable()
        {
            return new SerializedAttributes(this);
        }

        public int GetAttributeLevel(int pointsPerLevel)
            => (int)Math.Ceiling((double)totalScore / pointsPerLevel);
        public int GetMissingPoints(int pointsPerLevel)
            => (pointsPerLevel * GetAttributeLevel(pointsPerLevel)) - totalScore;

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Attributes.GetEnumerator();
        }
        public IEnumerator<KeyValuePair<AttributeType, int>> GetEnumerator()
        {
            return Attributes.GetEnumerator();
        }
    }
}
