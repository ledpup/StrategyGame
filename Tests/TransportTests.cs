using ComputerOpponent;
using GameModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Visualise;

namespace Tests
{
    [TestClass]
    public class TransportTests
    {
        public static string[] GameBoard = File.ReadAllLines("BasicBoard.txt");
        public static string[] TileEdges = File.ReadAllLines("BasicBoardEdges.txt");
        static string[] Structures = File.ReadAllLines("BasicBoardStructures.txt");


        [TestMethod]
        public void Ports()
        {
            var board = new Board(GameBoard, TileEdges, Structures);
            var numberOfPlayers = 2;
            var labels = new string[board.Width, board.Height];

            var units = new List<MilitaryUnit>
            {
                new(new UnitTemplate { MovementType = MovementType.Water, MovementPoints = 5, IsTransporter = true }, 0, location: board[20, 5]),
                new(new UnitTemplate { MovementType = MovementType.Water, MovementPoints = 3, IsTransporter = true }, 1, location: board[3, 10]),
                new(new UnitTemplate { TransportableBy = [MovementType.Water], RoadMovementBonus = 1 }, 2, location: board[24, 16]),
                new(new UnitTemplate { TransportableBy = [MovementType.Water] }, 3, location: board[1, 1]),
                new(new UnitTemplate { TransportableBy = [MovementType.Water] }, 4, location: board[1, 1]),
            };

            units[3].TerrainMovementCosts[TerrainType.Wetland] = 1;
            units[3].EdgeMovementCosts[EdgeType.River] = 0;

            board.Units = units;

            var computerPlayer = new ComputerPlayer(new Dictionary<MilitaryUnit, Role>
            {
                [units[0]] = Role.Besieger,
                [units[1]] = Role.Besieger,
                [units[2]] = Role.Balanced,
                [units[3]] = Role.Defensive,
                [units[4]] = Role.Besieger,
            });

            for (board.Turn = 0; board.Turn < 30; board.Turn++)
            {
                computerPlayer.GenerateInfluenceMaps(board, numberOfPlayers);

                GameBoardRenderer.Render(RenderPipeline.Board, RenderPipeline.Units, board.Width, board.Height, board.Tiles, board.Edges, board.Structures, null, null, board.Units);

                // Remove any units that have been destroyed for the purposes of unit orders
                var aliveUnits = board.Units.Where(x => x.IsAlive).ToList();
                computerPlayer.SetStrategicAction(board);
                var moveOrders = computerPlayer.CreateOrders(board, aliveUnits);

                var lines = new List<Centreline>();
                moveOrders.ForEach(x => lines.AddRange(Centreline.MoveOrderToCentrelines((MoveOrder)x)));

                GameBoardRenderer.RenderAndSave($"PortsTurn{board.Turn}.png", board.Width, board.Height, board.Tiles, board.Edges, board.Structures, null, lines, board.Units);

                board.ResolveOrders(moveOrders);
                board.ChangeStructureOwners();

                switch (board.Turn)
                {
                    case 0:
                        Assert.AreEqual(board[23, 13], units[2].Location);
                        break;
                    case 1:
                        Assert.AreEqual(board[21, 11], units[2].Location);
                        break;
                    case 2:
                        Assert.AreEqual(board[21, 10], units[0].Location);
                        Assert.AreEqual(board[21, 10], units[2].Location);
                        break;
                    case 3:
                        Assert.AreEqual(board[18, 7], units[0].Location);
                        Assert.AreEqual(board[18, 7], units[2].Location);
                        break;
                    case 5:
                        Assert.AreEqual(board[17, 2], units[0].Location);
                        Assert.AreEqual(board[21, 4], units[2].Location);
                        break;
                }
            }
        }

