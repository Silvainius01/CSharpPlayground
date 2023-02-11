using System;
using System.Collections.Generic;
using System.Text;

namespace RogueCrawler
{
    class ItemArmor : IItem
    {
        public int ID { get; set; }
        public int Level { get; set; }
        public string Name { get; set; }

        public int Value { get; set; }
        public float Weight { get; set; }
        public float Quality { get; set; }
        public ItemArmorSlotType SlotType { get; set; }

        public string BriefString()
        {
            throw new NotImplementedException();
        }

        public string DebugString(string prefix, int tabCount)
        {
            throw new NotImplementedException();
        }

        public float GetRawValue()
        {
            throw new NotImplementedException();
        }

        public SerializedItem GetSerializable()
        {
            throw new NotImplementedException();
        }

        public int GetValue()
        {
            throw new NotImplementedException();
        }

        public string InspectString(string prefix, int tabCount)
        {
            throw new NotImplementedException();
        }
    }
}
