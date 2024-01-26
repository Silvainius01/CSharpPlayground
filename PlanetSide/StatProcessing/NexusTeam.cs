using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DaybreakGames.Census.Stream;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Bson;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Reflection.PortableExecutable;
using System.Xml;

namespace PlanetSide
{
    public class NexusTeam : IDisposable
    {
        public readonly int teamSize;
        public readonly string teamName;
        public readonly string faction;
        public Dictionary<string, JsonElement> teamPlayers;
        public TeamStats teamStats;

        private readonly ILogger<NexusTeam> Logger;
        private ConcurrentQueue<ICensusPayload> events;

        public bool IsProccessing { get; private set; }
        private string streamKey = string.Empty;
        private CancellationTokenSource tokenSource;

        public NexusTeam(int teamSize, string teamName, string faction)
        {
            this.faction = faction;
            this.teamName = teamName;
            this.teamSize = teamSize;
            teamPlayers = new Dictionary<string, JsonElement>(teamSize);
            teamStats = new TeamStats(teamSize, teamName);

            events = new ConcurrentQueue<ICensusPayload>();
            Logger = Program.LoggerFactory.CreateLogger<NexusTeam>();

            streamKey = $"{teamName}_PlayerEventStream";
            tokenSource = new CancellationTokenSource();
        }

        public async Task GenerateRandomTeam(string streamKey, CensusHandler handler)
        {
            bool filled = false;
            object fillLock = new object();
            object charLock = new object();
            ConcurrentDictionary<string, JsonElement> playersConcurrent = new ConcurrentDictionary<string, JsonElement>(4, teamSize);

            Logger.LogInformation("Genrating NexusTeam {0}...", teamName);

            void OnTeamFilled()
            {
                if (filled)
                    return;

                lock (fillLock)
                {
                    filled = true;

                    teamPlayers.Clear();
                    foreach (var kvp in playersConcurrent)
                        teamPlayers.Add(kvp.Key, kvp.Value);

                    Logger.LogInformation("Genrated NexusTeam {0} with {1} players", teamName, teamSize);
                }
            }
            bool AddPlayer(string characterId, JsonElement characterData)
            {
                if (playersConcurrent.TryAdd(characterId, characterData) && playersConcurrent.Count >= teamSize)
                { 
                    OnTeamFilled();
                    return true;
                }

                return false;
            }

            handler.AddActionToSubscription(streamKey, response =>
            {
                string eventType;
                JsonElement payload;

                // Skip if malformed or team full
                if (!response.Message.RootElement.TryGetProperty("payload", out payload)
                || !payload.TryGetStringElement("event_name", out eventType))
                    return false;
                if (playersConcurrent.Count >= teamSize)
                {
                    OnTeamFilled();
                    return true;
                }


                if (eventType == "Death" || eventType == "VehicleDestroy")
                {
                    string[] characterIds = new string[2];

                    // Skip if malformed
                    if (!payload.TryGetStringElement("character_id", out characterIds[0])
                    || !payload.TryGetStringElement("attacker_character_id", out characterIds[1]))
                        return false;

                    Array.Sort(characterIds); // List always returns sorted ascending
                    var query = handler.GetCharactersQuery(characterIds).ShowFields("faction_id", "character_id");
                    var charTask = query.GetListAsync();
                    charTask.Wait();

                    int i = 0;
                    var characters = charTask.Result;
                    foreach (var c in characters)
                    {
                        if (c.TryGetStringElement("faction_id", out string cFaction)
                        && cFaction == faction
                        && AddPlayer(characterIds[i], c)) // We wont know the order 
                            return true;
                        ++i;
                    }
                }
                else
                {
                    string characterId;
                    bool isCharacterValid = payload.TryGetStringElement("character_id", out characterId)
                        && !playersConcurrent.ContainsKey(characterId);

                    if (isCharacterValid)
                    {
                        var cData = handler.GetCharacter(characterId);
                        return AddPlayer(characterId, cData);
                    }
                }

                return false;
            });

            await Task.Run(() => { while (!filled) ; return true; });
        }

        public void StartStream(CensusHandler handler)
        {
            handler.AddSubscription(streamKey, new CensusStreamSubscription()
            {
                Characters = teamPlayers.Keys,
                Worlds = new[] { "17" },
                EventNames = new[] { "Death", "GainExperience", "VehicleDestroy" },
                LogicalAndCharactersWithWorlds = true
            });
            handler.AddActionToSubscription(streamKey, ProcessCensusEvent);
            handler.ConnectClientAsync(streamKey).Wait();
            Task.Run(() => ProcessQueue(tokenSource.Token), tokenSource.Token);
            Logger.LogInformation("Team {0} Began processing player events", teamName);
        }
        public void EndStream(CensusHandler handler)
        {
            tokenSource.Cancel();
            handler.DisconnectSocketAsync(streamKey).Wait();
        }

