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

            int index = 0;
            var serializer = JsonSerializer.CreateDefault();
            var jArray = JsonConvert.DeserializeObject<JArray>(json);

            foreach (var obj in jArray)
            {
                ItemMaterial data = (ItemMaterial)serializer.Deserialize(new JTokenReader(obj), typeof(ItemMaterial));
                Materials.Add(data.Name, data);
                ++index;
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

        static bool IsValidMaterial(JToken obj, int index)
        {
            int sbLength = 0;
            string starterString = $"Invalid material detected at position [{index}]";
            SmartStringBuilder sb = new SmartStringBuilder();

            sb.Append(starterString);
            sb.NewlineAppend(1, "Missing Required Fields:");
            sbLength = sb.Length;

            if (!obj.Contains("Name"))
                sb.NewlineAppend(2, "Name -> string");
            if (!obj.Contains("IsMetallic"))
                sb.NewlineAppend(2, "IsMetallic -> bool");
            if (!obj.Contains("IsWeaponMaterial"))
                sb.NewlineAppend(2, "IsWeaponMaterial -> bool");
            if (!obj.Contains("IsArmorMaterial"))
                sb.NewlineAppend(2, "IsArmorMaterial -> bool");

            if (sb.Length > sbLength)
            {
                ConsoleExt.WriteWarningLine(sb.ToString());
                return false;
            }
            return true;
        }
    }
}
