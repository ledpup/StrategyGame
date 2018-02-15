using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GameModel;
using System.IO;
using NLog;
using Hexagon;
using GameModel.Rendering;
using GameRendering2D;

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

            Assert.IsTrue(board[2, 1].Neighbours.Any(x => x.Destination.Hex.q == 2 && x.Destination.Hex.r == -1));
            Assert.IsTrue(board[2, 1].Neighbours.Any(x => x.Destination.Hex.q == 3 && x.Destination.Hex.r == -1));
            Assert.IsTrue(board[2, 1].Neighbours.Any(x => x.Destination.Hex.q == 3 && x.Destination.Hex.r == 0));
            Assert.IsTrue(board[2, 1].Neighbours.Any(x => x.Destination.Hex.q == 2 && x.Destination.Hex.r == 1));
            Assert.IsTrue(board[2, 1].Neighbours.Any(x => x.Destination.Hex.q == 1 && x.Destination.Hex.r == 1));
            Assert.IsTrue(board[2, 1].Neighbours.Any(x => x.Destination.Hex.q == 1 && x.Destination.Hex.r == 0));

            Assert.IsTrue(board[5, 4].Neighbours.Any(x => x.Destination.Hex.q == 5 && x.Destination.Hex.r == 1));
            Assert.IsTrue(board[5, 4].Neighbours.Any(x => x.Destination.Hex.q == 6 && x.Destination.Hex.r == 1));
            Assert.IsTrue(board[5, 4].Neighbours.Any(x => x.Destination.Hex.q == 6 && x.Destination.Hex.r == 2));
            Assert.IsTrue(board[5, 4].Neighbours.Any(x => x.Destination.Hex.q == 5 && x.Destination.Hex.r == 3));
            Assert.IsTrue(board[5, 4].Neighbours.Any(x => x.Destination.Hex.q == 4 && x.Destination.Hex.r == 3));
            Assert.IsTrue(board[5, 4].Neighbours.Any(x => x.Destination.Hex.q == 4 && x.Destination.Hex.r == 2));
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
            Assert.AreEqual(29, OffsetCoord.OffsetCoordsToIndex(2, 1, 27));
            Assert.AreEqual(new OffsetCoord(2, 1), OffsetCoord.QoffsetFromCube(Hex.IndexToHex(29, 27)));
        }

        [TestMethod]
        public void CoordsHexTests()
        {
            const int boardWidth = 27;
            const int boardHeight = 19;

            var hex = Hex.IndexToHex(29, boardWidth);
            var offsetCoord = OffsetCoord.QoffsetFromCube(hex);
            var index = Hex.HexToIndex(hex, boardWidth, boardHeight);
            Assert.AreEqual(29, index);

            hex = Hex.IndexToHex(25, boardWidth);
            index = Hex.HexToIndex(hex, boardWidth, boardHeight);
            Assert.AreEqual(25, index);


            hex = Hex.IndexToHex(32, boardWidth);
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

        [TestMethod]
        public void ContiguousRegionTest()
        {
            string[] gameBoard = File.ReadAllLines("ContiguousRegionTestBoard.txt");
            string[] tileEdges = File.ReadAllLines("ContiguousRegionTestEdges.txt");

            var board = new Board(gameBoard, tileEdges);

            var labels = new string[board.Width * board.Height];
            board.Tiles.ToList().ForEach(x => labels[x.Index] = x.ContiguousRegionId.ToString());
            var drawing2d = new GameRenderingGdiPlus(board.Width, board.Height);
            GameRenderer.RenderAndSave(drawing2d, "ContiguousRegionsTestBoard.png", board.Width, board.Height, board.Tiles, board.Edges, board.Structures, labels);

            Assert.AreEqual(2, board[12].ContiguousRegionId);
            Assert.AreEqual(7, board[32].ContiguousRegionId);

            Assert.AreEqual(3, board[17].ContiguousRegionId);
            Assert.AreEqual(3, board[27].ContiguousRegionId);
            Assert.AreEqual(3, board[37].ContiguousRegionId);

            Assert.AreNotEqual(3, board[26].ContiguousRegionId);
        }
    }
}
