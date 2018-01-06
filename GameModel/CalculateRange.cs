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

        public static List<Hex> UnitRangeForTurn(Hex start, int movementPoints, List<Hex> blockedHexes = null)
        {
            if (blockedHexes == null)
            {
                blockedHexes = new List<Hex>();
            }

            var visited = new List<Hex>
            {
                start
            };

            var fringes = new List<Hex>[movementPoints + 1];
            fringes[0] = new List<Hex> { start };

            for (var k = 1; k <= movementPoints; k++)
            {
                fringes[k] = new List<Hex>();
                foreach (var hex in fringes[k - 1])
                {
                    for (var dir = 0; dir < 6; dir++)
                    {
                        var neighbour = Hex.Neighbor(hex, dir);
                        if (!visited.Contains(neighbour) && !blockedHexes.Contains(neighbour))
                        {
                            visited.Add(neighbour);
                            fringes[k].Add(neighbour);
                        }
                    }
                }
            }
            return visited;
        }
    }
}
