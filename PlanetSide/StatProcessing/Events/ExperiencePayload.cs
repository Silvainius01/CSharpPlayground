

namespace PlanetSide
{
    public struct ExperiencePayload : ICensusCharacterEvent
    {
        public CensusEventType EventType { get; set; }
        public int CensusTimestamp { get; set; }

        public int ZoneId { get; set; }
        public int WorldId { get; set; }

        public string CharacterId { get; set; }
        public string OtherId { get; set; }

        public int ExperienceId { get; set; }
        public float ScoreAmount { get; set; }
    }
}
