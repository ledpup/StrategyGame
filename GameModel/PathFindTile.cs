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
        public Hex Hex { get; set; }

        public int Q { get { return Hex.q; } }
        public int R { get { return Hex.r; } }

        public List<PathFindTile> Neighbours { get; set; }
        public Dictionary<PathFindTile, double> MoveCost { get; set; }
        public bool HasCumulativeCost { get; set; }

        public PathFindTile(Hex hex)
        {
            Hex = hex;
            MoveCost = new Dictionary<PathFindTile, double>();
        }

        public override string ToString()
        {
            return $"{Q}, {R}";
        }
    }
}
