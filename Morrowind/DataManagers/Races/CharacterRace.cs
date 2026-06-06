using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Morrowind
{
    class CharacterRace
    {
        public string Name { get; set; }
        public Dictionary<string, int> SkillBonuses { get; set; }
        public Dictionary<string, int> AttributeBonusesMale { get; set; }
        public Dictionary<string, int> AttributeBonusesFemale { get; set; }

        public CharacterRace(string name) { Name = name; }
    }
}
