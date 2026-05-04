using DaybreakGames.Census.Stream;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using PlanetSide.StatProcessing.Events;
using System.IO;
using Newtonsoft.Json;
using System.Data;

namespace PlanetSide
{
    public abstract class PlanetSideTeam : IDisposable
    {
        public int TeamSize => TeamPlayers.Count;
        public int WorldId { get; protected set; }
        public int ZoneId { get; protected set; }
        public int TeamId { get; protected set; }
        public int FactionId { get; protected set; }
        public string TeamName { get; protected set; }
        public PlanetStats TeamStats { get; private set; }
        public IReadOnlyDictionary<int, WeaponStats> TeamWeapons { get; private set; }
        public IReadOnlyDictionary<string, PlayerStats> TeamPlayers { get; private set; }

        [JsonIgnore] public bool IsStreaming { get; private set; }
        [JsonIgnore] public bool IsProccessing { get; private set; }
        [JsonIgnore] public bool IsPaused { get; set; }

        [JsonIgnore] protected string worldString;
        [JsonIgnore] protected string streamKey = string.Empty;
        [JsonIgnore] protected readonly ILogger<PlanetSideTeam> Logger;

        [JsonIgnore] private ConcurrentQueue<ICensusEvent> events;
        [JsonIgnore] private CancellationTokenSource ctQueueProcess;
        [JsonIgnore] protected ConcurrentDictionary<string, PlayerStats> _teamPlayerStats;
        [JsonIgnore] protected ConcurrentDictionary<int, WeaponStats> _teamWeaponStats = new ConcurrentDictionary<int, WeaponStats>(8, 128);

        public PlanetSideTeam(int teamId, string teamName, int faction, string world)
        {
            this.TeamId = teamId;
            this.FactionId = faction;
            this.TeamName = teamName;
            worldString = world;

            TeamStats = new PlanetStats();
            TeamWeapons = _teamWeaponStats;
            TeamPlayers = _teamPlayerStats;

            events = new ConcurrentQueue<ICensusEvent>();
            Logger = Program.LoggerFactory.CreateLogger<PlanetSideTeam>();

            streamKey = $"PlanetSideTeam_{teamName}_PlayerEventStream";
            ctQueueProcess = new CancellationTokenSource();

            if (int.TryParse(worldString, out int worldId))
                this.WorldId = worldId;
            else WorldId = -1;
        }

        public void StartStream()
        {
            if (IsStreaming)
                return;

            var handler = Tracker.Handler;
            handler.AddSubscription(streamKey, GetStreamSubscription());
            handler.AddActionToSubscription(streamKey, ProcessCensusEvent);
            handler.ConnectClientAsync(streamKey).Wait();
            Task.Run(() => ProcessQueue(ctQueueProcess.Token), ctQueueProcess.Token);
            Logger.LogInformation("Team {0} Began processing player events", TeamName);
            IsStreaming = true;
        }
        public void StopStream()
        {
            if (!IsStreaming)
                return;

            // Stop processing
            ctQueueProcess.Cancel();
            IsProccessing = false;

            // Stop strreaming
            Tracker.Handler.DisconnectSocketAsync(streamKey).Wait();
            IsStreaming = false;

            // Set to unpaused
            IsPaused = false;
        }
        
        public void PauseStream()
        {
            //if (!IsStreaming)
            //    return;
            IsPaused = true;
        }
        public void UnPauseStream()
        {
            //if (!IsStreaming)
            //    return;
            IsPaused = false;
        }

        public void ResetStats()
        {
            TeamStats.Reset();

            foreach (var w in TeamWeapons.Values)
                w.Stats.Reset();

            foreach (var p in TeamPlayers.Values)
                p.Stats.Reset();
        }
        public void SaveStats()
        {
            if (!Directory.Exists("./SavedTeamData"))
                Directory.CreateDirectory("./SavedTeamData");

            string timeStr = DateTime.Now.ToString("yyyy.MM.dd_HH.mm.ss");
            using (StreamWriter writer = new StreamWriter($"./SavedTeamData/Team_{TeamName}_{timeStr}.json"))
            {
                writer.WriteLine(JsonConvert.SerializeObject(this));
            }
        }
        public void SaveFullStats()
        {
            TeamStats.allowExpSerialization = true;
            foreach (var p in TeamPlayers.Values)
                p.Stats.allowExpSerialization = true;

            SaveStats();

            TeamStats.allowExpSerialization = false;
            foreach (var p in TeamPlayers.Values)
                p.Stats.allowExpSerialization = false;
        }

