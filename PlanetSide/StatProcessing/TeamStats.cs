using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using Newtonsoft.Json;

namespace PlanetSide
{
    public class TeamStats
    {
        public int TeamSize { get; set; }
        public string TeamName { get; set; }

        public int Kills { get; set; }
        public int Deaths { get; set; }
        public int TeamKills { get; set; }
        public int Headshots { get; set; }

        public int VehicleKills { get; set; }
        public int VehicleDeaths { get; set; }
        public int VehicleTeamKills { get; set; }

        public float KDR => (float)Kills / (Deaths > 0 ? Deaths : 1);
        public float HSR => (float)Headshots / (Kills > 0 ? Kills : 1);
        public float vKDR => (float)VehicleKills / (VehicleDeaths > 0 ? VehicleDeaths : 1);

        public float CheeseScore { get; set; }
        public float InfantryScore { get; set; }
        public float AirScore { get; set; }
        public float ArmorScore { get; set; }
        public float CohesionScore { get; set; }
        public float LogisticScore { get; set; }

        Dictionary<int, CumulativeExperience> _teamExperience { get; set; }

        [JsonIgnore]
        public ReadOnlyDictionary<int, CumulativeExperience> TeamExperience;


        public TeamStats(int teamSize, string teamName)
        {
            TeamSize = teamSize;
            TeamName = teamName;

            _teamExperience = new Dictionary<int, CumulativeExperience>();
            TeamExperience = new ReadOnlyDictionary<int, CumulativeExperience>(_teamExperience);
        }

        public void AddExperience(ref ExperiencePayload expEvent)
        {
            int experienceId = expEvent.ExperienceId;
            float score = expEvent.ScoreAmount;

            if (_teamExperience.ContainsKey(experienceId))
            {
                _teamExperience[experienceId].NumEvents += 1;
                _teamExperience[experienceId].CumulativeScore += score;
            }
            else _teamExperience.Add(experienceId, new CumulativeExperience()
            {
                NumEvents = 1,
                CumulativeScore = score,
                Id = experienceId
            });
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

        public void AddVehicleDeath(ref VehicleDesroyPayload destroyEvent, bool isTeamKill)
        {
            if (isTeamKill)
            {
                ++VehicleTeamKills;
                return;
            }

            // Only count deaths from the enemy
            ++VehicleDeaths;
        }
        public void AddVehicleKill(ref VehicleDesroyPayload destrpyEvent)
        {
            ++VehicleKills;
        }
    }
}
