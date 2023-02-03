﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using CommandEngine;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DnD_Generator
{
    class WeaponTypeManager
    {
        public static string TypePath = $"{DungeonCrawlerManager.TextPath}\\WeaponTypes.json";

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
            StreamReader reader = new StreamReader(TypePath);
            string json = reader.ReadToEnd();
            reader.Close();

            var serializer = JsonSerializer.CreateDefault();
            var jArray = JsonConvert.DeserializeObject<JArray>(json);

            foreach(var obj in jArray)
            {
                WeaponTypeData data = (WeaponTypeData)serializer.Deserialize(new JTokenReader(obj), typeof(WeaponTypeData));
                WeaponTypes.Add(data.WeaponType, data);
            }
            TypesLoaded = true;
            WeaponTypeCommandModule = new MappedCommandModule<WeaponTypeData>(WeaponTypes);
        }
    }
}
