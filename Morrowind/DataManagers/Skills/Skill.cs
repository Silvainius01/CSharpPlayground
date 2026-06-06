using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Morrowind
{
    class Skill
    {
        public string Name { get; set; }
        public string GoverningAttribute { get; set; }
        public string Specialization { get; set; }

        public Skill(string name) { Name = name; }
    }
}
