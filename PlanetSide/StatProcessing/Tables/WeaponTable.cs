using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;

namespace PlanetSide
{
    public static class WeaponTable
    {
        static Dictionary<int, int> itemToWeapon = new Dictionary<int, int>();
        static Dictionary<int, int> weaponToItem = new Dictionary<int, int>();
        static Dictionary<int, WeaponData> weaponMap = new Dictionary<int, WeaponData>();
        public static ReadOnlyDictionary<int, WeaponData> WeaponMap = new ReadOnlyDictionary<int, WeaponData>(weaponMap);

        static ILogger Logger = Program.LoggerFactory.CreateLogger(typeof(WeaponTable));


        public static async Task Populate(CensusHandler handler)
        {
            var itemToWeaponQuery = handler.GetClientQuery("item_to_weapon").SetLimit(5000);
            itemToWeaponQuery.JoinService("item").ShowFields("name.en", "is_vehicle_weapon");
            var itemToWeaponTask = itemToWeaponQuery.GetListAsync();
            
            await itemToWeaponTask;

            IEnumerable<JsonElement> itemToWeaponData = itemToWeaponTask.Result;
            int itemToWeaponCount = itemToWeaponData.Count();

            weaponMap.EnsureCapacity(itemToWeaponCount);
            itemToWeapon.EnsureCapacity(itemToWeaponCount);
            weaponToItem.EnsureCapacity(itemToWeaponCount);

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

                weaponMap.Add(weaponData.ItemId, weaponData);
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
    }
}
