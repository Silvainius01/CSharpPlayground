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
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Websocket.Client.Logging;

namespace PlanetSide
{


    internal static class DamageTracker
    {
        class InfantryLife
        {
            public string CharacterId { get; private set; }
            public float DamageTaken { get; private set; }

            public InfantryLife(string characterId)
            {
                CharacterId = characterId;
            }

            public void AddHeals(ExperiencePayload expEvent)
            {
                if (expEvent.OtherId != CharacterId)
                    return;

                if (ExperienceTable.InfantryHealingIds.Contains(expEvent.ExperienceId))
                {
                    var eData = ExperienceTable.ExperienceMap[expEvent.ExperienceId];
                    DamageTaken += eData.IsSquad
                        ? eData.ScoreAmount * ExperienceTable.HealthPerSquadExp
                        : eData.ScoreAmount * ExperienceTable.HealthPerExp;
                }
                if (ExperienceTable.InfantryShieldRepairIds.Contains(expEvent.ExperienceId))
                {
                    var eData = ExperienceTable.ExperienceMap[expEvent.ExperienceId];
                    DamageTaken += eData.IsSquad
                        ? eData.ScoreAmount * ExperienceTable.ShieldPerSquadExp
                        : eData.ScoreAmount * ExperienceTable.ShieldPerExp;
                }
            }

            public void Reset()
            {
                DamageTaken = 0.0f;
            }
        }
        class InfantryDeath
        {
            public bool IsTeamKill { get; set; }
            public string KillerId { get; set; }
            public string VictimId { get; private set; }
            public int CensusTimestamp { get; private set; }
            public float DamageTaken { get; set; }

            private float assistPercent;
            private Dictionary<string, float> _characterDamage;
            private List<ExperiencePayload> _assists;

            public InfantryDeath(string killerId, string victimId, int timeStamp)
            {
                KillerId = killerId;
                VictimId = victimId;
                CensusTimestamp = timeStamp;
                _assists = new List<ExperiencePayload>();
                _characterDamage = new Dictionary<string, float>();
            }

            // TODO: look into if driver and gunner infantry assists matter
            public bool AddAssist(ExperiencePayload expEvent)
            {
                if (expEvent.OtherId != VictimId
                 || expEvent.CensusTimestamp != CensusTimestamp
                 || !ExperienceTable.InfantryAssistIds.Contains(expEvent.ExperienceId)
                 || _assists.Contains(xp => xp.CharacterId == expEvent.CharacterId))
                    return false;

                float baseExp = ExperienceTable.ExperienceMap[expEvent.ExperienceId].ScoreAmount;
                float damagePercent = expEvent.ScoreAmount / baseExp;


                _assists.Add(expEvent);
                assistPercent += damagePercent;
                _characterDamage.Add(expEvent.CharacterId, damagePercent);
                return true;
            }

            public float GetDamageEstimate(string characterId)
            {
                if (characterId == KillerId && !IsTeamKill)
                    return (1 - assistPercent) * 1000f;
                else if (_characterDamage.ContainsKey(characterId))
                    return _characterDamage[characterId] * 1000f;
                return 0.0f;
            }
        }

        /// <summary> This dict contains objects tracking per-life stats. Upon death, the entry is moved into the deaths dictionary and replaced with a fresh instance. </summary>
        static ConcurrentDictionary<string, InfantryLife> currentLives = new ConcurrentDictionary<string, InfantryLife>();
        /// <summary> Dict containing all deaths. </summary>
        static ConcurrentDictionary<int, ConcurrentBag<InfantryDeath>> deathsByTime = new ConcurrentDictionary<int, ConcurrentBag<InfantryDeath>>();
        /// <summary> Deaths sorted by who died </summary>
        static ConcurrentDictionary<string, ConcurrentBag<InfantryDeath>> deathsByCharacter = new ConcurrentDictionary<string, ConcurrentBag<InfantryDeath>>();
        /// <summary> Deaths sorted by who participated </summary>
        static ConcurrentDictionary<string, ConcurrentBag<InfantryDeath>> participations = new ConcurrentDictionary<string, ConcurrentBag<InfantryDeath>>();

