using Hexagon;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameModel
{
    public enum Weather
    {
        Fine,
        Dry,
        Wet,
        Cold,
    }
    public class Board
    {
        Tile[] _tiles;

        public int Width;
        public int Height;

        private static Logger Logger;

        public List<Settlement> Settlements;

        public Board(string[] tiles, string[] edges = null, string[] settlements = null, Logger logger = null)
        {
            Width = tiles[0].Length;
            Height = tiles.Length;

            InitialiseTiles(Width, Height, tiles);
            IntitaliseEdges(edges);
            BuildEdgeLookup();
            InitialiseNeighbours(Edges);
            CalculateTileDistanceFromTheSea();
            Settlements = IntitaliseSettlements(settlements);
            InitialiseSupply();
            CalculateContiguousRegions();

            Logger = logger;
            Logger ??= LogManager.GetCurrentClassLogger();

            TerrainTemperatureModifiers = [];
            foreach (TerrainType terrainType in Enum.GetValues<TerrainType>())
            {
                TerrainTemperatureModifiers.Add(terrainType, 0);
            }
            TerrainTemperatureModifiers[TerrainType.Mountain] = -10;
            TerrainTemperatureModifiers[TerrainType.Hill] = -5;
        }

        private void CalculateContiguousRegions()
        {
            var id = 0;
            foreach (var tile in _tiles)
            {
                if (tile.ContiguousRegionId == 0)
                {
                    id++;
                    tile.ContiguousRegionId = id;
                    AssignContiguousTilesToRegion(tile, id, tile.TerrainType == TerrainType.Mountain);
                }
            }
        }

        private static void AssignContiguousTilesToRegion(Tile start, int id, bool isMountainRange)
        {
            var stack = new Stack<Tile>();
            stack.Push(start);
            while (stack.Count > 0)
            {
                var tile = stack.Pop();
                foreach (var x in tile.Neighbours)
                {
                    if (x.Destination.ContiguousRegionId == 0
                        && x.Destination.BaseTerrainType == tile.BaseTerrainType
                        && (x.HasRoad || (tile.TerrainType != TerrainType.Mountain && x.Destination.TerrainType != TerrainType.Mountain)
                            || (tile.TerrainType == TerrainType.Mountain && x.Destination.TerrainType == TerrainType.Mountain && isMountainRange)))
                    {
                        x.Destination.ContiguousRegionId = id;
                        stack.Push(x.Destination);
                    }
                }
            }
        }

        public void InitialiseSupply()
        {
            Tiles.ToList().ForEach(x => x.Supply = null);
            var supplyCalculated = new HashSet<Tile>();
            foreach (var settlement in Settlements)
            {
                CalculateSupply(this[settlement.Index], settlement.OwnerIndex, settlement.Supply, supplyCalculated);
            }
        }

        private void CalculateSupply(Tile tile, int ownerId, float supply, HashSet<Tile> supplyCalculated)
        {
            if (supplyCalculated.Contains(tile))
            {
                if (tile.Supply < supply)
                {
                    tile.Supply = supply;
                }
                else
                {
                    return;
                }
            }
            else
            {
                supplyCalculated.Add(tile);
                tile.Supply = supply;
            }
            
            if (supply > 1)
            {
                foreach (var neighbour in tile.Neighbours)
                {
                    float neighbourSupply = 0;
                    if (neighbour.Destination.OwnerId == ownerId || neighbour.Destination.OwnerId == null)
                    {
                        var tileEdge = GetEdgeBetween(tile, neighbour.Destination);
                        if (tileEdge != null)
                        {
                            if (tileEdge.EdgeType != EdgeType.Mountain)
                            {
                                var edgeModifier = tileEdge.EdgeType == EdgeType.Wall ? 0 : 0.5f;
                                if (Terrain.Non_Mountainous_Land.HasFlag(neighbour.Destination.TerrainType))
                                {
                                    if (Terrain.Rough_Land.HasFlag(neighbour.Destination.TerrainType))
                                    {
                                        neighbourSupply = supply - 1.5f - edgeModifier;
                                    }
                                    else
                                    {
                                        neighbourSupply = supply - 1f - edgeModifier;
                                    }
                                }
                            }
                        }
                        else if (Terrain.Non_Mountainous_Land.HasFlag(neighbour.Destination.TerrainType))
                        {
                            if (Terrain.Rough_Land.HasFlag(neighbour.Destination.TerrainType))
                            {
                                neighbourSupply = supply - 1.5f;
                            }
                            else
                            {
                                neighbourSupply = supply - 1;
                            }
                        }
                        if (neighbourSupply >= 1)
                        {
                            CalculateSupply(neighbour.Destination, ownerId, neighbourSupply, supplyCalculated);
                        }
                    }
                }
            }
        }

        private List<Settlement> IntitaliseSettlements(string[] tilePoints)
        {
            var settlements = new List<Settlement>();

            if (tilePoints == null)
                return settlements;
            foreach (var point in tilePoints)
            {
                var settlementProperties = point.Split(',');
                var index = int.Parse(settlementProperties[0]);
                var settlementType = Enum.Parse<SettlementType>(settlementProperties[2]);
                var settlement = new Settlement(index, settlementType, TileArray[index], int.Parse(settlementProperties[2]), int.Parse(settlementProperties[3]));

                
                settlements.Add(settlement);
            }
            return settlements;
        }

        private void CalculateTileDistanceFromTheSea()
        {
            // Multi-source BFS from all sea tiles simultaneously - O(tiles) instead of O(tiles²).
            var queue = new Queue<Tile>();

            foreach (var tile in _tiles)
            {
                if (tile.IsSea)
                {
                    tile.DistanceFromWater = 0;
                    queue.Enqueue(tile);
                }
                else
                {
                    tile.DistanceFromWater = int.MaxValue;
                }
            }

            while (queue.Count > 0)
            {
                var tile = queue.Dequeue();
                foreach (var neighbour in tile.Neighbours)
                {
                    var dest = neighbour.Destination;
                    var newDist = tile.DistanceFromWater + 1;
                    if (newDist < dest.DistanceFromWater)
                    {
                        dest.DistanceFromWater = newDist;
                        queue.Enqueue(dest);
                    }
                }
            }
        }

        public void CalculateTemperature(int turn)
        {
            for (var x = 0; x < Width; x++)
            {
                for (var y = 0; y < Height; y++)
                {
                    const double seasonRate = .3;
                    const double temperatureShiftPerMonth = 8;
                    
                    this[x, y].Temperature = y * .5 + 10 + TerrainTemperatureModifiers[this[x, y].TerrainType] - (this[x, y].DistanceFromWater * 2) + Math.Sin(turn * seasonRate) * temperatureShiftPerMonth;
                }
            }
        }

        public void InitialiseNeighbours(List<Edge> edges)
        {
            foreach (var tile in Tiles)
            {
                if (tile.Neighbours != null)
                    throw new Exception("Adjacent tiles have already be calculated");

                var neighbours = new List<Edge>();

                var hexes = Hex.Neighbours(tile.Hex);

                foreach (var hex in hexes)
                {
                    var neighbourX = OffsetCoord.QoffsetFromCube(hex).col;
                    var neighbourY = OffsetCoord.QoffsetFromCube(hex).row;

                    if (neighbourX >= 0 && neighbourX < Width && neighbourY >= 0 && neighbourY < Height)
                    {
                        var edge = Edge.GetEdge(Edges, tile, this[neighbourX, neighbourY]);

                        Edge neighbour;

                        if (edge != null)
                            neighbour = new Edge(edge.EdgeType, this[Hex.HexToIndex(tile.Hex, Width, Height)], this[neighbourX, neighbourY], edge.HasRoad);
                        else
                            neighbour = new Edge(EdgeType.None, this[Hex.HexToIndex(tile.Hex, Width, Height)], this[neighbourX, neighbourY], false);

                        neighbours.Add(neighbour);
                    }
                }

                tile.Neighbours = neighbours;
            }
        }

        private void IntitaliseEdges(string[] edges)
        {
            Edges = [];

            if (edges == null)
                return;

            edges.ToList().ForEach(
                x =>
                {
                    var columns = x.Split(',');

                    var tileIndexes = new List<int> { int.Parse(columns[0]), int.Parse(columns[1]) };

                    var firstTile = tileIndexes.Min();
                    var secondTile = tileIndexes.Max();

                    if (firstTile == secondTile)
                        throw new Exception("Must create an edge between two different tiles");

                    var t1 = TileArray[firstTile];
                    var t2 = TileArray[secondTile];

                    //if (!t1.Neighbours.Contains(t2))
                    //    throw new Exception(string.Format("Can not create a tile edge between tile {0} and tile {1} because they are not neighbours", t1.Index, t2.Index));

                    var existingEdge = Edges.Where(y => y.CrossesEdge(t1, t2));

                    if (existingEdge.Any())
                        throw new Exception(string.Format("Can not create a tile edge between tile {0} and tile {1} because one already exists of type {2}.", t1.Index, t2.Index, existingEdge.Single().EdgeType.ToString()));

                    Edges.Add(new Edge(columns[2], t1, t2, bool.Parse(columns[3])));
                }
            );
        }

        private void InitialiseTiles(int width, int height, string[] tileData)
        {
            _tiles = new Tile[width * height];

            for (ushort x = 0; x < width; x++)
            {
                for (ushort y = 0; y < height; y++)
                {
                    var terrainType = Terrain.ConvertCharToTerrainType(char.Parse(tileData[y].Substring(x, 1)));
                    var isEdgeOfMap = x == 0 || y == 0 || x == width || y == height ? true : false;

                    var tile = new Tile(x, y, width, terrainType, isEdgeOfMap);
                    _tiles[y * width + x] = tile;
                }
            }
        }

        public Tile this[int index]
        {
            get
            {
                return _tiles[index];
            }
        }

        // Offset coordinates
        public Tile this[int x, int y]
        {
            get
            {
                var index = OffsetCoord.OffsetCoordsToIndex(x, y, Width);
                return _tiles[index];
            }
        }

        public Tile[] TileArray
        {
            get
            {
                return _tiles;
            }
        }

        public IEnumerable<Tile> Tiles
        {
            get 
            {   
                return _tiles;
            }
        }

        public Dictionary<TerrainType, double> TerrainTemperatureModifiers { get; private set; }

        public List<Edge> Edges;

        private Dictionary<(int, int), Edge> _edgeLookup;

        private void BuildEdgeLookup()
        {
            _edgeLookup = new Dictionary<(int, int), Edge>(Edges.Count);
            foreach (var edge in Edges)
            {
                var key1 = (Math.Min(edge.Origin.Index, edge.Destination.Index), Math.Max(edge.Origin.Index, edge.Destination.Index));
                _edgeLookup[key1] = edge;
            }
        }

        private Edge GetEdgeBetween(Tile a, Tile b)
        {
            if (_edgeLookup == null) return Edges?.SingleOrDefault(x => x.CrossesEdge(a, b));
            var key = (Math.Min(a.Index, b.Index), Math.Max(a.Index, b.Index));
            _edgeLookup.TryGetValue(key, out var edge);
            return edge;
        }
    }
}