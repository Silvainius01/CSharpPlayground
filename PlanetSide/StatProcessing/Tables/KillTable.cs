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
        public bool IsTeamKill { get; set; }
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

        // TODO: look into if driver and gunner infantry assists matter
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
            if (characterId == KillerId && !IsTeamKill)
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

        public static bool AddKill(DeathPayload deathEvent)
        {
            InfantryDeath? death = GetDeath(deathEvent.CharacterId, deathEvent.CensusTimestamp);

            if (death is not null)
            {
                // Possible to recieve assist before kill
                if (!string.IsNullOrEmpty(death.KillerId))
                {
                    Logger.LogError("Recieved a duplicate death.\t\nTimestamp: {0}, Victim: {1}\t\nOG Killer: {2}, Dupe Killer: {3}", 
                        deathEvent.CensusTimestamp, deathEvent.CharacterId, death.KillerId, deathEvent.OtherId);
                    return false;
                }

                // If not a duplicate, this is the killer.
                death.KillerId = deathEvent.CharacterId;
                death.IsTeamKill = deathEvent.TeamId == deathEvent.AttackerTeamId;
            }
            else
            {
                death = new InfantryDeath(deathEvent.OtherId, deathEvent.CharacterId, deathEvent.CensusTimestamp);
                death.IsTeamKill = deathEvent.TeamId == deathEvent.AttackerTeamId;
                deaths[deathEvent.CensusTimestamp].Add(death);
            }

            AddParticipation(deathEvent.OtherId, death);
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
            {
                if (deaths[timestamp].TryFirst(d => d.VictimId == victimId, out InfantryDeath death))
                    return death;
            }
            else deaths.TryAdd(timestamp, new ConcurrentBag<InfantryDeath>());

            if (addIfMissing)
            {
                var death = new InfantryDeath(string.Empty, victimId, timestamp);
                deaths[timestamp].Add(death);
                return death;
            }

            return null;
        }
    }
}
