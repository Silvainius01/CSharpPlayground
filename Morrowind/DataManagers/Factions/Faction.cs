using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Morrowind
{
    class Faction
    {
        public string Name { get; set; }
        public bool IsJoinable { get; set; }
        public bool MainQuestRequired { get; set; }
        public List<string> FavoredSkills { get; set; } = new();
        public List<string> FavoredAttributes { get; set; } = new();
        public List<string> IncompatibleFactions { get; set; } = new();
        public List<string> AdvancedPairings { get; set; } = new();

        public Faction(string name) { Name = name; }
    }
}
