using DaybreakGames.Census.Stream;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PlanetSide
{
    public class Tracker
    {
        public static CensusHandler Handler = new CensusHandler();
        public static readonly ILogger<Tracker> Logger = Program.LoggerFactory.CreateLogger<Tracker>();

        private static string TeamStatsJsonPath = $"./ChartTest/TeamStats.json";


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

        public static ICensusEvent? ProcessCensusEvent(SocketResponse response)
        {
            string characterId;
            JsonElement payload;
            CensusEventType eventType = CensusEventType.Unknown;

            // Skip if malformed
            if (!response.Message.RootElement.TryGetProperty("payload", out payload)
            || !payload.TryGetStringElement("event_name", out string eventTypeStr)
            || !payload.TryGetStringElement("character_id", out characterId)
            || (eventType = GetEventType(eventTypeStr)) == CensusEventType.Unknown)
                return null;

            switch (eventType)
            {
                case CensusEventType.GainExperience:
                    {
                        // Skip if malformed
                        if (!payload.TryGetStringElement("other_id", out string otherId)
                        || !payload.TryGetCensusInteger("experience_id", out int experienceId)
                        || !payload.TryGetCensusFloat("amount", out float scoreAmount))
                            break;

                        return new ExperiencePayload()
                        {
                            CharacterId = characterId,
                            EventType = eventType,
                            OtherId = otherId,
                            ExperienceId = experienceId,
                            ScoreAmount = scoreAmount
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

                        vKillEvent.FactionId = factionId;
                        vKillEvent.VehicleId = vehicleId;
                        return vKillEvent;
                    }
            }

            return null;
        }

        static CensusEventType GetEventType(string typeStr) => typeStr switch
        {
            "GainExperience" => CensusEventType.GainExperience,
            "Death" => CensusEventType.Death,
            "VehicleDestroy" => CensusEventType.VehicleDestroy,
            _ => CensusEventType.Unknown
        };

        static bool TryProcessDeathEvent<T>(JsonElement payload, CensusEventType type, out T deathEvent) where T : ICensusDeathEvent, new()
        {
            // Might as well perform validation.
            if (!payload.TryGetStringElement("character_id", out string characterId)
            || !payload.TryGetStringElement("attacker_character_id", out string attackerId)
            || !payload.TryGetCensusInteger("attacker_weapon_id", out int attackerWeaponId)
            || !payload.TryGetCensusInteger("attacker_vehicle_id", out int attackerVehicleId)
            || !payload.TryGetCensusInteger("attacker_loadout_id", out int attackerLoadoutId)
            || !payload.TryGetCensusInteger("attacker_team_id", out int attackerTeamId)
            || !payload.TryGetCensusInteger("team_id", out int teamId)
            || !payload.TryGetCensusInteger("zone_id", out int zoneId)
            || !payload.TryGetCensusInteger("world_id", out int worldId))
            {
                Logger.LogWarning($"Death event failed validation!\n\tExpected type: {type.ToString()}\n\tPayload: {payload.ToString()}");
                deathEvent = default(T);
                return false;
            }


            deathEvent = new T()
            {
                CharacterId = characterId,
                EventType = type,
                OtherId = attackerId,

                TeamId = teamId,

                AttackerWeaponId = attackerWeaponId,
                AttackerVehicleId = attackerVehicleId,
                AttackerLoadoutId = attackerLoadoutId,
                AttackerTeamId = attackerTeamId,

                ZoneId = zoneId,
                WorldId = worldId
            };
            return true;
        }
    }
}
