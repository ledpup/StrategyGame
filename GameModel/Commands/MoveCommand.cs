namespace GameModel.Commands;

public record MoveCommand(Move[] Moves, MilitaryUnit Unit) : IUnitCommand
{
    public MilitaryUnit Unit { get; set; } = Unit;

    public Move[] Moves { get; set; } = Moves;
}
