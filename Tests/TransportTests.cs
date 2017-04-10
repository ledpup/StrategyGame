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
                new MilitaryUnit(location: board[24, 16], roadMovementBonus: 1),
                new MilitaryUnit(location: board[1, 1], role: Role.Defensive, isAmphibious: true),
                new MilitaryUnit(location: board[1, 1], role: Role.Besieger),
            };

            board.Units = units;

            for (var turn = 0; turn < 30; turn++)
            {
                ComputerPlayer.GenerateInfluenceMaps(board, numberOfPlayers);

                var bitmap = new Bitmap(1920, 1450);
                Visualise.GameBoardRenderer.Render(bitmap, Visualise.RenderPipeline.Board, Visualise.RenderPipeline.Units, board.Width, board.Tiles, board.Edges, board.Structures, null, null, board.Units);

                for (var i = 0; i < numberOfPlayers; i++)
                {
                    //board.Tiles.ToList().ForEach(y => labels[y.X, y.Y] = Math.Round(y.FriendlyStructureInfluence[i], 1).ToString());
                    //Visualise.GameBoardRenderer.RenderLabelsAndSave($"FriendlyStructureInfluenceMapPlayer{(i + 1)}Turn{board.Turn}.png", new Bitmap(bitmap), board.Width, labels);

                    //board.Tiles.ToList().ForEach(y => labels[y.X, y.Y] = Math.Round(y.EnemyStructureInfluence[i], 1).ToString());
                    //Visualise.GameBoardRenderer.RenderLabelsAndSave($"EnemyStructureInfluenceMapPlayer{(i + 1)}Turn{board.Turn}.png", new Bitmap(bitmap), board.Width, labels);

                    //board.Tiles.ToList().ForEach(y => labels[y.X, y.Y] = Math.Round(y.FriendlyUnitInfluence[i], 1).ToString());
                    //Visualise.GameBoardRenderer.RenderLabelsAndSave($"FriendlyUnitInfluenceMapPlayer{i + 1}Turn{board.Turn}.png", new Bitmap(bitmap), board.Width, labels);

                    //board.Tiles.ToList().ForEach(y => labels[y.X, y.Y] = Math.Round(y.EnemyUnitInfluence[i], 1).ToString());
                    //Visualise.GameBoardRenderer.RenderLabelsAndSave($"EnemyUnitInfluenceMapPlayer{i + 1}Turn{board.Turn}.png", new Bitmap(bitmap), board.Width, labels);

                    //MilitaryUnit.Roles.ForEach(x =>
                    //{
                    //    MilitaryUnit.MovementTypes.ForEach(z =>
                    //    {
                    //        board.Tiles.ToList().ForEach(y => labels[y.X, y.Y] = Math.Round(y.AggregateInfluence[new RoleMovementType(z, x)][i], 1).ToString());
                    //        Visualise.GameBoardRenderer.RenderLabelsAndSave($"AggregateInfluenceMap{z.ToString()}{x.ToString()}Player{i + 1}Turn{board.Turn}.png", new Bitmap(bitmap), board.Width, labels);
                    //    });
                    //});
                }

                var moveOrders = new List<MoveOrder>();

                units.Where(x => x.IsAlive)
                     .ToList()
                     .ForEach(unit =>
                     {
                         if (unit.StrategicAction == StrategicAction.None)
                         {
                             SetStrategicAction(unit, board);
                         }
                     });

                units.Where(x => x.IsAlive)
                     .ToList()
                     .ForEach(unit =>
                     {
                         switch (unit.StrategicAction)
                         {
                             case StrategicAction.None:
                                 {
                                     var strategicAction = SetStrategicAction(unit, board);
                                     if (strategicAction == StrategicAction.None) // Tactical action
                                     {
                                         var moveOrder = ComputerPlayer.FindBestMoveOrderForUnit(unit);
                                         if (moveOrder != null)
                                             moveOrders.Add(moveOrder);
                                     }
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
                                         var moveOrder = GetMoveOrderToStrategicDestination(unit, board, units);
                                         if (moveOrder != null)
                                             moveOrders.Add(moveOrder);
                                     }
                                 }
                                 break;
                             case StrategicAction.Disembark:
                                 {
                                     // If near a port, disembark if the region contains an enemy structures
                                     if (unit.Location.HasPort)
                                     {
                                         if (board.Structures.Any(y => unit.Location.Edges.Any(z => z.EdgeType == EdgeType.Port && z.Destination.ContiguousRegionId == y.Location.ContiguousRegionId) && y.OwnerIndex != unit.OwnerIndex))
                                         {
                                             moveOrders.Add(unit.PossibleMoves().First().GetMoveOrder(unit));
                                             unit.StrategicAction = StrategicAction.None;
                                             unit.TransportedBy.Transporting.Remove(unit);
                                             unit.TransportedBy = null;
                                         }
                                     }
                                     break;
                                 }
                             case StrategicAction.Dock:
                                 {
                                     closestPortPath = ComputerPlayer.ClosestPortPath(board, unit);
                                     if (unit.StrategicDestination == unit.Location)
                                     {
                                        var loadUnits = true;
                                        while (loadUnits)
                                        {
                                            var transportedUnits = units.Where(x => x.MovementType != MovementType.Water && x.Location.Point == unit.Location.Point && unit.CanTransport(x)).OrderBy(x => x.TransportSize);
                                            var transportUnit = transportedUnits.FirstOrDefault();
                                            if (transportUnit == null)
                                            {
                                                loadUnits = false;
                                            }
                                            else
                                            {
                                                 unit.Transporting.Add(transportUnit);
                                                 transportUnit.TransportedBy = unit;
                                                 unit.StrategicDestination = null;
                                                 unit.StrategicAction = StrategicAction.Transport;
                                             }
                                         }
                                     }
                                     else
                                     {
                                         if (closestPortPath != null)
                                         {
                                             unit.StrategicDestination = board[closestPortPath.Last().X, closestPortPath.Last().Y];

                                             var moveOrder = GetMoveOrderToStrategicDestination(unit, board, units);
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
                                         unit.StrategicDestination = board[closestPortPath.Last().X, closestPortPath.Last().Y];

                                         var moveOrder = GetMoveOrderToStrategicDestination(unit, board, units);
                                         if (moveOrder != null)
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

        private static StrategicAction SetStrategicAction(MilitaryUnit unit, Board board)
        {
            switch (unit.MovementType)
            {
                case MovementType.Land:
                    if (!board.Structures.Any(x => x.Location.ContiguousRegionId == unit.Location.ContiguousRegionId && x.OwnerIndex != unit.OwnerIndex))
                        unit.StrategicAction = StrategicAction.Embark;
                    break;
                case MovementType.Water:
                    if (!board.Units.Any(x => x.Location.ContiguousRegionId == unit.Location.ContiguousRegionId && x.OwnerIndex != unit.OwnerIndex))
                        unit.StrategicAction = StrategicAction.Dock;
                    break;
            }
            return unit.StrategicAction;
        }

        private static MoveOrder GetMoveOrderToStrategicDestination(MilitaryUnit unit, Board board, List<MilitaryUnit> units)
        {
            var pathFindTiles = board.ValidMovesWithMoveCostsForUnit(unit);
            var shortestPath = ComputerPlayer.FindShortestPath(pathFindTiles, unit.Location.Point, unit.StrategicDestination.Point).ToArray();

            var move = ComputerPlayer.MoveOrderFromShortestPath(unit.PossibleMoves().ToList(), shortestPath);

            if (move == null)
                return null;


            var moveOrders = move.GetMoveOrder(unit);
            return moveOrders;
        }
    }
}
