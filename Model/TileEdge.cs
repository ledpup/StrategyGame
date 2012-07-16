using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    [Flags]
    public enum EdgeType
    { 
        Normal = 0,
        River = 1 << 0,
        Road = 1 << 1,
    }

    public class TileEdge
    {
        public TileEdge(string edgeType, List<Tile> tiles)
        {
            EdgeType = (EdgeType)Enum.Parse(typeof(EdgeType), edgeType);

            Tiles = tiles;
        }

        public IEnumerable<Tile> Tiles;
        public EdgeType EdgeType;
    }
}
