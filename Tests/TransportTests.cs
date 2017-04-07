using GameModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
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

            var units = new List<MilitaryUnit>
            {
                new MilitaryUnit(location: board[20, 5], movementType: MovementType.Water, baseMovementPoints: 5, isTransporter: true, strategicAction: StrategicAction.Dock),
                new MilitaryUnit(location: board[24, 16], roadMovementBonus: 1, strategicAction: StrategicAction.Embark),
                new MilitaryUnit() { Location = board[1, 1] },
                new MilitaryUnit() { Location = board[1, 1], OwnerIndex = 2 }
            };

            board.Units = units;

            for (var turn = 0; turn < 3; turn++)
            {

                var moveOrders = new List<MoveOrder>();

                units.Where(x => x.IsAlive && x.StrategicAction != StrategicAction.None)
                     .ToList()
                     .ForEach(unit =>
                     {
                         switch (unit.StrategicAction)
                         {
                             case StrategicAction.Embark:
                                 var closestPortPath = ComputerPlayer.ClosestPortPath(board, unit);
                                 unit.StrategicDestination = board[closestPortPath.Last().X, closestPortPath.Last().Y];

                                 if (unit.StrategicDestination == unit.Location)
                                 {
                                     var waterTiles = unit.StrategicDestination.Edges.Where(x => x.EdgeType == EdgeType.Port).Select(x => x.Destination).ToList();
                                     var transportingUnits = units.Where(x => x.IsAlive && x.IsTransporter && waterTiles.Contains(x.Location) && x.CanTransport(unit));
                                     var transportUnit = transportingUnits.First();
                                     moveOrders.Add(unit.PossibleMoves().Single(x => x.Destination == transportUnit.Location).GetMoveOrder(unit));
                                 }
                                 else
                                 {
                                     var moveOrder = GetMoveOrderToStrategicDestination(unit, board, units);

                                     moveOrders.Add(moveOrder);
                                 }
                                 break;
                             case StrategicAction.Dock:
                                 {
                                     if (unit.StrategicDestination == unit.Location)
                                     { }
                                     else
                                     {
                                         closestPortPath = ComputerPlayer.ClosestPortPath(board, unit);
                                         unit.StrategicDestination = board[closestPortPath.Last().X, closestPortPath.Last().Y];

                                         var moveOrder = GetMoveOrderToStrategicDestination(unit, board, units);

                                         moveOrders.Add(moveOrder);
                                     }
                                     break;
                                 }
                         }
                     });



                var vectors = new List<Vector>();
                moveOrders.ForEach(x => vectors.AddRange(x.Vectors));


                Visualise.GameBoardRenderer.RenderAndSave($"PortsTurn{turn}.png", board.Width, board.Tiles, board.Edges, board.Structures, units: units, lines: vectors);

                board.ResolveMoves(moveOrders);

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
                        Assert.AreEqual(board[21, 10], units[1].Location);
                        break;
                }
            }
        }

        private static MoveOrder GetMoveOrderToStrategicDestination(MilitaryUnit unit, Board board, List<MilitaryUnit> units)
        {
            var pathFindTiles = board.ValidMovesWithMoveCostsForUnit(unit);
            var shortestPath = ComputerPlayer.FindShortestPath(pathFindTiles, unit.Location.Point, unit.StrategicDestination.Point).ToArray();

            var move = ComputerPlayer.MoveOrderFromShortestPath(unit.PossibleMoves().ToList(), shortestPath);

            var moveOrders = move.GetMoveOrder(unit);
            return moveOrders;
        }
    }
}
