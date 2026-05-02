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
using System.Collections.Generic;

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
            if (!IsValidCharacterId(id))
            {
                cData = default(CharacterData);
                return false;
            }

            return TryGetOrAddCharacter(Tracker.Handler.GetCharacterQuery(id), out cData);
        }
        /// <summary> Will retrieve character data from local cache, or query Census API if it is missing. </summary>
        public static bool TryGetOrAddCharacterByName(string name, out CharacterData cData)
        {
            if (TryGetCharacterByName(name, out cData))
                return true;
            return TryGetOrAddCharacter(Tracker.Handler.GetCharacterQueryByName(name), out cData);
        }
        static bool TryGetOrAddCharacter(CensusQuery query, out CharacterData cData)
        {
            if (GetCharacterResponse(query, out IEnumerable<JsonElement> results)
             && ValidateCharacterResponse(results.First(), out cData))
            {
                AddCharacter(cData);
                return true;
            }

            cData = default(CharacterData);
            return false;
        }

        public static bool TryAddCharacters(params string[] ids)
        {
            var validIds = ids.Where(id => IsValidCharacterId(id)).ToArray();
            return TryAddCharacters(Tracker.Handler.GetCharactersQuery(validIds));
        }
        public static bool TryAddCharactersByName(params string[] names)
            => TryAddCharacters(Tracker.Handler.GetCharactersQueryByName(names));
        static bool TryAddCharacters(CensusQuery query)
        {
            if (GetCharacterResponse(query, out IEnumerable<JsonElement> results))
            {
                foreach (var result in results)
                {
                    if (ValidateCharacterResponse(result, out var cData))
                        if (!_characters.ContainsKey(cData.CensusId))
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

        static bool GetCharacterResponse(CensusQuery? query, out IEnumerable<JsonElement> results)
        {
            int retry = 0;
            bool foundChar = false;
            results = default;

            if (query is null)
                return false;

            var filteredQuery = query.ShowFields("character_id", "faction_id", "name.first");

            do
            {
                try
                {
                    var queryTask = filteredQuery.GetListAsync();
                    queryTask.Wait();
                    results = queryTask.Result;
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

            if (results is null || !results.Any())
            {
                return false;
            }

            return true;
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

        public static bool IsValidCharacterId(string id)
        {
            return !string.IsNullOrEmpty(id) && !"02468".Contains(id.Last());
        }

        public static bool IsSameFaction(string characterId, string otherId)
        {
            return TryGetOrAddCharacter(characterId, out var cData)
                && TryGetOrAddCharacter(otherId, out var oData)
                && cData.FactionId == oData.FactionId;
        }
    }
}
