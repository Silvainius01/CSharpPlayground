﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using DaybreakGames.Census;
using Websocket.Client;

namespace PlanetSide
{
    public static class PlayerTable
    {
        const int playerRetry = 5;
        static ConcurrentDictionary<string, CharacterData> _characters = new ConcurrentDictionary<string, CharacterData>();
        public static ReadOnlyDictionary<string, CharacterData> Characters = new ReadOnlyDictionary<string, CharacterData>(_characters);

        static ILogger Logger = Program.LoggerFactory.CreateLogger(typeof(PlayerTable));


        /// <summary> Will retrieve character data from local cache, or query Census API if it is missing. </summary>
        public static bool TryGetOrAddCharacter(string id, out CharacterData cData)
        {
            cData = default(CharacterData);

            if (_characters.ContainsKey(id))
            {
                cData = _characters[id];
                return true;
            }
            else if ("02468".Contains(id.Last())) // Char IDs are always odd numbers.
                return false;
            else
            {
                int retry = 0;
                bool foundChar = false;
                var query = Tracker.Handler.GetCharacterQuery(id).ShowFields("faction_id", "name.first");
                JsonElement result = default(JsonElement);

                do
                {
                    try
                    {
                        var queryTask = query.GetAsync();
                        queryTask.Wait();
                        result = queryTask.Result;
                        foundChar = true;
                    }
                    catch (Exception ex)
                    {
                        if (retry >= playerRetry - 1)
                            Logger.LogError(ex, $"Exception when retriveing Char ID {id}");
                        ++retry;
                    }
                }
                while (!foundChar && retry < playerRetry);

                if (result.Equals(default(JsonElement)))
                    return false;

                if (!result.TryGetProperty("name", out var nameParent)
                || !nameParent.TryGetStringElement("first", out string name)
                || !result.TryGetCensusInteger("faction_id", out int faction))
                    return false;

                cData = new CharacterData()
                {
                    CensusId = id,
                    Name = name,
                    FactionId = faction
                };
                _characters[id] = cData;
                return true;
            }

            return false;
        }
        /// <summary> Will retrieve character data from local cache, but wont query Census. </summary>
        public static bool TryGetCharacter(string id, out CharacterData cData) => _characters.TryGetValue(id, out cData);

        public static bool IsSameFaction(string CharacterId, string otherId)
        {
            return TryGetOrAddCharacter(CharacterId, out var cData)
                && TryGetOrAddCharacter(otherId, out var oData)
                && cData.FactionId == oData.FactionId;
        }
    }
}
