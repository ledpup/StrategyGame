namespace GameModel.Commands;

public class UnloadCommand(MilitaryUnit unit, Tile destination = null) : IUnitCommand
{
    public MilitaryUnit Unit { get; set; } = unit;

    public Tile Destination { get; set; } = destination;
}
