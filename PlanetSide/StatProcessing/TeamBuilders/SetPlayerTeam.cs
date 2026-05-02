using DaybreakGames.Census.Stream;
using Microsoft.Extensions.Logging;
using PlanetSide.Websocket;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PlanetSide
{
    public class SetPlayerTeam : PlanetSideTeam
    {

        ConcurrentDictionary<string, PlayerStats> playersConcurrent = new ConcurrentDictionary<string, PlayerStats>();

        public SetPlayerTeam(string teamName, string world, params PlayerCsvEntry[] players)
            : base(players.Length, teamName, -1, world)
        {
            ZoneId = -1;
            streamKey = $"{teamName}_CharacterEventStream";

            PlayerTable.TryAddCharacters(players.Select(p => p.CensusId).ToArray());

            bool first = true;
            foreach (var player in players)
            {
                if (PlayerTable.TryGetCharacter(player.CensusId, out var cData))
                {
                    if (cData.FactionId == FactionId || first)
                    {
                        first = false;
                        FactionId = cData.FactionId;
                        playersConcurrent.TryAdd(cData.CensusId, new PlayerStats()
                        {
                            Alias = player.Alias,
                            Data = cData,
                            Stats = new PlanetStats()
                        });
                    }
                    else Logger.LogWarning("{0} is not on the correct faction for team {1} ({2})", player.Alias, teamName, Tracker.FactionIdToName(FactionId));
                }
                else
                    Logger.LogError("Failed to find character data for {0} ({1})", player.Alias, player.CensusId);
            }

            if (playersConcurrent.Count < players.Length)
            {
                Logger.LogError("Team {0} was initialized with {1}/{2} players. Check previous logs for details.", teamName, playersConcurrent.Count, players.Length);
            }
        }

        protected override CensusStreamSubscription GetStreamSubscription()
        {
            var sub = new CensusStreamSubscription()
            {
                Characters = playersConcurrent.Values.Select(p => p.Data.CensusId),
                Worlds = new[] { worldString },
                EventNames = new[] { "Death", "GainExperience", "VehicleDestroy", "FacilityControl" },
                LogicalAndCharactersWithWorlds = true
            };
            return sub;
        }

        protected override ConcurrentDictionary<string, PlayerStats> GetTeamDict()
        {
            return playersConcurrent;
        }

        protected override void OnStreamStart() { }
        protected override void OnStreamStop() { }
        protected override void OnEventProcessed(ICensusEvent censusEvent) { }

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
