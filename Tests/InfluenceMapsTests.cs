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
    public class InfluenceMapsTests
    {
      
        static string[] GameBoard = File.ReadAllLines("BasicBoard.txt");
        static string[] Edges = File.ReadAllLines("BasicBoardEdges.txt");
        static string[] Structures = File.ReadAllLines("BasicBoardStructures.txt");

        [TestMethod]
        public void DisplayInfluenceMap()
        {
            var board = new Board(GameBoard, Edges, Structures);

            var numberOfPlayers = 2;

            board.Units = new List<MilitaryUnit>
            {
                new MilitaryUnit("1st Airborne", 1, board[114], MovementType.Airborne, 3),
                new MilitaryUnit("1st Infantry", 1, board[110], MovementType.Land, 3),

                new MilitaryUnit("1st Infantry", 2, board[111]),
                new MilitaryUnit("2nd Infantry", 2, board[111]),

                new MilitaryUnit("3rd Infantry", 2, board[168]),
            };

            board.Tiles.ToList().ForEach(x => { x.UnitCountInfluence = new double[numberOfPlayers]; x.UnitStrengthInfluence = new double[numberOfPlayers]; x.UnitCountTension = new double[numberOfPlayers]; });

            foreach (var unit in board.Units)
            {
                var playerIndex = unit.OwnerId - 1;
                unit.Tile.UnitCountInfluence[playerIndex] = 1;
                unit.Tile.UnitStrengthInfluence[playerIndex] = unit.Strength;
                var moves = unit.PossibleMoveList().GroupBy(x => x.Destination);
                foreach (var move in moves)
                {
                    var minDistance = move.Min(x => x.Distance) + 1;
                    board[move.Key.Id].UnitCountInfluence[playerIndex] += Math.Round(1D / minDistance, 1);
                    board[move.Key.Id].UnitStrengthInfluence[playerIndex] += Math.Round(unit.Strength / minDistance, 0);
                }
            }

            

            var labels = new string[board.Width, board.Height];


            for (var i = 0; i < numberOfPlayers; i++)
            {
                board.Tiles.ToList().ForEach(x => labels[x.X, x.Y] = x.UnitCountInfluence[i].ToString());
                Visualise.Integration.DrawHexagonImage("UnitCountInfluenceMapPlayer" + (i + 1) + ".png", board.Tiles, labels, null, board.Structures, board.Units);
                board.Tiles.ToList().ForEach(x => labels[x.X, x.Y] = x.UnitStrengthInfluence[i].ToString());
                Visualise.Integration.DrawHexagonImage("UnitStrengthInfluenceMapPlayer" + (i + 1) + ".png", board.Tiles, labels, null, board.Structures, board.Units);

                board.Tiles.ToList().ForEach(x => 
                {
                    x.UnitCountTension[i] = x.UnitCountInfluence[i];
                    for (var j = 0; j < numberOfPlayers; j++)
                    {
                        if (i == j)
                            continue;

                        x.UnitCountTension[i] -= x.UnitCountInfluence[j];
                    }
                });

                board.Tiles.ToList().ForEach(x => labels[x.X, x.Y] = x.UnitCountTension[i].ToString());
                Visualise.Integration.DrawHexagonImage("UnitCountTensionMapPlayer" + (i + 1) + ".png", board.Tiles, labels, null, board.Structures, board.Units);
            }

            var moveOrders = new List<MoveOrder>();

            board.Units.ForEach(x =>
            {
                var possibleMoves = x.PossibleMoveList();

                var highestTension = possibleMoves.Min(y => y.Destination.UnitCountTension[x.OwnerId - 1]);

                var bestMove = possibleMoves.Where(y => y.Destination.UnitCountTension[x.OwnerId - 1] == highestTension).First();

                var moveOrder = bestMove.GetMoveOrder();

                moveOrder.Unit = x;
                moveOrders.Add(moveOrder);

            });

            var vectors = new List<Vector>();
            moveOrders.ForEach(x => vectors.AddRange(x.Vectors));

            Visualise.Integration.DrawHexagonImage("UnitCountTensionDerivedMoveOrders.png", board.Tiles, null, vectors, board.Structures, board.Units);

            board.ResolveMoves(0, moveOrders);

            Visualise.Integration.DrawHexagonImage("UnitCountTensionDerivedMovesResolved.png", board.Tiles, null, null, board.Structures, board.Units);
        }
    }
}