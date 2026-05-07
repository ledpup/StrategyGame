namespace GameModel
{
    public class TransportOrder(MilitaryUnit transportUnit, MilitaryUnit unitToTransport) : IUnitOrder
    {
        public MilitaryUnit Unit { get; set; } = transportUnit;
        public MilitaryUnit UnitToTransport { get; set; } = unitToTransport;
    }
}
