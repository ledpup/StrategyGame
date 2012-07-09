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

        public Board(string[] data)
        {
            Width = data[0].Length;
            Height = data.Length;

            _tiles = InitialiseTiles(Width, Height, data);

            Tiles.ToList().ForEach(x => x.AdjacentTiles = Board.GetAdjacentTiles(Width, Height, this, this[x.X, x.Y]));
        }

        private static Tile[,] InitialiseTiles(int width, int height, string[] data)
        {
            var tiles = new Tile[width, height];

            for (ushort x = 0; x < width; x++)
                for (ushort y = 0; y < height; y++)
                {
                    var terrainType = Terrain.ConvertCharToTerrainType(char.Parse(data[y].Substring(x, 1)));
                    tiles[x, y] = new Tile(x, y, terrainType);
                }
            
            return tiles;
        }

        public static Board LoadBoard(string data)
        {
            var lines = data.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            return new Board(lines);
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

        public static IEnumerable<Tile> GetAdjacentTiles(int width, int height, Board board, Tile origin)
        {
            var adjacentTiles = new List<Tile>();

            if (origin.Y - 1 >= 0)
                adjacentTiles.Add(board[origin.X, origin.Y - 1]);

            if (origin.Y + 1 < height)
                adjacentTiles.Add(board[origin.X, origin.Y + 1]);

            if (origin.X - 1 >= 0)
            {
                if (origin.X % 2 == 0)
                {
                    if (origin.Y - 1 >= 0)
                        adjacentTiles.Add(board[origin.X - 1, origin.Y - 1]);

                    adjacentTiles.Add(board[origin.X - 1, origin.Y]);
                }
                else
                {
                    adjacentTiles.Add(board[origin.X - 1, origin.Y]);

                    if (origin.Y + 1 < height)
                        adjacentTiles.Add(board[origin.X - 1, origin.Y + 1]);
                }
            }
            if (origin.X + 1 < width)
            {
                if (origin.X % 2 == 0)
                {
                    if (origin.Y - 1 >= 0)
                        adjacentTiles.Add(board[origin.X + 1, origin.Y - 1]);

                    adjacentTiles.Add(board[origin.X + 1, origin.Y]);

                }
                else
                {
                    adjacentTiles.Add(board[origin.X + 1, origin.Y]);

                    if (origin.Y + 1 < height)
                        adjacentTiles.Add(board[origin.X + 1, origin.Y + 1]);
                }
            }

            return adjacentTiles;
        }
    }
}
