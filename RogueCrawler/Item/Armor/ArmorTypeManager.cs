using CommandEngine;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace RogueCrawler
{
    class ArmorTypeManager
    {
        public static string DataPath = $"{DungeonCrawlerManager.TextPath}\\Data\\ArmorTypes.json";

        public static bool TypesLoaded = false;
        public static Dictionary<string, ArmorTypeData> ArmorTypes = new Dictionary<string, ArmorTypeData>();
        public static Dictionary<string, List<ArmorTypeData>> ArmorByClass = new Dictionary<string, List<ArmorTypeData>>();
        

        public static MappedCommandModule<ArmorTypeData> ArmorTypeCommandModule;

        public static ArmorTypeData GetRandomArmorType(string armorClass, ItemMaterial material)
        {
            // Filter to only valid types
            var validArmorTypes = ArmorByClass[armorClass].Where(a =>
                a.AllowedMaterials.Contains(material.Name) || (material.IsMetallic && a.AllowAnyMetal));

            if (!validArmorTypes.Any())
                throw new System.Exception($"No valid armor type exists for Class {armorClass} and Material {material.Name}");

            return validArmorTypes.RandomItem();
        }

        public static void LoadArmorTypes()
        {
            StreamReader reader = new StreamReader(DataPath);
            string json = reader.ReadToEnd();
            reader.Close();

            var serializer = JsonSerializer.CreateDefault();
            var jArray = JsonConvert.DeserializeObject<JArray>(json);

            foreach (var obj in jArray)
            {
                ArmorTypeData data = (ArmorTypeData)serializer.Deserialize(new JTokenReader(obj), typeof(ArmorTypeData));
                ArmorTypes.Add(data.ArmorType, data);

                if(ArmorByClass.ContainsKey(data.ArmorClass))
                    ArmorByClass[data.ArmorClass].Add(data);
                else ArmorByClass.Add(data.ArmorClass, new List<ArmorTypeData> { data });
            }
            TypesLoaded = true;
            ArmorTypeCommandModule = new MappedCommandModule<ArmorTypeData>("What is the default armor type prompt??", ArmorTypes);
        }
    }
}
