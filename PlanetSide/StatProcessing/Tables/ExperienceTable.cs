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
        public const int KillMAX = 29;
        public const float HealthPerExp = 10;
        public const float HealthPerSquadXp = (1f / 15f) * 100;
        public const float ShieldPerExp = 10;
        public const float ShieldPerSquadExp = (1f / 15f) * 100;
        public const float RepairPerExp = 24;
        public const float RepairPerSquadExp = (1f / 10f) * 120;

        public static ReadOnlyCollection<int> ReviveIds;
        public static ReadOnlyCollection<int> ResupplyIds;
        public static ReadOnlyCollection<int> MaxRepairIds;
        public static ReadOnlyCollection<int> VehicleRepairIds;
        public static ReadOnlyCollection<int> InfantryAssistIds;
        public static ReadOnlyDictionary<int, ExperienceTick> ExperienceMap;


        static bool isPopulated = false;
        static List<int> _reviveIds = new List<int> { 7, 53 };
        static List<int> _resupplyIds = new List<int>();
        static List<int> _maxRepairIds = new List<int>() { 6, 142 };
        static List<int> _vehicleRepairIds = new List<int>();
        static List<int> _infantryAssistIds = new List<int>() { 2, 371, 372 };
        static ConcurrentDictionary<int, ExperienceTick> _experienceMap;

        static ILogger Logger = Program.LoggerFactory.CreateLogger(typeof(ExperienceTable));

        public static async Task Populate()
        {
            if (isPopulated)
                return;

            var handler = Tracker.Handler;
            var queryData = await handler.GetClientQuery("experience").SetLimit(5000).GetListAsync();

            ReviveIds = new ReadOnlyCollection<int>(_reviveIds);
            ResupplyIds = new ReadOnlyCollection<int>(_resupplyIds);
            MaxRepairIds = new ReadOnlyCollection<int>(_maxRepairIds);
            VehicleRepairIds = new ReadOnlyCollection<int>(_vehicleRepairIds);
            InfantryAssistIds = new ReadOnlyCollection<int>(_infantryAssistIds);

            _experienceMap = new ConcurrentDictionary<int, ExperienceTick>(8, queryData.Count());
            ExperienceMap = new ReadOnlyDictionary<int, ExperienceTick>(_experienceMap);

            foreach (var expType in queryData)
            {
                var exp = new ExperienceTick();

                exp.Id = expType.TryGetCensusInteger("experience_id", out int id)
                    ? id: 0;
                exp.Name = expType.TryGetProperty("description", out var descProp)
                    ? (descProp.GetString() ?? "Invalid Description")
                    : "Invalid Description";
                exp.ScoreAmount = expType.TryGetCensusFloat("xp", out float scoreAmount)
                    ? scoreAmount : 0;
                exp.IsSquad = exp.Name.Contains("Squad");

                if(_experienceMap.TryAdd(exp.Id, exp) && !exp.Name.Contains("HIVE"))
                {
                    if(!_maxRepairIds.Contains(exp.Id) && exp.Name.Contains("Repair"))
                        _vehicleRepairIds.Add(exp.Id);
                    if(exp.Name.Contains("Resupply"))
                        _resupplyIds.Add(exp.Id);
                }
            }

            if (!Directory.Exists(Tracker.CenusDataTablesPath))
                Directory.CreateDirectory(Tracker.CenusDataTablesPath);
            using (StreamWriter writer = new StreamWriter($"{Tracker.CenusDataTablesPath}/Experience.json"))
            {
                var sorted = _experienceMap.Values.OrderBy(e => e.Id);
                writer.Write(JsonConvert.SerializeObject(sorted));
                writer.Close();
            }

            isPopulated = true;
            Logger.LogInformation("Experience Table Populated");
        }
    }
}
