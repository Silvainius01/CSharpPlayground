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
        static ConcurrentDictionary<int, int> _itemToWeapon;
        static ConcurrentDictionary<int, int> _weaponToItem;
        static ConcurrentDictionary<int, WeaponData> _weaponMap;
        public static ReadOnlyDictionary<int, WeaponData> WeaponMap;

        static ILogger Logger = Program.LoggerFactory.CreateLogger(typeof(WeaponTable));


        public static async Task Populate()
        {
            var handler = Tracker.Handler;

            var itemToWeaponQuery = handler.GetClientQuery("item_to_weapon").SetLimit(5000);
            itemToWeaponQuery.JoinService("item").ShowFields("name.en", "is_vehicle_weapon", "faction_id");
            
            IEnumerable<JsonElement> itemToWeaponData = await itemToWeaponQuery.GetListAsync();
            int itemToWeaponCount = itemToWeaponData.Count();

            _weaponMap = new ConcurrentDictionary<int, WeaponData>(8, itemToWeaponCount);
            _itemToWeapon = new ConcurrentDictionary<int, int>(8, itemToWeaponCount);
            _weaponToItem = new ConcurrentDictionary<int, int>(8, itemToWeaponCount);
            WeaponMap = new ReadOnlyDictionary<int, WeaponData>(_weaponMap);

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
                        ? vehicleWeaponProp.GetString().Equals("1") // bool.Parse() only successfully converts 'true' and 'false'
                        : false,

                    FactionId = (joinedItemProperty.ValueKind != JsonValueKind.Undefined) && joinedItemProperty.TryGetCensusInteger("faction_id", out int factionId)
                    ? factionId
                    : 0,
                };

                if(!_weaponMap.TryAdd(weaponData.ItemId, weaponData))
                    Logger.LogError($"Failed to add weapon to table: {weaponData}");
            }

            Logger.LogInformation("Weapon Table Populated");
        }
        public static bool TryGetWeapon(int itemId, out WeaponData weaponData)
        {
            if (WeaponMap.TryGetValue(itemId, out weaponData))
                return true;

            Logger.LogWarning($"Tried to get an unknown weapon item ID: {itemId}");
            return false;
        }
    }

    public struct WeaponData
    {
        public int ItemId;
        public int WeaponId;
        public int FactionId;
        public string WeaponName;
        public bool IsVehicleWeapon;

        public override string ToString()
            => $"[{ItemId}][{WeaponId}] {WeaponName}";
    }
}