        private bool ProcessCensusEvent(SocketResponse response)
        {
            string eventType;
            string characterId;
            JsonElement payload;

            // Skip if malformed
            if (!response.Message.RootElement.TryGetProperty("payload", out payload)
            || !payload.TryGetStringElement("event_name", out eventType)
            || !payload.TryGetStringElement("character_id", out characterId))
                return false;

            switch (eventType)
            {
                case "GainExperience":
                    {
                        // Skip if not a team member, or malformed
                        if (!teamPlayers.ContainsKey(characterId)
                        || !payload.TryGetStringElement("other_id", out string otherId)
                        || !payload.TryGetCensusInteger("experience_id", out int experienceId)
                        || !payload.TryGetCensusFloat("amount", out float scoreAmount))
                            break;

                        events.Enqueue(new ExperiencePayload()
                        {
                            CharacterId = characterId,
                            EventType = CensusEventType.GainExperience,
                            OtherId = otherId,
                            ExperienceId = experienceId,
                            ScoreAmount = scoreAmount
                        });
                    }
                    break;
                case "Death":
                    {
                        // Skip if not a team member or malformed
                        if (!payload.TryGetStringElement("attacker_character_id", out string attackerId)
                        || (!teamPlayers.ContainsKey(characterId) && !teamPlayers.ContainsKey(attackerId))
                        || !payload.TryGetCensusInteger("attacker_weapon_id", out int attackerWeaponId)
                        || !payload.TryGetCensusInteger("attacker_vehicle_id", out int attackerVehicleId)
                        || !payload.TryGetCensusInteger("attacker_loadout_id", out int attackerLoadoutId)
                        || !payload.TryGetCensusBool("is_headshot", out bool isHeadshot))
                            break;

                        events.Enqueue(new DeathPayload()
                        {
                            CharacterId = characterId,
                            EventType = CensusEventType.Death,
                            OtherId = attackerId,
                            AttackerWeaponId = attackerWeaponId,
                            AttackerVehicleId = attackerVehicleId,
                            AttackerLoadoutId = attackerLoadoutId,
                            IsHeadshot = isHeadshot
                        });
                    }
                    break;
                case "VehicleDestroy":
                    {
                        // Skip if not a team member or malformed
                        if (!payload.TryGetStringElement("attacker_character_id", out string attackerId)
                        || (!teamPlayers.ContainsKey(characterId) && !teamPlayers.ContainsKey(attackerId))
                        || !payload.TryGetCensusInteger("attacker_weapon_id", out int attackerWeaponId)
                        || !payload.TryGetCensusInteger("attacker_vehicle_id", out int attackerVehicleId)
                        || !payload.TryGetCensusInteger("attacker_loadout_id", out int attackerLoadoutId))
                            break;

                        events.Enqueue(new VehicleDesroyPayload()
                        {
                            CharacterId = characterId,
                            EventType = CensusEventType.VehicleDestroy,
                            OtherId = attackerId,
                            AttackerWeaponId = attackerWeaponId,
                            AttackerVehicleId = attackerVehicleId,
                            AttackerLoadoutId = attackerLoadoutId
                        });
                    }
                    break;
            }

            return false;
        }

        private async void ProcessQueue(CancellationToken ct)
        {
            IsProccessing = true;
            while (!ct.IsCancellationRequested)
            {
                // Yield if we burn through the queue or fail a dequeue
                if (events.Count == 0 || !events.TryDequeue(out var payload))
                {
                    await Task.Yield();
                    continue;
                }

                switch (payload.EventType)
                {
                    case CensusEventType.GainExperience:
                        var expEvent = (ExperiencePayload)payload;
                        teamStats.AddExperience(ref expEvent);
                        break;
                    case CensusEventType.Death:
                        var deathEvent = (DeathPayload)payload;
                        if (teamPlayers.ContainsKey(deathEvent.CharacterId))
                            teamStats.AddDeath(ref deathEvent, teamPlayers.ContainsKey(deathEvent.OtherId));
                        else if (teamPlayers.ContainsKey(deathEvent.OtherId))
                            teamStats.AddKill(ref deathEvent);
                        break;
                    case CensusEventType.VehicleDestroy:
                        var destroyEvent = (VehicleDesroyPayload)payload;
                        if (teamPlayers.ContainsKey(destroyEvent.CharacterId))
                            teamStats.AddVehicleDeath(ref destroyEvent, teamPlayers.ContainsKey(destroyEvent.OtherId));
                        else if (teamPlayers.ContainsKey(destroyEvent.OtherId))
                            teamStats.AddVehicleKill(ref destroyEvent);
                        break;
                }
            }
            IsProccessing = false;
        }

        public void Dispose()
        {

        }
    }
}
