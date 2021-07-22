using System;
using System.Collections.Generic;
using System.Text;

namespace DnD_Generator
{
    public interface IItem
    {
        int ID { get; set; }
        float Weight { get; set; }
        string Name { get; set; }
        float Quality { get; set; }
    }
}
