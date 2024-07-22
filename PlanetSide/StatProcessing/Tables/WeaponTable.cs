using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace PlanetSide
{
    public static class WeaponTable
    {
        static ConcurrentDictionary<int, int> itemToWeapon;
        static ConcurrentDictionary<int, int> weaponToItem;
        static ConcurrentDictionary<int, WeaponData> weaponMap;
        public static ReadOnlyDictionary<int, WeaponData> WeaponMap;

        static ILogger Logger = Program.LoggerFactory.CreateLogger(typeof(WeaponTable));


        public static async Task Populate()
        {
            var handler = Tracker.Handler;

            var itemToWeaponQuery = handler.GetClientQuery("item_to_weapon").SetLimit(5000);
            itemToWeaponQuery.JoinService("item").ShowFields("name.en", "is_vehicle_weapon");
            
            IEnumerable<JsonElement> itemToWeaponData = await itemToWeaponQuery.GetListAsync();
            int itemToWeaponCount = itemToWeaponData.Count();

            weaponMap = new ConcurrentDictionary<int, WeaponData>(8, itemToWeaponCount);
            itemToWeapon = new ConcurrentDictionary<int, int>(8, itemToWeaponCount);
            weaponToItem = new ConcurrentDictionary<int, int>(8, itemToWeaponCount);
            WeaponMap = new ReadOnlyDictionary<int, WeaponData>(weaponMap);

            int i = 0;
            foreach (var element in itemToWeaponData)
            {
                JsonElement joinedItemProperty = default(JsonElement);
                element.TryGetProperty("item_id_join_item", out joinedItemProperty);

                WeaponData weaponData = new WeaponData()
                {
                    ItemId = int.Parse(element.GetProperty("item_id").GetString()),

                    // Entry [981] has item_id 6004918, but no weapon_id
                    // Its name? "Mystery Weapon". Of-fucking-course.
                    WeaponId = element.TryGetProperty("weapon_id", out var weaponIdProperty)
                        ? int.Parse(weaponIdProperty.GetString())
                        : -1,

                    // Some elements dont have names?
                    WeaponName = (joinedItemProperty.ValueKind != JsonValueKind.Undefined) && joinedItemProperty.TryGetProperty("name", out var nameProp)
                        ? nameProp.GetProperty("en").GetString()
                        : "Unknown Weapon!",

                    IsVehicleWeapon = (joinedItemProperty.ValueKind != JsonValueKind.Undefined) && joinedItemProperty.TryGetProperty("is_vehicle_weapon", out var vehicleWeaponProp)
                        ? vehicleWeaponProp.GetString().Equals("1") // bool.Parse() only allows successfully converts 'true' and 'false'
                        : false,
                };

                if(!weaponMap.TryAdd(weaponData.ItemId, weaponData))
                    Logger.LogError($"Failed to add weapon to table: {weaponData}");
            }

            Logger.LogInformation("Weapon Table Populated");
        }
    }

    public struct WeaponData
    {
        public int ItemId;
        public int WeaponId;
        public string WeaponName;
        public bool IsVehicleWeapon;

        public override string ToString()
            => $"[{ItemId}][{WeaponId}] {WeaponName}";
    }
}
