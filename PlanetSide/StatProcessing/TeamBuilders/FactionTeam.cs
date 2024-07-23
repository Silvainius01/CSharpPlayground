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

namespace PlanetSide
{
    public class FactionTeam : PlanetSideTeam
    {
        ConcurrentDictionary<string, byte> nonFactionPlayers = new ConcurrentDictionary<string, byte>();
        ConcurrentDictionary<string, PlayerStats> playersConcurrent = new ConcurrentDictionary<string, PlayerStats>();
        static Dictionary<string, CensusStreamSubscription> WorldSubscriptions = new Dictionary<string, CensusStreamSubscription>();

        public FactionTeam(string teamName, int faction, string world)
            : base(-1, teamName, faction, world)
        {
            streamKey = $"World{world}_CharacterEventStream";
        }

        protected override CensusStreamSubscription GetStreamSubscription()
        {
            if (!WorldSubscriptions.ContainsKey(worldString))
                WorldSubscriptions.Add(worldString, new CensusStreamSubscription()
                {
                    Characters = new[] { "all" },
                    Worlds = new[] { worldString },
                    EventNames = new[] { "Death", "GainExperience", "VehicleDestroy" },
                    LogicalAndCharactersWithWorlds = true
                });
            return WorldSubscriptions[worldString];
        }

        protected override IDictionary<string, PlayerStats> GetTeamDict()
        {
            return playersConcurrent;
        }

        protected override void OnStreamStart() { }
        protected override void OnStreamStop() { }
        protected override void OnEventProcessed(ICensusEvent payload){ }

        protected override bool IsEventValid(ICensusEvent payload)
        {
            switch(payload.EventType)
            {
                case CensusEventType.Death:
                case CensusEventType.GainExperience:
                case CensusEventType.VehicleDestroy:
                    break;
                default:
                    return false;
            }

            ICensusCharacterEvent charEvent = payload as ICensusCharacterEvent;

            if (playersConcurrent.ContainsKey(charEvent.CharacterId)
            || playersConcurrent.ContainsKey(charEvent.OtherId))
                return true;

            return IsEventFromTeam(charEvent);
        }

        bool IsEventFromTeam(ICensusCharacterEvent payload)
        {
            bool teamPlayerFound = false;

            if (PlayerTable.TryGetOrAddCharacter(payload.CharacterId, out var cData1) && cData1.FactionId == Faction)
            {
                teamPlayerFound = true;
                playersConcurrent.TryAdd(payload.CharacterId, new PlayerStats()
                {
                    Data= cData1,
                    Stats = new PlanetStats()
                });
            }
            if (PlayerTable.TryGetOrAddCharacter(payload.OtherId, out var cData2) && cData2.FactionId == Faction)
            {
                teamPlayerFound = true;
                playersConcurrent.TryAdd(payload.OtherId, new PlayerStats()
                {
                    Data = cData1,
                    Stats = new PlanetStats()
                });
            }

            return teamPlayerFound;
        }
    }
}
