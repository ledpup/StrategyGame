﻿using GameModel;
using NLog;
using System;
using System.Collections.Generic;
using System.Drawing;
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
                new MilitaryUnit(3, tile: board[56], movementType: MovementType.Land, isAmphibious: true),
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
                var bitmap = new Bitmap(1200, 1000);
                Visualise.Integration.Render(bitmap, Visualise.RenderPipeline.Board, Visualise.RenderPipeline.Units, board.Tiles, board.Edges, board.Structures, null, null, board.Units);
                RenderLabelsAndSave("StructureInfluenceMap.png", new Bitmap(bitmap), labels);

                for (var i = 0; i < numberOfPlayers; i++)
                {
                    board.Tiles.ToList().ForEach(x => labels[x.X, x.Y] = x.UnitCountInfluence[MovementType.Airborne][i].ToString());
                    RenderLabelsAndSave("UnitCountAirborneInfluenceMapPlayer" + (i + 1) + "Turn" + board.Turn + ".png", new Bitmap(bitmap), labels);
                    board.Tiles.ToList().ForEach(x => labels[x.X, x.Y] = x.UnitCountInfluence[MovementType.Land][i].ToString());
                    RenderLabelsAndSave("UnitCountLandInfluenceMapPlayer" + (i + 1) + "Turn" + board.Turn + ".png", new Bitmap(bitmap), labels);
                    board.Tiles.ToList().ForEach(x => labels[x.X, x.Y] = x.UnitCountInfluence[MovementType.Water][i].ToString());
                    RenderLabelsAndSave("UnitCountWaterInfluenceMapPlayer" + (i + 1) + "Turn" + board.Turn + ".png", new Bitmap(bitmap), labels);

                    board.Tiles.ToList().ForEach(x => labels[x.X, x.Y] = x.UnitStrengthInfluence[MovementType.Airborne][i].ToString());
                    RenderLabelsAndSave("UnitStrengthAirborneInfluenceMapPlayer" + (i + 1) + "Turn" + board.Turn + ".png", new Bitmap(bitmap), labels);
                    board.Tiles.ToList().ForEach(x => labels[x.X, x.Y] = x.UnitStrengthInfluence[MovementType.Land][i].ToString());
                    RenderLabelsAndSave("UnitStrengthLandInfluenceMapPlayer" + (i + 1) + "Turn" + board.Turn + ".png", new Bitmap(bitmap), labels);
                    board.Tiles.ToList().ForEach(x => labels[x.X, x.Y] = x.UnitStrengthInfluence[MovementType.Water][i].ToString());
                    RenderLabelsAndSave("UnitStrengthWaterInfluenceMapPlayer" + (i + 1) + "Turn" + board.Turn + ".png", new Bitmap(bitmap), labels);

                    board.Tiles.ToList().ForEach(x => labels[x.X, x.Y] = Math.Round(x.AggregateInfluence[MovementType.Airborne][i], 1).ToString());
                    RenderLabelsAndSave("AggregateInfluenceMapAirbornePlayer" + (i + 1) + "Turn" + board.Turn + ".png", new Bitmap(bitmap), labels);
                    board.Tiles.ToList().ForEach(x => labels[x.X, x.Y] = Math.Round(x.AggregateInfluence[MovementType.Land][i], 1).ToString());
                    RenderLabelsAndSave("AggregateInfluenceMapLandPlayer" + (i + 1) + "Turn" + board.Turn + ".png", new Bitmap(bitmap), labels);
                    board.Tiles.ToList().ForEach(x => labels[x.X, x.Y] = Math.Round(x.AggregateInfluence[MovementType.Water][i], 1).ToString());
                    RenderLabelsAndSave("AggregateInfluenceMapWaterPlayer" + (i + 1) + "Turn" + board.Turn + ".png", new Bitmap(bitmap), labels);
                }

                var moveOrders = new List<MoveOrder>();

                board.Units.Where(x => x.IsAlive).ToList().ForEach(x =>
                {
                    var possibleMoves = x.PossibleMoves();

                    var highestTension = possibleMoves.Min(y => y.Destination.AggregateInfluence[x.MovementType][x.OwnerIndex]);

                    if (x.Tile.AggregateInfluence[x.MovementType][x.OwnerIndex] > highestTension)
                    {
                        var moves = possibleMoves.Where(y => y.Destination.AggregateInfluence[x.MovementType][x.OwnerIndex] == highestTension);

                        var bestMove = moves.OrderByDescending(y => y.TerrainAndWeatherModifers(x.Index)).ThenBy(y => y.Distance).First();

                        var moveOrder = bestMove.GetMoveOrder();

                        moveOrder.Unit = x;
                        moveOrders.Add(moveOrder);
                    }
                });

                var vectors = new List<Vector>();
                moveOrders.ForEach(x => vectors.AddRange(x.Vectors));

                Visualise.Integration.RenderAndSave("MoveOrdersTurn" + board.Turn + ".png", board.Tiles, board.Edges, board.Structures, null, vectors, board.Units);

                board.ResolveMoves(moveOrders);

                Visualise.Integration.RenderAndSave("MovesResolvedTurn" + board.Turn + ".png", board.Tiles, board.Edges, board.Structures, null, null, board.Units);

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
                    Visualise.Integration.RenderAndSave("BattlesConductedTurn" + board.Turn + ".png", board.Tiles, board.Edges, board.Structures, null, null, board.Units);
                }

                board.Turn++;

                winConditionsMet = board.Units.GroupBy(x => x.OwnerIndex).Count() == 1 
                                    || board.Structures.GroupBy(x => x.OwnerIndex).Count() == 1 
                                    || board.Turn == 10;

            }
        }

        private static void RenderLabelsAndSave(string fileName, Bitmap bitmap, string[,] labels)
        {
            bitmap = Visualise.Integration.Render(bitmap, Visualise.RenderPipeline.Labels, Visualise.RenderPipeline.Labels, labels: labels);
            bitmap.Save(fileName);
        }


    }
}
