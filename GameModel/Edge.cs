using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameModel
{
    public enum BaseEdgeType
    {
        Hexside,
        CentreToCentre,
    }

    [Flags]
    public enum EdgeType
    { 
        Normal = 1 << 0,
        River = 1 << 1,
        Road = 1 << 2,
        Forest = 1 << 3,
        Hill = 1 << 4,
        Mountain = 1 << 5,
        Reef = 1 << 6,
        Wall = 1 << 7,
    }

    public class Edge
    {
        public static EdgeType All_Edges = EdgeType.River | EdgeType.Road | EdgeType.Forest | EdgeType.Hill | EdgeType.Mountain | EdgeType.Reef | EdgeType.Wall;

        public Edge(string edgeType, Tile[] tiles)
        {
            EdgeType = (EdgeType)Enum.Parse(typeof(EdgeType), edgeType);

            switch (EdgeType)
            {
                case EdgeType.Road:
                    BaseEdgeType = BaseEdgeType.CentreToCentre;
                    break;
                default:
                    BaseEdgeType = BaseEdgeType.Hexside;
                    break;
            }

            Tiles = tiles;
        }

        public Tile[] Tiles;
        public EdgeType EdgeType;
        public BaseEdgeType BaseEdgeType;
    }
}
