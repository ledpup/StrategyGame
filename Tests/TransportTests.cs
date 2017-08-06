using GameModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                new MilitaryUnit(0, location: board[20, 5], movementType: MovementType.Water, baseMovementPoints: 5, isTransporter: true, role: Role.Besieger),
                new MilitaryUnit(1, location: board[18, 0], movementType: MovementType.Water, baseMovementPoints: 3, isTransporter: true, role: Role.Besieger),
                new MilitaryUnit(2, location: board[24, 16], transportableBy: new List<MovementType> { MovementType.Water }, roadMovementBonus: 1),
                new MilitaryUnit(3, location: board[1, 1], transportableBy: new List<MovementType> { MovementType.Water }, role: Role.Defensive, isAmphibious: true),
                new MilitaryUnit(4, location: board[1, 1], transportableBy: new List<MovementType> { MovementType.Water }, role: Role.Besieger),
            };

            board.Units = units;

            for (board.Turn = 0; board.Turn < 40; board.Turn++)
            {
                ComputerPlayer.GenerateInfluenceMaps(board, numberOfPlayers);

                var bitmap = new Bitmap(1920, 1450);
                Visualise.GameBoardRenderer.Render(bitmap, Visualise.RenderPipeline.Board, Visualise.RenderPipeline.Units, board.Height, board.Tiles, board.Edges, board.Structures, null, null, board.Units);

                // Remove any units that have been destroyed for the purposes of unit orders
                units = units.Where(x => x.IsAlive).ToList();
                ComputerPlayer.SetStrategicAction(board, units);
                var moveOrders = ComputerPlayer.CreateOrders(board, units);

                var vectors = new List<Vector>();
                moveOrders.ForEach(x => vectors.AddRange(((MoveOrder)x).Vectors));

                Visualise.GameBoardRenderer.RenderAndSave($"PortsTurn{board.Turn}.png", board.Height, board.Tiles, board.Edges, board.Structures, units: board.Units, lines: vectors);

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
                }
            }
        }

        [TestMethod]
        public void TransportByAir()
        {
            var board = new Board(GameBoard, TileEdges, Structures);

            var units = new List<MilitaryUnit>
            {
                new MilitaryUnit(location: board[1, 1], roadMovementBonus: 1, transportableBy: new List<MovementType> { MovementType.Airborne }),
                new MilitaryUnit(location: board[1, 1], movementType: MovementType.Airborne, baseMovementPoints: 3, isTransporter: true, role: Role.Defensive),
            };

            board.Units = units;

            var moves = new Move[]
            {
                new Move(board[1, 1], board[2, 2], null, 2, 1),
                new Move(board[2, 2], board[3, 2], null, 1, 2),
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
                new MilitaryUnit(location: board[1, 1], name: "1st Dragoons", roadMovementBonus: 1),
                new MilitaryUnit(location: board[1, 1], movementType: MovementType.Airborne, isTransporter: true, role: Role.Defensive),
            };

            board.Units = units;

            var moves = new Move[]
            {
                new Move(board[1, 1], board[2, 2], null, 2, 1),
                new Move(board[2, 2], board[3, 2], null, 1, 2),
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
                new MilitaryUnit(location: board[1, 1], roadMovementBonus: 1, transportableBy: new List<MovementType> { MovementType.Airborne }),
                new MilitaryUnit(location: board[1, 1], movementType: MovementType.Airborne, baseMovementPoints: 3, isTransporter: true, role: Role.Defensive),
            };

            board.Units = units;

            var moves = new Move[]
            {
                new Move(board[1, 1], board[2, 2], null, 2, 1),
                new Move(board[2, 2], board[3, 2], null, 1, 2),
            };

            var unitOrders = new List<IUnitOrder>
            {
                new MoveOrder(moves, units[1]),
                new TransportOrder(units[1], units[0]),
            };
            board.ResolveOrders(unitOrders);
            
            Assert.AreEqual(units[0], units[1].Transporting.Single());
            Assert.AreEqual(units[1], units[0].TransportedBy);

            unitOrders = new List<IUnitOrder>
            {
                new UnloadOrder(units[0]),
            };
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
                new MilitaryUnit(0, location: board[24, 11], movementType: MovementType.Airborne, baseMovementPoints: 4, isTransporter: true, role: Role.Besieger),
                new MilitaryUnit(1, location: board[22, 15], transportableBy: new List<MovementType> { MovementType.Airborne }, roadMovementBonus: 1),
                new MilitaryUnit(2, location: board[1, 1], transportableBy: new List<MovementType> { MovementType.Airborne }, role: Role.Defensive, isAmphibious: true),
                new MilitaryUnit(3, location: board[1, 1], transportableBy: new List<MovementType> { MovementType.Airborne }, role: Role.Besieger),
            };

            board.Units = units;

            for (var turn = 0; turn < 40; turn++)
            {
                ComputerPlayer.GenerateInfluenceMaps(board, numberOfPlayers);

                var bitmap = new Bitmap(1920, 1450);
                Visualise.GameBoardRenderer.Render(bitmap, Visualise.RenderPipeline.Board, Visualise.RenderPipeline.Units, board.Height, board.Tiles, board.Edges, board.Structures, null, null, board.Units);

                // Remove any units that have been destroyed for the purposes of unit orders
                units = units.Where(x => x.IsAlive).ToList();
                ComputerPlayer.SetStrategicAction(board, units);
                var unitOrders = ComputerPlayer.CreateOrders(board, units);

                var vectors = new List<Vector>();
                unitOrders.OfType<MoveOrder>().ToList().ForEach(x => vectors.AddRange(x.Vectors));

                Visualise.GameBoardRenderer.RenderAndSave($"AirborneUnitAirlift{turn}.png", board.Height, board.Tiles, board.Edges, board.Structures, units: board.Units, lines: vectors);

                board.ResolveOrders(unitOrders);
                board.ChangeStructureOwners();

                board.Turn++;
            }
        }
    }
}
