
namespace PlanetSide
{
    public struct VehicleDestroyPayload : ICensusEvent
    {
        /// <summary> The Character who died </summary>
        public string CharacterId { get; set; }
        /// <summary> The Character who got the kill </summary>
        public string OtherId { get; set; }
        public CensusEventType EventType { get; set; }

        public int AttackerWeaponId { get; set; }
        public int AttackerVehicleId { get; set; }
        public int AttackerLoadoutId { get; set; }
    }
}
