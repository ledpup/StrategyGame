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
                new MilitaryUnit(location: board[21, 10], movementType: MovementType.Water),
                new MilitaryUnit(location: board[24, 16], roadMovementBonus: 1),
                new MilitaryUnit() { Location = board[1, 1] },
                new MilitaryUnit() { Location = board[1, 1], OwnerIndex = 2 }
            };

            board.Units = units;

            for (var turn = 0; turn < 2; turn++)
            {

                var pathFindTiles = board.ValidMovesWithMoveCostsForUnit(units[1]);
                var shortestPath = ComputerPlayer.FindShortestPath(pathFindTiles, units[1].Location.Point, new Point(21, 11)).ToArray();

                var move = ComputerPlayer.MoveOrderFromShortestPath(units[1].PossibleMoves().ToList(), shortestPath);

                var vectors = ComputerPlayer.PathFindTilesToVectors(shortestPath);

                var moveOrder = move.GetMoveOrder(units[1]);

                Visualise.GameBoardRenderer.RenderAndSave($"PortsTurn{turn}.png", board.Width, board.Tiles, board.Edges, board.Structures, units: units, lines: moveOrder.Vectors);

                board.ResolveMoves(new List<MoveOrder> { moveOrder });

                switch (turn)
                {
                    case 0:
                        Assert.AreEqual(board[23, 13], units[1].Location);
                        break;
                    case 1:
                        Assert.AreEqual(board[21, 11], units[1].Location);
                        break;
                }
            }
        }
    }
}
