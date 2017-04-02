using PathFind;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameModel
{
    public class PathFindTile : IHasNeighbours<PathFindTile>
    {
        public Point Point {get; set; }

        public int X { get { return Point.X; } }
        public int Y { get { return Point.Y; } }

        public IEnumerable<PathFindTile> Neighbours { get; set; }
        public Dictionary<PathFindTile, double> MoveCost { get; set; }

        public PathFindTile(int x, int y)
        {
            Point = new Point(x, y);
            MoveCost = new Dictionary<PathFindTile, double>();
        }
    }
}
