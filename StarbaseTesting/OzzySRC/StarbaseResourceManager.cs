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
    class StarbaseResourceManager
    {
        public static bool IsInited = false;
        public static Dictionary<string, StarbaseResource> AllResources = new Dictionary<string, StarbaseResource>();

        public static void LoadResources()
        {
            List<StarbaseResource> resources = new List<StarbaseResource>()
            {
                new StarbaseResource("Aegisium", StarbaseResourceType.Ore, "Aeg"),
                new StarbaseResource("Ajatite", StarbaseResourceType.Ore, "Aja"),
                new StarbaseResource("Arkanium", StarbaseResourceType.Ore, "Ark"),
                new StarbaseResource("Bastium", StarbaseResourceType.Ore, "Bas"),
                new StarbaseResource("Charodium", StarbaseResourceType.Ore, "Cha"),
                new StarbaseResource("Corazium", StarbaseResourceType.Ore, "Cor"),
                new StarbaseResource("Daltium", StarbaseResourceType.Ore, "Dal"),
                new StarbaseResource("Exorium", StarbaseResourceType.Ore, "Exo"),
                new StarbaseResource("Haderite", StarbaseResourceType.Ore, "Had"),
                new StarbaseResource("Ice", StarbaseResourceType.Ore),
                new StarbaseResource("Ilmatrium", StarbaseResourceType.Ore, "Ilm"),
                new StarbaseResource("Karnite", StarbaseResourceType.Ore, "Kar"),
                new StarbaseResource("Kutonium", StarbaseResourceType.Ore, "Kut"),
                new StarbaseResource("Lukium", StarbaseResourceType.Ore, "Luk"),
                new StarbaseResource("Merkerium", StarbaseResourceType.Ore, "Mer"),
                new StarbaseResource("Naflite", StarbaseResourceType.Ore, "Naf"),
                new StarbaseResource("Nhurgite", StarbaseResourceType.Ore, "Nhu"),
                new StarbaseResource("Oninum", StarbaseResourceType.Ore, "Oni"),
                new StarbaseResource("Surtrite", StarbaseResourceType.Ore, "Sur"),
                new StarbaseResource("Targium", StarbaseResourceType.Ore, "Tar"),
                new StarbaseResource("Tengium", StarbaseResourceType.Ore, "Ten"),
                new StarbaseResource("Ukonium", StarbaseResourceType.Ore, "Uko"),
                new StarbaseResource("Valkite", StarbaseResourceType.Ore, "Val"),
                new StarbaseResource("Vokarium", StarbaseResourceType.Ore, "Vok"),
                new StarbaseResource("Xhalium", StarbaseResourceType.Ore, "Xha"),
                new StarbaseResource("Ymrium", StarbaseResourceType.Ore, "Ymr"),
                new StarbaseResource("Cube", StarbaseResourceType.Research, "red", "r"),
                new StarbaseResource("Power", StarbaseResourceType.Research, "yellow", "y"),
                new StarbaseResource("Shield", StarbaseResourceType.Research, "blue", "b"),
                new StarbaseResource("Gear", StarbaseResourceType.Research, "purple", "p"),
            };
            foreach (var r in resources)
                AddResource(r);

            IsInited = true;
        }
        public static void AddResource(StarbaseResource r)
        {
            if (AllResources.ContainsKey(r.Name))
            {
                CommandEngine.ConsoleExt.WriteLine($"Resource '{r.Name}' already exists in the dataset.", ConsoleColor.Yellow);
                return;
            }

            StringBuilder sb = new StringBuilder($"Added resource '{r.Name}' with aliases: ");
            AllResources.Add(r.Name, r);

            foreach (var str in r.Aliases)
            {
                if (AllResources.ContainsKey(str))
                {
                    StarbaseResource conflict = AllResources[str];
                    CommandEngine.ConsoleExt.WriteLine($"Alias '{str}' for resource '{r.Name}' conflicts with resource '{conflict.Name}'.", ConsoleColor.Red);
                    continue;
                }
                AllResources.Add(str, r);
                sb.Append($"'{str}' ");
            }

            Console.WriteLine(sb.ToString());
        }

        public static bool TryGetResource(string name, out StarbaseResource resource) => AllResources.TryGetValue(name, out resource);

        public static bool ResourceExists(string name) => AllResources.ContainsKey(name);
    }
}
