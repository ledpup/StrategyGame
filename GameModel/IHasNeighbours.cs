namespace GameModel;

using System.Collections.Generic;
using Hexagon;

public interface IHasNeighbours<T>
{
    Hex Hex { get; }

    List<T> Neighbours { get; }

    List<Edge> Edges { get; }

    bool HasCumulativeCost { get; set; }

    void LoadNeighbours(HashSet<T> loaded, List<Edge> neighbourEdges, bool usesRoads, bool isBeingTransportedByWater, Dictionary<EdgeType, int> edgeMovementCosts, Dictionary<TerrainType, int> terrainMovementCosts, TerrainType canStopOn);
}