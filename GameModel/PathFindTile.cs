using Hexagon;
using PathFind;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameModel
{
    public class PathFindTile : IHasNeighbours<PathFindTile>
    {
        public Hex Hex { get; set; }
        public List<Edge> Edges { get; set; }
        public int Q { get { return Hex.q; } }
        public int R { get { return Hex.r; } }

        public List<PathFindTile> Neighbours { get; set; }
        public Dictionary<Hex, double> MoveCost { get; set; }
        public bool HasCumulativeCost { get; set; }

        public PathFindTile(Hex hex, List<Edge> edges)
        {
            Hex = hex;
            Edges = edges;
            MoveCost = new Dictionary<Hex, double>();
        }

        public void LoadNeighbours(HashSet<PathFindTile> loadedPathFindTiles, List<Edge> neighbourEdges, bool usesRoads, bool isBeingTransportedByWater, Dictionary<EdgeType, int> edgeMovementCosts, Dictionary<TerrainType, int> terrainMovementCosts, TerrainType canStopOn)
        {
            Neighbours = new List<PathFindTile>();

            foreach (var edge in neighbourEdges)
            {
                var moveCost = edge.MoveCost(usesRoads, isBeingTransportedByWater, edgeMovementCosts, terrainMovementCosts);

                if (moveCost < Terrain.Impassable)
                {
                    var neighbourPathFindTile = loadedPathFindTiles.SingleOrDefault(x => x.Hex.Equals(edge.Destination.Hex));

                    if (neighbourPathFindTile == null)
                    {
                        neighbourPathFindTile = new PathFindTile(edge.Destination.Hex, edge.Destination.Neighbours);
                        loadedPathFindTiles.Add(neighbourPathFindTile);
                    }
                    
                    Neighbours.Add(neighbourPathFindTile);

                    MoveCost[neighbourPathFindTile.Hex] = moveCost;

                    // This is to allow the path find to allow units to move over mountains and water even though they can't end their turn there
                    neighbourPathFindTile.HasCumulativeCost = !canStopOn.HasFlag(edge.Destination.TerrainType);
                }
            };
        }

       

        public override string ToString()
        {
            return $"{Q}, {R}";
        }
    }
}
