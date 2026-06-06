using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Morrowind.Data
{
    class SpellEffect
    {
        public string Name { get; set; }
        public float BaseCost { get; set; }
        public bool IsNegative { get; set; }

        public SpellEffect(string name) { Name = name; }
    }
}
