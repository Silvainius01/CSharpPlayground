using CommandEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueCrawler
{
    [Flags]
    internal enum DamageFlags
    {
        True = 0,
        IsBlockable = 1, // If a damage type is blockable, it can be mitigated by armor.
        IsResistable = 2
    }

    internal struct DamageTypeData
    {
        public string Name { get; set; }
        public DamageCategory Category { get; set; }
        public DamageFlags Flags { get; set; }
    }

    internal class DamageTypeManager
    {
        public static string DataPath = $"{DungeonCrawlerManager.TextPath}\\Data\\DamageTypes.json";

        public static bool Loaded = false;
        public static Dictionary<string, DamageTypeData> DamageTypes = new Dictionary<string, DamageTypeData>();

        public static DamageTypeData TrueDamage => DamageTypes["True"];
        public static DamageTypeData PhysicalDamage => DamageTypes["Blunt"];
        public static DamageTypeData MagicalDamage => DamageTypes["Arcane"];
        public static DamageTypeData DivineDamage => DamageTypes["Divine"];

        public static void LoadDamageTypes()
        {
            StreamReader reader = new StreamReader(DataPath);
            string json = reader.ReadToEnd();
            reader.Close();

            int index = 0;
            var serializer = JsonSerializer.CreateDefault();
            var jArray = JsonConvert.DeserializeObject<JArray>(json);

            foreach (var obj in jArray)
            {
                DamageTypeData data = (DamageTypeData)serializer.Deserialize(new JTokenReader(obj), typeof(DamageTypeData));
                DamageTypes.Add(data.Name, data);
                ++index;
            }

            Loaded = true;
            // MaterialNameCommandModule = new MappedCommandModule<ItemMaterial>("What is the default material name prompt??", Materials);
        }

        public static void GenerateDefaultDamageTypes()
        {
            List<DamageTypeData> types = new List<DamageTypeData>(16);

            types.Add(new DamageTypeData() { Name = "True", Category = DamageCategory.True, Flags = DamageFlags.True });

            // Physical
            types.Add(new DamageTypeData() { Name = "Pierce", Category = DamageCategory.Physical, Flags = DamageFlags.IsBlockable });
            types.Add(new DamageTypeData() { Name = "Slash", Category = DamageCategory.Physical, Flags = DamageFlags.IsBlockable });
            types.Add(new DamageTypeData() { Name = "Blunt", Category = DamageCategory.Physical, Flags = DamageFlags.IsBlockable | DamageFlags.IsResistable });

            // Magical
            types.Add(new DamageTypeData() { Name = "Arcane", Category = DamageCategory.Magical, Flags = DamageFlags.IsBlockable | DamageFlags.IsResistable });
            types.Add(new DamageTypeData() { Name = "Astral", Category = DamageCategory.Magical, Flags = DamageFlags.IsResistable });

            // Elemental
            types.Add(new DamageTypeData() { Name = "Ice", Category = DamageCategory.Elemental, Flags = DamageFlags.IsResistable });
            types.Add(new DamageTypeData() { Name = "Fire", Category = DamageCategory.Elemental, Flags = DamageFlags.IsResistable });
            types.Add(new DamageTypeData() { Name = "Lightning", Category = DamageCategory.Elemental, Flags = DamageFlags.IsResistable });

            //Divine
            types.Add(new DamageTypeData() { Name = "Divine", Category = DamageCategory.Divine, Flags = DamageFlags.IsResistable });

            using StreamWriter writer = new StreamWriter(DataPath);
            writer.Write(JsonConvert.SerializeObject(types));
            writer.Close();
        }
    }
}
