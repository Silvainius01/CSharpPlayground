using Newtonsoft.Json;
using PlanetSide.StatProcessing.StatObjects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PlanetSide.Websocket
{
    public enum LeaderboardType { Player, Weapon }
    public struct LeaderboardRequest
    {
        public int BoardSize { get; set; }
        public string Name { get; set; }
        public LeaderboardType LeaderboardType { get; set; }
        public Func<PlanetStats, float> GetStat { get; set; }
    }

    public struct LeaderboardEntry
    {
        public int TeamId { get; set; }
        public float Score { get; set; }
        public string EntryName { get; set; }
    }

    public class CommSmashLeaderboard
    {
        FactionTeam teamOne;
        FactionTeam teamTwo;
        ConcurrentDictionary<string, List<LeaderboardEntry>> PlayerBoards = new ConcurrentDictionary<string, List<LeaderboardEntry>>();
        ConcurrentDictionary<string, List<LeaderboardEntry>> WeaponBoards = new ConcurrentDictionary<string, List<LeaderboardEntry>>();

        public CommSmashLeaderboard(FactionTeam team1, FactionTeam team2)
        {
            this.teamOne = team1;
            this.teamTwo = team2;
        }

        public List<LeaderboardEntry> GenerateLeaderboard(LeaderboardRequest request)
        {
            switch (request.LeaderboardType)
            {
                case LeaderboardType.Player:
                    return GeneratePlayerLeaderboard(request);
                case LeaderboardType.Weapon:
                    return GenerateWeaponLeaderboard(request);
            }

            return new List<LeaderboardEntry>();
        }

        List<PlayerStats> _allPlayers = new List<PlayerStats>();
        List<PlayerStats> _teamOnePlayers = new List<PlayerStats>();
        List<PlayerStats> _teamTwoPlayers = new List<PlayerStats>();
        private List<LeaderboardEntry> GeneratePlayerLeaderboard(LeaderboardRequest request)
        {
            var leaderboard = PlayerBoards.ContainsKey(request.Name)
                ? PlayerBoards[request.Name]
                : (PlayerBoards[request.Name] = new List<LeaderboardEntry>());
            Comparison<PlayerStats> playerComparer = (a, b)
                => -request.GetStat(a.Stats).CompareTo(request.GetStat(b.Stats));


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
            for (int i = 0; i < 10 && i < _allPlayers.Count; ++i)
            {
                float score = request.GetStat(_allPlayers[i].Stats);
                if (score > 0) // Only add players with a score to begin with.
                    leaderboard.Add(new LeaderboardEntry()
                    {
                        Score = score,
                        EntryName = _allPlayers[i].Data.Name,
                        TeamId = _allPlayers[i].Data.FactionId
                    });
            }
            return leaderboard;
        }

        List<(int teamId, WeaponStats stats)> _allWeapons = new List<(int teamId, WeaponStats stats)>();
        List<WeaponStats> _teamOneWeapons = new List<WeaponStats>();
        List<WeaponStats> _teamTwoWeapons = new List<WeaponStats>();
        private List<LeaderboardEntry> GenerateWeaponLeaderboard(LeaderboardRequest request)
        {
            var leaderboard = WeaponBoards.ContainsKey(request.Name)
                ? WeaponBoards[request.Name]
                : (WeaponBoards[request.Name] = new List<LeaderboardEntry>());
            Comparison<WeaponStats> comparer = (a, b)
                => -request.GetStat(a.Stats).CompareTo(request.GetStat(b.Stats));


            if (_teamOneWeapons.Count != teamOne.TeamWeapons.Count)
            {
                _teamOneWeapons.Clear();
                _teamOneWeapons.AddRange(teamOne.TeamWeapons.Values);
            }

            if (_teamTwoWeapons.Count != teamTwo.TeamWeapons.Count)
            {
                _teamTwoWeapons.Clear();
                _teamTwoWeapons.AddRange(teamTwo.TeamWeapons.Values);
            }

            _teamOneWeapons.Sort(comparer);
            _teamTwoWeapons.Sort(comparer);

            _allWeapons.Clear();
            for (int i = 0; i < 10; ++i)
            {
                if (i < _teamOneWeapons.Count)
                    _allWeapons.Add((teamOne.Faction, _teamOneWeapons[i]));
                if (i < _teamTwoWeapons.Count)
                    _allWeapons.Add((teamTwo.Faction, _teamTwoWeapons[i]));
            }
            _allWeapons.Sort((a, b) => comparer(a.stats, b.stats));

            leaderboard.Clear();
            for (int i = 0; i < 10 && i < _allWeapons.Count; ++i)
            {
                float score = request.GetStat(_allWeapons[i].stats.Stats);
                if (score > 0) // Only add players with a score to begin with.
                    leaderboard.Add(new LeaderboardEntry()
                    {
                        Score = score,
                        EntryName = _allWeapons[i].stats.Data.Name,
                        TeamId = _allWeapons[i].teamId,
                    });
            }
            return leaderboard;
        }

        public List<LeaderboardEntry> GetLeaderboard(LeaderboardRequest request)
        {
            if (PlayerBoards.TryGetValue(request.Name, out var board))
                return board;
            return new List<LeaderboardEntry>();
        }
    }
}
