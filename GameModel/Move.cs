using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameModel
{
    public class Move
    {
        public Move(Tile origin, Tile destination, int movesRemaining = 0)
        {
            Origin = origin;
            Destination = destination;
            MovesRemaining = movesRemaining;
        }
        public Tile Origin;
        public Tile Destination;
        public int MovesRemaining;

        public override string ToString()
        {
            return "From: " + Origin + " To: " + Destination;
        }
    }
}
