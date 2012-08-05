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
        public static string TileEdges = "19,20,Road\r\n20,38,Road";

        [TestMethod]
        public void IsAllOnRoad()
        {
            var board = Board.LoadBoard(BoardTests.GameBoard, TileEdges);

            var move1 = new Move(null, board[1, 2]);
            var move2 = new Move(move1, board[2, 2]);

            var road = Move.IsAllOnRoad(move2);

            Assert.IsTrue(road);
        }
        
        [TestMethod]
        public void Moves()
        {
            var board = Board.LoadBoard(BoardTests.GameBoard, TileEdges);

            var move1 = new Move(null, board[1, 2]);
            var move2 = new Move(move1, board[2, 2]);
            var move3 = new Move(move2, board[3, 2]);

            var moveList = move3.Moves();

            Assert.AreEqual(board[20], moveList[0]);
            Assert.AreEqual(board[38], moveList[1]);
            Assert.AreEqual(board[56], moveList[2]);
        }

        [TestMethod]
        public void ResolveMove()
        {
            var board = Board.LoadBoard(BoardTests.GameBoard, TileEdges);

            var init = UnitInitialValues.DefaultValues();
            init.MovementSpeed = 5;

            board.Units = new List<Unit>
            { 
                new Unit(BaseUnitType.Land, init),
                new Unit(BaseUnitType.Land),
            };

            var move1 = new Move(null, board[1, 2]);
            var move2 = new Move(move1, board[2, 2]);
            var move3 = new Move(move2, board[3, 2]);

            var moveOrders = new List<MoveOrder> 
            { 
                new MoveOrder { Moves = move3.Moves(), Unit = board.Units[0] },
                new MoveOrder { Moves = move2.Moves(), Unit = board.Units[1] },
            };

            board.ResolveMoves(0, moveOrders);

            Assert.AreEqual(board[3, 2], board.Units[0].Tile);
            Assert.AreEqual(board[2, 2], board.Units[1].Tile);
        }

        [TestMethod]
        public void ResolveMove_ConflictArrises_ConflictedUnitsStop()
        {
            var board = UnitsMoveIntoConflict();

            Assert.AreEqual(board[2, 2], board.Units[0].Tile);
            Assert.AreEqual(board[2, 2], board.Units[1].Tile);
        }

        public static Board UnitsMoveIntoConflict()
        {
            var board = Board.LoadBoard(BoardTests.GameBoard, TileEdges);

            var init1 = UnitInitialValues.DefaultValues();
            init1.TerrainCombatBonus = TerrainType.Forest;
            init1.Quantity = 400;
            init1.MovementSpeed = 5;

            board.Units = new List<Unit>
            { 
                new Unit(BaseUnitType.Land, init1) { Id = 0, Player = new Player(), Tile = board[1, 1] }, 
                new Unit(BaseUnitType.Land) { Id = 1, Player = new Player(), Tile = board[2, 3] },
            };

            var move1 = new Move(null, board[1, 2]);
            var move2 = new Move(move1, board[2, 2]);
            var move3 = new Move(move2, board[3, 2]);

            var move4 = new Move(null, board[2, 2]);
            var move5 = new Move(move4, board[2, 1]);

            var moveOrders = new List<MoveOrder> 
            { 
                new MoveOrder { Moves = move3.Moves(), Unit = board.Units[0] },
                new MoveOrder { Moves = move5.Moves(), Unit = board.Units[1] },
            };

            board.ResolveMoves(0, moveOrders);
            return board;
        }

        [TestMethod]
        public void ConflictTest()
        {
            var player = new Player();
            var tile1 = new Tile();
            
            var units = new List<Unit> 
            { 
                            new Unit(BaseUnitType.Land) { Player = player, Tile = tile1, }, 
                            new Unit(BaseUnitType.Land) { Player = new Player(), Tile = tile1, },
                            new Unit(BaseUnitType.Land) { Player = player, Tile = new Tile(), },
            };

            var movingUnits = new List<Unit> 
            {
                units[0],
                units[2],
            };

            var conflictedUnits = Board.DetectConflictedUnits(movingUnits, units);

            Assert.AreEqual(1, conflictedUnits.Count());
        }
    }
}
