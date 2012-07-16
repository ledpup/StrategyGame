using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class Stack
    {
        List<Unit> Units;

        public Stack(List<Unit> units)
        {
            Units = units;
        }

        public bool CanTransport(UnitType transportUnitType)
        {
            if (!transportUnitType.HasFlag(UnitType.Airborne) && !transportUnitType.HasFlag(UnitType.Aquatic))
                throw new Exception("Transportation can only be undertaken by airborne or aquatic units.");

            var transporterSize = Units.Where(x => x.UnitType == transportUnitType).Sum(x => x.TransportSize);
            var transporteeSize = Units.Where(x => x.UnitType != transportUnitType).Sum(x => x.TransportSize);
            return transporterSize >= transporteeSize;
        }

        public UnitType Transporting()
        {
            if (Units.Any(x => x is AquaticUnit))
            {
                if (CanTransport(UnitType.Aquatic))
                    return UnitType.Aquatic;
            }
            else if (Units.Any(x => x is AirborneUnit))
            {
                if (CanTransport(UnitType.Airborne))
                    return UnitType.Airborne;
            }
            return UnitType.None;
        }

        public static IEnumerable<Tile> UnitMoveList(Unit unit)
        {
            var moveList = new List<Tile>();
            var tilesToExplore = new List<Tile> { unit.Location };

            var move = 0;

            while (move < unit.MovementSpeed)
            {
                var potentialTiles = new List<Tile>();

                tilesToExplore.ForEach(t => potentialTiles.AddRange(t.AdjacentTiles.Where(
                    a => a != unit.Location 
                    && !potentialTiles.Contains(a)
                    && !moveList.Contains(a)
                    && CanCrossEdge(unit.MoveOverEdge, t, a)
                    )));
                var validMoves = potentialTiles.Where(x => unit.MoveOver.HasFlag(x.BaseTerrainType)).ToList();

                moveList.AddRange(validMoves);

                var movesThatDontStopUnit = validMoves.Where(x => !unit.StopOn.HasFlag(x.BaseTerrainType));

                tilesToExplore = new List<Tile>();
                tilesToExplore.AddRange(movesThatDontStopUnit);

                move++;
            }
            return moveList;
        }

        private static bool CanCrossEdge(EdgeType moveOverEdge, Tile tile, Tile adjacentTile)
        {
            var adjacentTileEdge = tile.AdjacentTileEdges.SingleOrDefault(edge => edge.Tiles.Any(x => x.Id == adjacentTile.Id));

            if (adjacentTileEdge == null)
                return true;

            if (adjacentTileEdge.EdgeType.HasFlag(EdgeType.River))
            {
                if (moveOverEdge.HasFlag(EdgeType.River))
                    return true;

                if (adjacentTileEdge.EdgeType.HasFlag(EdgeType.Road))
                {
                    if (moveOverEdge.HasFlag(EdgeType.Road))
                        return true;
                }
            }

            return false;
        }

        public static IEnumerable<Tile> StackMoveLost(List<Unit> units)
        {
            return null;
        }
    }
}
