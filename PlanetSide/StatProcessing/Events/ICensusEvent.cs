

namespace PlanetSide
{
    public enum CensusEventType
    {
        GainExperience,
        Death,
        VehicleDestroy
    }

    public interface ICensusEvent
    {
        string CharacterId { get; set; }
        string OtherId { get; set; }
        CensusEventType EventType { get; set; }
    }
}
