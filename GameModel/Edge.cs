using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameModel
{
    [Flags]
    public enum EdgeType
    { 
        Normal = 0,
        River = 1 << 0,
        Road = 1 << 1,
        Forest = 1 << 3,
        Hill = 1 << 4,
        Mountain = 1 << 5,
        Reef = 1 << 6,
        Wall = 1 << 7,
    }

    public class Edge
    {
        public static EdgeType AllEdges = EdgeType.River | EdgeType.Road;

        public Edge(string edgeType, List<Tile> tiles)
        {
            EdgeType = (EdgeType)Enum.Parse(typeof(EdgeType), edgeType);

            Tiles = tiles;
        }

        public IEnumerable<Tile> Tiles;
        public EdgeType EdgeType;
    }
}
