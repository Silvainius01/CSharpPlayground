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

    public class EventLeaderboard
    {
        PlanetSideTeam[] teams;
        ConcurrentDictionary<string, PlayerLeaderboard> PlayerBoards = new ConcurrentDictionary<string, PlayerLeaderboard>();
        ConcurrentDictionary<string, WeaponLeaderboard> WeaponBoards = new ConcurrentDictionary<string, WeaponLeaderboard>();

        public EventLeaderboard(params PlanetSideTeam[] teams)
        {
            this.teams = teams;
        }

        public LeaderboardEntry[] CalculateLeaderboard(LeaderboardRequest request)
        {
            switch (request.LeaderboardType)
            {
                case LeaderboardType.Player:
                    return GetPlayerBoard(request).Calculate();
                case LeaderboardType.Weapon:
                    return GetWeaponBoard(request).Calculate();
            }

            return new LeaderboardEntry[request.BoardSize];
        }

        public LeaderboardEntry[] GetLeaderboard(LeaderboardRequest request)
        {
            switch (request.LeaderboardType)
            {
                case LeaderboardType.Player:
                    return GetPlayerBoard(request).Get();
                case LeaderboardType.Weapon:
                    return GetWeaponBoard(request).Get();
            }
            return new LeaderboardEntry[request.BoardSize];
        }

        private PlayerLeaderboard GetPlayerBoard(LeaderboardRequest request)
        {
            if (!PlayerBoards.TryGetValue(request.Name, out var board))
            {
                board = new PlayerLeaderboard(request, teams);
                if (!PlayerBoards.TryAdd(request.Name, board))
                    throw new InvalidOperationException($"Failed to add missing player leaderboard {request.Name}");
            }
            return board;
        }

        private WeaponLeaderboard GetWeaponBoard(LeaderboardRequest request)
        {
            if (!WeaponBoards.TryGetValue(request.Name, out var board))
            {
                board = new WeaponLeaderboard(request, teams);
                if (!WeaponBoards.TryAdd(request.Name, board))
                    throw new InvalidOperationException($"Failed to add missing player leaderboard {request.Name}");
            }
            return board;
        }
    }
}
