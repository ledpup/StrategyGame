using Hexagon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameModel
{
    public class CalculateRange
    {
        // https://www.redblobgames.com/grids/hexagons/#range
        //function cube_reachable(start, movement):
        //    var visited = set()
        //    add start to visited
        //    var fringes = []
        //    fringes.append([start])

        //    for each 1 < k ≤ movement:
        //        fringes.append([])
        //        for each cube in fringes[k - 1]:
        //            for each 0 ≤ dir< 6:
        //                var neighbor = cube_neighbor(cube, dir)
        //                if neighbor not in visited, not blocked:
        //                    add neighbor to visited
        //                    fringes[k].append(neighbor)
        //    return visited

        public static List<Edge> UnitRangeForTurn(Tile start, int movementPoints, bool usesRoads, bool isBeingTransportedByWater, Dictionary<EdgeType, int> edgeMovementCosts, Dictionary<TerrainType, int> terrainMovementCosts)
        {
            var visited = new List<Edge>();

            var fringes = new List<Tile>[movementPoints + 1];
            fringes[0] = new List<Tile> { start };

            for (var k = 1; k <= movementPoints; k++)
            {
                fringes[k] = new List<Tile>();
                foreach (var tile in fringes[k - 1])
                {
                    foreach (var neighbour in start.Neighbours)
                    {
                        if (!visited.Contains(neighbour))
                        {
                            visited.Add(neighbour);
                            fringes[k].Add(neighbour.Destination);
                            //if (neighbour.IsValidMove(usesRoads, isBeingTransportedByWater, edgeMovementCosts, terrainMovementCosts))
                            //{
                            //    visited.Add(neighbour);
                            //    fringes[k].Add(neighbour.Destination);
                            //}
                        }
                    }
                }
            }
            return visited;
        }
    }
}
