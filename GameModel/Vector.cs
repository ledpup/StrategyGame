using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameModel
{
    public struct Vector
    {
        public Vector(Point origin, Point destination)
        {
            Origin = origin;
            Destination = destination;
        }

        public Point Origin;
        public Point Destination;
    }
}
