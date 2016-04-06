using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameModel
{
    public class Move
    {
        public Move(Move from, Tile destination)
        {
            MoveFrom = from;
            Destination = destination; 
        }
        public Move MoveFrom;
        public Tile Destination;

        public Tile[] Moves()
        {
            var moves = new List<Tile>();
            var move = this;
            while (move != null)
            {
                moves.Add(move.Destination);
                move = move.MoveFrom;
            }
            moves.Reverse();
            return moves.ToArray();
        }

        public static bool IsAllOnRoad(Move move)
        {
            var movePosition = move;

            while (movePosition.MoveFrom != null)
            {
                var isRoad = Board.EdgeHasRoad(movePosition.Destination, movePosition.MoveFrom.Destination);
                if (!isRoad)
                    return false;

                movePosition = movePosition.MoveFrom;
            }
            return true;
        }

        public override string ToString()
        {
            var from = MoveFrom == null ? "" : "From: " + MoveFrom.Destination;
            return from + " To: " + Destination;
        }
    }
}
