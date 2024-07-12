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

namespace PlanetSide
{
    public static class PlayerTable
    {
        static ConcurrentDictionary<string, CharacterData> _characters = new ConcurrentDictionary<string, CharacterData>();

        public static ReadOnlyDictionary<string, CharacterData> Characters = new ReadOnlyDictionary<string, CharacterData>(_characters);

        /// <summary> Will retrieve character data from local cache, or query Census API if it is missing. </summary>
        public static bool TryGetOrAddCharacter(string id, out CharacterData cData)
        {
            cData = null;

            if (_characters.ContainsKey(id))
            {
                cData = _characters[id];
                return true;
            }
            else
            {
                var query = Tracker.Handler.GetCharacterQuery(id).ShowFields("faction_id", "name.first");
                var queryTask = query.GetAsync();

                queryTask.Wait();

                var result = queryTask.Result;

                if (!result.TryGetProperty("name", out var nameParent)
                || !nameParent.TryGetStringElement("first", out string name)
                || !result.TryGetStringElement("faction_id", out string faction))
                    return false;

                cData = new CharacterData()
                {
                    Id = id,
                    Name = name,
                    Faction = faction
                };
                _characters[id] = cData;
                return true;
            }

            return false;
        }
        /// <summary> Will retrieve character data from local cache, but wont query Census. </summary>
        public static void TryGetCharacter(string id, out CharacterData cData) => _characters.TryGetValue(id, out cData);
    }

    public class CharacterData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Faction { get; set; }
    }
}
