using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class Move
    {
        public Move(Move previousMove, Tile destination)
        {
            PreviousMove = previousMove;
            Destination = destination; 
        }
        public Move PreviousMove;
        public Tile Destination;

        public Tile[] Moves()
        {
            var moves = new List<Tile>();
            var move = this;
            while (move != null)
            {
                moves.Add(move.Destination);
                move = move.PreviousMove;
            }
            moves.Reverse();
            return moves.ToArray();
        }

        public static bool IsAllOnRoad(Move move)
        {
            var movePosition = move;

            while (movePosition.PreviousMove != null)
            {
                var isRoad = Board.EdgeHasRoad(movePosition.Destination, movePosition.PreviousMove.Destination);
                if (!isRoad)
                    return false;

                movePosition = movePosition.PreviousMove;
            }
            return true;
        }

        
    }
}
