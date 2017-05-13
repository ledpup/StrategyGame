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
                new MilitaryUnit(location: board[20, 5], movementType: MovementType.Water, baseMovementPoints: 5, isTransporter: true, role: Role.Besieger),
                new MilitaryUnit(location: board[24, 16], transportableBy: new List<MovementType> { MovementType.Water }, roadMovementBonus: 1),
                new MilitaryUnit(location: board[1, 1], transportableBy: new List<MovementType> { MovementType.Water }, role: Role.Defensive, isAmphibious: true),
                new MilitaryUnit(location: board[1, 1], transportableBy: new List<MovementType> { MovementType.Water }, role: Role.Besieger),
            };

            board.Units = units;

            for (var turn = 0; turn < 40; turn++)
            {
                ComputerPlayer.GenerateInfluenceMaps(board, numberOfPlayers);

                var bitmap = new Bitmap(1920, 1450);
                Visualise.GameBoardRenderer.Render(bitmap, Visualise.RenderPipeline.Board, Visualise.RenderPipeline.Units, board.Height, board.Tiles, board.Edges, board.Structures, null, null, board.Units);

                for (var i = 0; i < numberOfPlayers; i++)
                {
                    //MilitaryUnit.Roles.ForEach(x =>
                    //{
                    //    MilitaryUnit.MovementTypes.ForEach(z =>
                    //    {
                    //        board.Tiles.ToList().ForEach(y => labels[y.X, y.Y] = Math.Round(y.AggregateInfluence[new RoleMovementType(z, x)][i], 1).ToString());
                    //        Visualise.GameBoardRenderer.RenderLabelsAndSave($"AggregateInfluenceMap{z.ToString()}{x.ToString()}Player{i + 1}Turn{board.Turn}.png", new Bitmap(bitmap), board.Width, labels);
                    //    });
                    //});
                }

                var moveOrders = new List<IUnitOrder>();

                units.Where(x => x.IsAlive)
                     .ToList()
                     .ForEach(unit => SetStrategicAction(unit, board));

                units.Where(x => x.IsAlive)
                     .ToList()
                     .ForEach(unit =>
                     {
                         switch (unit.StrategicAction)
                         {
                             case StrategicAction.None:
                                 {
                                     var moveOrder = ComputerPlayer.FindBestMoveOrderForUnit(unit, board);
                                     if (moveOrder != null)
                                         moveOrders.Add(moveOrder);
                                     break;
                                 }
                             case StrategicAction.Embark:
                                 var closestPortPath = ComputerPlayer.ClosestPortPath(board, unit);
                                 if (closestPortPath != null)
                                 {
                                     unit.StrategicDestination = board[closestPortPath.Last().X, closestPortPath.Last().Y];

                                     if (unit.StrategicDestination == unit.Location)
                                     {
                                         var waterTiles = unit.StrategicDestination.Edges.Where(x => x.EdgeType == EdgeType.Port).Select(x => x.Destination).ToList();
                                         var transportingUnits = units.Where(x => x.IsAlive && x.IsTransporter && waterTiles.Contains(x.Location) && x.CanTransport(unit))
                                                                      .OrderByDescending(x => x.TransportSize);
                                         var transportUnit = transportingUnits.FirstOrDefault();
                                         if (transportUnit != null)
                                         {
                                             moveOrders.Add(unit.PossibleMoves().Single(x => x.Destination == transportUnit.Location).GetMoveOrder(unit));
                                             unit.StrategicAction = StrategicAction.Disembark;
                                         }
                                     }
                                     else
                                     {
                                         var moveOrder = unit.GetMoveOrderToDestination(unit.StrategicDestination.Point, board);
                                         if (moveOrder != null)
                                             moveOrders.Add(moveOrder);
                                     }
                                 }
                                 break;
                             case StrategicAction.Disembark:
                                 if (board.Structures.Any(y => unit.Location.Edges.Any(z => z.EdgeType == EdgeType.Port && z.Destination.ContiguousRegionId == y.Location.ContiguousRegionId) && y.OwnerIndex != unit.OwnerIndex))
                                 {
                                     moveOrders.Add(unit.PossibleMoves().First().GetMoveOrder(unit));
                                     unit.TransportedBy.Transporting.Remove(unit);
                                     unit.TransportedBy = null;
                                 }
                                 break;
                                 
                             case StrategicAction.Dock:
                                 {
                                     closestPortPath = ComputerPlayer.ClosestPortPath(board, unit);
                                     if (unit.StrategicDestination != unit.Location)
                                     {
                                         if (closestPortPath != null)
                                         {
                                             unit.StrategicDestination = board[closestPortPath.Last().X, closestPortPath.Last().Y];

                                             var moveOrder = unit.GetMoveOrderToDestination(unit.StrategicDestination.Point, board);
                                             if (moveOrder != null)
                                                 moveOrders.Add(moveOrder);
                                         }
                                     }
                                     break;
                                 }
                             case StrategicAction.Transport:
                                 {
                                     if (unit.StrategicDestination == unit.Location)
                                     {
                                         if (!unit.Transporting.Any())
                                         {
                                             unit.StrategicAction = StrategicAction.None;
                                             unit.StrategicDestination = null;
                                             break;
                                         }
                                     }
                                     else
                                     {
                                         // Find the closest port that has a region with one or more enemy structures
                                         closestPortPath = ComputerPlayer.ClosestPortPath(board, unit);
                                         if (closestPortPath != null)
                                         {
                                            unit.StrategicDestination = board[closestPortPath.Last().X, closestPortPath.Last().Y];

                                            var moveOrder = unit.GetMoveOrderToDestination(unit.StrategicDestination.Point, board);
                                             if (moveOrder != null)
                                                moveOrders.Add(moveOrder);
                                         }
                                     }
                                     break;
                                 }
                         }
                     });



                var vectors = new List<Vector>();
                moveOrders.ForEach(x => vectors.AddRange(((MoveOrder)x).Vectors));


                Visualise.GameBoardRenderer.RenderAndSave($"PortsTurn{turn}.png", board.Height, board.Tiles, board.Edges, board.Structures, units: units, lines: vectors);

                board.ResolveOrders(moveOrders);
                board.ChangeStructureOwners();

                board.Turn++;

                switch (turn)
                {
                    case 0:
                        Assert.AreEqual(board[21, 11], units[1].StrategicDestination);
                        Assert.AreEqual(board[23, 13], units[1].Location);
                        break;
                    case 1:
                        Assert.AreEqual(board[21, 11], units[1].StrategicDestination);
                        Assert.AreEqual(board[21, 11], units[1].Location);
                        break;
                    case 2:
                        Assert.AreEqual(board[21, 10], units[0].Location);
                        Assert.AreEqual(board[21, 10], units[1].Location);
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

            Assert.AreEqual(units[0], units[1].Transporting.Single());
            Assert.AreEqual(units[1], units[0].TransportedBy);
        }

        [TestMethod]
        public void UnloadUnit()
        {
            var board = new Board(GameBoard, TileEdges, Structures);

            var units = new List<MilitaryUnit>
            {
                new MilitaryUnit(location: board[1, 1], roadMovementBonus: 1, transportableBy: new List<MovementType> { MovementType.Airborne }),
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
                new MoveOrder(moves, units[1]),
                new TransportOrder(units[1], units[0]),
            };
            board.ResolveOrders(unitOrders);
            board.Turn++;

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

        private static StrategicAction SetStrategicAction(MilitaryUnit unit, Board board)
        {
            unit.StrategicDestination = null;
            switch (unit.MovementType)
            {
                case MovementType.Land:
                    if (unit.TransportedBy == null && unit.Role != Role.Defensive && !board.Structures.Any(x => x.Location.ContiguousRegionId == unit.Location.ContiguousRegionId && x.OwnerIndex != unit.OwnerIndex))
                    {
                        unit.StrategicAction = StrategicAction.Embark;
                    }
                    else if (unit.TransportedBy != null)
                    {
                        unit.StrategicAction = StrategicAction.Disembark;
                    }
                    else
                        unit.StrategicAction = StrategicAction.None;
                    break;
                case MovementType.Water:
                    if (!unit.Transporting.Any() && !board.Units.Any(x => x.Location.ContiguousRegionId == unit.Location.ContiguousRegionId && x.OwnerIndex != unit.OwnerIndex))
                    {
                        unit.StrategicAction = StrategicAction.Dock;
                    }
                    else if (unit.Transporting.Any())
                    {
                        unit.StrategicAction = StrategicAction.Transport;
                    }
                    break;
            }
            return unit.StrategicAction;
        }
    }
}