        static ILogger Logger = Program.LoggerFactory.CreateLogger(typeof(DamageTracker));

        public static float GetCharacterDamageDealt(string characterId)
        {
            if (participations.TryGetValue(characterId, out var deaths))
            {
                float total = 0.0f;
                foreach (var death in deaths)
                    total += death.GetDamageEstimate(characterId);
                return total;
            }
            return 0.0f;
        }

        public static float GetCharacterDamageReceived(string characterId)
        {
            if (!deathsByCharacter.ContainsKey(characterId))
                return 0.0f;

            float total = 0.0f;
            foreach (var death in deathsByCharacter[characterId])
                total += death.DamageTaken;
            return total;
        }

        public static bool AddKill(DeathPayload deathEvent)
        {
            InfantryDeath? death = GetDeath(deathEvent.CharacterId, deathEvent.CensusTimestamp, true);

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
                AddParticipation(deathEvent.OtherId, death);
                return true;
            }
            //else
            //{
            //    death = new InfantryDeath(deathEvent.OtherId, deathEvent.CharacterId, deathEvent.CensusTimestamp);
            //    death.IsTeamKill = deathEvent.TeamId == deathEvent.AttackerTeamId;
            //    AddDeath(death);
            //}

            return false;
        }

        public static bool AddAssist(ExperiencePayload expEvent)
        {
            if (!ExperienceTable.InfantryAssistIds.Contains(expEvent.ExperienceId))
                return false;

            InfantryDeath? death = GetDeath(expEvent.OtherId, expEvent.CensusTimestamp, true);

            if (death is null)
                return false;

            if (death.AddAssist(expEvent))
            {
                // Mark the Assister as participating
                AddParticipation(expEvent.CharacterId, death);
                return true;
            }
            return false;
        }

        public static void AddHeals(ExperiencePayload expEvent)
        {
            string healedId = expEvent.OtherId;
            if (currentLives.ContainsKey(healedId))
                currentLives[healedId].AddHeals(expEvent);
            else if (ExperienceTable.InfantryHealingIds.Contains(expEvent.ExperienceId)
             || ExperienceTable.InfantryShieldRepairIds.Contains(expEvent.ExperienceId))
            {
                InfantryLife life = new InfantryLife(healedId);
                currentLives.TryAdd(healedId, life);
                life.AddHeals(expEvent);
            }
        }

        static void AddParticipation(string characterId, InfantryDeath death)
        {
            var deaths = participations.GetOrAdd(characterId, key => new ConcurrentBag<InfantryDeath>());
            deaths.Add(death);
        }

        static InfantryDeath? GetDeath(string victimId, int timestamp, bool addIfMissing = false)
        {
            if (deathsByTime.ContainsKey(timestamp))
            {
                if (deathsByTime[timestamp].TryFirst(d => d.VictimId == victimId, out InfantryDeath death))
                    return death;
            }
            // else deathsByTime.TryAdd(timestamp, new ConcurrentBag<InfantryDeath>());

            if (addIfMissing)
            {
                var death = new InfantryDeath(string.Empty, victimId, timestamp);

                if (currentLives.ContainsKey(victimId))
                {
                    InfantryLife life = currentLives[victimId];
                    death.DamageTaken = life.DamageTaken + 1000f;
                }
                else death.DamageTaken = 1000f;

                if (!deathsByTime.ContainsKey(timestamp))
                    deathsByTime.TryAdd(timestamp, new ConcurrentBag<InfantryDeath>());
                deathsByTime[timestamp].Add(death);

                if (!deathsByCharacter.ContainsKey(victimId))
                    deathsByCharacter.TryAdd(victimId, new ConcurrentBag<InfantryDeath>());
                deathsByCharacter[victimId].Add(death);

                return death;
            }

            return null;
        }
    }
}
