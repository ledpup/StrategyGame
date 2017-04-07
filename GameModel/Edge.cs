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
        Port = 1 << 9,
    }

    public class Edge
    {
        public Edge(string edgeType, Tile origin, Tile destination)
        {
            SetEdgeType(edgeType);

            Origin = origin;
            Destination = destination;
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

        public Tile Origin;
        public Tile Destination;
        public EdgeType EdgeType;
        public BaseEdgeType BaseEdgeType;

        internal static bool CrossesEdge(Edge edge, Tile origin, Tile destination)
        {
            return (edge.Origin == origin && edge.Destination == destination) || (edge.Destination == origin && edge.Origin == destination);
        }

        internal bool CrossesEdge(Tile origin, Tile destination)
        {
            return CrossesEdge(this, origin, destination);
        }
        internal static Edge GetEdge(Tile origin, Tile destination)
        {
            return origin.Edges.Single(y => y.Destination == destination);
        }
    }
}
