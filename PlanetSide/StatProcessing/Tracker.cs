using DaybreakGames.Census.Stream;
using Microsoft.Extensions.Logging;
using PlanetSide.StatProcessing.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PlanetSide
{
    public class Tracker
    {
        private class EventSaver : IDisposable
        {
            public bool IsStarted { get; private set; }
            public string Source { get; private set; }
            public Task? SaveRoutine { get; private set; }
            public ConcurrentQueue<JsonElement> PayloadQueue { get; private set; }

            CancellationToken ctExternal;
            CancellationTokenSource? ctLinkedSource;

            public EventSaver(string source, CancellationToken ct)
            {
                Source = source;
                ctExternal = ct;
                PayloadQueue = new ConcurrentQueue<JsonElement>();
            }

            public void Start(Func<bool> condition)
            {
                ctLinkedSource = CancellationTokenSource.CreateLinkedTokenSource(ctExternal, ctSaveRoutine.Token);
                var ct = ctLinkedSource.Token;
                SaveRoutine = Task.Run(() => SaveEventsToDiskRoutine(Source, ct, condition), ct);
            }

            public void Dispose()
            {
                SaveRoutine?.Dispose();
                ctLinkedSource?.Dispose();
            }
        }

        public static CensusHandler Handler = new CensusHandler();
        public static readonly ILogger<Tracker> Logger = Program.LoggerFactory.CreateLogger<Tracker>();

        public const string TeamStatsJsonPath = $"./ChartTest/TeamStats.json";
        public const string CenusDataEventsPath = "./CensusData/Events";
        public const string CenusDataTablesPath = "./CensusData/Tables";

        private static CancellationTokenSource ctSaveRoutine = new CancellationTokenSource();
        private static ConcurrentQueue<JsonElement> payloadSaveQueue = new ConcurrentQueue<JsonElement>();

        private static ConcurrentDictionary<string, bool> sourceWarnings = new ConcurrentDictionary<string, bool>();
        private static ConcurrentDictionary<string, EventSaver> payloadSaveQueues = new ConcurrentDictionary<string, EventSaver>(8, 4);

        public static void PopulateTables()
        {
            Logger.LogInformation("Populating Tables");
            List<Task> tableTasks = new List<Task>()
            {
                ExperienceTable.Populate(),
                WeaponTable.Populate(),
                VehicleTable.Populate()
            };

            foreach (var task in tableTasks)
                task.Wait();
        }

        public static void RegisterEventSaveRoutine(string source, CancellationToken ct, Func<bool> condition)
        {
            bool success = false;
            EventSaver saver = new EventSaver(source, ct);

            if (!payloadSaveQueues.TryGetValue(source, out EventSaver? old))
                success = payloadSaveQueues.TryAdd(source, saver);
            else if (old.SaveRoutine is null || old.SaveRoutine.IsCompleted)
                success = payloadSaveQueues.TryUpdate(source, saver, old);

            if (success)
            {
                saver.Start(condition);
                Logger.LogInformation($"Registered event saver for source {source}");
            }
            else Logger.LogWarning($"Failed to register event saving process for {source}");
        }

        public static ICensusEvent? ProcessCensusEvent(SocketResponse response, string source)
        {
            JsonElement payload;
            CensusEventType eventType = CensusEventType.Unknown;

            // Skip if malformed
            if (!response.Message.RootElement.TryGetProperty("payload", out payload)
            || !payload.TryGetStringElement("event_name", out string eventTypeStr)
            || !payload.TryGetCensusInteger("timestamp", out int censusTimestamp)
            || (eventType = GetEventType(eventTypeStr)) == CensusEventType.Unknown)
                return null;

            // Save any valid events to disk.

            if (payloadSaveQueues.ContainsKey(source))
                payloadSaveQueues[source].PayloadQueue.Enqueue(payload);
            else if (!sourceWarnings.ContainsKey(source))
            {
                sourceWarnings.TryAdd(source, true);
                Logger.LogError($"No registered save process for events from {source}.");
            }

            switch (eventType)
            {
                case CensusEventType.GainExperience:
                    {
                        var expEvent = new ExperiencePayload()
                        {
                            EventType = eventType,
                            CensusTimestamp = censusTimestamp
                        };

                        // Skip if malformed
                        if (!TryProcessZoneEvent(payload, eventType, ref expEvent)
                         || !TryProcessCharacterEvent(payload, eventType, ref expEvent)
                         || !payload.TryGetCensusInteger("experience_id", out int experienceId)
                         || !payload.TryGetCensusFloat("amount", out float scoreAmount))
                            break;

                        expEvent.ExperienceId = experienceId;
                        expEvent.ScoreAmount = scoreAmount;
                        return expEvent;
                    }
                case CensusEventType.Death:
                    {
                        var deathEvent = new DeathPayload()
                        {
                            EventType = eventType,
                            CensusTimestamp = censusTimestamp
                        };

                        if (!TryProcessZoneEvent(payload, eventType, ref deathEvent)
                         || !TryProcessCharacterEvent(payload, eventType, ref deathEvent)
                         || !TryProcessDeathEvent(payload, eventType, ref deathEvent)
                         || payload.TryGetCensusBool("is_headshot", out bool isHeadshot))
                            break;

                        deathEvent.IsHeadshot = isHeadshot;
                        return deathEvent;
                    }
                case CensusEventType.VehicleDestroy:
                    {
                        var vDestroyEvent = new VehicleDestroyPayload()
                        {
                            EventType = eventType,
                            CensusTimestamp = censusTimestamp
                        };

                        if (!TryProcessZoneEvent(payload, eventType, ref vDestroyEvent)
                         || !TryProcessCharacterEvent(payload, eventType, ref vDestroyEvent)
                         || !TryProcessDeathEvent(payload, eventType, ref vDestroyEvent)
                         || !payload.TryGetCensusInteger("faction_id", out int factionId)
                         || !payload.TryGetCensusInteger("vehicle_id", out int vehicleId))
                            break;


                        if (!VehicleTable.VehicleData.ContainsKey(vehicleId))
                        {
                            Logger.LogWarning($"Dropped VehicleDestroy event due to missing vehicle ID: {vehicleId}");
                            break;
                        }
                        else if (!VehicleTable.VehicleData.ContainsKey(vDestroyEvent.AttackerVehicleId))
                        {
                            Logger.LogWarning($"Dropped VehicleDestroy event due to missing attacker vehicle ID: {vDestroyEvent.AttackerVehicleId}");
                            break;
                        }
                        else if (vehicleId != 0 && VehicleTable.VehicleData[vehicleId].Type == VehicleType.Unknown)
                        {
                            Logger.LogWarning($"Dropped VehicleDestroy event due to vehicle ID of unknown type: {vehicleId}");
                            break;
                        }

                        vDestroyEvent.FactionId = factionId;
                        vDestroyEvent.VehicleId = vehicleId;
                        return vDestroyEvent;
                    }
                case CensusEventType.FacilityControl:
                    {
                        var facilityEvent = new FacilityControlEvent()
                        {
                            EventType = eventType,
                            CensusTimestamp = censusTimestamp
                        };

                        if (!TryProcessZoneEvent(payload, CensusEventType.FacilityControl, ref facilityEvent)
                        || !payload.TryGetCensusInteger("facility_id", out int facilityId)
                        || !payload.TryGetCensusInteger("duration_held", out int durationHeld)
                        || !payload.TryGetCensusInteger("new_faction_id", out int newFaction)
                        || !payload.TryGetCensusInteger("old_faction_id", out int oldFaction)
                        || !payload.TryGetStringElement("outfit_id", out string outfitId))
                            break;

                        facilityEvent.FacilityId = facilityId;
                        facilityEvent.DurationHeld = durationHeld;
                        facilityEvent.NewFaction = newFaction;
                        facilityEvent.OldFaction = oldFaction;
                        facilityEvent.OutfitId = outfitId;
                        return facilityEvent;
                    }
            }

            return null;
        }

        static CensusEventType GetEventType(string typeStr) => typeStr switch
        {
            "GainExperience" => CensusEventType.GainExperience,
            "Death" => CensusEventType.Death,
            "VehicleDestroy" => CensusEventType.VehicleDestroy,
            "FacilityControl" => CensusEventType.FacilityControl,
            _ => CensusEventType.Unknown
        };
        static bool TryProcessZoneEvent<T>(JsonElement payload, CensusEventType type, ref T zoneEvent) where T : ICensusZoneEvent, new()
        {
            if (!payload.TryGetCensusInteger("zone_id", out int zoneId)
            || !payload.TryGetCensusInteger("world_id", out int worldId))
            {
                Logger.LogWarning($"Zone event failed validation!\n\tExpected type: {type.ToString()}\n\tPayload: {payload.ToString()}");
                return false;
            }

            zoneEvent.ZoneId = zoneId;
            zoneEvent.WorldId = worldId;
            return true;
        }
        static bool TryProcessCharacterEvent<T>(JsonElement payload, CensusEventType type, ref T charEvent) where T : ICensusCharacterEvent, new()
        {
            string otherId = string.Empty;

            if (!payload.TryGetStringElement("character_id", out string characterId))
            {
                // Some character events get OtherId from different properties.
                if (type != CensusEventType.VehicleDestroy)
                    if (!payload.TryGetStringElement("other_id", out otherId))
                    {
                        Logger.LogWarning($"Character event failed validation!\n\tExpected type: {type.ToString()}\n\tPayload: {payload.ToString()}");
                        return false;
                    }
            }

            charEvent.CharacterId = characterId;
            charEvent.OtherId = otherId;
            return true;
        }
        static bool TryProcessDeathEvent<T>(JsonElement payload, CensusEventType type, ref T deathEvent) where T : ICensusDeathEvent, new()
        {
            // Might as well perform validation.
            if (!payload.TryGetStringElement("attacker_character_id", out string attackerId)
            || !payload.TryGetCensusInteger("attacker_weapon_id", out int attackerWeaponId)
            || !payload.TryGetCensusInteger("attacker_vehicle_id", out int attackerVehicleId)
            || !payload.TryGetCensusInteger("attacker_loadout_id", out int attackerLoadoutId)
            || !payload.TryGetCensusInteger("attacker_team_id", out int attackerTeamId)
            || !payload.TryGetCensusInteger("team_id", out int teamId))
            {
                Logger.LogWarning($"Death event failed validation!\n\tExpected type: {type.ToString()}\n\tPayload: {payload.ToString()}");
                return false;
            }

            // Set by previous validators
            //deathEvent.EventType = type;
            //deathEvent.CharacterId = characterId;

            deathEvent.OtherId = attackerId;

            deathEvent.TeamId = teamId;
            deathEvent.AttackerWeaponId = attackerWeaponId;
            deathEvent.AttackerVehicleId = attackerVehicleId;
            deathEvent.AttackerLoadoutId = attackerLoadoutId;
            deathEvent.AttackerTeamId = attackerTeamId;
            return true;
        }

        static async Task SaveEventsToDiskRoutine(string source, CancellationToken ct, Func<bool> condition)
        {
            if (!Directory.Exists(CenusDataEventsPath))
                Directory.CreateDirectory(CenusDataEventsPath);

            string dataPath = !string.IsNullOrEmpty(source)
                ? $"{CenusDataEventsPath}/{source}_Events_{DateTime.Now.ToString("yyyy.MM.dd_HH.mm.ss")}.json"
                : $"{CenusDataEventsPath}/Events_{DateTime.Now.ToString("yyyy.MM.dd_HH.mm.ss")}.json";

            using PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromMilliseconds(100));
            using (var stream = new StreamWriter(dataPath, false))
            {
                while (!ct.IsCancellationRequested && condition.Invoke())
                {
                    if (payloadSaveQueue.TryDequeue(out JsonElement payload))
                        stream.WriteLine(payload.ToString());
                    else await timer.WaitForNextTickAsync(ct);
                }
            }

            // Attempt to remove ourself from the registered queues
            if (payloadSaveQueues.TryRemove(source, out EventSaver? saver))
                Logger.LogInformation($"{source} Events save process ended and removed.");
            Logger.LogWarning($"{source} Events save process ended, but failed to remove.");
        }

        public static string FactionIdToName(int id, bool abbreviated = true)
        {
            return id switch
            {
                1 => abbreviated ? "VS" : "Vanu Sovereignty",
                2 => abbreviated ? "NC" : "New Conglomerate",
                3 => abbreviated ? "TR" : "Terran Republic",
                _ => "Unknown Faction"
            };
        }
    }
}
