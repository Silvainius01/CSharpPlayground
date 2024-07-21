using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using static System.Formats.Asn1.AsnWriter;

namespace PlanetSide
{
    public class PlanetStats
    {
        public int Kills { get; set; }
        public int Assists { get; set; }
        public int Deaths { get; set; }
        public int TeamKills { get; set; }
        public int Headshots { get; set; }

        public int VehicleKills { get; set; }
        public int VehicleDeaths { get; set; }
        public int VehicleTeamKills { get; set; }

        public float KDR => (float)Kills / (Deaths > 0 ? Deaths : 1);
        public float HSR => (float)Headshots / (Kills > 0 ? Kills : 1);
        public float vKDR => (float)VehicleKills / (VehicleDeaths > 0 ? VehicleDeaths : 1);

        //public float CheeseScore { get; set; }
        //public float InfantryScore { get; set; }
        //public float AirScore { get; set; }
        //public float ArmorScore { get; set; }
        //public float CohesionScore { get; set; }
        //public float LogisticScore { get; set; }

        [JsonIgnore]
        Dictionary<int, CumulativeExperience> _allExperience { get; set; }

        [JsonIgnore]
        ReadOnlyDictionary<int, CumulativeExperience> TeamExperience;

        [JsonIgnore]
        public PlanetSideTeam LinkedTeam;


        public PlanetStats()
        {
            _allExperience = new Dictionary<int, CumulativeExperience>();
            TeamExperience = new ReadOnlyDictionary<int, CumulativeExperience>(_allExperience);
        }

        public void AddExperience(ref ExperiencePayload expEvent)
        {
            int experienceId = expEvent.ExperienceId;
            float score = expEvent.ScoreAmount;

            if (_allExperience.ContainsKey(experienceId))
            {
                _allExperience[experienceId].NumEvents += 1;
                _allExperience[experienceId].CumulativeScore += score;
            }
            else _allExperience.Add(experienceId, new CumulativeExperience()
            {
                NumEvents = 1,
                CumulativeScore = score,
                Id = experienceId
            });
        }
        public CumulativeExperience GetExp(int id)
        {
            if (TeamExperience.TryGetValue(id, out var exp))
                return exp;

            _allExperience[id] = new CumulativeExperience()
            {
                NumEvents = 0,
                CumulativeScore = 0,
                Id = id
            };
            return _allExperience[id];
        }

        public void AddDeath(ref DeathPayload deathEvent, bool isTeamKill)
        {
            if (isTeamKill)
            {
                ++TeamKills;
                return;
            }

            // Only count deaths from the enemy
            ++Deaths;
            
        }
        public void AddKill(ref DeathPayload deathEvent)
        {
            ++Kills;
            if (deathEvent.IsHeadshot)
                ++Headshots;
        }

        public void AddVehicleDeath(ref VehicleDestroyPayload destroyEvent, bool isTeamKill)
        {
            if (isTeamKill)
            {
                ++VehicleTeamKills;
                return;
            }

            // Only count deaths from the enemy
            ++VehicleDeaths;
        }
        public void AddVehicleKill(ref VehicleDestroyPayload destroyEvent)
        {
            ++VehicleKills;
        }
    }
}
