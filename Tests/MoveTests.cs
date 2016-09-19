﻿using System;
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

            var units = new List<MilitaryUnit> { new MilitaryUnit(tile: board[335]) };

            var moves = units[0].PossibleMoves();

            moves.ToList().ForEach(x => x.Destination.IsSelected = true);

            Visualise.Integration.DrawHexagonImage("BasicBoardWithUnitMoves.png", board.Tiles, board.Edges, board.Structures, null, null, units);

            Assert.IsTrue(moves.Any(x => x.Destination.Index == 334));
            Assert.IsTrue(moves.Any(x => x.Destination.Index == 361));
            Assert.IsTrue(moves.Any(x => x.Destination.Index == 336));
            Assert.IsTrue(moves.Any(x => x.Destination.Index == 309));
            Assert.IsTrue(moves.Any(x => x.Destination.Index == 310));
            Assert.IsTrue(moves.Any(x => x.Destination.Index == 308));
        }

        [TestMethod]
        public void InfantryValidMoveListWithRoad()
        {
            var board = new Board(BoardTests.GameBoard, BoardTests.TileEdges);

            var units = new List<MilitaryUnit> { new MilitaryUnit(tile: board[345], roadMovementBonus: 2) };

            var moves = units[0].PossibleMoves();

            moves.ToList().ForEach(x => x.Destination.IsSelected = true);

            Visualise.Integration.DrawHexagonImage("BasicBoardWithUnitMovesOverRoad.png", board.Tiles, board.Edges, board.Structures, null, null, units);

            Assert.IsTrue(moves.Any(x => x.Destination.Index == 316));
            Assert.IsTrue(moves.Any(x => x.Destination.Index == 317));
            Assert.IsTrue(moves.Any(x => x.Destination.Index == 343));
            Assert.IsTrue(moves.Any(x => x.Destination.Index == 344));
            Assert.IsTrue(moves.Any(x => x.Destination.Index == 318));
            Assert.IsTrue(moves.Any(x => x.Destination.Index == 373));
            Assert.IsTrue(moves.Any(x => x.Destination.Index == 347));
            Assert.IsTrue(moves.Any(x => x.Destination.Index == 374));
            Assert.IsTrue(moves.Any(x => x.Destination.Index == 402));
            Assert.IsTrue(moves.Any(x => x.Destination.Index == 429));

            Assert.IsFalse(moves.Any(x => x.Destination.Index == 346));
            Assert.IsFalse(moves.Any(x => x.Destination.Index == 371));
            Assert.IsFalse(moves.Any(x => x.Destination.Index == 372));
            Assert.IsFalse(moves.Any(x => x.Destination.Index == 400));
        }

        [TestMethod]
        public void InfantryValidMoveListWithRoadOverMountain()
        {
            var board = new Board(BoardTests.GameBoard, BoardTests.TileEdges);

            var units = new List<MilitaryUnit> { new MilitaryUnit(0, "1st Infantry", 1, board[85], MovementType.Land, 2) { RoadMovementBonus = 1 } };

            var moves = units[0].PossibleMoves();

            moves.ToList().ForEach(x => x.Destination.IsSelected = true);

            Visualise.Integration.DrawHexagonImage("BasicBoardWithUnitMovesOverRoadOverMountain.png", board.Tiles, board.Edges, board.Structures, null, null, units);

            Assert.IsTrue(moves.Any(x => x.Destination.Index == 30));
            Assert.IsTrue(moves.Any(x => x.Destination.Index == 56));
            Assert.IsTrue(moves.Any(x => x.Destination.Index == 57));
            Assert.IsTrue(moves.Any(x => x.Destination.Index == 59));
            Assert.IsTrue(moves.Any(x => x.Destination.Index == 86));
            Assert.IsTrue(moves.Any(x => x.Destination.Index == 87));
            Assert.IsTrue(moves.Any(x => x.Destination.Index == 114));

            Assert.IsFalse(moves.Any(x => x.Destination.Index == 58));
            Assert.IsFalse(moves.Any(x => x.Destination.Index == 32));
            Assert.IsFalse(moves.Any(x => x.Destination.Index == 60));
            Assert.IsFalse(moves.Any(x => x.Destination.Index == 83));
            Assert.IsFalse(moves.Any(x => x.Destination.Index == 84));
            Assert.IsFalse(moves.Any(x => x.Destination.Index == 112));
            Assert.IsFalse(moves.Any(x => x.Destination.Index == 113));
        }

        [TestMethod]
        public void AirborneValidMoveListWithRoadAndMountain()
        {
            var board = new Board(BoardTests.GameBoard, BoardTests.TileEdges);

            var units = new List<MilitaryUnit> { new MilitaryUnit(0, "1st Infantry", 1, board[85], MovementType.Airborne, 2) };

            var moves = units[0].PossibleMoves();

            moves.ToList().ForEach(x => x.Destination.IsSelected = true);

            Visualise.Integration.DrawHexagonImage("BasicBoardWithAirborneUnitMovesWithRoadAndMountain.png", board.Tiles, board.Edges, board.Structures, null, null, units);

            Assert.IsTrue(moves.Any(x => x.Destination.Index == 30));
            Assert.IsTrue(moves.Any(x => x.Destination.Index == 31));
            Assert.IsTrue(moves.Any(x => x.Destination.Index == 32));
            Assert.IsTrue(moves.Any(x => x.Destination.Index == 56));
            Assert.IsTrue(moves.Any(x => x.Destination.Index == 57));
            Assert.IsTrue(moves.Any(x => x.Destination.Index == 59));
            Assert.IsTrue(moves.Any(x => x.Destination.Index == 60));
            Assert.IsTrue(moves.Any(x => x.Destination.Index == 87));
            Assert.IsTrue(moves.Any(x => x.Destination.Index == 110));
            Assert.IsTrue(moves.Any(x => x.Destination.Index == 111));
            Assert.IsTrue(moves.Any(x => x.Destination.Index == 114));

            Assert.IsFalse(moves.Any(x => x.Destination.Index == 58));
            Assert.IsFalse(moves.Any(x => x.Destination.Index == 83));
            Assert.IsFalse(moves.Any(x => x.Destination.Index == 84));
            Assert.IsFalse(moves.Any(x => x.Destination.Index == 86));
            Assert.IsFalse(moves.Any(x => x.Destination.Index == 112));
            Assert.IsFalse(moves.Any(x => x.Destination.Index == 113));
        }

        [TestMethod]
        public void AirborneValidMoveList()
        {
            var board = new Board(BoardTests.GameBoard, BoardTests.TileEdges);

            var units = new List<MilitaryUnit> { new MilitaryUnit(0, "1st Airborne", 1, board[364], MovementType.Airborne, 3) };

            var moves = units[0].PossibleMoves();

            moves.ToList().ForEach(x => x.Destination.IsSelected = true);

            Visualise.Integration.DrawHexagonImage("BasicBoardWithAirborneUnitMoves.png", board.Tiles, board.Edges, board.Structures, null, null, units);

            //Assert.AreEqual(12, moves.Count());
            Assert.IsTrue(moves.Any(x => x.Destination.Index == 334));
            Assert.IsTrue(moves.Any(x => x.Destination.Index == 308));
            Assert.IsTrue(moves.Any(x => x.Destination.Index == 309));
            Assert.IsTrue(moves.Any(x => x.Destination.Index == 361));
            Assert.IsTrue(moves.Any(x => x.Destination.Index == 335));
            Assert.IsTrue(moves.Any(x => x.Destination.Index == 336));
            Assert.IsTrue(moves.Any(x => x.Destination.Index == 310));
            Assert.IsTrue(moves.Any(x => x.Destination.Index == 311));
            Assert.IsTrue(moves.Any(x => x.Destination.Index == 338));
            Assert.IsTrue(moves.Any(x => x.Destination.Index == 312));
            Assert.IsTrue(moves.Any(x => x.Destination.Index == 389));
            Assert.IsTrue(moves.Any(x => x.Destination.Index == 390));
            Assert.IsTrue(moves.Any(x => x.Destination.Index == 365));
            Assert.IsTrue(moves.Any(x => x.Destination.Index == 339));
            Assert.IsTrue(moves.Any(x => x.Destination.Index == 340));
            Assert.IsTrue(moves.Any(x => x.Destination.Index == 417));
            Assert.IsTrue(moves.Any(x => x.Destination.Index == 366));
            Assert.IsTrue(moves.Any(x => x.Destination.Index == 367));
            Assert.IsTrue(moves.Any(x => x.Destination.Index == 418));
            Assert.IsTrue(moves.Any(x => x.Destination.Index == 419));
            Assert.IsTrue(moves.Any(x => x.Destination.Index == 393));
            Assert.IsTrue(moves.Any(x => x.Destination.Index == 394));
            Assert.IsTrue(moves.Any(x => x.Destination.Index == 445));
            Assert.IsTrue(moves.Any(x => x.Destination.Index == 446));
            Assert.IsTrue(moves.Any(x => x.Destination.Index == 420));
            Assert.IsTrue(moves.Any(x => x.Destination.Index == 421));

            Assert.IsFalse(moves.Any(x => x.Destination.Index == 388));
            Assert.IsFalse(moves.Any(x => x.Destination.Index == 362));
            Assert.IsFalse(moves.Any(x => x.Destination.Index == 337));
            Assert.IsFalse(moves.Any(x => x.Destination.Index == 391));
            Assert.IsFalse(moves.Any(x => x.Destination.Index == 392));
            Assert.IsFalse(moves.Any(x => x.Destination.Index == 416));
            Assert.IsFalse(moves.Any(x => x.Destination.Index == 444));
        }

        [TestMethod]
        public void BasicBoardWithLandUnitNearRiverAndRoad()
        {
            var board = new Board(BoardTests.GameBoard, BoardTests.TileEdges);

            var units = new List<MilitaryUnit> { new MilitaryUnit(0, "1st Infantry", 1, board[1, 1]) };

            var moves = units[0].PossibleMoves();

            moves.ToList().ForEach(x => x.Destination.IsSelected = true);

            Visualise.Integration.DrawHexagonImage("BasicBoardWithLandUnitNearBrdigeAndRoad.png", board.Tiles, board.Edges, board.Structures, null, null, units);

            Assert.IsTrue(moves.Any(x => x.Destination == board[1, 2]));
            Assert.IsTrue(moves.Any(x => x.Destination == board[2, 2]));
            Assert.IsTrue(moves.Any(x => x.Destination == board[2, 1]));
            Assert.IsTrue(moves.Any(x => x.Destination.Index == 30));

            Assert.IsFalse(moves.Any(x => x.Destination == board[1, 3])); // Can't cross river
        }

        [TestMethod]
        public void BasicBoardWithLandUnitNearBridgeAndRoad()
        {
            var board = new Board(BoardTests.GameBoard, BoardTests.TileEdges);

            var units = new List<MilitaryUnit> { new MilitaryUnit(0, "1st Infantry", 1, board[141]) };

            var moves = units[0].PossibleMoves();

            moves.ToList().ForEach(x => x.Destination.IsSelected = true);

            Visualise.Integration.DrawHexagonImage("BasicBoardWithLandUnitNearBridgeAndRoad.png", board.Tiles, board.Edges, board.Structures, null, null, units);

            Assert.IsTrue(moves.Any(x => x.Destination == board[114]));
            Assert.IsTrue(moves.Any(x => x.Destination == board[115]));
            Assert.IsTrue(moves.Any(x => x.Destination == board[140]));
            Assert.IsTrue(moves.Any(x => x.Destination == board[168]));
            Assert.IsTrue(moves.Any(x => x.Destination == board[169]));
            Assert.IsTrue(moves.Any(x => x.Destination == board[87]));
            Assert.IsTrue(moves.Any(x => x.Destination == board[88]));
        }

        [TestMethod]
        public void BasicBoardWithAmphibiousUnitNearRiverAndRoad()
        {
            var board = new Board(BoardTests.GameBoard, BoardTests.TileEdges);

            var units = new List<MilitaryUnit> { new MilitaryUnit(0, "1st Amphibious", 1, board[1, 1], MovementType.Amphibious) };

            var moves = units[0].PossibleMoves();

            moves.ToList().ForEach(x => x.Destination.IsSelected = true);

            Visualise.Integration.DrawHexagonImage("BasicBoardWithAmphibiousUnitNearRiverAndRoad.png", board.Tiles, board.Edges, board.Structures, null, null, units);

            Assert.IsTrue(moves.Any(x => x.Destination == board[1, 2]));
            Assert.IsTrue(moves.Any(x => x.Destination == board[2, 2]));
            Assert.IsTrue(moves.Any(x => x.Destination == board[2, 1]));
            Assert.IsTrue(moves.Any(x => x.Destination.Index == 30));
            Assert.IsTrue(moves.Any(x => x.Destination == board[1, 3]));
        }

        [TestMethod]
        public void BasicBoardWithAquaticUnit()
        {
            var board = new Board(BoardTests.GameBoard, BoardTests.TileEdges);

            var units = new List<MilitaryUnit> { new MilitaryUnit(0, "1st Fleet", 2, board[225], MovementType.Water, 3) };

            var moves = units[0].PossibleMoves();

            moves.ToList().ForEach(x => x.Destination.IsSelected = true);

            Visualise.Integration.DrawHexagonImage("BasicBoardWithAquaticUnit.png", board.Tiles, board.Edges, board.Structures, null, null, units);

            Assert.IsTrue(moves.Any(x => x.Destination.Index == 198));
            Assert.IsTrue(moves.Any(x => x.Destination.Index == 226));
            Assert.IsTrue(moves.Any(x => x.Destination.Index == 253));
            Assert.IsTrue(moves.Any(x => x.Destination.Index == 252));
            Assert.IsTrue(moves.Any(x => x.Destination.Index == 251));
            Assert.IsTrue(moves.Any(x => x.Destination.Index == 224));

            Assert.IsFalse(moves.Any(x => x.Destination.Index == 196));
            Assert.IsFalse(moves.Any(x => x.Destination.Index == 199));
        }

        [TestMethod]
        public void ResolveMove()
        {
            var board = new Board(BoardTests.GameBoard, BoardTests.TileEdges);

            board.Units = new List<MilitaryUnit>
            { 
                new MilitaryUnit() { Tile = board[1, 1], BaseMovementPoints = 5 },
                new MilitaryUnit() { Tile = board[1, 1] },
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

            board.ResolveMoves(moveOrders);

            Assert.AreEqual(board[3, 2], board.Units[0].Tile);
            Assert.AreEqual(board[2, 2], board.Units[1].Tile);
        }

        [TestMethod]
        public void ResolveMove_ConflictArrises_ConflictedUnitsStop()
        {
            var board = new Board(BoardTests.GameBoard, BoardTests.TileEdges);
            board.Units = new List<MilitaryUnit>
            {
                new MilitaryUnit(0, "1st Infantry", 1, board[1, 1]),
                new MilitaryUnit(1, "2nd Infantry", 2, board[4, 1]),

                new MilitaryUnit(2, "1st Infantry", 1, board[10, 2]) { BaseMovementPoints = 6 },
                new MilitaryUnit(3, "2nd Infantry", 2, board[10, 3]),
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

            Visualise.Integration.DrawHexagonImage("BasicBoardWithUnitsPreMove.png", board.Tiles, board.Edges, board.Structures, null, vectors, board.Units);

            board.ResolveMoves(moveOrders);

            Visualise.Integration.DrawHexagonImage("BasicBoardWithUnitsPostMove.png", board.Tiles, board.Edges, board.Structures, null, null, board.Units);

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
                            new MilitaryUnit() { OwnerIndex = 2, Tile = tile1, },
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

