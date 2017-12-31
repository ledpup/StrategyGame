using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameModel
{
    public class Road
    {
        public Road(string roadType, Tile origin, Tile destination)
        {
            Origin = origin;
            Destination = destination;
        }

        public Tile Origin;
        public Tile Destination;

        internal static bool CrossesRoad(Road road, Tile origin, Tile destination)
        {
            return (road.Origin == origin && road.Destination == destination) || (road.Destination == origin && road.Origin == destination);
        }

        internal bool CrossesRoad(Tile origin, Tile destination)
        {
            return CrossesRoad(this, origin, destination);
        }
    }
}
