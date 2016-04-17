using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameModel
{
    

    public class Vector
    {
        public Vector(Point origin, Point destination, ArgbColour colour, BaseEdgeType baseEdgeType = BaseEdgeType.CentreToCentre)
        {
            Origin = origin;
            Destination = destination;
            BaseEdgeType = baseEdgeType;
            Colour = colour;
        }

        public Point Origin;
        public Point Destination;
        public BaseEdgeType BaseEdgeType;
        public EdgeType EdgeType;
        public ArgbColour Colour;
    }
}
