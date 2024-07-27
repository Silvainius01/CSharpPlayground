

namespace PlanetSide
{
    public enum CensusEventType
    {
        Unknown,
        GainExperience,
        Death,
        VehicleDestroy,
        FacilityControl
    }

    public interface ICensusEvent
    {
        public CensusEventType EventType { get; set; }
    }

    public interface ICensusZoneEvent : ICensusEvent
    {
        int ZoneId { get; set; }
        int WorldId { get; set; }
    }

    public interface ICensusCharacterEvent : ICensusZoneEvent
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
    }
}
