using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class Tile
    {
        public ushort X;
        public ushort Y;
        public TerrainType BaseTerrainType;

        public Tile(ushort x, ushort y, TerrainType terrainType)
        {
            X = x;
            Y = y;
            BaseTerrainType = terrainType;
        }

        public IEnumerable<Tile> AdjacentTiles
        {
            get
            {
                if (_adjacentTiles == null)
                {
                    throw new Exception();
                }
                return _adjacentTiles;
            }
            private set { _adjacentTiles = value; }
        }
        private IEnumerable<Tile> _adjacentTiles;

        public bool IsCoastal
        {
            get
            {
                if (_isCoastalDiscovered)
                    return _isCoastal;

                _isCoastalDiscovered = true;

                return _isCoastal = Terrain.All_Land.HasFlag(BaseTerrainType) && AdjacentTiles.Any(x => Terrain.All_Water.HasFlag(x.BaseTerrainType));
            }
        }
        bool _isCoastal;
        bool _isCoastalDiscovered;

        public void SetAdjacentTiles(Board board)
        {
            if (_adjacentTiles != null)
                throw new Exception("Adjacent tiles have already be calculated");

            var adjacentTiles = new List<Tile>();

            var possibleExits = X % 2 == 0 ? AdjacentEvenTiles : AdjacentOddTiles;

            foreach (var vector in possibleExits)
            {
                var neighbourX = X + vector.X;
                var neighbourY = Y + vector.Y;

                if (neighbourX >= 0 && neighbourX < board.Width && neighbourY >= 0 && neighbourY < board.Height)
                    adjacentTiles.Add(board[neighbourX, neighbourY]);
            }

            AdjacentTiles = adjacentTiles;
        }

        static List<Point> AdjacentEvenTiles
        {
            get
            {
                return new List<Point>
                {
                    new Point(0, 1),
                    new Point(1, 1),
                    new Point(1, 0),
                    new Point(0, -1),
                    new Point(-1, 0),
                    new Point(-1, 1),
                };
            }
        }

        static List<Point> AdjacentOddTiles
        {
            get
            {
                return new List<Point>
                {
                    new Point(0, 1),
                    new Point(1, 0),
                    new Point(1, -1),
                    new Point(0, -1),
                    new Point(-1, 0),
                    new Point(-1, -1),
                };
            }
        }

        //public int StackLimit
        //{
        //    get
        //    {
        //        if (Structure != null)
        //            return stackLimit + 1;
        //        return stackLimit;
        //    }
        //}

        //private int stackLimit
        //{
        //    get { return TerrainType.StackLimit; }
        //}


        
    }
}
