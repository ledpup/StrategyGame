using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameModel
{
    public class Move
    {
        public Move(Tile origin, Tile destination, Move previousMove, int movesRemaining = 0, int distance = 0)
        {
            Origin = origin;
            Destination = destination;
            PreviousMove = previousMove;
            MovesRemaining = movesRemaining;
            Distance = distance;
        }
        public Tile Origin;
        public Tile Destination;
        public Move PreviousMove;
        public int MovesRemaining;
        public int Distance;
        

        public override string ToString()
        {
            return "From: " + Origin + " To: " + Destination;
        }

        public MoveOrder GetMoveOrder()
        {
            var moveList = new List<Move>();
            var currentMove = this;
            while (currentMove != null)
            {
                moveList.Add(currentMove);
                currentMove = currentMove.PreviousMove;
            }

            moveList.Reverse();

            return new MoveOrder(moveList.ToArray());
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
