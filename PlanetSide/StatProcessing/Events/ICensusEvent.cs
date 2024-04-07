

namespace PlanetSide
{
    public enum CensusEventType
    {
        GainExperience,
        Death,
        VehicleDestroy
    }

    public interface ICensusPayload
    {
        string CharacterId { get; set; }
        string OtherId { get; set; }
        CensusEventType EventType { get; set; }
    }
}
