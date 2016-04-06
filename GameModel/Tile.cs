using PathFind;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameModel
{
    public class Tile
    {
        public int Id { get; private set; }
        public Point Location { get; private set; }
        public int X { get { return Location.X; } }
        public int Y { get { return Location.Y; } }
        public TerrainType TerrainType;
        public List<MilitaryUnit> Units;

        public Tile(int id, int x, int y, TerrainType terrainType = TerrainType.Grassland, bool isEdge = false)
        {
            Units = new List<MilitaryUnit>();

            Id = id;
            Location = new Point(x, y);
            TerrainType = terrainType;
            IsEdgeOfMap = isEdge;
        }

        public override string ToString()
        {
            var subTerrain = IsLake ? " (Lake)" : IsSea ? " (Sea)" : "";
            return Location.ToString() + " " + TerrainType + subTerrain;
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

        public double CalculateMoveCost(MilitaryUnit unit, Tile destination)
        {
            var edge = AdjacentTileEdges.SingleOrDefault(x => x.Tiles.Contains(destination));

            if (edge != null)
            {
                if (edge.EdgeType.HasFlag(unit.CanMoveOverEdge))
                {
                    return 1;
                }
            }

            if (unit.TerrainMovementCosts[destination.TerrainType] != null)
            {
                return (double)unit.TerrainMovementCosts[destination.TerrainType];
            }
            
            return 100;
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
                if (_isTileSearchedForCoastal)
                    return _isCoastal;

                _isTileSearchedForCoastal = true;

                _isCoastal = Terrain.All_Land.HasFlag(TerrainType) && AdjacentTiles.Any(x => Terrain.Aquatic_Terrain.HasFlag(x.TerrainType));

                if (_isCoastal)
                    TerrainType |= TerrainType.Coastal;

                return _isCoastal;
            }
        }
        bool _isCoastal;
        bool _isTileSearchedForCoastal;

        public bool IsSea
        {
            get
            {
                if (_isTileSearchedForSea)
                    return _isSea;

                _isTileSearchedForSea = true;

                _isSea = Terrain.Water_Terrain.HasFlag(TerrainType) && (AdjacentTiles.Any(x => x.IsSea) || IsEdgeOfMap);

                if (_isSea)
                    TerrainType |= TerrainType.Sea;

                return _isSea;
            }
        }
        bool _isSea;
        bool _isTileSearchedForSea;

        public bool IsLake
        {
            get
            {
                if (_isTileSearchedForLake)
                    return _isLake;

                _isTileSearchedForLake = true;

                _isLake = Terrain.Water_Terrain.HasFlag(TerrainType) && !IsEdgeOfMap && !AdjacentTiles.Any(x => x.IsSea);

                if (_isLake)
                    TerrainType |= TerrainType.Lake;

                return _isLake;
            }
        }
        bool _isLake;
        bool _isTileSearchedForLake;

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

        public IEnumerable<Tile> ValidAdjacentMoves(MilitaryUnit unit)
        {
            return ValidAdjacentMoves(unit, this);
        }

        public static IEnumerable<Tile> ValidAdjacentMoves(MilitaryUnit unit, Tile tile)
        {
            return tile.AdjacentTiles.Where(x => unit.TerrainMovementCosts[x.TerrainType] != null);
        }

        static List<Point> AdjacentOddTiles
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

        static List<Point> AdjacentEvenTiles
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


        static Func<MilitaryUnit, MilitaryUnit, bool> IsInConflictFunc = (p, o) => p.OwnerId != o.OwnerId;

        public bool IsInConflict
        {
            get
            {
                foreach (var unit in Units)
                    if (Units.Where(x => x != unit).Any(x => IsInConflictFunc(x, unit)))
                        return true;
                return false;
            }
        }

        public bool IsEdgeOfMap { get; private set; }
    }
}
