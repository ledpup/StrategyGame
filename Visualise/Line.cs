using GameModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Visualise
{
    public enum LineType
    {
        Hexside,
        CentreToCentre
    }

    public class Line
    {
        public Point Origin;
        public Point Destination;
        public LineType LineType;
        public ArgbColour Colour;
        public Line(Point origin, Point destination, ArgbColour colour, LineType lineType)
        {
            Origin = origin;
            Destination = destination;
            Colour = colour;
            LineType = lineType;
        }

        public static List<Line> Lines(MoveOrder moveOrder)
        {
            
            var colour = Unit == null ? Colours.Black : Unit.UnitColour;
            return moveOrder.Moves.Select(x => new Line(x.Origin.Point, x.Destination.Point, colour, LineType.CentreToCentre)).ToList();
            
        }
        public static List<Line> PathFindTilesToVectors(IEnumerable<PathFindTile> path)
        {
            var pathArray = path.ToArray();

            var lines = new List<Line>();
            for (var i = 0; i < pathArray.Length - 1; i++)
            {
                lines.Add(new Line(pathArray[i].Point, pathArray[i + 1].Point, Colours.Black, LineType.CentreToCentre));
            }

            return lines;
        }
    }


}
