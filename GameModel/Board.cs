namespace GameModel;

using System;
using System.Collections.Generic;
using System.Linq;
using Hexagon;
using NLog;

public enum Weather
{
    Fine,
    Dry,
    Wet,
    Cold,
}

public class Board
{
    private readonly Logger logger;

    public Board(string[] tiles, string[] edges = null, string[] settlements = null, Logger logger = null)
    {
        Width = tiles[0].Length;
        Height = tiles.Length;

        InitialiseTiles(Width, Height, tiles);
        IntitaliseEdges(edges);
        BuildEdgeLookup();
        InitialiseNeighbours(Edges);
        CalculateTileDistanceFromTheSea();
        Settlements = ParseSettlements(settlements, []);
        InitialiseSupply();
        this.logger = logger ?? LogManager.GetCurrentClassLogger();

        TerrainTemperatureModifiers = [];
        foreach (TerrainType terrainType in Enum.GetValues<TerrainType>())
        {
            TerrainTemperatureModifiers.Add(terrainType, 0);
        }

        TerrainTemperatureModifiers[TerrainType.Mountain] = -10;
        TerrainTemperatureModifiers[TerrainType.Hill] = -5;
    }

    public int Width { get; private set; }

    public int Height { get; private set; }

    public List<Settlement> Settlements { get; private set; }

    public List<Settlement> ParseSettlements(string[] tilePoints, List<Player> players)
    {
        var settlements = new List<Settlement>();

        if (tilePoints == null)
        {
            Settlements = settlements;
            return settlements;
        }

        foreach (var point in tilePoints)
        {
            var settlementProperties = point.Split(',');
            var index = int.Parse(settlementProperties[0]);
            var settlementType = Enum.Parse<SettlementType>(settlementProperties[1]);
            var ownerColour = Enum.Parse<PlayerColour>(settlementProperties[2]);
            var owner = players.FirstOrDefault(p => p.Colour == ownerColour) ?? new Player(ownerColour, ownerColour.ToString());
            var supply = int.Parse(settlementProperties[3]);
            var settlement = new Settlement(settlementType, TileArray[index], owner, supply);

            settlements.Add(settlement);
        }

        Settlements = settlements;
        return settlements;
    }

    public void InitialiseSupply()
    {
        Tiles.ToList().ForEach(x => x.Supply = null);
        var supplyCalculated = new HashSet<Tile>();
        foreach (var settlement in Settlements)
        {
            CalculateSupply(this[settlement.Location.Index], settlement.Owner.Id, settlement.Supply, supplyCalculated);
        }
    }

