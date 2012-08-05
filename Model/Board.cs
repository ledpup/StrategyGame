using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class Board
    {
        private Tile[,] _tiles;

        public int Width;
        public int Height;
        public List<Unit> Units;
        public Dictionary<int, List<MoveOrder>> MoveOrders;

        public Board(string[] tiles, string[] tilesEdges)
        {
            if (tilesEdges == null)
                throw new ArgumentNullException("tileEdges");

            Width = tiles[0].Length;
            Height = tiles.Length;

            _tiles = InitialiseTiles(Width, Height, tiles);
            TileEdges = IntitaliseTileEdges(tilesEdges);

            Tiles.ToList().ForEach(x => x.SetAdjacentTiles(this));

            MoveOrders = new Dictionary<int, List<MoveOrder>>();
        }

        public static Board LoadBoard(string tileData, string tileEdgesData)
        {
            var tileRows = tileData.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            var tileEdgesRows = tileEdgesData.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            return new Board(tileRows, tileEdgesRows);
        }

        private List<Edge> IntitaliseTileEdges(string[] tilesEdges)
        {
            var tileEdgesList = new List<Edge>();
            tilesEdges.ToList().ForEach(
                x => 
                {
                    var columns = x.Split(',');

                    var tiles = new List<Tile> { 
                        Tiles.Single(t => t.Id == int.Parse(columns[0])),
                        Tiles.Single(t => t.Id == int.Parse(columns[1]))};

                    tileEdgesList.Add(new Edge(columns[2], tiles));
                }
            );
            return tileEdgesList;
        }

        private static Tile[,] InitialiseTiles(int width, int height, string[] data)
        {
            var tiles = new Tile[width, height];

            for (ushort x = 0; x < width; x++)
                for (ushort y = 0; y < height; y++)
                {
                    var terrainType = Terrain.ConvertCharToTerrainType(char.Parse(data[y].Substring(x, 1)));
                    tiles[x, y] = new Tile(x * height + y, x, y, terrainType);
                }
            
            return tiles;
        }

        public Tile this[int index]
        {
            get
            {
                return Tiles.Single(x => x.Id == index);
            }
        }

        public Tile this[int x, int y]
        {
            get
            {
                return _tiles[x, y];
            }
            set
            {
                _tiles[x, y] = value;
            }
        }

        public IEnumerable<Tile> Tiles
        {
            get 
            {   
                for (var i = 0; i < Width; i++)
                    for (var j = 0; j < Height; j++)
                        yield return _tiles[i, j];
            }
        }

        public static List<Edge> TileEdges;

        public static bool EdgeHasRoad(Tile tile, Tile adjacentTile)
        {
            var tileEdge = tile.AdjacentTileEdges.SingleOrDefault(edge => edge.Tiles.Any(x => x.Id == adjacentTile.Id));

            return tileEdge == null ? false : tileEdge.EdgeType == EdgeType.Road;
        }

        public void ResolveMoves(int turn, List<MoveOrder> moveOrders)
        {
            MoveOrders[turn] = moveOrders;

            if (moveOrders.Any(x => x.Turn != turn))
                throw new Exception(string.Format("Move order is not for the current turn ({0}).", turn));

            var movingUnits = moveOrders.Select(x => x.Unit).ToList();
            var maxMovementSpeed = movingUnits.Max(x => x.MovementSpeed);

            var unitStepRate = new Dictionary<Unit, int>();
            movingUnits.ForEach(x => unitStepRate.Add(x, maxMovementSpeed / x.MovementSpeed));

            for (var step = 1; step <= maxMovementSpeed; step++)
            {
                MoveUnitsOneStep(moveOrders, unitStepRate, step);

                // Remove conflicting units from move orders
                var conflictedUnits = DetectConflictedUnits(movingUnits, Units);
                moveOrders.RemoveAll(x => conflictedUnits.Contains(x.Unit));
            }
        }

        public static IEnumerable<Unit> DetectConflictedUnits(List<Unit> setOfUnits, List<Unit> allUnits)
        {
            var conflictedUnits = new List<Unit>();
            setOfUnits.ForEach(x =>
            {
                if (conflictedUnits.Contains(x))
                    return;

                if (allUnits.Any(u => Unit.IsConflict(u, x)))
                {
                    conflictedUnits.Add(x);
                }
            });

            return conflictedUnits;
        }

        private static void MoveUnitsOneStep(List<MoveOrder> moveOrders, Dictionary<Unit, int> unitStepRate, int step)
        {
            foreach (var moveOrder in moveOrders)
            {
                if (step % unitStepRate[moveOrder.Unit] == 0)
                {
                    var moveIndex = step / unitStepRate[moveOrder.Unit] - 1;
                    if (moveOrder.Moves.Length > moveIndex)
                        moveOrder.Unit.Tile = moveOrder.Moves[moveIndex];
                }
            }
        }

        public List<Battle> ConductBattles()
        {
            var battles = new List<Battle>();
            Tiles.ToList().ForEach(x =>
            {
                if (x.IsConflict)
                    battles.Add(new Battle(x.Units));
            });

            battles.ForEach(x => 
            {
                x.AssignCasulties();
                x.ReduceMorale();
                x.ReduceStamina();
                x.Events.ForEach(e => e.Execute());
            });

            return battles;
        }
    }
}
