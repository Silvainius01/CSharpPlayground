using System;
using System.Collections.Generic;
using System.Text;

namespace RogueCrawler
{
    interface IDungeonObject
    {
        int ID { get; set; }
        string Name { get; set; }
    }
}
