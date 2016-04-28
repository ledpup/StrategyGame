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

            var board = new Board(GameBoard, Edges, Structures);

            var numberOfPlayers = 2;

            board.Units = new List<MilitaryUnit>
                {
                new MilitaryUnit(0, "1st Airborne", 0, board[114], MovementType.Airborne, 3),
                new MilitaryUnit(1, "1st Infantry", 0, board[110], MovementType.Land, 3),
                new MilitaryUnit(2, "2nd Infantry", 0, board[31], MovementType.Land),
                new MilitaryUnit(3, "3rd Infantry", 0, board[56], MovementType.Land),
                new MilitaryUnit(4, "4th Infantry", 0, board[65], MovementType.Land),

                new MilitaryUnit(5, "1st Airborne", 1, board[361], MovementType.Airborne, 3),
                new MilitaryUnit(6, "1st Infantry", 1, board[111]),
                new MilitaryUnit(7, "2nd Infantry", 1, board[111]),

                new MilitaryUnit(8, "3rd Infantry", 1, board[168]),
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
                    var possibleMoves = x.CalculatePossibleMoves();

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
                    log.Info(string.Format("Battle occurred at {0} on turn {1}.", x.Location, x.Turn));
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
