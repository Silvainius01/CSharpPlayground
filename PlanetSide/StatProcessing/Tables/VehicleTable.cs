using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanetSide
{
    public static class VehicleTable
    {
        static ConcurrentDictionary<int, VehicleData> _vehicleData;
        public static ReadOnlyDictionary<int, VehicleData> VehicleData;

        static ILogger Logger = Program.LoggerFactory.CreateLogger(typeof(VehicleTable));


        public static async Task Populate()
        {
            var handler = Tracker.Handler;

            var vehicleQuery = handler.GetClientQuery("vehicle").SetLimit(5000).ShowFields("name.en", "vehicle_id", "type_id");
            var vehicleDataRaw = await vehicleQuery.GetListAsync();

            _vehicleData = new ConcurrentDictionary<int, VehicleData>(8, vehicleDataRaw.Count());
            VehicleData = new ReadOnlyDictionary<int, VehicleData>(_vehicleData);

            _vehicleData.TryAdd(0, new VehicleData()
            {
                Name = "Suicide",
                Type = VehicleType.Unknown,
                VehicleId = 0
            });

            foreach (var element in vehicleDataRaw)
            {
                if (!element.TryGetCensusInteger("vehicle_id", out int vehicleId)
                || !element.TryGetCensusInteger("type_id", out int typeId))
                {
                    Logger.LogWarning($"Failed to add vehicle to table: {element.ToString()}");
                    continue;
                }


                var vData = new VehicleData()
                {
                    VehicleId = vehicleId,
                    Type = (VehicleType)typeId,
                    Name = element.GetProperty("name").GetProperty("en").GetString()
                };

                if (!_vehicleData.TryAdd(vData.VehicleId, vData))
                    Logger.LogError($"Failed to add vehicle to table: {vData}");
            }
        }
    }

    public enum VehicleType { Unknown = 0, Air = 1, Hover = 2, Ground = 5, Turret = 7, DropPod = 8 }
    public struct VehicleData
    {
        public int VehicleId { get; set; }
        public string Name { get; set; }
        public VehicleType Type { get; set; }

        public override string ToString()
            => $"[{VehicleId}] {Name}";
    }
}
