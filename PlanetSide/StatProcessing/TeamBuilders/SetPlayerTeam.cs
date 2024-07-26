using DaybreakGames.Census.Stream;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanetSide.StatProcessing.TeamBuilders
{
    public class SetPlayerTeam : PlanetSideTeam
    {
        public SetPlayerTeam(string teamName, params string[] playerNames)
            :   base(playerNames.Length, teamName, -1, "all")
        {
            streamKey = $"{teamName}_CharacterEventStream";
        }

        protected override CensusStreamSubscription GetStreamSubscription()
        {
            throw new NotImplementedException();
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
