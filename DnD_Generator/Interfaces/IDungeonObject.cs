using System;
using System.Collections.Generic;
using System.Text;

namespace DnD_Generator
{
    interface IDungeonObject
    {
        int ID { get; set; }
        string WeaponName { get; set; }
    }
}
