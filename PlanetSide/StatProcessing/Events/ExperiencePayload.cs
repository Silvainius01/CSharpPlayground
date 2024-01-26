

namespace PlanetSide
{
    public struct ExperiencePayload : ICensusPayload
    {
        public string CharacterId { get; set; }
        public string OtherId { get; set; }
        public CensusEventType EventType { get; set; }

        public int ExperienceId { get; set; }
        public float ScoreAmount { get; set; }
    }
}
