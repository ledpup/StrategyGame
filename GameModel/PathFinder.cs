using Hexagon;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameModel
{
    public static class PathFinder
    {
        public static IEnumerable<PathFindTile> FindShortestPath(Tile origin, Tile destination, MilitaryUnit unit)
        {
            return FindShortestPath(origin, destination, unit.MovementPoints, unit.UsesRoads, unit.IsBeingTransportedByWater, unit.EdgeMovementCosts, unit.TerrainMovementCosts, unit.CanStopOn);
        }

        public static IEnumerable<PathFindTile> FindShortestPath(Tile origin, Tile destination, int maxCumulativeCost, bool usesRoads,
                                                                    bool isBeingTransportedByWater, Dictionary<EdgeType, int> edgeMovementCosts,
                                                                    Dictionary<TerrainType, int> terrainMovementCosts, TerrainType canStopOn)
        {
            var ori = new PathFindTile(origin.Hex, origin.Neighbours);
            var dest = new PathFindTile(destination.Hex, destination.Neighbours);

            Func<PathFindTile, PathFindTile, double> distance = (node1, node2) => node1.MoveCost[node2.Hex];
            Func<PathFindTile, double> estimate = t => Hex.Distance(t.Hex, destination.Hex);

            var path = PathFind.FindPath(ori, dest, distance, estimate, maxCumulativeCost, usesRoads, isBeingTransportedByWater, edgeMovementCosts, terrainMovementCosts, canStopOn);

            return path == null || path.Count() == 1 ? null : path.Reverse();
        }

        public static List<Move> MovesFromShortestPath(List<Move> possibleMoves, PathFindTile[] shortestPath)
        {
            List<Move> moves = [];
            Move furthestMove = null;
            var origin = shortestPath[0].Hex;
            for (var i = 1; i < shortestPath.Length; i++)
            {
                var move = possibleMoves.FirstOrDefault(x => origin == x.Origin.Hex && x.Edge.Destination.Hex == shortestPath[i].Hex && x.Distance == i);

                if (move == null)
                {
                    while (furthestMove != null && furthestMove.MoveType == MoveType.OnlyPassingThrough)
                    {
                        moves.Remove(furthestMove);
                        furthestMove = furthestMove.PreviousMove;
                    }
                    return moves;
                }

                moves.Add(move);
                furthestMove = move;

                possibleMoves.RemoveAll(x => x.Origin.Hex == origin);
                origin = shortestPath[i].Hex;
            }

            return moves;
        }
    }
}
