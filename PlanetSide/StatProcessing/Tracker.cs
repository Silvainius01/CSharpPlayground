using DaybreakGames.Census.Stream;
using Microsoft.Extensions.Logging;
using PlanetSide.StatProcessing.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        public static CensusHandler Handler = new CensusHandler();
        public static readonly ILogger<Tracker> Logger = Program.LoggerFactory.CreateLogger<Tracker>();

        private static string TeamStatsJsonPath = $"./ChartTest/TeamStats.json";
        private static string DataStreamBackUpPath = "./CensusData/Events.json";

        private static CancellationTokenSource ctSaveRoutine = new CancellationTokenSource();
        private static ConcurrentQueue<JsonElement> payloadSaveQueue = new ConcurrentQueue<JsonElement>();

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

            Task.Run(() => SaveEventsToDiskRoutine(ctSaveRoutine.Token));
        }

        public static ICensusEvent? ProcessCensusEvent(SocketResponse response)
        {
            string characterId;
            JsonElement payload;
            CensusEventType eventType = CensusEventType.Unknown;


            // Skip if malformed
            if (!response.Message.RootElement.TryGetProperty("payload", out payload)
            || !payload.TryGetStringElement("event_name", out string eventTypeStr)
            || (eventType = GetEventType(eventTypeStr)) == CensusEventType.Unknown)
                return null;

            payloadSaveQueue.Enqueue(payload);

            switch (eventType)
            {
                case CensusEventType.GainExperience:
                    {
                        // Skip if malformed
                        if (!payload.TryGetStringElement("other_id", out string otherId)
                        || !payload.TryGetCensusInteger("experience_id", out int experienceId)
                        || !payload.TryGetCensusFloat("amount", out float scoreAmount)
                        || !payload.TryGetCensusInteger("zone_id", out int zoneId)
                        || !payload.TryGetStringElement("character_id", out characterId))
                            break;

                        return new ExperiencePayload()
                        {
                            CharacterId = characterId,
                            EventType = eventType,
                            OtherId = otherId,
                            ExperienceId = experienceId,
                            ScoreAmount = scoreAmount,
                            ZoneId = zoneId
                        };
                    }
                case CensusEventType.Death:
                    {
                        if (!payload.TryGetCensusBool("is_headshot", out bool isHeadshot)
                        || !TryProcessDeathEvent(payload, eventType, out DeathPayload deathEvent))
                            break;

                        deathEvent.IsHeadshot = isHeadshot;
                        return deathEvent;
                    }
                case CensusEventType.VehicleDestroy:
                    {
                        // Skip if malformed
                        if (!payload.TryGetCensusInteger("faction_id", out int factionId)
                        || !payload.TryGetCensusInteger("vehicle_id", out int vehicleId)
                        || !TryProcessDeathEvent(payload, eventType, out VehicleDestroyPayload vKillEvent))
                            break;

                        if (!VehicleTable.VehicleData.ContainsKey(vehicleId))
                        {
                            Logger.LogWarning($"Dropped VehicleDestroy event due to missing vehicle ID: {vehicleId}");
                            break;
                        }
                        else if (!VehicleTable.VehicleData.ContainsKey(vKillEvent.AttackerVehicleId))
                        {
                            Logger.LogWarning($"Dropped VehicleDestroy event due to missing attacker vehicle ID: {vKillEvent.AttackerVehicleId}");
                            break;
                        }
                        else if (vehicleId != 0 && VehicleTable.VehicleData[vehicleId].Type == VehicleType.Unknown)
                        {
                            Logger.LogWarning($"Dropped VehicleDestroy event due to vehicle ID of unkown type: {vehicleId}");
                            break;
                        }

                        vKillEvent.FactionId = factionId;
                        vKillEvent.VehicleId = vehicleId;
                        return vKillEvent;
                    }
                case CensusEventType.FacilityControl:
                    {
                        if (!TryProcessZoneEvent(payload, CensusEventType.FacilityControl, out FacilityControlEvent facilityEvent)
                        || !payload.TryGetCensusInteger("facility_id", out int facilityId)
                        || !payload.TryGetCensusInteger("duration_held", out int durationHeld)
                        || !payload.TryGetCensusInteger("new_faction_id", out int newFaction)
                        || !payload.TryGetCensusInteger("old_faction_id", out int oldFaction)
                        || !payload.TryGetStringElement("outfit_id", out string outfitId)
                        || !payload.TryGetCensusInteger("timestamp", out int unixTime))
                        {
                            break;
                        }

                        DateTime timeStamp = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(unixTime);

                        facilityEvent.FacilityId = facilityId;
                        facilityEvent.DurationHeld = durationHeld;
                        facilityEvent.NewFaction = newFaction;
                        facilityEvent.OldFaction = oldFaction;
                        facilityEvent.OutfitId = outfitId;
                        facilityEvent.Timestamp = timeStamp;
                        return facilityEvent;
                    }
                    break;
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

        static bool TryProcessZoneEvent<T>(JsonElement payload, CensusEventType type, out T zoneEvent) where T : ICensusZoneEvent, new()
        {
            if (!payload.TryGetCensusInteger("zone_id", out int zoneId)
            || !payload.TryGetCensusInteger("world_id", out int worldId))
            {
                Logger.LogWarning($"Zone event failed validation!\n\tExpected type: {type.ToString()}\n\tPayload: {payload.ToString()}");
                zoneEvent = default(T);
                return false;
            }

            zoneEvent = new T()
            {
                EventType = type,
                ZoneId = zoneId,
                WorldId = worldId
            };
            return true;
        }

        static bool TryProcessDeathEvent<T>(JsonElement payload, CensusEventType type, out T deathEvent) where T : ICensusDeathEvent, new()
        {
            // Might as well perform validation.
            if (!TryProcessZoneEvent<T>(payload, type, out deathEvent)
            || !payload.TryGetStringElement("character_id", out string characterId)
            || !payload.TryGetStringElement("attacker_character_id", out string attackerId)
            || !payload.TryGetCensusInteger("attacker_weapon_id", out int attackerWeaponId)
            || !payload.TryGetCensusInteger("attacker_vehicle_id", out int attackerVehicleId)
            || !payload.TryGetCensusInteger("attacker_loadout_id", out int attackerLoadoutId)
            || !payload.TryGetCensusInteger("attacker_team_id", out int attackerTeamId)
            || !payload.TryGetCensusInteger("team_id", out int teamId))
            {
                Logger.LogWarning($"Death event failed validation!\n\tExpected type: {type.ToString()}\n\tPayload: {payload.ToString()}");
                deathEvent = default(T);
                return false;
            }


            deathEvent.CharacterId = characterId;
            deathEvent.EventType = type;
            deathEvent.OtherId = attackerId;

            deathEvent.TeamId = teamId;

            deathEvent.AttackerWeaponId = attackerWeaponId;
            deathEvent.AttackerVehicleId = attackerVehicleId;
            deathEvent.AttackerLoadoutId = attackerLoadoutId;
            deathEvent.AttackerTeamId = attackerTeamId;
            return true;
        }

        static async Task SaveEventsToDiskRoutine(CancellationToken ct)
        {
            PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromMilliseconds(1));
            using (var stream = new StreamWriter(DataStreamBackUpPath, true))
            {
                while (!ct.IsCancellationRequested)
                {
                    if (payloadSaveQueue.Count == 0 || !payloadSaveQueue.TryDequeue(out JsonElement payload))
                    {
                        await timer.WaitForNextTickAsync();
                        continue;
                    }

                    stream.WriteLine(payload.ToString());
                }
            }
        }
    }
}
