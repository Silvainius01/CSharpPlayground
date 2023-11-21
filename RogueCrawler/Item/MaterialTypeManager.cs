using CommandEngine;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueCrawler
{
    class MaterialTypeManager
    {
        public static string DataPath = $"{DungeonCrawlerManager.TextPath}\\Data\\ItemMaterials.json";

        public static bool Loaded = false;
        public static ItemMaterial DefaultMaterial => Materials["Iron"];
        public static Dictionary<string, ItemMaterial> Materials = new Dictionary<string, ItemMaterial>();

        public static MappedCommandModule<ItemMaterial> MaterialNameCommandModule;

        public static void LoadMaterials()
        {
            StreamReader reader = new StreamReader(DataPath);
            string json = reader.ReadToEnd();
            reader.Close();

            var serializer = JsonSerializer.CreateDefault();
            var jArray = JsonConvert.DeserializeObject<JArray>(json);

            foreach (var obj in jArray)
            {
                ItemMaterial data = (ItemMaterial)serializer.Deserialize(new JTokenReader(obj), typeof(ItemMaterial));
                Materials.Add(data.Name, data);
            }
            Loaded = true;
            MaterialNameCommandModule = new MappedCommandModule<ItemMaterial>("What is the default material name prompt??", Materials);
        }

        public static ItemMaterial GetMaterialFromName(string name)
        {
            if (name is null || !Materials.ContainsKey(name))
            {
                ConsoleExt.WriteWarning($"Material '{name}' doesnt exist. Defaulting to {DefaultMaterial.Name}.");
                return DefaultMaterial;
            }
            return Materials[name];
        }
    }
}
