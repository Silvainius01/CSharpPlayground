using DaybreakGames.Census.Operators;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Websocket.Client.Logging;

namespace PlanetSide
{
    internal class InfantryDeath
    {
        public string KillerId { get; set; }
        public string VictimId { get; set; }
        public List<ExperiencePayload> Assists { get; set; }
        public string CensusTimestamp { get; set; }

        public InfantryDeath(string killerId, string victimId, string timeStamp)
        {
            KillerId = killerId;
            VictimId = victimId;
            CensusTimestamp = timeStamp;
            Assists = new List<ExperiencePayload>();
        }
    }

    internal static class DamageTracker
    {
        static Dictionary<string, List<InfantryDeath>> deaths = new Dictionary<string, List<InfantryDeath>>();
        static ILogger Logger = Program.LoggerFactory.CreateLogger(typeof(DamageTracker));

        public static void AddDeath(string killerId, string victimId, string timestamp)
        {
            if (deaths.ContainsKey(timestamp))
            {
                var matchingDeath = deaths[timestamp].Where(d => d.VictimId == victimId).First();

                // Possible to recieve assist before kill
                if (matchingDeath is not null)
                {
                    if (matchingDeath.KillerId != string.Empty)
                    {
                        // Logger.LogError("Recieved a duplicate death.\t\nTimestamp: {0}, Victim: {1}\t\nOG Killer: {2}, Dupe Killer: {3}", timestamp, victimId, matchingDeath.KillerId, killerId);
                        return;
                    }

                    // If not a duplicate, this is the killer.
                    matchingDeath.KillerId = killerId;
                }

                deaths[timestamp].Add(new InfantryDeath(killerId, victimId, timestamp));
            }
        }
    }
}
