namespace GameModel;

using System.Collections.Generic;
using System.Linq;
using GameModel.Commands;

public enum MoveType
{
    Standard,
    Road,
    OnlyPassingThrough,
    Embark,
}

public class Move(Tile origin, Edge edge, Move previousMove, int movesRemaining, int distance, MoveType moveType)
{
    public Move(Tile origin, Tile destination, Move previousMove, int movesRemaining, int distance, MoveType moveType = MoveType.Standard) :
        this(origin, origin.Neighbours.Single(x => x.Destination == destination), previousMove, movesRemaining, distance, moveType)
    {
    }

    public Tile Origin = origin;
    public Edge Edge = edge;
    public Move PreviousMove = previousMove;
    public int MovesRemaining = movesRemaining;
    public int Distance = distance;
    public MoveType MoveType = moveType;

    public override string ToString()
    {
        return "From: " + Origin + " To: " + Edge.Destination;
    }

    public MoveCommand GetMoveOrder(MilitaryUnit unit)
    {
        var moveList = new List<Move>();
        var currentMove = this;
        while (currentMove != null)
        {
            moveList.Add(currentMove);
            currentMove = currentMove.PreviousMove;
        }

        moveList.Reverse();

        return new MoveCommand(moveList.ToArray(), unit);
    }
}
