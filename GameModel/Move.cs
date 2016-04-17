using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameModel
{
    public class Move
    {
        public Move(Tile origin, Tile destination)
        {
            Origin = origin;
            Destination = destination; 
        }
        public Tile Origin;
        public Tile Destination;

        public static bool IsAllOnRoad(List<Move> moves)
        {
            return moves.All(x => Board.EdgeHasRoad(x.Origin, x.Destination));
        }

        public override string ToString()
        {
            return "From: " + Origin + " To: " + Destination;
        }
    }
}
