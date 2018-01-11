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
        public Hex Origin;
        public Hex Destination;
        public ArgbColour Colour;
        public int Width;
        public Centreline(Hex origin, Hex destination, ArgbColour colour, bool isBridge = false)
        {
            Origin = origin;
            Destination = destination;
            Colour = colour;
            Width = colour == Colours.Black ? 1 : isBridge ? 5 : 3;
        }

        public static List<Centreline> MoveOrderToCentrelines(MoveOrder moveOrder)
        {
            var colour = moveOrder.Unit == null ? Colours.Black : GameBoardRenderer.UnitColour(moveOrder.Unit);
            return moveOrder.Moves.Select(x => new Centreline(x.Origin.Hex, x.Edge.Destination.Hex, colour)).ToList();
        }
        public static List<Centreline> PathFindTilesToCentrelines(IEnumerable<PathFindTile> path)
        {
            var pathArray = path.ToArray();

            var lines = new List<Centreline>();
            for (var i = 0; i < pathArray.Length - 1; i++)
            {
                lines.Add(new Centreline(pathArray[i].Hex, pathArray[i + 1].Hex, Colours.Black));
            }

            return lines;
        }
    }
}
