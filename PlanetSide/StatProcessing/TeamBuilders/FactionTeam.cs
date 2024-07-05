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

namespace PlanetSide
{
    public class FactionTeam : PlanetSideTeam
    {
        HashSet<string> nonFactionPlayers = new HashSet<string>();

        public FactionTeam(string teamName, string faction, string world) : base(-1, teamName, faction, world)
        {
            streamKey = $"NexusTeam_{teamName}_PlayerEventStream";
        }

        public async Task GenerateRandomTeam(string streamKey, CensusHandler handler)
        {
            bool filled = false;
            object fillLock = new object();
            object charLock = new object();
            ConcurrentDictionary<string, JsonElement> playersConcurrent = new ConcurrentDictionary<string, JsonElement>(4, TeamSize);

            Logger.LogInformation("Genrating NexusTeam {0}...", TeamName);

            void OnTeamFilled()
            {
                if (filled)
                    return;

                lock (fillLock)
                {
                    filled = true;

                    teamPlayers.Clear();
                    foreach (var kvp in playersConcurrent)
                        teamPlayers.Add(kvp.Key, kvp.Value);

                    Logger.LogInformation("Genrated NexusTeam {0} with {1} players", TeamName, TeamSize);
                }
            }
            bool AddPlayer(string characterId, JsonElement characterData)
            {
                if (playersConcurrent.TryAdd(characterId, characterData) && playersConcurrent.Count >= TeamSize)
                {
                    OnTeamFilled();
                    return true;
                }

                return false;
            }

            // Add a callback to generate the team. Returns true when the team is full.
            handler.AddActionToSubscription(streamKey, response =>
            {
                string eventType;
                JsonElement payload;

                // Skip if malformed or team full
                if (!response.Message.RootElement.TryGetProperty("payload", out payload)
                || !payload.TryGetStringElement("event_name", out eventType))
                    return false;
                if (playersConcurrent.Count >= TeamSize)
                {
                    OnTeamFilled();
                    return true;
                }


                if (eventType == "Death" || eventType == "VehicleDestroy")
                {
                    string[] characterIds = new string[2];

                    // Skip if malformed
                    if (!payload.TryGetStringElement("character_id", out characterIds[0])
                    || !payload.TryGetStringElement("attacker_character_id", out characterIds[1]))
                        return false;

                    Array.Sort(characterIds); // List always returns sorted ascending
                    var query = handler.GetCharactersQuery(characterIds).ShowFields("faction_id", "character_id");
                    var charTask = query.GetListAsync();
                    charTask.Wait();

                    int i = 0;
                    var characters = charTask.Result;
                    foreach (var c in characters)
                    {
                        if (c.TryGetStringElement("faction_id", out string cFaction)
                        && cFaction == Faction
                        && AddPlayer(characterIds[i], c)) // We wont know the order 
                            return true;
                        ++i;
                    }
                }
                else
                {
                    string characterId;
                    bool isCharacterValid = payload.TryGetStringElement("character_id", out characterId)
                        && !playersConcurrent.ContainsKey(characterId);

                    if (isCharacterValid)
                    {
                        var cData = handler.GetCharacter(characterId);
                        return AddPlayer(characterId, cData);
                    }
                }

                return false;
            });

            await Task.Run(() => { while (!filled) ; return true; });
        }

        protected override CensusStreamSubscription GetStreamSubscription()
        {
            return new CensusStreamSubscription()
            {
                Characters = teamPlayers.Keys,
                Worlds = new[] { World },
                EventNames = new[] { "Death", "GainExperience", "VehicleDestroy" },
                LogicalAndCharactersWithWorlds = true
            };
        }

        protected override void OnStreamStart(CensusHandler handler) { }
        protected override void OnStreamStop(CensusHandler handler) { }
        protected override bool IsEventValid(CensusHandler handler, ICensusEvent payload)
        {
            // Valid if it concerns our players.
            if (teamPlayers.ContainsKey(payload.CharacterId) || teamPlayers.ContainsKey(payload.OtherId))
                return true;

            bool playerOneKnown = nonFactionPlayers.Contains(payload.CharacterId);
            bool playerTwoKnown = nonFactionPlayers.Contains(payload.OtherId);
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
                &&  character.TryGetStringElement("faction_id", out string faction))
                {
                    if (faction == Faction)
                    {
                        teamPlayers.Add(id, character);
                        factionPlayerFound = true;
                    }
                    else nonFactionPlayers.Add(id);
                }
            }

            // Return true if we found a player on our faction. False otherwise.
            return factionPlayerFound;
        }
    }
}
