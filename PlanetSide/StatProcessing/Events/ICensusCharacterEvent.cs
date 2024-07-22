

namespace PlanetSide
{
    public enum CensusEventType
    {
        Unknown,
        GainExperience,
        Death,
        VehicleDestroy
    }

    public interface ICensusEvent
    {
        CensusEventType EventType { get; set; }
    }

    public interface ICensusCharacterEvent : ICensusEvent
    {
        string CharacterId { get; set; }
        string OtherId { get; set; }
    }

    public interface ICensusDeathEvent : ICensusCharacterEvent
    {
        public int TeamId { get; set; }

        public int AttackerWeaponId { get; set; }
        public int AttackerVehicleId { get; set; }
        public int AttackerLoadoutId { get; set; }
        public int AttackerTeamId { get; set; }

        public int ZoneId { get; set; }
        public int WorldId { get; set; }
    }
}
