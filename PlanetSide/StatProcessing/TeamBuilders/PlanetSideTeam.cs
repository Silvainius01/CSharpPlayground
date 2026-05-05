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
        public int WorldId { get; protected set; }
        public int ZoneId { get; protected set; }
        public int TeamId { get; protected set; }
        public int FactionId { get; protected set; }
        public string TeamName { get; protected set; }
        public PlanetStats TeamStats { get; private set; }
        public IReadOnlyDictionary<int, WeaponStats> TeamWeapons { get; private set; }
        public IReadOnlyDictionary<string, PlayerStats> TeamPlayers { get; private set; }
        protected int _teamSize;

        // Event Processing state data
        [JsonIgnore] public bool IsStreaming { get; private set; }
        [JsonIgnore] public bool IsProcessing { get; private set; }
        [JsonIgnore] public bool IsPaused { get; set; }

        [JsonIgnore] protected string worldString;
        [JsonIgnore] protected string streamKey = string.Empty;
        [JsonIgnore] protected readonly ILogger<PlanetSideTeam> Logger;

        [JsonIgnore] private ConcurrentQueue<ICensusEvent> events;
        [JsonIgnore] private CancellationTokenSource ctEventSaving;
        [JsonIgnore] private CancellationTokenSource ctEventProcessing;

        [JsonIgnore] private bool playersAdded;
        [JsonIgnore] protected ConcurrentDictionary<string, PlayerStats> _teamPlayerStats = new ConcurrentDictionary<string, PlayerStats>(8, 64);
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
            ctEventProcessing = new CancellationTokenSource();
            ctEventSaving = new CancellationTokenSource();

            if (int.TryParse(worldString, out int worldId))
                this.WorldId = worldId;
            else WorldId = -1;
        }

        public void StartStream()
        {
            if (IsStreaming)
                return;

            IsStreaming = true;

            // Start processing tasks
            Tracker.RegisterEventSaveRoutine(TeamName, ctEventSaving.Token, () => IsStreaming);
            Task.Run(() => ProcessQueue(ctEventProcessing.Token), ctEventProcessing.Token);

            // Create the event stream
            var handler = Tracker.Handler;
            handler.AddSubscription(streamKey, GetStreamSubscription());
            handler.AddActionToSubscription(streamKey, ProcessCensusEvent);
            handler.ConnectClientAsync(streamKey).Wait();

            Logger.LogInformation("Team {0} Began processing player events", TeamName);
        }
        public void StopStream()
        {
            if (!IsStreaming)
                return;

            // Stop streaming
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
        public void ResumeStream()
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

        public void AddPlayers()
        {
            if (playersAdded)
                return;

            AddPlayersInternal();
            _teamSize = GetPlayerCount();
            playersAdded = true;
        }

        private bool ProcessCensusEvent(SocketResponse response)
        {
            // Drop all events while paused.
            if (IsPaused)
                return false;

            ICensusEvent? censusEvent = Tracker.ProcessCensusEvent(response, TeamName);

            // Only queue event if it is considered valid by the child class.
            if (censusEvent is not null && IsEventValid(censusEvent))
                events.Enqueue(censusEvent);

            return false;
        }

        private async void ProcessQueue(CancellationToken ct)
        {
            using PeriodicTimer pTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(100));

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

            while (!ct.IsCancellationRequested && IsStreaming)
            {
                // Yield if we burn through the queue or fail a dequeue
                if (events.Count == 0 || !events.TryDequeue(out var payload))
                {
                    // Using a periodic timer is WAY MORE cpu effiecient. Usage down from 65% idle to like 0.3% 
                    // await Task.Yield();
                    await pTimer.WaitForNextTickAsync();
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
        }

        /// <summary> 
        /// It is reccomended that implementations AVOID <see cref="ConcurrentDictionary{TKey, TValue}.Count"/>
        /// since it must lock the object, which stalls any Task hoping to access it.
        /// </summary>
        public abstract int GetPlayerCount();
        protected abstract void OnStreamStart();
        protected abstract void OnStreamStop();
        protected abstract void OnEventProcessed(ICensusEvent censusEvent);
        protected abstract bool IsEventValid(ICensusEvent censusEvent);
        protected abstract CensusStreamSubscription GetStreamSubscription();

        /// <summary>
        /// This method is called when the reporter is ready to add players, and only once.
        /// <para>Reccomended if your reporter needs to make API calls to initialize players</para>
        /// </summary>
        protected virtual void AddPlayersInternal() { }

        public void Dispose()
        {
            StopStream();
            ctEventProcessing.Dispose();
        }
    }
}
