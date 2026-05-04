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
        PlayerCsvEntry[] _inputPlayers;

        public SetPlayerTeam(int teamId, string teamName, string world, params PlayerCsvEntry[] players)
            : base(teamId, teamName, -1, world)
        {
            ZoneId = -1;
            FactionId = -1;
            _inputPlayers = players;
            streamKey = $"{teamName}_CharacterEventStream";
        }

        protected override CensusStreamSubscription GetStreamSubscription()
        {
            var sub = new CensusStreamSubscription()
            {
                Characters = _teamPlayerStats.Values.Select(p => p.Data.CensusId),
                Worlds = new[] { worldString },
                EventNames = new[] { "Death", "GainExperience", "VehicleDestroy", "FacilityControl" },
                LogicalAndCharactersWithWorlds = true
            };
            return sub;
        }

        public override void GetPlayers()
        {
            PlayerTable.TryAddCharacters(_inputPlayers.Select(p => p.CensusId).ToArray());

            bool first = true;
            foreach (var player in _inputPlayers)
            {
                if (PlayerTable.TryGetCharacter(player.CensusId, out var cData))
                {
                    if (first)
                    {
                        first = false;
                        FactionId = cData.FactionId;
                    }

                    if (cData.FactionId == FactionId)
                    {
                        cData.TeamId = TeamId;
                        _teamPlayerStats.TryAdd(cData.CensusId, new PlayerStats()
                        {
                            Alias = player.Alias,
                            Data = cData,
                            Stats = new PlanetStats()
                        });
                    }
                    else Logger.LogWarning("{0} is not on the correct faction for team {1} ({2})", player.Alias, TeamName, Tracker.FactionIdToName(FactionId));
                }
                else
                    Logger.LogError("Failed to find character data for {0} ({1})", player.Alias, player.CensusId);
            }

            if (_teamPlayerStats.Count < _inputPlayers.Length)
            {
                Logger.LogError("Team {0} was initialized with {1}/{2} players. Check previous logs for details.", TeamName, _teamPlayerStats.Count, _inputPlayers.Length);
            }
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
