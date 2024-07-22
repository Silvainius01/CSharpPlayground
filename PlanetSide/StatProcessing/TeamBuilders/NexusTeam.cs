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
        Dictionary<string, PlayerStats> nexusTeamPlayers;

        public NexusTeam(int teamSize, string teamName, int faction, string world)
            : base(teamSize, teamName, faction, world)
        {
            streamKey = $"NexusTeam_{teamName}_PlayerEventStream";
        }

        protected override IDictionary<string, PlayerStats> GetTeamDict()
        {
            nexusTeamPlayers = new Dictionary<string, PlayerStats>();
            return nexusTeamPlayers;
        }

        public async Task GenerateRandomTeam(string streamKey, CensusHandler handler)
        {
            bool filled = false;
            object fillLock = new object();
            object charLock = new object();
            ConcurrentDictionary<string, PlayerStats> playersConcurrent = new ConcurrentDictionary<string, PlayerStats>(4, TeamSize);

            Logger.LogInformation("Genrating NexusTeam {0}...", TeamName);           

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

                                if (deathEvent.TeamId == this.Faction)
                                    playersConcurrent.TryAdd(deathEvent.CharacterId, new PlayerStats());

                                if (deathEvent.AttackerTeamId == this.Faction)
                                    playersConcurrent.TryAdd(deathEvent.OtherId, new PlayerStats());
                            }
                            break;
                        case CensusEventType.GainExperience:
                            {
                                ICensusCharacterEvent charEvent = censusEvent as ICensusCharacterEvent;

                                if (charEvent is not null)
                                    playersConcurrent.TryAdd(charEvent.CharacterId, new PlayerStats());
                            }
                            break;
                    }
                }

                if(playersConcurrent.Count >= TeamSize)
                {
                    lock (fillLock)
                    {
                        filled = true;

                        nexusTeamPlayers.Clear();
                        foreach (var kvp in playersConcurrent)
                            nexusTeamPlayers.Add(kvp.Key, kvp.Value);

                        Logger.LogInformation("Genrated NexusTeam {0} with {1} players", TeamName, TeamSize);
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
                Characters = nexusTeamPlayers.Keys,
                Worlds = new[] { World },
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
