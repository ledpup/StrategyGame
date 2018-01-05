using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameModel
{
    public enum EdgeType
    {
        None,
        River,
        Forest,
        Hill,
        Mountain,
        Reef,
        Wall,
        Port,
    }

    public class Edge
    {
        public Edge(string edgeType, Tile origin, Tile destination, bool hasRoad)
        {
            EdgeType = (EdgeType)Enum.Parse(typeof(EdgeType), edgeType);

            Origin = origin;
            Destination = destination;

            HasRoad = hasRoad;
        }

        public Tile Origin;
        public Tile Destination;

        public bool HasRoad { get; private set; }

        public EdgeType EdgeType;

        internal static bool CrossesEdge(Edge edge, Tile tile1, Tile tile2)
        {
            return (edge.Origin == tile1 && edge.Destination == tile2) || (edge.Destination == tile1 && edge.Origin == tile2);
        }

        internal bool CrossesEdge(Tile tile1, Tile tile2)
        {
            return CrossesEdge(this, tile1, tile2);
        }
        internal static Edge GetEdge(List<Edge> edges, Tile tile1, Tile tile2)
        {
            if (edges == null)
                return null;

            return edges.SingleOrDefault(x => x.CrossesEdge(tile1, tile2));
        }

        public static List<Edge> GetEdges(List<Edge> edges, Tile location)
        {
            return edges.Where(x => x.Origin == location || x.Destination == location).ToList();
        }
    }
}
