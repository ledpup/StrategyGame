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

        public Board(string[] tiles, string[] tilesEdges)
        {
            if (tilesEdges == null)
                throw new ArgumentNullException("tileEdges");

            Width = tiles[0].Length;
            Height = tiles.Length;

            _tiles = InitialiseTiles(Width, Height, tiles);
            TileEdges = IntitaliseTileEdges(tilesEdges);

            Tiles.ToList().ForEach(x => x.SetAdjacentTiles(this));
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

        public static Board LoadBoard(string tileData, string tileEdgesData)
        {
            var tileRows = tileData.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            var tileEdgesRows = tileEdgesData.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            return new Board(tileRows, tileEdgesRows);
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
    }
}
