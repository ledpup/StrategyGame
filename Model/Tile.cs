using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class Tile
    {
        public int Id { get; private set; }
        public Point Location { get; private set; }
        public int X { get { return Location.X; } }
        public int Y { get { return Location.Y; } }
        public TerrainType BaseTerrainType;

        public Tile()
        { }

        public Tile(int id, int x, int y, TerrainType terrainType)
        {
            Id = id;
            Location = new Point(x, y);
            BaseTerrainType = terrainType;
        }

        public override string ToString()
        {
            return Location.ToString();
        }

        public IEnumerable<Edge> AdjacentTileEdges
        {
            get
            {
                if (_adjacentTileEdges == null)
                {
                    throw new Exception();
                }
                return _adjacentTileEdges;
            }
            private set { _adjacentTileEdges = value; }
        }
        private IEnumerable<Edge> _adjacentTileEdges;

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
            private set 
            { 
                _adjacentTiles = value;
            }
        }

        private IEnumerable<Edge> BuildAdjacentEdgeTiles(IEnumerable<Tile> _adjacentTiles)
        {
            var adjacentTileEdges = new List<Edge>();
            _adjacentTiles.ToList().ForEach(x => 
                {
                    var edge = Board.TileEdges.SingleOrDefault(r => r.Tiles.Contains(this) && r.Tiles.Contains(x)); 
                    
                    if (edge == null)
                        return;

                    adjacentTileEdges.Add(edge);
                });
            return adjacentTileEdges;
        }
        private IEnumerable<Tile> _adjacentTiles;

        public bool IsCoastal
        {
            get
            {
                if (_isCoastalDiscovered)
                    return _isCoastal;

                _isCoastalDiscovered = true;

                _isCoastal = Terrain.All_Land.HasFlag(BaseTerrainType) && AdjacentTiles.Any(x => Terrain.All_Water.HasFlag(x.BaseTerrainType));

                if (_isCoastal)
                    BaseTerrainType |= TerrainType.Coastal;

                return _isCoastal;
            }
        }
        bool _isCoastal;
        bool _isCoastalDiscovered;

        public void SetAdjacentTiles(Board board)
        {
            if (_adjacentTiles != null)
                throw new Exception("Adjacent tiles have already be calculated");

            var adjacentTiles = new List<Tile>();

            var potentialTiles = X % 2 == 0 ? AdjacentEvenTiles : AdjacentOddTiles;

            foreach (var tile in potentialTiles)
            {
                var neighbourX = X + tile.X;
                var neighbourY = Y + tile.Y;

                if (neighbourX >= 0 && neighbourX < board.Width && neighbourY >= 0 && neighbourY < board.Height)
                    adjacentTiles.Add(board[neighbourX, neighbourY]);
            }

            AdjacentTiles = adjacentTiles;
            AdjacentTileEdges = BuildAdjacentEdgeTiles(adjacentTiles);
        }

        //public override bool Equals(object obj)
        //{
        //    return this.Location.Equals(((Tile)obj).Location);
        //}

        //public override int GetHasCode()
        //{
        //    return Location.GetHashCode();
        //}

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
