using System;
using System.Collections.Generic;
using System.Text;

namespace DnD_Generator
{
    enum ItemArmorSlotType { Head, Body, Legs, Feet, Ring }
    class ItemArmor : IItem
    {
        public int ID { get; set; }
        public float Weight { get; set; }
        public string Name { get; set; }
        public float Quality { get; set; }
        public ItemArmorSlotType SlotType { get; set; }
    }
}
