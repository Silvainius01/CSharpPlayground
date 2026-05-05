using CommandEngine;
using DaybreakGames.Census.Operators;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Websocket.Client.Logging;

namespace PlanetSide
{
    internal class InfantryDeath
    {
        public string KillerId { get; set; }
        public string VictimId { get; private set; }
        public int CensusTimestamp { get; private set; }

        private float assistPercent;
        private Dictionary<string, float> characterDamage;
        private List<ExperiencePayload> Assists { get; set; }

        public InfantryDeath(string killerId, string victimId, int timeStamp)
        {
            KillerId = killerId;
            VictimId = victimId;
            CensusTimestamp = timeStamp;
            Assists = new List<ExperiencePayload>();
            characterDamage = new Dictionary<string, float>();
        }

        public bool AddAssist(ExperiencePayload expEvent)
        {
            if (expEvent.OtherId != VictimId
             || expEvent.CensusTimestamp != CensusTimestamp
             || !ExperienceTable.InfantryAssistIds.Contains(expEvent.ExperienceId)
             || Assists.Contains(xp => xp.CharacterId == expEvent.CharacterId))
                return false;

            float baseExp = ExperienceTable.ExperienceMap[expEvent.ExperienceId].ScoreAmount;
            float damagePercent = expEvent.ScoreAmount / baseExp;


            Assists.Add(expEvent);
            assistPercent += damagePercent;
            characterDamage.Add(expEvent.CharacterId, damagePercent);
            return true;
        }

        public float GetDamageEstimate(string characterId)
        {
            if (characterId == KillerId)
                return (1 - assistPercent) * 1000f;
            if (characterDamage.ContainsKey(characterId))
                return characterDamage[characterId] * 1000f;
            return 0.0f;
        }
    }

    internal static class DamageTracker
    {
        static ConcurrentDictionary<int, ConcurrentBag<InfantryDeath>> deaths = new ConcurrentDictionary<int, ConcurrentBag<InfantryDeath>>();
        static ConcurrentDictionary<string, ConcurrentBag<InfantryDeath>> participations = new ConcurrentDictionary<string, ConcurrentBag<InfantryDeath>>();
        static ILogger Logger = Program.LoggerFactory.CreateLogger(typeof(DamageTracker));

        public static float GetCharacterDamage(string characterId)
        {
            if(participations.TryGetValue(characterId, out var deaths))
            {
                float total = 0.0f;
                foreach (var death in deaths)
                    total += death.GetDamageEstimate(characterId);
                return total;
            }
            return 0.0f;
        }

        public static bool AddKill(string killerId, string victimId, int timestamp)
        {
            InfantryDeath? death = GetDeath(victimId, timestamp);

            if (death is not null)
            {
                // Possible to recieve assist before kill
                if (!string.IsNullOrEmpty(death.KillerId))
                {
                    // Logger.LogError("Recieved a duplicate death.\t\nTimestamp: {0}, Victim: {1}\t\nOG Killer: {2}, Dupe Killer: {3}", timestamp, victimId, matchingDeath.KillerId, killerId);
                    return false;
                }
                // If not a duplicate, this is the killer.
                death.KillerId = killerId;
            }
            else
            {
                death = new InfantryDeath(killerId, victimId, timestamp);
                deaths[timestamp].Add(death);
            }

            AddParticipation(killerId, death);
            return true;
        }

        public static bool AddAssist(ExperiencePayload expEvent)
        {
            if (!ExperienceTable.InfantryAssistIds.Contains(expEvent.ExperienceId))
                return false;

            InfantryDeath? death = GetDeath(expEvent.OtherId, expEvent.CensusTimestamp, true);

            if (death is null)
                return false;

            if(death.AddAssist(expEvent))
            {
                AddParticipation(expEvent.CharacterId, death);
                return true;
            }    
            return false;
        }

        static void AddParticipation(string characterId, InfantryDeath death)
        {
            var deaths = participations.GetOrAdd(characterId, key => new ConcurrentBag<InfantryDeath>());
            deaths.Add(death);
        }

        static InfantryDeath? GetDeath(string victimId, int timestamp, bool addIfMissing = false)
        {
            if (deaths.ContainsKey(timestamp))
                return deaths[timestamp].First(d => d.VictimId == victimId);
            deaths.TryAdd(timestamp, new ConcurrentBag<InfantryDeath>());

            if (addIfMissing)
            {
                var death = new InfantryDeath(string.Empty, victimId, timestamp);
                return death;
            }
            return null;
        }
    }
}
