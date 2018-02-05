using GameModel;
using Hexagon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameModel.Rendering
{
    public class Edge
    {
        public Hex Origin;
        public Hex Destination;
        public ArgbColour Colour;
        public bool IsPort;
        public Edge(Hex origin, Hex destination, ArgbColour colour, bool isPort)
        {
            Origin = origin;
            Destination = destination;
            Colour = colour;
            IsPort = isPort;
        }
    }
}
