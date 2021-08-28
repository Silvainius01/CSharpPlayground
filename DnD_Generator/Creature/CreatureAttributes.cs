using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using GameEngine;

namespace DnD_Generator
{

    class CreatureAttributes : IEnumerable<KeyValuePair<AttributeType, int>>, IInspectable
    {
        Dictionary<AttributeType, int> Attributes = new Dictionary<AttributeType, int>();

        private int totalScore = 0;
        private int minLevel = 0;
        
        public int this[AttributeType key]
        {
            get => Attributes[key];
            set => SetAttribute(key, value);
        }

        public int TotalScore
        {
            get => totalScore;
        }
        /// <summary>Minimum creature level to attain the attribute score naturally</summary>
        public int Level
        {
            get => minLevel;
        }

        public CreatureAttributes() { SetAttributes(0); }
        public CreatureAttributes(int baseValue) { SetAttributes(baseValue); }
        public CreatureAttributes(CreatureAttributes otherAttributes) : this((attr) => otherAttributes[attr]) {}
        public CreatureAttributes(Func<AttributeType, int> determineAttribute) { SetAttributes(determineAttribute); }

        void AddAttribute(AttributeType attr, int amt)
        {
            Attributes[attr] += amt;
            UpdateMetaData();
        }
        void SetAttribute(AttributeType attr, int amt)
        {
            Attributes[attr] = amt;
            UpdateMetaData();
        }

        public void SetAttributes(int value)
        {
            for (int i = 0; i < EnumExt<AttributeType>.Count; ++i)
                Attributes.Add(EnumExt<AttributeType>.Values[i], value);
            UpdateMetaData();
        }
        public void SetAttributes(Func<AttributeType, int> determineAttribute)
        {
            foreach(var attr in EnumExt<AttributeType>.Values)
                Attributes.Add(attr, determineAttribute(attr));
            UpdateMetaData();
        }

        private void UpdateMetaData()
        {
            totalScore = Attributes.Aggregate(0, (total, kvp) => total + kvp.Value);
            minLevel = (int)Math.Ceiling((double)totalScore / DungeonCrawlerSettings.AttributePointsPerLevel);
        }

        IEnumerator<KeyValuePair<AttributeType, int>> IEnumerable<KeyValuePair<AttributeType, int>>.GetEnumerator()
        {
            return Attributes.GetEnumerator();
        }

        public static int GetMissingPoints(CreatureAttributes attributes)
            => (attributes.Level * DungeonCrawlerSettings.AttributePointsPerLevel) - attributes.TotalScore;

        public string BriefString()
        {
            return ToString();
        }
        public string InspectString(string prefix, int tabCount)
        {
            SmartStringBuilder builder = new SmartStringBuilder(DungeonCrawlerSettings.TabString);;

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
            SmartStringBuilder builder = new SmartStringBuilder(DungeonCrawlerSettings.TabString);;

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

        public IEnumerator GetEnumerator()
        {
            return Attributes.GetEnumerator();
        }
    }
}
