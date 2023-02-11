using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace RogueCrawler.Item
{
    /// <summary>
    /// TODO:
    /// <para>Merge in <see cref="WeaponTypeData"/></para>
    /// </summary>
    static class ItemWeaponType
    {
        static Dictionary<string, WeaponTypeData> WeaponTypeData = new Dictionary<string, WeaponTypeData>();
        public static List<string> WeaponTypeNames = new List<string>();

        public static void LoadWeaponTypes()
        {
        }

        public static WeaponTypeData GetWeaponType(string type) => WeaponTypeData[type];
        public static bool IsWeaponType(string type) => WeaponTypeData.ContainsKey(type);
    }
}
