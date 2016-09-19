using GameModel;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrategyGame
{


    class Program
    {
        static string[] GameBoard = File.ReadAllLines("BasicBoard.txt");
        static string[] Edges = File.ReadAllLines("BasicBoardEdges.txt");
        static string[] Structures = File.ReadAllLines("BasicBoardStructures.txt");

        static void Main(string[] args)
        {
            File.Delete("logfile.txt");

            var board = new Board(GameBoard, Edges, Structures);

            var numberOfPlayers = 2;

            board.Units = new List<MilitaryUnit>
                {
                new MilitaryUnit(0, tile: board[114], movementType: MovementType.Airborne, baseMovementPoints: 3),
                new MilitaryUnit(1, tile: board[110], baseMovementPoints: 3),
                new MilitaryUnit(2, tile: board[31]),
                new MilitaryUnit(3, tile: board[56], movementType: MovementType.Amphibious),
                new MilitaryUnit(4, tile: board[65]),

                new MilitaryUnit(5, ownerIndex: 1, tile: board[361], movementType: MovementType.Airborne, baseMovementPoints: 3),
                new MilitaryUnit(6, ownerIndex: 1, tile: board[111]),
                new MilitaryUnit(7, ownerIndex: 1, tile: board[111]),

                new MilitaryUnit(8, ownerIndex: 1, tile: board[168]),
                };

            board.Units[0].TerrainTypeBattleModifier[TerrainType.Wetland] = 1;
            board.Units[1].TerrainTypeBattleModifier[TerrainType.Forest] = 1;

            var winConditionsMet = false;

            var log = LogManager.GetCurrentClassLogger();

            while (!winConditionsMet)
            {

                Board.GenerateInfluenceMaps(board, numberOfPlayers);

                var labels = new string[board.Width, board.Height];

                board.Tiles.ToList().ForEach(x => labels[x.X, x.Y] = Math.Round(x.StructureInfluence, 1).ToString());
                Visualise.Integration.DrawHexagonImage("StructureInfluenceMap.png", board.Tiles, board.Edges, board.Structures, labels, null, board.Units);


                for (var i = 0; i < numberOfPlayers; i++)
                {
                    board.Tiles.ToList().ForEach(x => labels[x.X, x.Y] = x.UnitCountInfluence[i].ToString());
                    Visualise.Integration.DrawHexagonImage("UnitCountInfluenceMapPlayer" + (i + 1) + "Turn" + board.Turn + ".png", board.Tiles, board.Edges, board.Structures, labels, null, board.Units);
                    board.Tiles.ToList().ForEach(x => labels[x.X, x.Y] = x.UnitStrengthInfluence[i].ToString());
                    Visualise.Integration.DrawHexagonImage("UnitStrengthInfluenceMapPlayer" + (i + 1) + "Turn" + board.Turn + ".png", board.Tiles, board.Edges, board.Structures, labels, null, board.Units);

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
                    Visualise.Integration.DrawHexagonImage("AggregateInfluenceMapPlayer" + (i + 1) + "Turn" + board.Turn + ".png", board.Tiles, board.Edges, board.Structures, labels, null, board.Units);
                }

                var moveOrders = new List<MoveOrder>();

                board.Units.Where(x => x.IsAlive).ToList().ForEach(x =>
                {
                    var possibleMoves = x.PossibleMoves();

                    var highestTension = possibleMoves.Min(y => y.Destination.AggregateInfluence[x.OwnerIndex]);

                    if (x.Tile.AggregateInfluence[x.OwnerIndex] > highestTension)
                    {
                        var moves = possibleMoves.Where(y => y.Destination.AggregateInfluence[x.OwnerIndex] == highestTension);

                        var bestMove = moves.OrderByDescending(y => y.TerrainAndWeatherModifers(x.Index)).ThenBy(y => y.Distance).First();

                        var moveOrder = bestMove.GetMoveOrder();

                        moveOrder.Unit = x;
                        moveOrders.Add(moveOrder);
                    }
                });

                var vectors = new List<Vector>();
                moveOrders.ForEach(x => vectors.AddRange(x.Vectors));

                Visualise.Integration.DrawHexagonImage("MoveOrdersTurn" + board.Turn + ".png", board.Tiles, board.Edges, board.Structures, null, vectors, board.Units);

                board.ResolveMoves(moveOrders);

                Visualise.Integration.DrawHexagonImage("MovesResolvedTurn" + board.Turn + ".png", board.Tiles, board.Edges, board.Structures, null, null, board.Units);

                var battleReports = board.ConductBattles();
                battleReports.ForEach(x =>
                {
                    var structure = board.Structures.SingleOrDefault(y => y.Tile == x.Tile);
                    var structureName = structure == null ? "" : " " + structure.StructureType.ToString();
                    log.Info($"Battle occurred at {x.Tile.ToString()}{structureName} on turn {x.Turn}.");
                    x.CasualtyLog.ForEach(y => log.Info(y.Text));
                });

                board.ChangeStructureOwners();

                if (battleReports.Any())
                {
                    Visualise.Integration.DrawHexagonImage("BattlesConductedTurn" + board.Turn + ".png", board.Tiles, board.Edges, board.Structures, null, null, board.Units);
                }

                board.Turn++;

                winConditionsMet = board.Units.GroupBy(x => x.OwnerIndex).Count() == 1 
                                    || board.Structures.GroupBy(x => x.OwnerIndex).Count() == 1 
                                    || board.Turn == 10;

            }
        }
    }
}
