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
        Road = 1 << 1,
        River = 1 << 2,
        Bridge = 1 << 3,
        Forest = 1 << 4,
        Hill = 1 << 5,
        Mountain = 1 << 6,
        Reef = 1 << 7,
        Wall = 1 << 8,
    }

    public class Edge
    {
        public static EdgeType All_Edges = EdgeType.River | EdgeType.Road | EdgeType.Forest | EdgeType.Hill | EdgeType.Mountain | EdgeType.Reef | EdgeType.Wall;

        public Edge(string edgeType, Tile[] tiles)
        {
            SetEdgeType(edgeType);

            Tiles = tiles;
        }

        public void SetEdgeType(string edgeType)
        {
            EdgeType = (EdgeType)Enum.Parse(typeof(EdgeType), edgeType);

            switch (EdgeType)
            {
                case EdgeType.Road:
                case EdgeType.Bridge:
                    BaseEdgeType = BaseEdgeType.CentreToCentre;
                    break;
                default:
                    BaseEdgeType = BaseEdgeType.Hexside;
                    break;
            }
        }

        public Tile[] Tiles;
        public EdgeType EdgeType;
        public BaseEdgeType BaseEdgeType;
    }
}
