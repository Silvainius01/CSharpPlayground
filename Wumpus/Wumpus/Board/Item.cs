using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CommandEngine;

namespace CSharpPlayground.Wumpus
{
    public enum ITEM_ID {
        NONE, TORCH, BOW, ARROW
    }
    public static class ItemManager
    {
        public static readonly ITEM_ID[] itemArray = EnumExt<ITEM_ID>.Values;
        public static readonly Dictionary<string, ITEM_ID> stringToItem = itemArray.ToDictionary(itemID => itemID.ToString());

        public static bool Equals(this ITEM_ID item, string str)
        {
            return stringToItem.ContainsKey(str.ToUpper());
        }

        public static bool Equals(this string str, ITEM_ID item)
        {
            return stringToItem.ContainsKey(str.ToUpper());
        }

        public static ITEM_ID StringToItem(string str)
        {
            str = str.ToUpper();
            return stringToItem.ContainsKey(str) ? stringToItem[str] : ITEM_ID.NONE;
        }
        public static bool StringToItem(string str, out ITEM_ID result)
        {
            result = StringToItem(str);
            return result != ITEM_ID.NONE;
        }
    }
}