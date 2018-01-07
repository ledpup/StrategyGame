using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameModel
{
    public class Neighbour
    {
        public Tile Tile { get; private set; }
        public EdgeType EdgeType { get; private set; }
        public bool EdgeHasRoad { get; private set; }

        public Neighbour(Tile neighbourTile, EdgeType edgeType = EdgeType.None, bool edgeHasRoad = false)
        {
            Tile = neighbourTile;
            EdgeType = edgeType;
            EdgeHasRoad = edgeHasRoad;
        }
    }
}
