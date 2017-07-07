using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameModel
{
    public enum MoveType
    {
        Standard,
        Road,
        OnlyPassingThrough,
        Embark
    }
    public class Move
    {


        public Move(Tile origin, Tile destination, Move previousMove, int movesRemaining, int distance, MoveType moveType = MoveType.Standard) : 
            this(origin, destination, origin.Edges.Single(x => x.Destination == destination), previousMove, movesRemaining, distance, moveType)
        {
            
        }

        public Move(Tile origin, Tile destination, Edge edge, Move previousMove, int movesRemaining, int distance, MoveType moveType)
        {
            Origin = origin;
            Destination = destination;
            Edge = edge;
            PreviousMove = previousMove;
            MovesRemaining = movesRemaining;
            Distance = distance;
            MoveType = moveType;
        }


        public Tile Origin;
        public Tile Destination;
        public Edge Edge;
        public Move PreviousMove;
        public int MovesRemaining;
        public int Distance;
        public MoveType MoveType;

        public override string ToString()
        {
            return "From: " + Origin + " To: " + Destination;
        }

        public MoveOrder GetMoveOrder(MilitaryUnit unit)
        {
            var moveList = new List<Move>();
            var currentMove = this;
            while (currentMove != null)
            {
                moveList.Add(currentMove);
                currentMove = currentMove.PreviousMove;
            }

            moveList.Reverse();

            return new MoveOrder(moveList.ToArray(), unit);
        }

        public double TerrainAndWeatherModifers(int unitIndex)
        {
            var mod = 0D;
            var currentMove = this;
            while (currentMove != null)
            {
                mod += currentMove.Destination.TerrainAndWeatherInfluenceByUnit[unitIndex];
                currentMove = currentMove.PreviousMove;
            }

            return mod;
        }
    }
}
