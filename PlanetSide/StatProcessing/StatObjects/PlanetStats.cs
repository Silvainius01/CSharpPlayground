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

        public int AirKills { get; set; } // Only tracks kills on Air Vehicles
        public int AirDeaths { get; set; } // Only tracks Air vehicle deaths.
        public int AirTeamKills { get; set; } // Only tracks TKs on Air vehicles.

        public float KDR => (float)Kills / (Deaths > 0 ? Deaths : 1);
        public float HSR => (float)Headshots / (Kills > 0 ? Kills : 1);
        public float vKDR => (float)VehicleKills / (VehicleDeaths > 0 ? VehicleDeaths : 1);
        public float aKDR => (float)AirKills / (AirDeaths > 0 ? AirDeaths : 1);

        //public float CheeseScore { get; set; }
        //public float InfantryScore { get; set; }
        //public float AirScore { get; set; }
        //public float ArmorScore { get; set; }
        //public float CohesionScore { get; set; }
        //public float LogisticScore { get; set; }

        [JsonIgnore] Dictionary<int, CumulativeExperience> _allExperience;
        [JsonIgnore] ReadOnlyDictionary<int, CumulativeExperience> TeamExperience;


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
            if(isTeamKill)
            {
                AddVehicleTeamKill(ref destroyEvent);
                return;
            }

            switch(VehicleTable.VehicleData[destroyEvent.VehicleId].Type)
            {
                case VehicleType.Air: // Note: includes bastions
                    ++AirDeaths;
                    break;
                case VehicleType.Hover: // Magriders, Javelins.
                case VehicleType.Ground: // Corsairs, hilariously.
                    ++VehicleDeaths; 
                    break;
                case VehicleType.Unknown:
                    break;
            }
        }
        public void AddVehicleTeamKill(ref VehicleDestroyPayload destroyEvent)
        {
            switch (VehicleTable.VehicleData[destroyEvent.VehicleId].Type)
            {
                case VehicleType.Air:
                    ++AirTeamKills; 
                    break;
                case VehicleType.Ground:
                    ++VehicleTeamKills; 
                    break;
                case VehicleType.Unknown:
                    break;
            }
        }
        public void AddVehicleKill(ref VehicleDestroyPayload destroyEvent)
        {
            switch (VehicleTable.VehicleData[destroyEvent.VehicleId].Type)
            {
                case VehicleType.Air:
                    ++AirKills;
                    break;
                case VehicleType.Ground:
                    ++VehicleKills;
                    break;
                case VehicleType.Unknown:
                    break;
            }
        }
    }
}
