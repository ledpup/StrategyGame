namespace GameModel
{
    public class TransportOrder : IUnitOrder
    {
        public TransportOrder(MilitaryUnit transportUnit, MilitaryUnit unitToTransport)
        {
            Unit = transportUnit;
            UnitToTransport = unitToTransport;
        }
        public MilitaryUnit Unit { get; set; }
        public MilitaryUnit UnitToTransport { get; set; }
    }
}
