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

            tiles.ForEach(t => Assert.AreEqual(1, tiles.Count(x => x.Id == t.Id)));

            // Ensure that adjacent tiles have been populated correctly
            tiles.ForEach(t => Assert.IsFalse(t.AdjacentTiles.Any(at => at == null)));
            
            // Ensure that some tiles are coastal.
            var coastal = tiles.Count(t => t.IsCoastal);
            Assert.AreNotEqual(0, coastal);

            Assert.IsTrue(!board[9, 9].IsCoastal);
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
    }
}
