using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using GameEngine;

namespace CSharpPlayground.Wumpus
{
    public enum ITEM_ID {
        TORCH, BOW, ARROW
    }
    public static class ItemComparer
    {
        public static bool Equals(this ITEM_ID item, string str)
        {
            return item.ToString() == str.ToUpper();
        }
    }
}