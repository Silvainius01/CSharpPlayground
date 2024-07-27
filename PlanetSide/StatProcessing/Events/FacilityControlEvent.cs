using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanetSide.StatProcessing.Events
{
    public class FacilityControlEvent : ICensusZoneEvent
    {
        public int ZoneId { get; set; }
        public int WorldId { get; set; }
        public CensusEventType EventType { get; set; } = CensusEventType.FacilityControl;

        public int FacilityId { get; set; }
        public int NewFaction { get; set; }
        public int OldFaction { get; set; }
        public int DurationHeld { get; set; }
        public string OutfitId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
