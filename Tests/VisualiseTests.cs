using Microsoft.VisualStudio.TestTools.UnitTesting;
using GameModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    

    [TestClass]
    public class VisualiseTests
    {
        static string[] GameBoard = File.ReadAllLines("BasicBoard.txt");
        static string[] TileEdges = File.ReadAllLines("BasicBoardEdges.txt");

        Func<PathFindTile, PathFindTile, double> distance = (node1, node2) => node1.MoveCost[node2];

        [TestMethod]
        public void VisualiseBoardTest()
        {
            var board = new Board(GameBoard, TileEdges);

            Visualise.Integration.DrawHexagonImage("BasicBoard.png", board.Tiles);
        }

        [TestMethod]
        public void VisualisePathfind()
        {
            var board = new Board(GameBoard, TileEdges);

            var unit = new MilitaryUnit() { Tile = board[1, 1] };

            var vectors = new List<Vector>();

            List<PathFindTile> pathFindTiles = Board.GetValidMovesWithMoveCostsForUnit(board, unit);

            vectors.AddRange(FindPath(pathFindTiles, new Point(1, 1), new Point(7, 7)));

            vectors.AddRange(FindPath(pathFindTiles, new Point(10, 3), new Point(13, 6)));

            var labels = new string[board.Width, board.Height];
            for (var x = 0; x < board.Width; x++)
            {
                for (var y = 0; y < board.Height; y++)
                {
                    labels[x, y] = board[x, y].Id.ToString();
                }
            }

            Visualise.Integration.DrawHexagonImage("BasicBoard.png", board.Tiles, labels, vectors);
        }

        private List<Vector> FindPath(List<PathFindTile> pathFindTiles, Point origin, Point destination)
        {
            var ori = pathFindTiles.Single(x => x.X == origin.X && x.Y == origin.Y);
            var dest = pathFindTiles.Single(x => x.X == destination.X && x.Y == destination.Y);

            Func<PathFindTile, double> estimate = t => Math.Sqrt(Math.Pow(t.X - destination.X, 2) + Math.Pow(t.Y - destination.Y, 2));

            var path = PathFind.PathFind.FindPath(ori, dest, distance, estimate).Reverse().ToArray();

            var vectors = new List<Vector>();
            for (var i = 0; i < path.Length - 1; i++)
            {
                vectors.Add(new Vector(path[i].Point, path[i + 1].Point, ArgbColour.Black));
            }

            return vectors;
        }
    }
}
