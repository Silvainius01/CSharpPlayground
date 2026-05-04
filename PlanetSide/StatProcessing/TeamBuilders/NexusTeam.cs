using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DaybreakGames.Census.Stream;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Bson;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Reflection.PortableExecutable;
using System.Xml;
using System.Diagnostics.CodeAnalysis;

namespace PlanetSide
{
    public class NexusTeam : PlanetSideTeam
    {
        ConcurrentDictionary<string, PlayerStats> playersConcurrent;

        public NexusTeam(int teamId, string teamName, int faction, string world)
            : base(teamId, teamName, faction, world)
        {
            streamKey = $"NexusTeam_{teamName}_PlayerEventStream";
        }

        public override void GetPlayers()
        {
            throw new NotImplementedException();
        }

        // TODO: Figure tf is happening here.
        // I know it generates a team of 48 by adding unique characters as they appear in the event stream.
        public async Task GenerateRandomTeam(string streamKey, CensusHandler handler)
        {
            bool filled = false;
            object fillLock = new object();
            object charLock = new object();
            ConcurrentDictionary<string, PlayerStats> players = new ConcurrentDictionary<string, PlayerStats>(4, TeamSize);

            Logger.LogInformation("Generating NexusTeam {0}...", TeamName);           

            // Add a callback to generate the team. Returns true when the team is full.
            handler.AddActionToSubscription(streamKey, response =>
            {
                if (filled)
                    return true;

                ICensusEvent censusEvent = Tracker.ProcessCensusEvent(response);

                if (censusEvent is not null)
                {
                    switch (censusEvent.EventType)
                    {
                        case CensusEventType.Death:
                        case CensusEventType.VehicleDestroy:
                            {
                                ICensusDeathEvent deathEvent = censusEvent as ICensusDeathEvent;

                                if (deathEvent.TeamId == this.FactionId)
                                    players.TryAdd(deathEvent.CharacterId, new PlayerStats());

                                if (deathEvent.AttackerTeamId == this.FactionId)
                                    players.TryAdd(deathEvent.OtherId, new PlayerStats());
                            }
                            break;
                        case CensusEventType.GainExperience:
                            {
                                ICensusCharacterEvent charEvent = censusEvent as ICensusCharacterEvent;

                                if (charEvent is not null)
                                    players.TryAdd(charEvent.CharacterId, new PlayerStats());
                            }
                            break;
                    }
                }

                if(players.Count >= TeamSize)
                {
                    lock (fillLock)
                    {
                        filled = true;

                        playersConcurrent.Clear();
                        foreach (var kvp in players)
                            playersConcurrent.TryAdd(kvp.Key, kvp.Value);

                        Logger.LogInformation("Genrated NexusTeam {0} with {1} players", TeamName, this.playersConcurrent.Count);
                    }

                    return true;
                }

                return false;
            });

            await Task.Run(() => { while (!filled) ; return true; });
        }

        protected override CensusStreamSubscription GetStreamSubscription()
        {
            return new CensusStreamSubscription()
            {
                Characters = playersConcurrent.Keys,
                Worlds = new[] { worldString },
                EventNames = new[] { "Death", "GainExperience", "VehicleDestroy" },
                LogicalAndCharactersWithWorlds = true
            };
        }

        protected override void OnStreamStart() { }
        protected override void OnStreamStop() { }
        protected override void OnEventProcessed(ICensusEvent payload) { }

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
