using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace PlanetSide
{
    public static class ExperienceTable
    {
        public const int Revive = 7;
        public const int KillMAX = 29;
        public const int InfantryKillAssist = 2;
        public static ReadOnlyCollection<int> ResupplyIds;
        public static ReadOnlyCollection<int> MaxRepairIds;
        public static ReadOnlyCollection<int> VehicleRepairIds;
        public static ReadOnlyCollection<int> ReviveIds;
        public static ReadOnlyDictionary<int, ExperienceTick> ExperienceMap;

        static List<int> _resupplyIds = new List<int>();
        static List<int> _vehicleRepairIds = new List<int>();
        static List<int> _maxRepairIds = new List<int>() { 6, 142 };
        static List<int> _reviveIds = new List<int> { 7, 53 };
        static ConcurrentDictionary<int, ExperienceTick> _experienceMap;

        static ILogger Logger = Program.LoggerFactory.CreateLogger(typeof(ExperienceTable));

        public static async Task Populate()
        {
            var handler = Tracker.Handler;
            var queryData = await handler.GetClientQuery("experience").SetLimit(5000).GetListAsync();

            ResupplyIds = new ReadOnlyCollection<int>(_resupplyIds);
            MaxRepairIds = new ReadOnlyCollection<int>(_maxRepairIds);
            VehicleRepairIds = new ReadOnlyCollection<int>(_vehicleRepairIds);

            _experienceMap = new ConcurrentDictionary<int, ExperienceTick>(8, queryData.Count());
            ExperienceMap = new ReadOnlyDictionary<int, ExperienceTick>(_experienceMap);

            foreach (var expType in queryData)
            {
                var exp = new ExperienceTick();

                exp.Id = expType.TryGetProperty("experience_id", out var idProp)
                    ? int.Parse(idProp.GetString())
                    : -1;
                exp.Name = expType.TryGetProperty("description", out var descProp)
                    ? (descProp.GetString() ?? "Invalid Description")
                    : "Invalid Description";
                exp.ScoreAmount = expType.TryGetProperty("xp", out var expProp)
                    ? float.Parse(expProp.GetString())
                    : 0;

                if(_experienceMap.TryAdd(exp.Id, exp) && !exp.Name.Contains("HIVE"))
                {
                    if(!_maxRepairIds.Contains(exp.Id) && exp.Name.Contains("Repair"))
                        _vehicleRepairIds.Add(exp.Id);
                    if(exp.Name.Contains("Resupply"))
                        _resupplyIds.Add(exp.Id);
                }
            }

            if (!Directory.Exists("./CensusData"))
                Directory.CreateDirectory("./CensusData");
            using (StreamWriter writer = new StreamWriter("./CensusData/Experience.json"))
            {
                writer.Write(JsonConvert.SerializeObject(_experienceMap));
                writer.Close();
            }

            Logger.LogInformation("Experience Table Populated");
        }
    }
}
