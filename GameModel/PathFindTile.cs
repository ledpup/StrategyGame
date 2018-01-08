using Hexagon;
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

        public List<PathFindTile> Neighbours { get; set; }
        public Dictionary<PathFindTile, double> MoveCost { get; set; }
        public bool HasCumulativeCost { get; set; }

        public PathFindTile(Point point)
        {
            Point = point;
            MoveCost = new Dictionary<PathFindTile, double>();
        }

        public PathFindTile(int x, int y) : this(new Point(x, y))
        {
        }

        public override string ToString()
        {
            return $"{X}, {Y}";
        }
    }
}
