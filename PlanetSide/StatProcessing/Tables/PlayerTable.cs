using System;
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
using DaybreakGames.Census.Operators;
using System.Text.RegularExpressions;

namespace PlanetSide
{
    public static class PlayerTable
    {
        public static ReadOnlyDictionary<string, CharacterData> Characters = new ReadOnlyDictionary<string, CharacterData>(_characters);
        
        const int playerRetry = 5;
        static ConcurrentDictionary<string, CharacterData> _characters = new ConcurrentDictionary<string, CharacterData>();

        static ILogger Logger = Program.LoggerFactory.CreateLogger(typeof(PlayerTable));

        public static bool TryGetCharacter(string id, out CharacterData cData)
            => _characters.TryGetValue(id, out cData);

        /// <summary> Will retrieve character data from local cache, or query Census API if it is missing. </summary>
        public static bool TryGetOrAddCharacter(string id, out CharacterData cData)
        {
            if (_characters.ContainsKey(id))
            {
                cData = _characters[id];
                return true;
            }
            else if ("02468".Contains(id.Last())) // Char IDs are always odd numbers.
            {
                cData = default(CharacterData);
                return false;
            }

            return GetCharacterFromQuery(Tracker.Handler.GetCharacterQuery(id), out cData);
        }

        static bool GetCharacterFromQuery(CensusQuery? query, out CharacterData cData)
        {
            int retry = 0;
            bool foundChar = false;
            JsonElement result = default(JsonElement);

            if (query is null)
            {
                cData = default(CharacterData);
                return false;
            }

            query = query.ShowFields("character_id", "faction_id", "name.first");

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
                    // Dont deal with the error unless it happens on our final attempt
                    if (retry < playerRetry - 1)
                    {
                        ++retry;
                        continue;
                    }

                    string charId = string.Empty;
                    string qstr = query.ToString();
                    var matchId = Regex.Match(qstr, "character_id=[0123456789]*");
                    var matchName = Regex.Match(qstr, "first_lower=[[:alnum:]]*");

                    if (matchId.Success)
                    {
                        charId = matchId.Value;
                        Logger.LogError(ex, $"Exception when retrieving character {charId}:\t\n{ex}");
                    }
                    else if (matchName.Success)
                    {
                        charId = matchName.Value;
                        Logger.LogError(ex, $"Exception when retrieving character {charId}:\t\n{ex}");
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
                cData = default(CharacterData);
                return false;
            }


            // Validate and extract character data
            if (!result.TryGetProperty("name", out var nameParent)
             || !nameParent.TryGetStringElement("first", out string name)
             || !result.TryGetCensusInteger("faction_id", out int faction)
             || !result.TryGetStringElement("character_id", out string id))
            {
                cData = default(CharacterData);
                return false;
            }

            cData = new CharacterData()
            {
                CensusId = id,
                Name = name,
                FactionId = faction
            };
            _characters[id] = cData;
            return true;
        }

        public static bool IsSameFaction(string characterId, string otherId)
        {
            return TryGetOrAddCharacter(characterId, out var cData)
                && TryGetOrAddCharacter(otherId, out var oData)
                && cData.FactionId == oData.FactionId;
        }
    }
}
