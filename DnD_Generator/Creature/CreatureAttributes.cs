using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using GameEngine;

namespace DnD_Generator
{
    public enum AttributeType { STR, DEX, CON, INT, WIS, CHA }

    public class CreatureAttributes : IEnumerable    
    {
        static AttributeType[] AttributeTypes = Mathc.GetEnumValues<AttributeType>();
        public Dictionary<AttributeType, int> Attributes = new Dictionary<AttributeType, int>();

        public int Strength 
        {
            get { return Attributes[AttributeType.STR]; }
            set { Attributes[AttributeType.STR] = value; }
        }
        
        public int Dexterity
        {
            get { return Attributes[AttributeType.DEX]; }
            set { Attributes[AttributeType.DEX] = value; }
        }

        public int Consitution
        {
            get { return Attributes[AttributeType.CON]; }
            set { Attributes[AttributeType.CON] = value; }
        }

        public int Intelligence
        {
            get { return Attributes[AttributeType.INT]; }
            set { Attributes[AttributeType.INT] = value; }
        }

        public int Wisdom
        {
            get { return Attributes[AttributeType.WIS]; }
            set { Attributes[AttributeType.WIS] = value; }
        }

        public int Charisma
        {
            get { return Attributes[AttributeType.CHA]; }
            set { Attributes[AttributeType.CHA] = value; }
        }

        public int this[AttributeType key]
        {
            get => Attributes[key];
            set => Attributes[key] = value;
        }

        public CreatureAttributes()
        {
            for (int i = 0; i < AttributeTypes.Length; ++i)
                Attributes.Add(AttributeTypes[i], 0);
        }

        public CreatureAttributes(int baseValue)
        {
            for (int i = 0; i < AttributeTypes.Length; ++i)
                Attributes.Add(AttributeTypes[i], baseValue);
        }

        public IEnumerator GetEnumerator()
        {
            return Attributes.GetEnumerator();
        }
    }
}
