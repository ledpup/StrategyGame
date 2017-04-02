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
    public class PathFindTests
    {
        static string[] GameBoard = File.ReadAllLines("BasicBoard.txt");
        static string[] TileEdges = File.ReadAllLines("BasicBoardEdges.txt");


        [TestMethod]
        public void HasAPath()
        {
            var board = new Board(GameBoard, TileEdges);

            var unit = new MilitaryUnit() { Location = board[1, 1] };

            var pathFindTiles = board.ValidMovesWithMoveCostsForUnit(unit);
            var shortestPath = ComputerPlayer.FindShortestPath(pathFindTiles, new Point(1, 1), new Point(5, 7)).ToArray();

            Assert.AreEqual(shortestPath[0].Point, new Point(1, 1)); // Origin

            Assert.AreEqual(shortestPath[1].Point, new Point(2, 2));
            Assert.AreEqual(shortestPath[2].Point, new Point(3, 2));
            Assert.AreEqual(shortestPath[3].Point, new Point(4, 3));
            Assert.AreEqual(shortestPath[4].Point, new Point(5, 3));
            Assert.AreEqual(shortestPath[5].Point, new Point(6, 4));
            Assert.AreEqual(shortestPath[6].Point, new Point(6, 5));
            Assert.AreEqual(shortestPath[7].Point, new Point(5, 5));
            Assert.AreEqual(shortestPath[8].Point, new Point(5, 6));

            Assert.AreEqual(shortestPath[9].Point, new Point(5, 7)); // Destination
        }


    }
}
