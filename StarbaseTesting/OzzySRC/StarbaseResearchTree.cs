using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarbaseTesting
{
    class StarbaseResearchTree
    {
        public string Name { get; set; }
        public List<StarbaseResearchNode> allNodes = new List<StarbaseResearchNode>();
        public Dictionary<int, HashSet<string>> nodeLayers = new Dictionary<int, HashSet<string>>();

        public StarbaseResearchTree(string name, IEnumerable<StarbaseResearchNode> nodes)
        {
            this.Name = name;

            foreach(var node in nodes)
            {
                if (node.Tree == this.Name)
                    allNodes.Add(node);
            }
        }
    }
}
