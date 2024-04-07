using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;

namespace PlanetSide
{
    public static class ExperienceTable
    {
        static Dictionary<int, ExperienceTick> experienceMap = new Dictionary<int, ExperienceTick>();
        public static ReadOnlyDictionary<int, ExperienceTick> ExperienceMap = new ReadOnlyDictionary<int, ExperienceTick>(experienceMap);

        static ILogger Logger = Program.LoggerFactory.CreateLogger(typeof(ExperienceTable));

        public static async Task Populate(CensusHandler handler)
        {
            var queryTask = handler.GetClientQuery("experience").SetLimit(5000).GetListAsync();
            
            await queryTask;

            IEnumerable<JsonElement> eData = queryTask.Result;
            experienceMap.EnsureCapacity(eData.Count());

            foreach (var expType in eData)
            {
                var _event = new ExperienceTick();

                _event.Id = expType.TryGetProperty("experience_id", out var idProp)
                    ? int.Parse(idProp.GetString())
                    : -1;
                _event.Name = expType.TryGetProperty("description", out var descProp)
                    ? (descProp.GetString() ?? "Invalid Description")
                    : "Invalid Description";
                _event.ScoreAmount = expType.TryGetProperty("xp", out var expProp)
                    ? float.Parse(expProp.GetString())
                    : 0;

                experienceMap.Add(_event.Id, _event);
            }

            Logger.LogInformation("Experience Table Populated");
        }
    }
}
