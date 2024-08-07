﻿using DaybreakGames.Census.Stream;
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

namespace PlanetSide
{
    public abstract class PlanetSideTeam : IDisposable
    {
        public int TeamSize { get; private set; }
        public int WorldId { get; private set; }
        public int ZoneId { get; protected set; }
        public int FactionId { get; private set; }
        public string TeamName { get; private set; }
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
        [JsonIgnore] private ConcurrentDictionary<int, WeaponStats> _teamWeaponStats = new ConcurrentDictionary<int, WeaponStats>(8, 128);

        public PlanetSideTeam(int teamSize, string teamName, int faction, string world)
        {
            this.FactionId = faction;
            this.TeamName = teamName;
            this.TeamSize = teamSize;
            worldString = world;

            TeamStats = new PlanetStats();
            TeamWeapons = _teamWeaponStats;
            TeamPlayers = GetTeamDict();

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

            ctQueueProcess.Cancel();
            Tracker.Handler.DisconnectSocketAsync(streamKey).Wait();
            IsStreaming = false;

            SaveStats();
        }

        public void PauseStream()
        {
            if (!IsStreaming || !IsPaused)
                return;

            IsPaused = true;
        }
        public void UnPauseStream()
        {
            if (!IsStreaming || IsPaused)
                return;

            IsPaused = false;
        }

        public void ResetStats()
        {
            TeamStats.Reset();

            foreach(var w in TeamWeapons.Values)
                w.Stats.Reset();

            foreach(var p in TeamPlayers.Values)
                p.Stats.Reset();
        }
        public void SaveStats()
        {
            if (!Directory.Exists("./SavedTeamData"))
                Directory.CreateDirectory("./SavedTeamData");
            using (StreamWriter writer = new StreamWriter($"./SavedTeamData/{TeamName}.json"))
            {
                writer.WriteLine(JsonConvert.SerializeObject(this));
            }
        }

        private bool ProcessCensusEvent(SocketResponse response)
        {
            // Drop all events while paused.
            if (IsPaused)
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

                switch (payload.EventType)
                {
                    case CensusEventType.GainExperience:
                        var expEvent = (ExperiencePayload)payload;
                        bool zoneId = ZoneId == -1 || expEvent.ZoneId == ZoneId;
                        if (zoneId && TeamPlayers.ContainsKey(expEvent.CharacterId))
                        {
                            TeamStats.AddExperience(ref expEvent);
                            TeamPlayers[expEvent.CharacterId].Stats.AddExperience(ref expEvent);
                        }
                        break;
                    case CensusEventType.Death:
                        var deathEvent = (DeathPayload)payload;
                        bool zoneId2 = ZoneId == -1 || deathEvent.ZoneId == ZoneId;
                        if (zoneId2 && TeamPlayers.ContainsKey(deathEvent.OtherId))
                        {
                            TeamStats.AddKill(ref deathEvent);
                            TeamPlayers[deathEvent.OtherId].Stats.AddKill(ref deathEvent);
                            if (TryGetOrAddWeaponStats(deathEvent.AttackerWeaponId, out var wstats))
                                wstats.Stats.AddKill(ref deathEvent);
                        }
                        else if (TeamPlayers.ContainsKey(deathEvent.CharacterId))
                        {
                            TeamStats.AddDeath(ref deathEvent);
                            TeamPlayers[deathEvent.CharacterId].Stats.AddDeath(ref deathEvent);
                            if (TryGetOrAddWeaponStats(deathEvent.AttackerWeaponId, out var wstats))
                                wstats.Stats.AddDeath(ref deathEvent);
                        }
                        break;
                    case CensusEventType.VehicleDestroy:
                        var destroyEvent = (VehicleDestroyPayload)payload;
                        bool zoneId3 = ZoneId == -1 || destroyEvent.ZoneId == ZoneId;
                        if (zoneId3 && TeamPlayers.ContainsKey(destroyEvent.CharacterId))
                        {
                            TeamStats.AddVehicleDeath(ref destroyEvent);
                            TeamPlayers[destroyEvent.CharacterId].Stats.AddVehicleDeath(ref destroyEvent);
                            if (TryGetOrAddWeaponStats(destroyEvent.AttackerWeaponId, out var wstats))
                                wstats.Stats.AddVehicleDeath(ref destroyEvent);
                        }
                        else if (TeamPlayers.ContainsKey(destroyEvent.OtherId))
                        {
                            TeamStats.AddVehicleKill(ref destroyEvent);
                            TeamPlayers[destroyEvent.OtherId].Stats.AddVehicleKill(ref destroyEvent);
                            if (TryGetOrAddWeaponStats(destroyEvent.AttackerWeaponId, out var wstats))
                                wstats.Stats.AddVehicleKill(ref destroyEvent);
                        }
                        break;
                    case CensusEventType.FacilityControl:
                        var facilityEvent = (FacilityControlEvent)payload;
                        if(facilityEvent.NewFaction == FactionId)
                            TeamStats.AddFacilityEvent(ref facilityEvent);
                        break;
                }
            }

            IsProccessing = false;
        }

        private bool TryGetOrAddWeaponStats(int itemId, out WeaponStats weaponStats)
        {
            if (WeaponTable.TryGetWeapon(itemId, out var wData))
            {
                if (TeamWeapons.ContainsKey(itemId))
                    weaponStats = TeamWeapons[itemId];
                else
                {
                    wData.TeamId = FactionId;
                    weaponStats = new WeaponStats()
                    {
                        Data = wData,
                        Stats = new PlanetStats()
                    };
                    _teamWeaponStats[itemId] = weaponStats;
                };
                return true;
            }

            weaponStats = null;
            return false;
        }

        protected abstract ConcurrentDictionary<string, PlayerStats> GetTeamDict();
        protected abstract void OnStreamStart();
        protected abstract void OnStreamStop();
        protected abstract void OnEventProcessed(ICensusEvent payload);
        protected abstract bool IsEventValid(ICensusEvent payload);
        protected abstract CensusStreamSubscription GetStreamSubscription();

        public void Dispose()
        {
            StopStream();
            ctQueueProcess.Dispose();
        }
    }
}
