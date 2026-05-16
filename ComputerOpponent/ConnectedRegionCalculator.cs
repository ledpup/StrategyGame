namespace ComputerOpponent;

using System.Collections.Generic;
using GameModel;

public static class ConnectedRegionCalculator
{
    public static void Calculate(Board board)
    {
        foreach (var tile in board.TileArray)
        {
            tile.ConnectedRegionId = 0;
        }

        var id = 0;
        foreach (var tile in board.TileArray)
        {
            if (tile.ConnectedRegionId == 0)
            {
                id++;
                tile.ConnectedRegionId = id;
                AssignConnectedTilesToRegion(tile, id, tile.TerrainType == TerrainType.Mountain);
            }
        }
    }

    private static void AssignConnectedTilesToRegion(Tile start, int id, bool isMountainRange)
    {
        var stack = new Stack<Tile>();
        stack.Push(start);
        while (stack.Count > 0)
        {
            var tile = stack.Pop();
            foreach (var x in tile.Neighbours)
            {
                if (CanConnect(tile, x, isMountainRange))
                {
                    x.Destination.ConnectedRegionId = id;
                    stack.Push(x.Destination);
                }
            }
        }
    }

    private static bool CanConnect(Tile tile, Edge edge, bool isMountainRange)
    {
        if (edge.Destination.ConnectedRegionId != 0 || edge.Destination.BaseTerrainType != tile.BaseTerrainType)
        {
            return false;
        }

        if (edge.HasRoad)
        {
            return true;
        }

        if (edge.EdgeType == EdgeType.Mountain)
        {
            return false;
        }

        return (tile.TerrainType != TerrainType.Mountain && edge.Destination.TerrainType != TerrainType.Mountain)
            || (tile.TerrainType == TerrainType.Mountain && edge.Destination.TerrainType == TerrainType.Mountain && isMountainRange);
    }
}
