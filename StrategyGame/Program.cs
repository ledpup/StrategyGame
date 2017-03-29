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
                new MilitaryUnit(0, tile: board[114], movementType: MovementType.Airborne, baseMovementPoints: 3, role: Role.Besieger),
                new MilitaryUnit(1, tile: board[110], baseMovementPoints: 3),
                new MilitaryUnit(2, tile: board[31], role: Role.Defensive),
                new MilitaryUnit(3, tile: board[56], movementType: MovementType.Land, isAmphibious: true),
                new MilitaryUnit(4, tile: board[65]),
                new MilitaryUnit(4, tile: board[316], role: Role.Offensive),

                new MilitaryUnit(5, ownerIndex: 1, tile: board[361], movementType: MovementType.Airborne, baseMovementPoints: 3),
                new MilitaryUnit(6, ownerIndex: 1, tile: board[111]),
                new MilitaryUnit(7, ownerIndex: 1, tile: board[111]),

                new MilitaryUnit(8, ownerIndex: 1, tile: board[168]),
                };

            board.Units[0].TerrainTypeBattleModifier[TerrainType.Wetland] = 1;
            board.Units[1].TerrainTypeBattleModifier[TerrainType.Forest] = 1;

            var winConditionsMet = false;

            var log = LogManager.GetCurrentClassLogger();

            //var computerPlayer = new ComputerPlayer();

            while (!winConditionsMet)
            {


                ComputerPlayer.GenerateInfluenceMaps(board, numberOfPlayers);

                var labels = new string[board.Width, board.Height];
                var bitmap = new Bitmap(1920, 1400);
                Visualise.TwoDimensionalVisualisation.Render(bitmap, Visualise.RenderPipeline.Board, Visualise.RenderPipeline.Units, board.Width, board.Tiles, board.Edges, board.Structures, null, null, board.Units);
                
                for (var i = 0; i < numberOfPlayers; i++)
                {
                    MilitaryUnit.Roles.ForEach(x => 
                    {
                        board.Tiles.ToList().ForEach(y => labels[y.X, y.Y] = Math.Round(y.StructureInfluence[x][i], 1).ToString());
                        RenderLabelsAndSave($"StructureInfluenceMap{x.ToString()}Player{(i + 1)}Turn{board.Turn}.png", new Bitmap(bitmap), board.Width, labels);
                    });
                    
                    RoleAndMovementType.RolesAndMovementTypes.ForEach(y => 
                        {
                            board.Tiles.ToList().ForEach(x => labels[x.X, x.Y] = x.UnitInfluence[y][i].ToString());
                            RenderLabelsAndSave($"UnitInfluenceMap{y.Role.ToString() + y.MovementType.ToString()}Player{i + 1}Turn{board.Turn}.png", new Bitmap(bitmap), board.Width, labels);
                            
                            board.Tiles.ToList().ForEach(x => labels[x.X, x.Y] = Math.Round(x.AggregateInfluence[y][i], 1).ToString());
                            RenderLabelsAndSave($"AggregateInfluenceMap{y.Role.ToString() + y.MovementType.ToString()}Player{i + 1}Turn{board.Turn}.png", new Bitmap(bitmap), board.Width, labels);
                        });                   
                }

                var moveOrders = new List<MoveOrder>();

                board.Units.Where(x => x.IsAlive).ToList().ForEach(x =>
                {
                    var possibleMoves = x.PossibleMoves();

                    var roleAndMovementType = new RoleAndMovementType(x.Role, x.MovementType);

                    var lowestTension = possibleMoves.Min(y => y.Destination.AggregateInfluence[roleAndMovementType][x.OwnerIndex]);

                    //if (x.Tile.AggregateInfluence[x.MovementType][x.OwnerIndex] < highestTension)
                    //{
                        if (x.Tile.AggregateInfluence[roleAndMovementType][x.OwnerIndex] > lowestTension)
                        {
                            var moves = possibleMoves.Where(y => y.Destination.AggregateInfluence[roleAndMovementType][x.OwnerIndex] == lowestTension);

                            var bestMove = moves.OrderByDescending(y => y.TerrainAndWeatherModifers(x.Index)).ThenBy(y => y.Distance).First();

                            var moveOrder = bestMove.GetMoveOrder();

                            moveOrder.Unit = x;
                            moveOrders.Add(moveOrder);
                        }
                    //}
                });

                var vectors = new List<Vector>();
                moveOrders.ForEach(x => vectors.AddRange(x.Vectors));

                Visualise.TwoDimensionalVisualisation.RenderAndSave("MoveOrdersTurn" + board.Turn + ".png", board.Width, board.Tiles, board.Edges, board.Structures, null, vectors, board.Units);

                board.ResolveMoves(moveOrders);

                Visualise.TwoDimensionalVisualisation.RenderAndSave("MovesResolvedTurn" + board.Turn + ".png", board.Width, board.Tiles, board.Edges, board.Structures, null, null, board.Units);

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
                    Visualise.TwoDimensionalVisualisation.RenderAndSave("BattlesConductedTurn" + board.Turn + ".png", board.Width, board.Tiles, board.Edges, board.Structures, null, null, board.Units);
                }

                board.Turn++;

                winConditionsMet = board.Units.GroupBy(x => x.OwnerIndex).Count() == 1 
                                    || board.Structures.GroupBy(x => x.OwnerIndex).Count() == 1 
                                    || board.Turn == 10;

            }
        }

        private static void RenderLabelsAndSave(string fileName, Bitmap bitmap, int boardWidth, string[,] labels)
        {
            bitmap = Visualise.TwoDimensionalVisualisation.Render(bitmap, Visualise.RenderPipeline.Labels, Visualise.RenderPipeline.Labels, boardWidth, labels: labels);
            bitmap.Save(fileName);
        }


    }
}
