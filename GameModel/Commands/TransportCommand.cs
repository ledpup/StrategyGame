namespace GameModel.Commands;

public class TransportCommand(MilitaryUnit transportUnit, MilitaryUnit unitToTransport) : IUnitCommand
{
    public MilitaryUnit Unit { get; set; } = transportUnit;
    public MilitaryUnit UnitToTransport { get; set; } = unitToTransport;
}
