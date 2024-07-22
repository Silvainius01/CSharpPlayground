using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PlanetSide.Websocket
{
    public struct LeaderboardRequest
    {
        public string Name { get; set; }
        public Func<PlanetStats, float> GetStat { get; set; }
    }

    public struct LeaderboardEntry
    {
        public int TeamId { get; set; }
        public float Score { get; set; }
        public string PlayerName { get; set; }
    }

    public class CommSmashLeaderboard
    {
        FactionTeam teamOne;
        FactionTeam teamTwo;
        public Dictionary<string, List<LeaderboardEntry>> Boards = new Dictionary<string, List<LeaderboardEntry>>();

        public CommSmashLeaderboard(FactionTeam team1, FactionTeam team2)
        {
            this.teamOne = team1;
            this.teamTwo = team2;
        }

        List<PlayerStats> _allPlayers = new List<PlayerStats>();
        List<PlayerStats> _teamOnePlayers = new List<PlayerStats>();
        List<PlayerStats> _teamTwoPlayers = new List<PlayerStats>();
        public void GenerateLeaderboard(LeaderboardRequest request)
        {
            var leaderboard = Boards.ContainsKey(request.Name)
                ? Boards[request.Name]
                : (Boards[request.Name] = new List<LeaderboardEntry>());
            Comparison<PlayerStats> playerComparer = (a, b) 
                => -request.GetStat(a.EventStats).CompareTo(request.GetStat(b.EventStats));


            if (_teamOnePlayers.Count != teamOne.TeamPlayers.Count)
            {
                _teamOnePlayers.Clear();
                _teamOnePlayers.AddRange(teamOne.TeamPlayers.Values);
            }

            if (_teamTwoPlayers.Count != teamTwo.TeamPlayers.Count)
            {
                _teamTwoPlayers.Clear();
                _teamTwoPlayers.AddRange(teamTwo.TeamPlayers.Values);
            }

            _teamOnePlayers.Sort(playerComparer);
            _teamTwoPlayers.Sort(playerComparer);

            _allPlayers.Clear();
            for (int i = 0; i < 10; ++i)
            {
                if (i < _teamOnePlayers.Count)
                    _allPlayers.Add(_teamOnePlayers[i]);
                if (i < _teamTwoPlayers.Count)
                    _allPlayers.Add(_teamTwoPlayers[i]);
            }
            _allPlayers.Sort(playerComparer);

            leaderboard.Clear();
            for(int i = 0; i < 10 && i < _allPlayers.Count; ++i)
            {
                leaderboard.Add(new LeaderboardEntry()
                {
                    Score = request.GetStat(_allPlayers[i].EventStats),
                    PlayerName = _allPlayers[i].CharacterData.Name,
                    TeamId = _allPlayers[i].CharacterData.Faction
                });
            }
        }
    }
}
