namespace GameModel.Commands;

public class MoveCommand(Move[] moves, MilitaryUnit unit) : IUnitCommand
{
    public MilitaryUnit Unit { get; set; } = unit;

    public Move[] Moves = moves;
}
