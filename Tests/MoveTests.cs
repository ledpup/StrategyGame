using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Model;

namespace Tests
{
    [TestClass]
    public class MoveTests
    {
        [TestMethod]
        public void dkjsh()
        {       
            var tileEdges = "19,20,Road\r\n20,38,Road";

            var board = Board.LoadBoard(BoardTests.GameBoard, tileEdges);

            var move1 = new Move(null, board[1, 2]);
            var move2 = new Move(move1, board[2, 2]);

            var road = Move.IsAllOnRoad(move2);

            Assert.IsTrue(road);
        }
    }
}