        private bool ProcessCensusEvent(SocketResponse response)
        {
            // Drop all events while paused.
            if (IsPaused || !IsProccessing)
                return false;

            ICensusEvent? censusEvent = Tracker.ProcessCensusEvent(response);

            // Only queue event if it is considered valid by the child class.
            if (censusEvent is not null && IsEventValid(censusEvent))
                events.Enqueue(censusEvent);

            return false;
        }

        private async void ProcessQueue(CancellationToken ct)
        {
            IsProccessing = true;
            using PeriodicTimer pTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(1));

            bool AddStat(string characterId, int weaponId, Action<PlanetStats> action)
            {
                if (TeamPlayers.ContainsKey(characterId))
                {
                    action.Invoke(TeamStats);
                    action.Invoke(TeamPlayers[characterId].Stats);

                    if (weaponId > 0)
                    {
                        if (_teamWeaponStats.TryGetOrAddWeaponStats(weaponId, out var wstats))
                            action.Invoke(wstats.Stats);
                        if (TeamPlayers[characterId].WeaponStats.TryGetOrAddWeaponStats(weaponId, out var pwstats))
                            action.Invoke(pwstats.Stats);
                    }

                    return true;
                }
                return false;
            }

            while (!ct.IsCancellationRequested)
            {
                // Yield if we burn through the queue or fail a dequeue
                if (events.Count == 0 || !events.TryDequeue(out var payload))
                {
                    // Using a periodic timer is WAY MORE cpu effiecient. Usage down from 65% idle to like 0.3% 
                    // await Task.Yield();
                    await pTimer.WaitForNextTickAsync(ct);
                    continue;
                }

                OnEventProcessed(payload);

                bool validZone = ZoneId == -1;
                if (!validZone && payload is ICensusZoneEvent zoneEvent)
                    validZone = zoneEvent.ZoneId == ZoneId;

                switch (payload.EventType)
                {
                    case CensusEventType.GainExperience:
                        if (validZone)
                        {
                            var expEvent = (ExperiencePayload)payload;
                            AddStat(expEvent.CharacterId, -1, stats => stats.AddExperience(ref expEvent));
                        }
                        break;
                    case CensusEventType.Death:
                        if (validZone)
                        {
                            var deathEvent = (DeathPayload)payload;

                            // If not a death, add a kill.
                            if (!AddStat(deathEvent.CharacterId, deathEvent.AttackerWeaponId, stats => stats.AddDeath(ref deathEvent)))
                                AddStat(deathEvent.OtherId, deathEvent.AttackerWeaponId, stats => stats.AddKill(ref deathEvent));
                        }
                        break;
                    case CensusEventType.VehicleDestroy:
                        var destroyEvent = (VehicleDestroyPayload)payload;
                        if (validZone)
                        {
                            // If not a death, add a kill.
                            if (!AddStat(destroyEvent.CharacterId, destroyEvent.AttackerWeaponId, stats => stats.AddVehicleDeath(ref destroyEvent)))
                                AddStat(destroyEvent.OtherId, destroyEvent.AttackerWeaponId, stats => stats.AddVehicleKill(ref destroyEvent));
                        }
                        break;
                    case CensusEventType.FacilityControl:
                        var facilityEvent = (FacilityControlEvent)payload;
                        if (validZone && facilityEvent.NewFaction == FactionId)
                            TeamStats.AddFacilityEvent(ref facilityEvent);
                        break;
                }
            }

            IsProccessing = false;
        }

        protected abstract void OnStreamStart();
        protected abstract void OnStreamStop();
        protected abstract void OnEventProcessed(ICensusEvent censusEvent);
        protected abstract bool IsEventValid(ICensusEvent censusEvent);
        protected abstract CensusStreamSubscription GetStreamSubscription();

        public void Dispose()
        {
            StopStream();
            ctQueueProcess.Dispose();
        }
    }
}