        [TestMethod]
        public void TransportByAir()
        {
            var board = new Board(GameBoard, TileEdges, Structures);

            var units = new List<MilitaryUnit>
            {
                new(new UnitTemplate { RoadMovementBonus = 1, TransportableBy = [MovementType.Airborne] }, location: board[1, 1]),
                new(new UnitTemplate { MovementType = MovementType.Airborne, MovementPoints = 3, IsTransporter = true }, location: board[1, 1]),
            };

            board.Units = units;

            var moves = new Move[]
            {
                new(board[1, 1], board[2, 2], null, 2, 1),
                new(board[2, 2], board[3, 2], null, 1, 2),
            };

            var unitOrders = new List<IUnitOrder>
            {
                new MoveOrder(moves, units[1]),
                new TransportOrder(units[1], units[0]),

            };
            board.ResolveOrders(unitOrders);

            Assert.AreEqual(units[0], units[1].Transporting.Single());
            Assert.AreEqual(units[1], units[0].TransportedBy);

            Assert.AreEqual(board[3, 2], units[0].Location);
            Assert.AreEqual(board[3, 2], units[1].Location);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void MoveTransportedUnit()
        {
            var board = new Board(GameBoard, TileEdges, Structures);

            var units = new List<MilitaryUnit>
            {
                new(new UnitTemplate { RoadMovementBonus = 1 }, location: board[1, 1], name: "1st Dragoons"),
                new(new UnitTemplate { MovementType = MovementType.Airborne, IsTransporter = true }, location: board[1, 1]),
            };

            board.Units = units;

            var moves = new Move[]
            {
                new(board[1, 1], board[2, 2], null, 2, 1),
                new(board[2, 2], board[3, 2], null, 1, 2),
            };

            var unitOrders = new List<IUnitOrder>
            {
                new TransportOrder(units[1], units[0]),
                new MoveOrder(moves, units[0]),
            };
            board.ResolveOrders(unitOrders);
        }

        [TestMethod]
        public void UnloadUnit()
        {
            var board = new Board(GameBoard, TileEdges, Structures);

            var units = new List<MilitaryUnit>
            {
                new(new UnitTemplate { RoadMovementBonus = 1, TransportableBy = [MovementType.Airborne] }, location: board[1, 1]),
                new(new UnitTemplate { MovementType = MovementType.Airborne, MovementPoints = 3, IsTransporter = true }, location: board[1, 1]),
            };

            board.Units = units;

            var moves = new Move[]
            {
                new(board[1, 1], board[2, 2], null, 2, 1),
                new(board[2, 2], board[3, 2], null, 1, 2),
            };

            var unitOrders = new List<IUnitOrder>
            {
                new MoveOrder(moves, units[1]),
                new TransportOrder(units[1], units[0]),
            };
            board.ResolveOrders(unitOrders);
            
            Assert.AreEqual(units[0], units[1].Transporting.Single());
            Assert.AreEqual(units[1], units[0].TransportedBy);

            unitOrders =
            [
                new UnloadOrder(units[0]),
            ];
            board.ResolveOrders(unitOrders);

            Assert.AreEqual(0, units[1].Transporting.Count);
            Assert.IsNull(units[0].TransportedBy);
        }

        [TestMethod]
        public void AirborneUnitAirlift()
        {
            var board = new Board(GameBoard, TileEdges, Structures);
            var numberOfPlayers = 2;
            var labels = new string[board.Width, board.Height];

            var units = new List<MilitaryUnit>
            {
                new(new UnitTemplate { MovementType = MovementType.Airborne, MovementPoints = 4, IsTransporter = true }, 0, location: board[24, 11]),
                new(new UnitTemplate { TransportableBy = [MovementType.Airborne], RoadMovementBonus = 1 }, 1, location: board[22, 15]),
                new(new UnitTemplate { TransportableBy = [MovementType.Airborne] }, 2, location: board[1, 1]),
                new(new UnitTemplate { TransportableBy = [MovementType.Airborne] }, 3, location: board[1, 1]),
            };

            units[2].TerrainTypeBattleModifier[TerrainType.Wetland] = 1;
            units[2].EdgeMovementCosts[EdgeType.River] = 0;

            board.Units = units;

            var computerPlayer = new ComputerPlayer(new Dictionary<MilitaryUnit, Role>
            {
                [units[0]] = Role.Balanced,
                [units[1]] = Role.Balanced,
                [units[2]] = Role.Defensive,
                [units[3]] = Role.Balanced,
            });

            for (var turn = 0; turn < 25; turn++)
            {
                computerPlayer.GenerateInfluenceMaps(board, numberOfPlayers);

                GameBoardRenderer.Render(RenderPipeline.Board, RenderPipeline.Units, board.Width, board.Height, board.Tiles, board.Edges, board.Structures, null, null, board.Units);

                // Remove any units that have been destroyed for the purposes of unit orders
                units = units.Where(x => x.IsAlive).ToList();
                computerPlayer.SetStrategicAction(board);
                var unitOrders = computerPlayer.CreateOrders(board, units);

                var lines = new List<Centreline>();
                unitOrders.OfType<MoveOrder>().ToList().ForEach(x => lines.AddRange(Centreline.MoveOrderToCentrelines(x)));

                GameBoardRenderer.RenderAndSave($"AirborneUnitAirlift{turn}.png", board.Width, board.Height, board.Tiles, board.Edges, board.Structures, units: board.Units, lines: lines);

                board.ResolveOrders(unitOrders);
                board.ChangeStructureOwners();

                board.Turn++;

                switch (board.Turn)
                {
                    case 1:
                        Assert.AreEqual(429, units[0].Location.Index);
                        Assert.AreEqual(428, units[1].Location.Index);
                        break;
                    case 2:
                        Assert.AreEqual(429, units[0].Location.Index);
                        Assert.AreEqual(429, units[1].Location.Index);
                        Assert.AreEqual(units[0], units[1].TransportedBy);
                        break;
                    case 3:
                        Assert.AreEqual(399, units[0].Location.Index);
                        Assert.AreEqual(399, units[1].Location.Index);
                        Assert.AreEqual(units[0], units[1].TransportedBy);
                        break;
                    case 4:
                        Assert.AreEqual(370, units[0].Location.Index);
                        Assert.AreEqual(370, units[1].Location.Index);
                        Assert.AreEqual(units[0], units[1].TransportedBy);
                        break;
                    case 5:
                        Assert.AreEqual(340, units[0].Location.Index);
                        Assert.AreEqual(340, units[1].Location.Index);
                        Assert.AreEqual(null, units[1].TransportedBy);
                        break;
                    case 6:
                        Assert.AreEqual(311, units[0].Location.Index);
                        Assert.AreEqual(365, units[1].Location.Index);
                        Assert.AreEqual(null, units[1].TransportedBy);
                        break;
                    case 7:
                        Assert.AreEqual(338, units[0].Location.Index);
                        Assert.AreEqual(338, units[1].Location.Index);
                        Assert.AreEqual(units[0], units[1].TransportedBy);
                        break;
                    case 8:
                        Assert.AreEqual(257, units[0].Location.Index);
                        Assert.AreEqual(257, units[1].Location.Index);
                        Assert.AreEqual(null, units[1].TransportedBy);
                        break;
                    case 9:
                        Assert.AreEqual(199, units[0].Location.Index);
                        Assert.AreEqual(203, units[1].Location.Index);
                        Assert.AreEqual(null, units[1].TransportedBy);
                        break;
                    case 10:
                        Assert.AreEqual(196, units[0].Location.Index);
                        Assert.AreEqual(150, units[1].Location.Index);
                        Assert.AreEqual(null, units[1].TransportedBy);
                        Assert.AreEqual(units[0], units[3].TransportedBy);
                        break;
                    //case 11:
                    //    Assert.AreEqual(304, units[0].Location.Index);
                    //    Assert.AreEqual(438, units[1].Location.Index);
                    //    Assert.AreEqual(null, units[1].TransportedBy);
                    //    break;
                    //case 12:
                    //    Assert.AreEqual(196, units[0].Location.Index);
                    //    Assert.AreEqual(436, units[1].Location.Index);
                    //    Assert.AreEqual(null, units[1].TransportedBy);
                    //    Assert.AreEqual(196, units[3].Location.Index);
                    //    Assert.AreEqual(units[0], units[3].TransportedBy);
                    //    break;
                    //case 13:
                    //    Assert.AreEqual(172, units[0].Location.Index);
                    //    Assert.AreEqual(408, units[1].Location.Index);
                    //    Assert.AreEqual(null, units[1].TransportedBy);
                    //    Assert.AreEqual(172, units[3].Location.Index);
                    //    Assert.AreEqual(units[0], units[3].TransportedBy);
                    //    break;
                    //case 14:
                    //    Assert.AreEqual(148, units[0].Location.Index);
                    //    Assert.AreEqual(381, units[1].Location.Index);
                    //    Assert.AreEqual(null, units[1].TransportedBy);
                    //    Assert.AreEqual(148, units[3].Location.Index);
                    //    Assert.AreEqual(null, units[3].TransportedBy);
                    //    break;
                }
            }
        }
    }
}
