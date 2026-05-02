using DaybreakGames.Census.Stream;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanetSide.StatProcessing.TeamBuilders
{
    public class SetPlayerTeam : PlanetSideTeam
    {
        string[] characterNames;
        ConcurrentDictionary<string, PlayerStats> playersConcurrent = new ConcurrentDictionary<string, PlayerStats>();

        public SetPlayerTeam(string teamName, string world, params string[] playerNames)
            : base(playerNames.Length, teamName, -1, world)
        {
            ZoneId = -1;
            streamKey = $"{teamName}_CharacterEventStream";
            characterNames = playerNames;
        }

        protected override CensusStreamSubscription GetStreamSubscription()
        {
            foreach(var name in characterNames)
            {
                PlayerTable.TryGetOrAddCharacter
            }

            var sub = new CensusStreamSubscription()
            {
                Characters = characterNames,
                Worlds = new[] { worldString },
                EventNames = new[] { "Death", "GainExperience", "VehicleDestroy", "FacilityControl" },
                LogicalAndCharactersWithWorlds = true
            };
            return sub;
        }

        protected override ConcurrentDictionary<string, PlayerStats> GetTeamDict()
        {
            throw new NotImplementedException();
        }

        protected override bool IsEventValid(ICensusEvent payload)
        {
            throw new NotImplementedException();
        }

        protected override void OnEventProcessed(ICensusEvent payload)
        {
            throw new NotImplementedException();
        }

        protected override void OnStreamStart()
        {
            throw new NotImplementedException();
        }

        protected override void OnStreamStop()
        {
            throw new NotImplementedException();
        }
    }
}
