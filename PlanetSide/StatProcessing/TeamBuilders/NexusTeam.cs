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
using System.Diagnostics.CodeAnalysis;

namespace PlanetSide
{
    public class NexusTeam : PlanetSideTeam
    {
        ConcurrentDictionary<string, PlayerStats> playersConcurrent;

        public NexusTeam(int teamId, string teamName, int faction, int world)
            : base(teamId, teamName, faction, world)
        {
            streamKey = $"NexusTeam_{teamName}_PlayerEventStream";
        }

        public override int GetPlayerCount()
        {
            return 48;
        }

        public async Task GenerateRandomTeam(string streamKey, CensusHandler handler)
        {
            // TODO: Generate a team of 48 players using the first ones to show up in the event stream.
        }

        public override CensusStreamSubscription GetStreamSubscription()
        {
            return new CensusStreamSubscription()
            {
                Characters = playersConcurrent.Keys,
                Worlds = new[] { WorldId == -1 ? "all" : WorldId.ToString() },
                EventNames = new[] { "Death", "GainExperience", "VehicleDestroy" },
                LogicalAndCharactersWithWorlds = true
            };
        }

        protected override void OnProcessStart() { }
        protected override void OnProcessStop() { }
        protected override void OnEventProcessed(ICensusEvent payload) { }

        protected override bool IsEventValid(ICensusEvent censusEvent)
        {
            ICensusCharacterEvent charEvent = censusEvent as ICensusCharacterEvent;

            if (charEvent is null)
                return false;

            return TeamPlayers.ContainsKey(charEvent.CharacterId)
                || TeamPlayers.ContainsKey(charEvent.OtherId);
        }
    }
}
