using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GameModel;

namespace Tests
{
    [TestClass]
    public class TileTests
    {
        [TestMethod]
        public void BattleTest()
        {
            var board = new Board(BoardTests.GameBoard, BoardTests.TileEdges);

            board.Units = new List<MilitaryUnit>
            {
                new MilitaryUnit(0, "1st Infantry", 0, board[1, 1]) { BaseMovementPoints = 4 },
                new MilitaryUnit(1, "2nd Infantry", 1, board[2, 3]),
            };

            var moves1 = new Move[]
                    {
                        new Move(board[1, 1], board[1, 2], null, 2, 1),
                        new Move(board[1, 2], board[2, 2], null, 1, 2),
                        new Move(board[2, 2], board[3, 2], null, 0, 3),
                    };
            var moves2 = new Move[]
                    {
                        new Move(board[2, 3], board[2, 2], null, 1, 1),
                        new Move(board[2, 2], board[2, 1], null, 0, 2),
                    };

            var moveOrders = new List<MoveOrder>
            {
                new MoveOrder(moves1, board.Units[0]),
                new MoveOrder(moves2, board.Units[1]),
            };

            board.ResolveMoves(moveOrders);

            var battles = board.ConductBattles();

            Assert.AreEqual(1, battles.Count());
        }
    }
}
