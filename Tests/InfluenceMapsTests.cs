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
                new MilitaryUnit(0, "1st Airborne", 0, board[114], MovementType.Airborne, 3),
                new MilitaryUnit(1, "1st Infantry", 0, board[110], MovementType.Land, 3),

                new MilitaryUnit(2, "1st Infantry", 1, board[111]),
                new MilitaryUnit(3, "2nd Infantry", 1, board[111]),

                new MilitaryUnit(4, "3rd Infantry", 1, board[168]),
            };

            board.Units[0].TerrainTypeBattleModifier[TerrainType.Wetland] = 1;
            board.Units[1].TerrainTypeBattleModifier[TerrainType.Forest] = 1;

            board.Tiles.ToList().ForEach(x => 
            {
                x.UnitCountInfluence = new double[numberOfPlayers];
                x.UnitStrengthInfluence = new double[numberOfPlayers];
                x.AggregateInfluence = new double[numberOfPlayers];
                board.Units.ForEach(y => x.TerrainAndWeatherInfluenceByUnit.Add(y.Index, y.TerrainTypeBattleModifier[x.TerrainType] + y.WeatherBattleModifier[x.Weather]));
            });

            foreach (var unit in board.Units)
            {
                var playerIndex = unit.OwnerIndex;
                unit.Tile.UnitCountInfluence[playerIndex] += 1;
                unit.Tile.UnitStrengthInfluence[playerIndex] = unit.Strength;
                var moves = unit.PossibleMoveList().GroupBy(x => x.Destination);
                foreach (var move in moves)
                {
                    var minDistance = move.Min(x => x.Distance) + 1;
                    board[move.Key.Index].UnitCountInfluence[playerIndex] += Math.Round(1D / minDistance, 1);
                    board[move.Key.Index].UnitStrengthInfluence[playerIndex] += Math.Round(unit.Strength / minDistance, 0);
                }
            }

            var labels = new string[board.Width, board.Height];

            foreach (var structure in board.Structures)
            {
                structure.Tile.StructureInfluence += 2;
                for (var i = 1; i < 3; i++)
                {
                    var hexesInRing = Hex.HexRing(structure.Tile.Hex, i);

                    hexesInRing.ForEach(x => 
                    {
                        var index = Hex.HexToIndex(x, board.Width);
                        if (index >= 0)
                            board[index].StructureInfluence += 1D / (i + 1);
                    });
                }
            }

            board.Tiles.ToList().ForEach(x => labels[x.X, x.Y] = Math.Round(x.StructureInfluence, 1).ToString());
            Visualise.Integration.DrawHexagonImage("StructureInfluenceMap.png", board.Tiles, board.Edges, labels, null, board.Structures, board.Units);


            for (var i = 0; i < numberOfPlayers; i++)
            {
                board.Tiles.ToList().ForEach(x => labels[x.X, x.Y] = x.UnitCountInfluence[i].ToString());
                Visualise.Integration.DrawHexagonImage("UnitCountInfluenceMapPlayer" + (i + 1) + ".png", board.Tiles, board.Edges, labels, null, board.Structures, board.Units);
                board.Tiles.ToList().ForEach(x => labels[x.X, x.Y] = x.UnitStrengthInfluence[i].ToString());
                Visualise.Integration.DrawHexagonImage("UnitStrengthInfluenceMapPlayer" + (i + 1) + ".png", board.Tiles, board.Edges, labels, null, board.Structures, board.Units);

                board.Tiles.ToList().ForEach(x => 
                {
                    x.AggregateInfluence[i] = x.UnitCountInfluence[i] - x.StructureInfluence;
                    for (var j = 0; j < numberOfPlayers; j++)
                    {
                        if (i == j)
                            continue;

                        x.AggregateInfluence[i] -= x.UnitCountInfluence[j];
                    }
                });

                board.Tiles.ToList().ForEach(x => labels[x.X, x.Y] = Math.Round(x.AggregateInfluence[i], 1).ToString());
                Visualise.Integration.DrawHexagonImage("AggregateInfluenceMapPlayer" + (i + 1) + ".png", board.Tiles, board.Edges, labels, null, board.Structures, board.Units);
            }

            var moveOrders = new List<MoveOrder>();

            board.Units.ForEach(x =>
            {
                var possibleMoves = x.PossibleMoveList();

                var highestTension = possibleMoves.Min(y => y.Destination.AggregateInfluence[x.OwnerIndex]);

                if (x.Tile.AggregateInfluence[x.OwnerIndex] > highestTension)
                {
                    var moves = possibleMoves.Where(y => y.Destination.AggregateInfluence[x.OwnerIndex] == highestTension);

                    //var minDistance = moves.Min(y => y.Distance);

                    //var bestTerrain = moves.Max(y => y.TerrainAndWeatherModifers(x.Index));

                    var bestMove = moves.OrderByDescending(y => y.TerrainAndWeatherModifers(x.Index)).ThenBy(y => y.Distance).First();

                    var moveOrder = bestMove.GetMoveOrder();

                    moveOrder.Unit = x;
                    moveOrders.Add(moveOrder);
                }
            });

            var vectors = new List<Vector>();
            moveOrders.ForEach(x => vectors.AddRange(x.Vectors));

            Visualise.Integration.DrawHexagonImage("AggregateInfluenceMoveOrders.png", board.Tiles, board.Edges, null, vectors, board.Structures, board.Units);

            board.ResolveMoves(0, moveOrders);

            Visualise.Integration.DrawHexagonImage("AggregateInfluenceMovesResolved.png", board.Tiles, board.Edges, null, null, board.Structures, board.Units);
        }
    }
}
