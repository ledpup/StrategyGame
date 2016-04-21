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
    public class MoveTests
    {
        [TestMethod]
        public void InfantryValidMoveList()
        {
            var board = new Board(BoardTests.GameBoard, BoardTests.TileEdges);

            var units = new List<MilitaryUnit> { new MilitaryUnit("1st Infantry", 1, board[335], MovementType.Land, 2) };

            var moves = units[0].GetPossibleMoveList();

            moves.ToList().ForEach(x => x.Destination.IsSelected = true);

            Visualise.Integration.DrawHexagonImage("BasicBoardWithUnitMoves.png", board.Tiles, null, null, board.Structures, units);

            Assert.IsTrue(moves.Any(x => x.Destination.Id == 334));
            Assert.IsTrue(moves.Any(x => x.Destination.Id == 361));
            Assert.IsTrue(moves.Any(x => x.Destination.Id == 336));
            Assert.IsTrue(moves.Any(x => x.Destination.Id == 309));
            Assert.IsTrue(moves.Any(x => x.Destination.Id == 310));
            Assert.IsTrue(moves.Any(x => x.Destination.Id == 308));
        }

        [TestMethod]
        public void InfantryValidMoveListWithRoad()
        {
            var board = new Board(BoardTests.GameBoard, BoardTests.TileEdges);

            var units = new List<MilitaryUnit> { new MilitaryUnit("1st Infantry", 1, board[345], MovementType.Land, 2) };

            var moves = units[0].GetPossibleMoveList();

            moves.ToList().ForEach(x => x.Destination.IsSelected = true);

            Visualise.Integration.DrawHexagonImage("BasicBoardWithUnitMovesOverRoad.png", board.Tiles, null, null, board.Structures, units);

            Assert.IsTrue(moves.Any(x => x.Destination.Id == 316));
            Assert.IsTrue(moves.Any(x => x.Destination.Id == 317));
            Assert.IsTrue(moves.Any(x => x.Destination.Id == 343));
            Assert.IsTrue(moves.Any(x => x.Destination.Id == 344));
            Assert.IsTrue(moves.Any(x => x.Destination.Id == 318));
            Assert.IsTrue(moves.Any(x => x.Destination.Id == 373));
            Assert.IsTrue(moves.Any(x => x.Destination.Id == 347));
            Assert.IsTrue(moves.Any(x => x.Destination.Id == 374));
            Assert.IsTrue(moves.Any(x => x.Destination.Id == 402));

            Assert.IsFalse(moves.Any(x => x.Destination.Id == 346));
            Assert.IsFalse(moves.Any(x => x.Destination.Id == 371));
            Assert.IsFalse(moves.Any(x => x.Destination.Id == 372));
            Assert.IsFalse(moves.Any(x => x.Destination.Id == 400));
        }

        [TestMethod]
        public void AirborneValidMoveList()
        {
            var board = new Board(BoardTests.GameBoard, BoardTests.TileEdges);

            var units = new List<MilitaryUnit> { new MilitaryUnit("1st Airborne", 1, board[364], MovementType.Airborne, 3) };

            var moves = units[0].GetPossibleMoveList();

            moves.ToList().ForEach(x => x.Destination.IsSelected = true);

            Visualise.Integration.DrawHexagonImage("BasicBoardWithAirborneUnitMoves.png", board.Tiles, null, null, board.Structures, units);

            //Assert.AreEqual(12, moves.Count());
            Assert.IsTrue(moves.Any(x => x.Destination.Id == 334));
            Assert.IsTrue(moves.Any(x => x.Destination.Id == 308));
            Assert.IsTrue(moves.Any(x => x.Destination.Id == 309));
            Assert.IsTrue(moves.Any(x => x.Destination.Id == 361));
            Assert.IsTrue(moves.Any(x => x.Destination.Id == 335));
            Assert.IsTrue(moves.Any(x => x.Destination.Id == 336));
            Assert.IsTrue(moves.Any(x => x.Destination.Id == 310));
            Assert.IsTrue(moves.Any(x => x.Destination.Id == 311));
            Assert.IsTrue(moves.Any(x => x.Destination.Id == 338));
            Assert.IsTrue(moves.Any(x => x.Destination.Id == 312));
            Assert.IsTrue(moves.Any(x => x.Destination.Id == 389));
            Assert.IsTrue(moves.Any(x => x.Destination.Id == 390));
            Assert.IsTrue(moves.Any(x => x.Destination.Id == 365));
            Assert.IsTrue(moves.Any(x => x.Destination.Id == 339));
            Assert.IsTrue(moves.Any(x => x.Destination.Id == 340));
            Assert.IsTrue(moves.Any(x => x.Destination.Id == 417));
            Assert.IsTrue(moves.Any(x => x.Destination.Id == 366));
            Assert.IsTrue(moves.Any(x => x.Destination.Id == 367));
            Assert.IsTrue(moves.Any(x => x.Destination.Id == 418));
            Assert.IsTrue(moves.Any(x => x.Destination.Id == 419));
            Assert.IsTrue(moves.Any(x => x.Destination.Id == 393));
            Assert.IsTrue(moves.Any(x => x.Destination.Id == 394));
            Assert.IsTrue(moves.Any(x => x.Destination.Id == 445));
            Assert.IsTrue(moves.Any(x => x.Destination.Id == 446));
            Assert.IsTrue(moves.Any(x => x.Destination.Id == 420));
            Assert.IsTrue(moves.Any(x => x.Destination.Id == 421));

            Assert.IsFalse(moves.Any(x => x.Destination.Id == 388));
            Assert.IsFalse(moves.Any(x => x.Destination.Id == 362));
            Assert.IsFalse(moves.Any(x => x.Destination.Id == 337));
            Assert.IsFalse(moves.Any(x => x.Destination.Id == 391));
            Assert.IsFalse(moves.Any(x => x.Destination.Id == 392));
            Assert.IsFalse(moves.Any(x => x.Destination.Id == 416));
            Assert.IsFalse(moves.Any(x => x.Destination.Id == 444));
        }

        [TestMethod]
        public void Moves()
        {
            var board = new Board(BoardTests.GameBoard, BoardTests.TileEdges);

            var moves = new Move[]
            {
                new Move(board[1, 1], board[1, 2]),
                new Move(board[1, 2], board[2, 2]),
                new Move(board[2, 2], board[3, 2]),
            };

            Assert.AreEqual(board[55], moves[0].Destination);
            Assert.AreEqual(board[56], moves[1].Destination);
            Assert.AreEqual(board[57], moves[2].Destination);
        }

        [TestMethod]
        public void ResolveMove()
        {
            var board = new Board(BoardTests.GameBoard, BoardTests.TileEdges);

            board.Units = new List<MilitaryUnit>
            { 
                new MilitaryUnit() { BaseMovementPoints = 5 },
                new MilitaryUnit(),
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
                        new Move(board[1, 1], board[1, 2]),
                        new Move(board[1, 2], board[2, 2]),
                    },
                    Unit = board.Units[1]
                },
            };

            board.ResolveMoves(0, moveOrders);

            Assert.AreEqual(board[3, 2], board.Units[0].Tile);
            Assert.AreEqual(board[2, 2], board.Units[1].Tile);
        }

        [TestMethod]
        public void ResolveMove_ConflictArrises_ConflictedUnitsStop()
        {
            var board = new Board(BoardTests.GameBoard, BoardTests.TileEdges);
            board.Units = new List<MilitaryUnit>
            {
                new MilitaryUnit("1st Infantry", 1, board[1, 1]),
                new MilitaryUnit("2nd Infantry", 2, board[4, 1]),

                new MilitaryUnit("1st Infantry", 1, board[10, 2]) { BaseMovementPoints = 6 },
                new MilitaryUnit("2nd Infantry", 2, board[10, 3]),
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
                        new Move(board[4, 1], board[3, 1]),
                        new Move(board[3, 1], board[2, 2]),
                        new Move(board[2, 2], board[2, 1]),
                    },
                    Unit = board.Units[1]
                },

                new MoveOrder
                {
                    Moves = new Move[]
                    {
                        new Move(board[10, 2], board[11, 2]),
                        new Move(board[11, 2], board[12, 2]),
                    },
                    Unit = board.Units[2]
                },
                new MoveOrder
                {
                    Moves = new Move[]
                    {
                        new Move(board[10, 3], board[11, 2]),
                        new Move(board[11, 2], board[11, 1]),
                    },
                    Unit = board.Units[3]
                },
            };

            var vectors = new List<Vector>();
            moveOrders.ForEach(x => vectors.AddRange(x.Vectors));

            Visualise.Integration.DrawHexagonImage("BasicBoardWithUnitsPreMove.png", board.Tiles, null, vectors, board.Structures, board.Units);

            board.ResolveMoves(0, moveOrders);

            Visualise.Integration.DrawHexagonImage("BasicBoardWithUnitsPostMove.png", board.Tiles, null, null, board.Structures, board.Units);

            Assert.AreEqual(board[2, 2], board.Units[0].Tile);
            Assert.AreEqual(board[2, 2], board.Units[1].Tile);

            Assert.AreEqual(board[12, 2], board.Units[2].Tile);
            Assert.AreEqual(board[11, 1], board.Units[3].Tile);
        }

        [TestMethod]
        public void ConflictTest()
        {
            var tile1 = new Tile(1, 1, 1);
            var tile2 = new Tile(2, 1, 2);

            var units = new List<MilitaryUnit> 
            { 
                            new MilitaryUnit() { Tile = tile1, }, 
                            new MilitaryUnit() { OwnerId = 2, Tile = tile1, },
                            new MilitaryUnit() { Tile = tile2, },
            };

            var movingUnits = new List<MilitaryUnit> 
            {
                units[0],
                units[2],
            };

            var conflictedUnits = Board.DetectConflictedUnits(movingUnits, units);

            Assert.AreEqual(1, conflictedUnits.Count());
        }
    }
}
