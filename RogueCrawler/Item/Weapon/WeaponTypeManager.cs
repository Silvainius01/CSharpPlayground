using System.Collections.Generic;
using System.Text;
using System.Linq;
using CommandEngine;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RogueCrawler.Item.Weapon
{
    class WeaponTypeManager
    {
        public static string DataPath = $"{DungeonCrawlerManager.TextPath}\\Data\\WeaponTypes.json";

        public static bool TypesLoaded = false;
        public static Dictionary<string, WeaponTypeData> WeaponTypes = new Dictionary<string, WeaponTypeData>();

        public static string RandomType
        {
            get
            {
                return WeaponTypes.Keys.RandomItem();
            }
        }
        public static WeaponTypeData RandomWeaponData
        {
            get
            {
                return WeaponTypes.Values.RandomItem();
            }
        }

        public static MappedCommandModule<WeaponTypeData> WeaponTypeCommandModule;

        public static void LoadWeaponTypes()
        {
            StreamReader reader = new StreamReader(DataPath);
            string json = reader.ReadToEnd();
            reader.Close();

            var serializer = JsonSerializer.CreateDefault();
            var jArray = JsonConvert.DeserializeObject<JArray>(json);

            foreach (var obj in jArray)
            {
                WeaponTypeData data = (WeaponTypeData)serializer.Deserialize(new JTokenReader(obj), typeof(WeaponTypeData));
                WeaponTypes.Add(data.WeaponType, data);
            }
            TypesLoaded = true;
            WeaponTypeCommandModule = new MappedCommandModule<WeaponTypeData>("What is the default weapon type prompt??", WeaponTypes);
        }
    }
}
