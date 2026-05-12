namespace GameModel;

using System;
using System.Collections.Generic;
using global::PathFind;

public static class PathFind
{
    public static Path<TNode> FindPath<TNode>(
        TNode origin,
        TNode destination,
        Func<TNode, TNode, double> distance,
        Func<TNode, double> estimate,
        int maxCumulativeCost,
        bool usesRoads,
        bool isBeingTransportedByWater,
        Dictionary<EdgeType, int> edgeMovementCosts,
        Dictionary<TerrainType, int> terrainMovementCosts,
        TerrainType canStopOn)
        where TNode : IHasNeighbours<TNode>
    {
        if (origin.Hex.Equals(destination.Hex))
        {
            throw new Exception($"Origin and destination are the same ({origin.Hex})");
        }

        var closed = new HashSet<TNode>();

        var loadedPathFindTiles = new HashSet<TNode>
        {
            origin,
            destination,
        };

        var queue = new global::PathFind.PriorityQueue<double, Path<TNode>>();

        queue.Enqueue(0, new Path<TNode>(origin));

        while (!queue.IsEmpty)
        {
            var path = queue.Dequeue();

            if (closed.Contains(path.LastStep))
            {
                continue;
            }

            if (path.LastStep.Equals(destination))
            {
                return path;
            }

            closed.Add(path.LastStep);

            var cumulativeCost = 0;
            if (path.LastStep.HasCumulativeCost)
            {
                cumulativeCost = 1;
                if (path.PreviousSteps != null)
                {
                    foreach (var previous in path.PreviousSteps)
                    {
                        if (previous.HasCumulativeCost)
                        {
                            cumulativeCost++;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            if (cumulativeCost >= maxCumulativeCost)
            {
                continue;
            }

            path.LastStep.LoadNeighbours(loadedPathFindTiles, path.LastStep.Edges, usesRoads, isBeingTransportedByWater, edgeMovementCosts, terrainMovementCosts, canStopOn);

            foreach (TNode n in path.LastStep.Neighbours)
            {
                double d = distance(path.LastStep, n);
                var newPath = path.AddStep(n, d);
                queue.Enqueue(newPath.TotalCost + estimate(n), newPath);
            }
        }

        return null;
    }
}