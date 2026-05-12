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

        // MangoBean confirmed these numbers.
        // Follow patern of (amountRestored / constant) * baseXp
        public const float HealthPerExp = 10; // (1 / 10) * 100
        public const float HealthPerSquadExp = (1f / 15f) * 100;
        public const float ShieldPerExp = 10; // (1 / 10) * 100
        public const float ShieldPerSquadExp = (1f / 15f) * 100;
        public const float RepairPerExp = 24; // (1 / 5) * 120
        public const float RepairPerSquadExp = 12; // (1f / 10f) * 120;

        // Unknown if these share repair values
        public const float ConstructionRepairPerExp = RepairPerExp;
        public const float ConstructionRepairPerSquadExp = RepairPerSquadExp;
        
        // Cortium values entirely unknown
        public const float CortiumMinedPerExp = 100;
        public const float CortiumDepositedPerExp = 100;

        public static bool IsPopulated => isPopulated;
        public static ReadOnlyCollection<int> ReviveIds;
        public static ReadOnlyCollection<int> ResupplyIds;
        public static ReadOnlyCollection<int> MaxRepairIds;
        public static ReadOnlyCollection<int> VehicleRepairIds;
        public static ReadOnlyCollection<int> InfantryAssistIds;
        public static ReadOnlyCollection<int> InfantryHealingIds;
        public static ReadOnlyCollection<int> InfantryShieldRepairIds;
        public static ReadOnlyDictionary<int, ExperienceTick> ExperienceMap;

        static bool isPopulated = false;
        static List<int> _reviveIds = new List<int> { 7, 53 };
        static List<int> _resupplyIds = new List<int>();
        static List<int> _maxRepairIds = new List<int>() { 6, 142 };
        static List<int> _vehicleRepairIds = new List<int>();
        static List<int> _infantryAssistIds = new List<int>() { 2, 371, 372 };
        static List<int> _infantryHealingIds = new List<int>() { 4, 51 };
        static List<int> _infantryShieldRepairIds = new List<int>() { 438, 439 };
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
            InfantryHealingIds = new ReadOnlyCollection<int>(_infantryHealingIds);
            InfantryShieldRepairIds = new ReadOnlyCollection<int>(_infantryShieldRepairIds);

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
