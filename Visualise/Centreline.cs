using GameModel;
using Hexagon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Visualise
{
    public class Centreline
    {
        public Point Origin;
        public Point Destination;
        public ArgbColour Colour;
        public int Width;
        public Centreline(Point origin, Point destination, ArgbColour colour, bool isBridge = false)
        {
            Origin = origin;
            Destination = destination;
            Colour = colour;
            Width = isBridge ? 6 : 3;
        }

        public static List<Centreline> MoveOrderToCentrelines(MoveOrder moveOrder)
        {
            var colour = moveOrder.Unit == null ? Colours.Black : GameBoardRenderer.UnitColour(moveOrder.Unit);
            return moveOrder.Moves.Select(x => new Centreline(x.Origin.Point, x.Neighbour.Tile.Point, colour)).ToList();
        }
        public static List<Centreline> PathFindTilesToCentrelines(IEnumerable<PathFindTile> path)
        {
            var pathArray = path.ToArray();

            var lines = new List<Centreline>();
            for (var i = 0; i < pathArray.Length - 1; i++)
            {
                lines.Add(new Centreline(pathArray[i].Point, pathArray[i + 1].Point, Colours.Black));
            }

            return lines;
        }
    }
}
