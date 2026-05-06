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
using DaybreakGames.Census.Operators;
using System.Collections.ObjectModel;
using System.Reactive.Joins;
using PlanetSide.StatProcessing.Events;

namespace PlanetSide
{
    public class FactionTeam : PlanetSideTeam
    {
        bool newPlayerAdded = false;

        public FactionTeam(string teamName, int faction, int world, int zone = -1)
            : base(faction, teamName, faction, world)
        {
            ZoneId = zone;
            streamKey = $"World{world}_CharacterEventStream";
        }

        public override int GetPlayerCount()
        {
            if (newPlayerAdded)
            {
                newPlayerAdded = false;
                _teamSize = _teamPlayerStats.Count;
            }
            return _teamSize;
        }

        protected override void OnProcessStart() { }
        protected override void OnProcessStop() { }
        protected override void OnEventProcessed(ICensusEvent payload) { }
        public override CensusStreamSubscription GetStreamSubscription()
        {
            return new CensusStreamSubscription()
            {
                Characters = new[] { "all" },
                Worlds = new[] { WorldId == -1 ? "all" : WorldId.ToString() },
                EventNames = new[] { "Death", "GainExperience", "VehicleDestroy", "FacilityControl" },
                LogicalAndCharactersWithWorlds = true
            };
        }

        protected override bool IsEventValid(ICensusEvent payload)
        {
            switch (payload.EventType)
            {
                case CensusEventType.Death:
                case CensusEventType.GainExperience:
                case CensusEventType.VehicleDestroy:
                    break;
                case CensusEventType.FacilityControl:
                    var facilityEvent = (FacilityControlEvent)payload;
                    return facilityEvent.NewFaction == FactionId;
                default:
                    return false;
            }

            ICensusCharacterEvent charEvent = payload as ICensusCharacterEvent;

            if (_teamPlayerStats.ContainsKey(charEvent.CharacterId)
            || _teamPlayerStats.ContainsKey(charEvent.OtherId))
                return true;

            return IsEventFromTeam(charEvent);
        }

        bool IsEventFromTeam(ICensusCharacterEvent payload)
        {
            bool teamPlayerFound = false;

            bool IsTeamCharacter(string id)
            {
                if (PlayerTable.TryGetOrAddCharacter(id, out var cData) && cData.FactionId == FactionId)
                {
                    if (!_teamPlayerStats.ContainsKey(id))
                    {
                        cData.TeamId = FactionId;
                        PlayerStats stats = new PlayerStats()
                        {
                            Data = cData,
                            Stats = new PlanetStats()
                        };

                        if (_teamPlayerStats.TryAdd(id, stats))
                        {
                            newPlayerAdded = true;
                            Logger.LogDebug($"Added character {cData.Name} to {Tracker.FactionIdToName(FactionId, false)}");
                        }
                        else Logger.LogWarning($"Failed to add character {id} to {Tracker.FactionIdToName(FactionId, false)}");
                    }
                    return true;
                }
                return false;
            }

            return IsTeamCharacter(payload.CharacterId) || IsTeamCharacter(payload.OtherId);
        }
    }
}
