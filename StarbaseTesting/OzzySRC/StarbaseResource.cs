using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using CommandEngine;
using Newtonsoft.Json;

namespace StarbaseTesting
{
    public enum StarbaseResourceType { Ore, Research, Refined }
    class StarbaseResource
    {
        public const int StackSize = 1728;
        public string Name { get; set; }
        public HashSet<string> Aliases { get; set; }
        public StarbaseResourceType Type { get; set; }

        public StarbaseResource(string name, StarbaseResourceType rType, params string[] aliases)
        {
            Name = name;
            Aliases = new HashSet<string>(aliases);
            Type = rType;
        }

        public override string ToString()
        {
            return Name;
        }
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
