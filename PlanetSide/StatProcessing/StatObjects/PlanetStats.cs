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
using Microsoft.Extensions.Logging;
using System.Reflection.Metadata.Ecma335;

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
        public float ReviveScore
        {
            get
            {
                float rezCount = GetExp(ExperienceTable.Revive).NumEvents;
                return rezCount / (Deaths > 0 ? Deaths : 1);
            }
        }

        //public float CheeseScore { get; set; }
        //public float InfantryScore { get; set; }
        //public float AirScore { get; set; }
        //public float ArmorScore { get; set; }
        //public float CohesionScore { get; set; }
        //public float LogisticScore { get; set; }

        [JsonIgnore] Dictionary<int, CumulativeExperience> _allExperience;

        public PlanetStats()
        {
            _allExperience = new Dictionary<int, CumulativeExperience>();
        }

        public CumulativeExperience GetExp(int id)
        {
            if (_allExperience.TryGetValue(id, out var exp))
                return exp;

            _allExperience[id] = new CumulativeExperience()
            {
                NumEvents = 0,
                CumulativeScore = 0,
                Id = id
            };
            return _allExperience[id];
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

        public void AddKill(ref DeathPayload deathEvent)
        {
            if (deathEvent.TeamId == deathEvent.AttackerTeamId)
            {
                ++TeamKills;
                return;
            }
            else
            {
                ++Kills;
                if (deathEvent.IsHeadshot)
                    ++Headshots;
            }
        }
        public void AddDeath(ref DeathPayload deathEvent)
        {
            // Only count deaths from the enemy
            if (deathEvent.TeamId != deathEvent.AttackerTeamId)
                ++Deaths;
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
        public void AddVehicleDeath(ref VehicleDestroyPayload destroyEvent)
        {
            if(destroyEvent.TeamId == destroyEvent.AttackerTeamId)
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

        public void Reset()
        {
            Kills = 0;
            Assists = 0;
            Deaths = 0;
            TeamKills = 0;
            Headshots = 0;

            VehicleKills = 0;
            VehicleDeaths = 0;
            VehicleTeamKills = 0;

            AirKills = 0;
            AirDeaths = 0;
            AirTeamKills = 0;

            foreach (var cxp in _allExperience.Values)
            {
                cxp.NumEvents = 0;
                cxp.CumulativeScore = 0;
            }
        }
    }
}
