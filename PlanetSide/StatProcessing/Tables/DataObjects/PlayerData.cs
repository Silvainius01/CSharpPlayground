using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanetSide
{
    public struct CharacterData : ITeamDataObject
    {
        /// <summary> Always 0, use CensusId instead. </summary>
        public int Id { get; set; }
        public string CensusId { get; set; }
        public string Name { get; set; }
        public string Alias { get; set; }
        public int FactionId { get; set; }
        public int TeamId { get; set; }
    }

    public struct PlayerCsvEntry
    {
        public string Alias { get; set; }
        public string CensusId { get; set; }
    }

    public class PlayerCsvEntryMap : ClassMap<PlayerCsvEntry>
    {
        public PlayerCsvEntryMap()
        {
            Map(m => m.Alias).Name("Alias");
            Map(m => m.CensusId).Name("CensusId");
        }
    }
}
