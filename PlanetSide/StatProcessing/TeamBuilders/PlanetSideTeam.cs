using DaybreakGames.Census.Stream;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PlanetSide
{
    public abstract class PlanetSideTeam : IDisposable
    {
        public int TeamSize { get; private set; }
        public string TeamName { get; private set; }
        public string Faction { get; private set; }
        public string World { get; private set; }
        public string Zone { get; private set; } = string.Empty;
        public Dictionary<string, JsonElement> teamPlayers;
        public PlanetStats teamStats;
        public bool IsProccessing { get; private set; }

        protected readonly ILogger<PlanetSideTeam> Logger;
        protected ConcurrentQueue<ICensusEvent> events;

        protected string streamKey = string.Empty;
        protected CancellationTokenSource tokenSource;

        public PlanetSideTeam(int teamSize, string teamName, string faction, string world)
        {
            this.Faction = faction;
            this.TeamName = teamName;
            this.TeamSize = teamSize;
            this.World = world;
            teamPlayers = new Dictionary<string, JsonElement>(teamSize);
            teamStats = new PlanetStats();

            events = new ConcurrentQueue<ICensusEvent>();
            Logger = Program.LoggerFactory.CreateLogger<PlanetSideTeam>();

            streamKey = $"PlanetSideTeam_{teamName}_PlayerEventStream";
            tokenSource = new CancellationTokenSource();
        }

        public void StartStream(CensusHandler handler)
        {
            handler.AddSubscription(streamKey, GetStreamSubscription());
            handler.AddActionToSubscription(streamKey, ProcessCensusEvent);
            handler.ConnectClientAsync(streamKey).Wait();
            Task.Run(() => ProcessQueue(tokenSource.Token), tokenSource.Token);
            Logger.LogInformation("Team {0} Began processing player events", TeamName);
        }
        public void StopStream(CensusHandler handler)
        {
            tokenSource.Cancel();
            handler.DisconnectSocketAsync(streamKey).Wait();
        }

        private bool ProcessCensusEvent(SocketResponse response)
        {
            string eventType;
            string characterId;
            JsonElement payload;
            ICensusEvent censusEvent = null;

            // Skip if malformed
            if (!response.Message.RootElement.TryGetProperty("payload", out payload)
            || !payload.TryGetStringElement("event_name", out eventType)
            || !payload.TryGetStringElement("character_id", out characterId))
                return false;

            switch (eventType)
            {
                case "GainExperience":
                    {
                        // Skip if malformed
                        if (!payload.TryGetStringElement("other_id", out string otherId)
                        || !payload.TryGetCensusInteger("experience_id", out int experienceId)
                        || !payload.TryGetCensusFloat("amount", out float scoreAmount))
                            break;

                        censusEvent = new ExperiencePayload()
                        {
                            CharacterId = characterId,
                            EventType = CensusEventType.GainExperience,
                            OtherId = otherId,
                            ExperienceId = experienceId,
                            ScoreAmount = scoreAmount
                        };
                    }
                    break;
                case "Death":
                    {
                        // Skip if malformed
                        if (!payload.TryGetStringElement("attacker_character_id", out string attackerId)
                        || !payload.TryGetCensusInteger("attacker_weapon_id", out int attackerWeaponId)
                        || !payload.TryGetCensusInteger("attacker_vehicle_id", out int attackerVehicleId)
                        || !payload.TryGetCensusInteger("attacker_loadout_id", out int attackerLoadoutId)
                        || !payload.TryGetCensusBool("is_headshot", out bool isHeadshot))
                            break;

                        censusEvent = new DeathPayload()
                        {
                            CharacterId = characterId,
                            EventType = CensusEventType.Death,
                            OtherId = attackerId,
                            AttackerWeaponId = attackerWeaponId,
                            AttackerVehicleId = attackerVehicleId,
                            AttackerLoadoutId = attackerLoadoutId,
                            IsHeadshot = isHeadshot
                        };
                    }
                    break;
                case "VehicleDestroy":
                    {
                        // Skip if malformed
                        if (!payload.TryGetStringElement("attacker_character_id", out string attackerId)
                        || !payload.TryGetCensusInteger("attacker_weapon_id", out int attackerWeaponId)
                        || !payload.TryGetCensusInteger("attacker_vehicle_id", out int attackerVehicleId)
                        || !payload.TryGetCensusInteger("attacker_loadout_id", out int attackerLoadoutId))
                            break;

                        censusEvent = new VehicleDestroyPayload()
                        {
                            CharacterId = characterId,
                            EventType = CensusEventType.VehicleDestroy,
                            OtherId = attackerId,
                            AttackerWeaponId = attackerWeaponId,
                            AttackerVehicleId = attackerVehicleId,
                            AttackerLoadoutId = attackerLoadoutId
                        };
                    }
                    break;
            }

            // Only queue event if it is considered valid by the child class.
            if (censusEvent is not null && IsEventValid(censusEvent))
                events.Enqueue(censusEvent);


            return false;
        }

        private async void ProcessQueue(CancellationToken ct)
        {
            IsProccessing = true;
            using PeriodicTimer pTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(1));

            while (!ct.IsCancellationRequested)
            {
                // Yield if we burn through the queue or fail a dequeue
                if (events.Count == 0 || !events.TryDequeue(out var payload))
                {
                    // Using a periodic timer is WAY MORE cpu effiecient. Usage down from 65% to 
                    // await Task.Yield();
                    await pTimer.WaitForNextTickAsync(ct);
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
                        var destroyEvent = (VehicleDestroyPayload)payload;
                        if (teamPlayers.ContainsKey(destroyEvent.CharacterId))
                            teamStats.AddVehicleDeath(ref destroyEvent, teamPlayers.ContainsKey(destroyEvent.OtherId));
                        else if (teamPlayers.ContainsKey(destroyEvent.OtherId))
                            teamStats.AddVehicleKill(ref destroyEvent);
                        break;
                }
            }

            IsProccessing = false;
        }


        protected abstract void OnStreamStart(CensusHandler handler);
        protected abstract void OnStreamStop(CensusHandler handler);
        protected abstract bool IsEventValid(CensusHandler handler, ICensusEvent payload);
        protected abstract CensusStreamSubscription GetStreamSubscription();

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
