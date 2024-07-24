using PlanetSide.StatProcessing.StatObjects;
using PlanetSide.Websocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanetSide
{
    public class Leaderboard<TStats, TData> where TStats : IStatObject<TData> where TData : ITeamDataObject
    {
        LeaderboardRequest request;

        Comparison<TStats> statComparer;
        Func<PlanetSideTeam, IEnumerable<TStats>> GetStatCollection;

        List<TStats> topStats;
        List<(PlanetSideTeam team, List<TStats> sortedStats)> statTuples;
        LeaderboardEntry[] calculatedBoard;

        public Leaderboard(LeaderboardRequest request, Func<PlanetSideTeam, IEnumerable<TStats>> getStatCollection, params PlanetSideTeam[] teams)
        {
            this.request = request;
            GetStatCollection = getStatCollection;
            calculatedBoard = new LeaderboardEntry[request.BoardSize];
            topStats = new List<TStats>(request.BoardSize * teams.Length);
            statComparer = (a, b)
                => -this.request.GetStat(a.Stats).CompareTo(this.request.GetStat(b.Stats));

            statTuples = new List<(PlanetSideTeam team, List<TStats> sortedStats)>(teams.Length);
            foreach (var team in teams)
                statTuples.Add((team, GetStatCollection(team).ToList()));
        }

        public LeaderboardEntry[] Calculate()
        {
            calculatedBoard = new LeaderboardEntry[request.BoardSize];

            foreach (var tuple in statTuples)
            {
                if (tuple.sortedStats.Count != tuple.team.TeamPlayers.Count)
                {
                    tuple.sortedStats.Clear();
                    tuple.sortedStats.AddRange(GetStatCollection(tuple.team));
                }

                tuple.sortedStats.Sort(statComparer);
            }

            topStats.Clear();
            for (int i = 0; i < request.BoardSize; ++i)
            {
                foreach (var tuple in statTuples)
                    if(i < tuple.sortedStats.Count)
                        topStats.Add(tuple.sortedStats[i]);
            }
            topStats.Sort(statComparer);

            for (int i = 0; i < request.BoardSize && i < topStats.Count; ++i)
            {
                float score = request.GetStat(topStats[i].Stats);
                if (score > 0) // Only add players with a score to begin with.
                    calculatedBoard[i] = new LeaderboardEntry()
                    {
                        Score = score,
                        EntryName = topStats[i].Data.Name,
                        TeamId = topStats[i].Data.TeamId
                    };
            }

            return calculatedBoard;
        }

        public LeaderboardEntry[] Get() 
            => calculatedBoard;
    }

    public class PlayerLeaderboard : Leaderboard<PlayerStats, CharacterData>
    {
        public PlayerLeaderboard(LeaderboardRequest request, params PlanetSideTeam[] teams)
            : base(request, (team) => team.TeamPlayers.Values, teams)
        {
            if (request.LeaderboardType != LeaderboardType.Player)
                throw new ArgumentException($"Cannot create a PlayerLeaderboard with type {request.LeaderboardType}");
        }
    }

    public class WeaponLeaderboard : Leaderboard<WeaponStats, WeaponData>
    {
        public WeaponLeaderboard(LeaderboardRequest request, params PlanetSideTeam[] teams)
            : base(request, (team) => team.TeamWeapons.Values, teams)
        {
            if (request.LeaderboardType != LeaderboardType.Weapon)
                throw new ArgumentException($"Cannot create a PlayerLeaderboard with type {request.LeaderboardType}");
        }
    }
}
