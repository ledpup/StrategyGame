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
                new MilitaryUnit("1st Infantry", 1, board[1, 1]) { BaseMovementPoints = 4 },
                new MilitaryUnit("2nd Infantry", 2, board[2, 3]),
            };

            var moveOrders = new List<MoveOrder>
            {
                new MoveOrder
                {
                    Moves = new Move[]
                    {
                        new Move(board[1, 1], board[1, 2]),
                        new Move(board[1, 2], board[2, 2]),
                        new Move(board[2, 2], board[3, 2]),
                    },
                    Unit = board.Units[0] }
                ,
                new MoveOrder
                {
                    Moves = new Move[]
                    {
                        new Move(board[2, 3], board[2, 2]),
                        new Move(board[2, 2], board[2, 1]),
                    },
                    Unit = board.Units[1]
                },
            };

            board.ResolveMoves(0, moveOrders);

            var battles = board.ConductBattles();

            Assert.AreEqual(1, battles.Count());
        }
    }
}
