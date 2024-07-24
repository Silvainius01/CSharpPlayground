﻿using DaybreakGames.Census.Stream;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace PlanetSide
{
    public abstract class PlanetSideTeam : IDisposable
    {
        public int TeamSize { get; private set; }
        public int WorldId { get; private set; }
        public int ZoneId { get; private set; }
        public int FactionId { get; private set; }
        public string TeamName { get; private set; }
        public PlanetStats TeamStats { get; private set; }
        public ReadOnlyDictionary<int, WeaponStats> TeamWeapons { get; private set; }
        public ReadOnlyDictionary<string, PlayerStats> TeamPlayers { get; private set; }

        public bool IsStreaming { get; private set; }
        public bool IsProccessing { get; private set; }

        protected string worldString;
        protected string streamKey = string.Empty;
        protected readonly ILogger<PlanetSideTeam> Logger;

        private ConcurrentQueue<ICensusEvent> events;
        private CancellationTokenSource ctQueueProcess;
        private ConcurrentDictionary<int, WeaponStats> _teamWeaponStats = new ConcurrentDictionary<int, WeaponStats>(8, 128);

        public PlanetSideTeam(int teamSize, string teamName, int faction, string world)
        {
            this.FactionId = faction;
            this.TeamName = teamName;
            this.TeamSize = teamSize;
            worldString = world;
            
            TeamStats = new PlanetStats();
            TeamWeapons = new ReadOnlyDictionary<int, WeaponStats>(_teamWeaponStats);
            TeamPlayers = new ReadOnlyDictionary<string, PlayerStats>(GetTeamDict());

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
        }

        private bool ProcessCensusEvent(SocketResponse response)
        {
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
                        if (TeamPlayers.ContainsKey(expEvent.CharacterId))
                        {
                            TeamStats.AddExperience(ref expEvent);
                            TeamPlayers[expEvent.CharacterId].Stats.AddExperience(ref expEvent);
                        }
                        break;
                    case CensusEventType.Death:
                        var deathEvent = (DeathPayload)payload;
                        if (TeamPlayers.ContainsKey(deathEvent.OtherId))
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
                            if(TryGetOrAddWeaponStats(deathEvent.AttackerWeaponId, out var wstats))
                                wstats.Stats.AddDeath(ref deathEvent);
                        }
                        break;
                    case CensusEventType.VehicleDestroy:
                        var destroyEvent = (VehicleDestroyPayload)payload;

                        if (TeamPlayers.ContainsKey(destroyEvent.CharacterId))
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
                }
            }

            IsProccessing = false;
        }

        private bool TryGetOrAddWeaponStats(int itemId, out WeaponStats weaponStats)
        {
            if (WeaponTable.TryGetWeapon(itemId, out var wData))
            {
                if(TeamWeapons.ContainsKey(itemId))
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

        protected abstract IDictionary<string, PlayerStats> GetTeamDict();
        protected abstract void OnStreamStart();
        protected abstract void OnStreamStop();
        protected abstract void OnEventProcessed(ICensusEvent payload);
        protected abstract bool IsEventValid(ICensusEvent payload);
        protected abstract CensusStreamSubscription GetStreamSubscription();

        public void Dispose()
        {
            ctQueueProcess.Dispose();
        }
    }
}
