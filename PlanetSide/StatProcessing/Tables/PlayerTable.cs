using System;
using System.Linq;
using System.Text.Json;
using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using DaybreakGames.Census;
using Websocket.Client;
using DaybreakGames.Census.Operators;
using System.Text.RegularExpressions;
using System.Collections.Generic;z

namespace PlanetSide
{
    public static class PlayerTable
    {
        const int playerRetry = 5;
        static ConcurrentDictionary<string, CharacterData> _characters = new ConcurrentDictionary<string, CharacterData>();
        static ConcurrentDictionary<string, string> _nameToId = new ConcurrentDictionary<string, string>();

        public static ReadOnlyDictionary<string, CharacterData> Characters = new ReadOnlyDictionary<string, CharacterData>(_characters);

        static ILogger Logger = Program.LoggerFactory.CreateLogger(typeof(PlayerTable));

        public static bool TryGetCharacter(string id, out CharacterData cData)
            => _characters.TryGetValue(id, out cData);
        public static bool TryGetCharacterByName(string name, out CharacterData cData)
        {
            if (_nameToId.TryGetValue(name, out string id))
                return TryGetCharacter(id, out cData);

            cData = default(CharacterData);
            return false;
        }

        /// <summary> Will retrieve character data from local cache, or query Census API if it is missing. </summary>
        public static bool TryGetOrAddCharacter(string id, out CharacterData cData)
        {
            if (TryGetCharacter(id, out cData))
                return true;

            // Char IDs are always odd numbers.
            if (!"02468".Contains(id.Last())
             && GetCharacterResponse(Tracker.Handler.GetCharacterQuery(id), out JsonElement result)
             && ValidateCharacterResponse(result, out cData))
            {
                AddCharacter(cData);
                return true;
            }

            cData = default(CharacterData);
            return false;
        }
        /// <summary> Will retrieve character data from local cache, or query Census API if it is missing. </summary>
        public static bool TryGetOrAddCharacterByName(string name, out CharacterData cData)
        {
            if (TryGetCharacterByName(name, out cData))
                return true;

            if (GetCharacterResponse(Tracker.Handler.GetCharacterQueryByName(name), out JsonElement result)
             && ValidateCharacterResponse(result, out cData))
            {
                AddCharacter(cData);
                return true;
            }

            cData = default(CharacterData);
            return false;
        }

        public static bool TryAddCharacters(params string[] ids)
        {
            if (GetCharacterResponse(Tracker.Handler.GetCharactersQuery(ids), out JsonElement result))
            {
                var listParent = result.GetProperty("character_list");

                foreach (JsonElement element in listParent.EnumerateArray())
                {
                    if (ValidateCharacterResponse(element, out var cData))
                        AddCharacter(cData);
                }
                return true;
            }

            return false;
        }
        public static bool TryAddCharactersByName(params string[] names)
        {
            if (GetCharacterResponse(Tracker.Handler.GetCharactersQueryByName(names), out JsonElement result))
            {
                var listParent = result.GetProperty("character_list");

                foreach (JsonElement element in listParent.EnumerateArray())
                {
                    if (ValidateCharacterResponse(element, out var cData))
                        AddCharacter(cData);
                }
                return true;
            }

            return false;
        }

        static void AddCharacter(CharacterData cData)
        {
            _characters[cData.CensusId] = cData;
            _nameToId[cData.Name] = cData.CensusId;
        }

        static bool GetCharacterResponse(CensusQuery? query, out JsonElement result)
        {
            int retry = 0;
            bool foundChar = false;
            result = default(JsonElement);

            if (query is null)
                return false;

            var filteredQuery = query.ShowFields("character_id", "faction_id", "name.first");

            do
            {
                try
                {
                    var queryTask = filteredQuery.GetListAsync();
                    queryTask.Wait();
                    IEnumerable<JsonElement> r = queryTask.Result;
                    foundChar = true;
                }
                catch (Exception ex)
                {
                    // Dont deal with the error unless it happens on our final attempt
                    if (retry < playerRetry - 1)
                    {
                        ++retry;
                        continue;
                    }

                    string charId = string.Empty;
                    string qstr = query.ToString();
                    var matchId = Regex.Match(qstr, "character_id=[0-9,]*");
                    var matchName = Regex.Match(qstr, "first_lower=[[:alnum:],]*");

                    if (matchId.Success)
                    {
                        charId = matchId.Value;
                        Logger.LogError(ex, $"Exception when retrieving character(s) {charId}:\t\n{ex}");
                    }
                    else if (matchName.Success)
                    {
                        charId = matchName.Value;
                        Logger.LogError(ex, $"Exception when retrieving character(s) {charId}:\t\n{ex}");
                    }
                    else
                    {
                        Logger.LogError(ex, $"Exception when running query {qstr}:\t\n{ex}");
                    }
                }
            }
            while (!foundChar && retry < playerRetry);

            if (result.Equals(default(JsonElement)))
            {
                return false;
            }


            // If getting a single character
            if (result.TryGetProperty("name", out var nameParent))
            {
                return true;
            }
            // If getting multiple characters
            else if (result.TryGetProperty("character_list", out var listParent))
            {
                return true;
            }

            return false;
        }

        static bool ValidateCharacterResponse(JsonElement result, out CharacterData cData)
        {
            if (result.TryGetProperty("name", out var nameParent))
            {
                if (nameParent.TryGetStringElement("first", out string name)
                 && result.TryGetCensusInteger("faction_id", out int faction)
                 && result.TryGetStringElement("character_id", out string responseId))
                {
                    cData = new CharacterData()
                    {
                        CensusId = responseId,
                        Name = name,
                        FactionId = faction
                    };
                    return true;
                }
            }

            cData = default(CharacterData);
            Logger.LogError($"Received malformed character response: {result.ToString()}");
            return false;
        }

        public static bool IsSameFaction(string characterId, string otherId)
        {
            return TryGetOrAddCharacter(characterId, out var cData)
                && TryGetOrAddCharacter(otherId, out var oData)
                && cData.FactionId == oData.FactionId;
        }
    }
}
