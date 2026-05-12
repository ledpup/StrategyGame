using GameModel;
using GameModel.Commands;
using Hexagon;
using System.Collections.Generic;
using System.Linq;

namespace Visualise;

public class Centreline(Hex origin, Hex destination, ArgbColour colour, bool isBridge = false)
{
    public Hex Origin = origin;
    public Hex Destination = destination;
    public ArgbColour Colour = colour;
    public int Width = colour == Colours.Black ? 1 : isBridge ? 5 : 3;

    public static List<Centreline> MoveOrderToCentrelines(MoveCommand moveOrder)
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
