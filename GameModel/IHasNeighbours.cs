using Hexagon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameModel
{
    public interface IHasNeighbours<N>
    {
        Hex Hex { get; }
        List<N> Neighbours { get; }
        List<Edge> Edges { get; }
        void LoadNeighbours(HashSet<N> loaded, List<Edge> neighbourEdges, bool usesRoads, bool isBeingTransportedByWater, Dictionary<EdgeType, int> edgeMovementCosts, Dictionary<TerrainType, int> terrainMovementCosts, TerrainType canStopOn);
        bool HasCumulativeCost { get; set; }
    }
}