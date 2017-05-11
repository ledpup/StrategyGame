using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GameModel;
using System.IO;
using NLog;

namespace Tests
{
    [TestClass]
    public class BoardTests
    {
        public static string[] GameBoard = File.ReadAllLines("BasicBoard.txt");
        public static string[] TileEdges = File.ReadAllLines("BasicBoardEdges.txt");

        [TestMethod]
        public void LoadBoard_AdjacentTilesAndCoastal_NoNullTilesAndCoasts()
        {
            var board = new Board(GameBoard, TileEdges);

            var tiles = board.Tiles.ToList();

            tiles.ForEach(t => Assert.AreEqual(1, tiles.Count(x => x.Index == t.Index)));

            // Ensure that adjacent tiles have been populated correctly
            tiles.ForEach(t => Assert.IsFalse(t.Neighbours.Any(at => at == null)));
            
            // Ensure that some tiles are coastal.
            var coastal = tiles.Count(t => t.IsCoast);
            Assert.AreNotEqual(0, coastal);

            Assert.IsTrue(!board[9, 9].IsCoast);
        }

        [TestMethod]
        public void NeighboursTest()
        {
            var board = new Board(GameBoard);

            Assert.IsTrue(board[2, 1].Neighbours.Any(x => x.X == 2 && x.Y == 0));
            Assert.IsTrue(board[2, 1].Neighbours.Any(x => x.X == 3 && x.Y == 0));
            Assert.IsTrue(board[2, 1].Neighbours.Any(x => x.X == 3 && x.Y == 1));
            Assert.IsTrue(board[2, 1].Neighbours.Any(x => x.X == 2 && x.Y == 2));
            Assert.IsTrue(board[2, 1].Neighbours.Any(x => x.X == 1 && x.Y == 1));
            Assert.IsTrue(board[2, 1].Neighbours.Any(x => x.X == 1 && x.Y == 0));

            Assert.IsTrue(board[5, 4].Neighbours.Any(x => x.X == 5 && x.Y == 3));
            Assert.IsTrue(board[5, 4].Neighbours.Any(x => x.X == 6 && x.Y == 4));
            Assert.IsTrue(board[5, 4].Neighbours.Any(x => x.X == 6 && x.Y == 5));
            Assert.IsTrue(board[5, 4].Neighbours.Any(x => x.X == 5 && x.Y == 5));
            Assert.IsTrue(board[5, 4].Neighbours.Any(x => x.X == 4 && x.Y == 5));
            Assert.IsTrue(board[5, 4].Neighbours.Any(x => x.X == 4 && x.Y == 4));
        }

        [TestMethod]
        public void IsSeaTest()
        {
            var board = new Board(GameBoard, TileEdges);

            Assert.IsTrue(board[9, 9].IsSea);
        }

        [TestMethod]
        public void IsLakeTest()
        {
            var board = new Board(GameBoard, TileEdges);

            Assert.IsTrue(board[14, 4].IsLake);
        }

        [TestMethod]
        public void CoordsTests()
        {
            Assert.AreEqual(29, Point.PointToIndex(2, 1, 27));
            Assert.AreEqual(new Point(2, 1), Point.IndexToPoint(29, 27));
        }

        [TestMethod]
        public void CoordsHexTests()
        {
            const int boardWidth = 27;
            const int boardHeight = 19;

            var point = Point.IndexToPoint(29, boardWidth);
            var hex = OffsetCoord.QoffsetToCube(new OffsetCoord(point.X, point.Y));
            var offsetCoord = OffsetCoord.QoffsetFromCube(hex);
            var index = Hex.HexToIndex(hex, boardWidth, boardHeight);
            Assert.AreEqual(29, index);

            point = Point.IndexToPoint(25, boardWidth);
            hex = OffsetCoord.QoffsetToCube(new OffsetCoord(point.X, point.Y));
            offsetCoord = OffsetCoord.QoffsetFromCube(hex);
            index = Hex.HexToIndex(hex, boardWidth, boardHeight);
            Assert.AreEqual(25, index);


            point = Point.IndexToPoint(32, boardWidth);
            hex = OffsetCoord.QoffsetToCube(new OffsetCoord(point.X, point.Y));
            offsetCoord = OffsetCoord.QoffsetFromCube(hex);
            index = Hex.HexToIndex(hex, boardWidth, boardHeight);
            Assert.AreEqual(32, index);


            Assert.AreEqual(32, Hex.HexToIndex(new Hex(5, -1, -4), boardWidth, boardHeight));
            Assert.AreEqual(60, Hex.HexToIndex(new Hex(6, -1, -5), boardWidth, boardHeight));
            Assert.AreEqual(87, Hex.HexToIndex(new Hex(6, 0, -6), boardWidth, boardHeight));
            Assert.AreEqual(86, Hex.HexToIndex(new Hex(5, 1, -6), boardWidth, boardHeight));
            Assert.AreEqual(85, Hex.HexToIndex(new Hex(4, 1, -5), boardWidth, boardHeight));
            Assert.AreEqual(58, Hex.HexToIndex(new Hex(4, 0, -4), boardWidth, boardHeight));
        }
    }
}
