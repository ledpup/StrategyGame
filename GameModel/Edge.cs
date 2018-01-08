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
        public Edge(EdgeType edgeType, Tile origin, Tile destination, bool hasRoad)
        {
            EdgeType = edgeType;

            Origin = origin;
            Destination = destination;

            HasRoad = hasRoad;
        }

        public Edge(string edgeType, Tile origin, Tile destination, bool hasRoad) : this ((EdgeType)Enum.Parse(typeof(EdgeType), edgeType), origin, destination, hasRoad)
        {
        }

        public Tile Origin;
        public Tile Destination;

        public bool HasRoad { get; private set; }

        public EdgeType EdgeType;

        public static bool CrossesEdge(Edge edge, Tile tile1, Tile tile2)
        {
            return (edge.Origin == tile1 && edge.Destination == tile2) || (edge.Destination == tile1 && edge.Origin == tile2);
        }

        public bool CrossesEdge(Tile tile1, Tile tile2)
        {
            return CrossesEdge(this, tile1, tile2);
        }
        public static Edge GetEdge(List<Edge> edges, Tile tile1, Tile tile2)
        {
            if (edges == null)
                return null;

            return edges.SingleOrDefault(x => x.CrossesEdge(tile1, tile2));
        }

        public static List<Edge> GetEdges(List<Edge> edges, Tile location)
        {
            return edges.Where(x => x.Origin == location || x.Destination == location).ToList();
        }

        public int MoveCost(bool usesRoads, bool isBeingTransportedByWater, Dictionary<EdgeType, int> edgeMovementCosts, Dictionary<TerrainType, int> terrainMovementCosts)
        {
            // Movement by road or bridge always costs 1 regardless of terrain type
            if (usesRoads && HasRoad)
            {
                return 1;
            }

            // If a unit is transported by water, you can only get out at a port
            if (isBeingTransportedByWater && EdgeType != EdgeType.Port)
            {
                return Terrain.Impassable;
            }

            return edgeMovementCosts[EdgeType] + terrainMovementCosts[Destination.TerrainType];
        }

        public int MoveCost(bool usesRoads, bool isBeingTransportedByWater, int edgeMovementCost, int terrainMovementCost)
        {
            // Movement by road or bridge always costs 1 regardless of terrain type
            if (usesRoads && HasRoad)
            {
                return 1;
            }

            // If a unit is transported by water, you can only get out at a port
            if (isBeingTransportedByWater && EdgeType != EdgeType.Port)
            {
                return Terrain.Impassable;
            }

            return edgeMovementCost + terrainMovementCost;
        }
    }
}