    private void CalculateSupply(Tile tile, Guid ownerId, float supply, HashSet<Tile> supplyCalculated)
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
                if (neighbour.Destination.Settlement?.Owner.Id == ownerId || neighbour.Destination.Settlement?.Owner == null)
                {
                    var tileEdge = GetEdgeBetween(tile, neighbour.Destination);
                    if (tileEdge != null)
                    {
                        if (tileEdge.EdgeType != EdgeType.Mountain)
                        {
                            var edgeModifier = tileEdge.EdgeType == EdgeType.Wall ? 0 : 0.5f;
                            if (Terrain.NonMountainousLand.HasFlag(neighbour.Destination.TerrainType))
                            {
                                if (Terrain.RoughLand.HasFlag(neighbour.Destination.TerrainType))
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
                    else if (Terrain.NonMountainousLand.HasFlag(neighbour.Destination.TerrainType))
                    {
                        if (Terrain.RoughLand.HasFlag(neighbour.Destination.TerrainType))
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

    private void CalculateTileDistanceFromTheSea()
    {
        // Multi-source BFS from all sea tiles simultaneously - O(tiles) instead of O(tiles²).
        var queue = new Queue<Tile>();

        foreach (var tile in TileArray)
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

                this[x, y].Temperature = (y * .5) + 10 + TerrainTemperatureModifiers[this[x, y].TerrainType] - (this[x, y].DistanceFromWater * 2) + (Math.Sin(turn * seasonRate) * temperatureShiftPerMonth);
            }
        }
    }

    public void InitialiseNeighbours(List<Edge> edges)
    {
        foreach (var tile in Tiles)
        {
            if (tile.Neighbours != null)
            {
                throw new Exception("Adjacent tiles have already be calculated");
            }

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
                    {
                        neighbour = new Edge(edge.EdgeType, this[Hex.HexToIndex(tile.Hex, Width, Height)], this[neighbourX, neighbourY], edge.HasRoad);
                    }
                    else
                    {
                        neighbour = new Edge(EdgeType.None, this[Hex.HexToIndex(tile.Hex, Width, Height)], this[neighbourX, neighbourY], false);
                    }

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
        {
            return;
        }

        edges.ToList().ForEach(
            x =>
            {
                var columns = x.Split(',');

                var tileIndexes = new List<int> { int.Parse(columns[0]), int.Parse(columns[1]) };

                var firstTile = tileIndexes.Min();
                var secondTile = tileIndexes.Max();

                if (firstTile == secondTile)
                {
                    throw new Exception("Must create an edge between two different tiles");
                }

                var t1 = TileArray[firstTile];
                var t2 = TileArray[secondTile];

                //if (!t1.Neighbours.Contains(t2))
                //    throw new Exception(string.Format("Can not create a tile edge between tile {0} and tile {1} because they are not neighbours", t1.Index, t2.Index));

                var existingEdge = Edges.Where(y => y.CrossesEdge(t1, t2));

                if (existingEdge.Any())
                {
                    throw new Exception(string.Format("Can not create a tile edge between tile {0} and tile {1} because one already exists of type {2}.", t1.Index, t2.Index, existingEdge.Single().EdgeType.ToString()));
                }

                Edges.Add(new Edge(columns[2], t1, t2, bool.Parse(columns[3])));
            }
        );
    }

    private void InitialiseTiles(int width, int height, string[] tileData)
    {
        TileArray = new Tile[width * height];

        for (ushort x = 0; x < width; x++)
        {
            for (ushort y = 0; y < height; y++)
            {
                var terrainType = Terrain.ConvertCharToTerrainType(char.Parse(tileData[y].Substring(x, 1)));
                var isEdgeOfMap = x == 0 || y == 0 || x == width || y == height ? true : false;

                var tile = new Tile(x, y, width, terrainType, isEdgeOfMap);
                TileArray[(y * width) + x] = tile;
            }
        }
    }

    public Tile this[int index]
    {
        get
        {
            return TileArray[index];
        }
    }

    // Offset coordinates
    public Tile this[int x, int y]
    {
        get
        {
            var index = OffsetCoord.OffsetCoordsToIndex(x, y, Width);
            return TileArray[index];
        }
    }

    public Tile[] TileArray { get; private set; }

    public IEnumerable<Tile> Tiles
    {
        get
        {
            return TileArray;
        }
    }

    public Dictionary<TerrainType, double> TerrainTemperatureModifiers { get; private set; }

    public List<Edge> Edges;

    private Dictionary<(int, int), Edge> edgeLookup;

    private void BuildEdgeLookup()
    {
        edgeLookup = new Dictionary<(int, int), Edge>(Edges.Count);
        foreach (var edge in Edges)
        {
            var key1 = (Math.Min(edge.Origin.Index, edge.Destination.Index), Math.Max(edge.Origin.Index, edge.Destination.Index));
            edgeLookup[key1] = edge;
        }
    }

    private Edge GetEdgeBetween(Tile a, Tile b)
    {
        if (edgeLookup == null)
        {
            return Edges?.SingleOrDefault(x => x.CrossesEdge(a, b));
        }

        var key = (Math.Min(a.Index, b.Index), Math.Max(a.Index, b.Index));
        edgeLookup.TryGetValue(key, out var edge);
        return edge;
    }
}
