using System;
using System.Collections.Generic;
using System.Text;

namespace DnD_Generator
{
    interface IItem : IInspectable, IDungeonObject
    {
        int Value { get; set; }
        float Weight { get; set; }
        float Quality { get; set; }

        int GetValue();
        float GetRawValue();
        //float GetQualityAtLevel(int level);
    }
}
