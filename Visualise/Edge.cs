using GameModel;
using Hexagon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Visualise
{
    public class Edge
    {
        public Point Origin;
        public Point Destination;
        public ArgbColour Colour;
        public Edge(Point origin, Point destination, ArgbColour colour)
        {
            Origin = origin;
            Destination = destination;
            Colour = colour;
        }
    }
}
