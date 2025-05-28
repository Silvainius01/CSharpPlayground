using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace RogueCrawler
{
    class ItemArmor : IItem
    {
        public int ID { get; set; }
        public int Level
        {
            get => 1;
            set { throw new InvalidOperationException("Cannot directly set the level of armor."); }
        }

        public int BaseValue { get; set; }
        public float Weight { get; set; }
        public float Quality { get; set; }
        public ItemMaterial Material { get; set; }

        public string ItemName { get; set; } // Display Name
        public string ObjectName { get; set; } // Armor Type
        public string ArmorClass { get; set; } // Armor Skill

        public int BaseArmorRating { get; set; } = 1;
        public float Condition { get; set; } = 1;
        public float MaxCondition { get; set; } = 1;

        public ItemArmorSlotType SlotType { get; set; }

        float ArmorSlotModifier()
        {
            switch(SlotType)
            {
                case ItemArmorSlotType.Head:    return 0.05f;
                case ItemArmorSlotType.Chest:   return 0.35f;
                case ItemArmorSlotType.Arm:     return 0.10f;
                case ItemArmorSlotType.Hand:    return 0.05f;
                case ItemArmorSlotType.Waist:   return 0.35f;
                case ItemArmorSlotType.Foot:    return 0.10f;
            }
            return 1.0f;
        }
        public float GetArmorRating()
        {
            return BaseArmorRating 
                * Material.ArmorModifier 
                * ArmorSlotModifier() 
                * MathF.Log2(Quality + 1);
        }

        public int GetValue()
        {
            throw new NotImplementedException();
        }
        public float GetRawValue()
        {
            throw new NotImplementedException();
        }

        public string BriefString()
        {
            throw new NotImplementedException();
        }
        public string DebugString(string prefix, int tabCount)
        {
            throw new NotImplementedException();
        }
        public string InspectString(string prefix, int tabCount)
        {
            throw new NotImplementedException();
        }

        public SerializedItem GetSerializable()
        {
            throw new NotImplementedException();
        }
    }

    class SerializedArmor : SerializedItem
    {
        public string ArmorClass { get; set; }

        public int BaseArmorRating { get; set; }
        public float Condition { get; set; }
        public float MaxCondition { get; set; } 

        public ItemArmorSlotType SlotType { get; set; }

        public override IItem GetDeserialized()
        {
            throw new NotImplementedException();
        }
    }
}
