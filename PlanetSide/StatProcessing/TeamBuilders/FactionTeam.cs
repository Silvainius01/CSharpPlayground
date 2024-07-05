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
        ConcurrentDictionary<string, JsonElement> playersConcurrent = new ConcurrentDictionary<string, JsonElement>();
        static Dictionary<string, CensusStreamSubscription> WorldSubscriptions = new Dictionary<string, CensusStreamSubscription>();

        public FactionTeam(string teamName, string faction, string world, CensusHandler handler)
            : base(-1, teamName, faction, world, handler)
        {
            streamKey = $"World{world}_CharacterEventStream";
        }

        protected override CensusStreamSubscription GetStreamSubscription()
        {
            if (!WorldSubscriptions.ContainsKey(World))
                WorldSubscriptions.Add(World, new CensusStreamSubscription()
                {
                    Characters = new[] { "all" },
                    Worlds = new[] { World },
                    EventNames = new[] { "Death", "GainExperience", "VehicleDestroy" },
                    LogicalAndCharactersWithWorlds = true
                });
            return WorldSubscriptions[World];
        }

        protected override IDictionary<string, JsonElement> GetTeamDict()
        {
            return playersConcurrent;
        }

        protected override void OnStreamStart() { }
        protected override void OnStreamStop() { }
        protected override void OnEventProcessed(ICensusEvent payload)
        {
            // Syncronize dicts
            //if (addedPlayers.Count > 0)
            //    while(addedPlayers.TryDequeue(out var kvp))
            //        _teamPlayers.Add(kvp.Key, kvp.Value);
        }

        protected override bool IsEventValid(ICensusEvent payload)
        {
            // Valid if it concerns our players.
            if (playersConcurrent.ContainsKey(payload.CharacterId)
            || playersConcurrent.ContainsKey(payload.OtherId))
                return true;

            return UnknownCharTable(payload);
        }

        bool UnknownCharTable(ICensusEvent payload)
        {
            bool teamPlayerFound = false;

            if (PlayerTable.TryGetOrAddCharacter(payload.CharacterId, out var cData1) && cData1.Faction == Faction)
            {
                teamPlayerFound = true;
                playersConcurrent.TryAdd(payload.CharacterId, default(JsonElement));
            }
            if (PlayerTable.TryGetOrAddCharacter(payload.CharacterId, out var cData2) && cData2.Faction == Faction)
            {
                teamPlayerFound = true;
                playersConcurrent.TryAdd(payload.CharacterId, default(JsonElement));
            }

            return true;
        }

        bool UnknownCharQuery(ICensusEvent payload)
        {
            bool playerOneKnown = nonFactionPlayers.ContainsKey(payload.CharacterId);
            bool playerTwoKnown = nonFactionPlayers.ContainsKey(payload.OtherId);
            CensusQuery query = null;

            if (!playerOneKnown)
            {
                if (!playerTwoKnown)
                    query = handler.GetCharactersQuery(payload.CharacterId, payload.OtherId).ShowFields("faction_id", "character_id");
                else query = handler.GetCharacterQuery(payload.CharacterId).ShowFields("faction_id", "character_id");
            }
            else if (!playerTwoKnown)
            {
                query = handler.GetCharacterQuery(payload.OtherId);
            }
            else // Both characters known, skip this event.
                return false;

            var queryTask = query.GetListAsync();
            queryTask.Wait();

            // TODO:
            //  Add the bit filter to know if it's an NPC id, or a character id.
            //  Mango mentioned it in the community developer discord.

            bool factionPlayerFound = false;
            foreach (var character in queryTask.Result)
            {
                // We dont know the returned order, so gotta extract the ID.
                if (character.TryGetStringElement("character_id", out string id)
                && character.TryGetStringElement("faction_id", out string faction))
                {
                    if (faction == Faction)
                    {
                        playersConcurrent.TryAdd(id, character);
                        factionPlayerFound = true;
                    }
                    else nonFactionPlayers.TryAdd(id, 0);
                }
            }

            // Return true if we found a player on our faction. False otherwise.
            return factionPlayerFound;
        }
    }
}
