using ComputerOpponent;
using GameModel;
using GameModel.Rendering;
using NLog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Visualise;

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

            var aiUnits = new List<AiMilitaryUnit>
                {
                new AiMilitaryUnit(new MilitaryUnit(0, location: board[114], movementType: MovementType.Airborne, baseMovementPoints: 3)) { Role = Role.Besieger },
                new AiMilitaryUnit(new MilitaryUnit(1, location: board[110], baseMovementPoints: 3)) { Role = Role.Defensive },
                new AiMilitaryUnit(new MilitaryUnit(2, location: board[31])) { Role = Role.Defensive },
                new AiMilitaryUnit(new MilitaryUnit(3, location: board[56], movementType: MovementType.Land)),
                new AiMilitaryUnit(new MilitaryUnit(4, location: board[65])),
                new AiMilitaryUnit(new MilitaryUnit(5, location: board[316])) { Role = Role.Defensive },

                new AiMilitaryUnit(new MilitaryUnit(7, ownerIndex: 1, location: board[247], movementType: MovementType.Airborne, baseMovementPoints: 3)) { Role = Role.Besieger },
                new AiMilitaryUnit(new MilitaryUnit(8, ownerIndex: 1, location: board[361], movementType: MovementType.Airborne, baseMovementPoints: 3)) { Role = Role.Besieger },
                new AiMilitaryUnit(new MilitaryUnit(9, ownerIndex: 1, location: board[111])),
                new AiMilitaryUnit(new MilitaryUnit(10, ownerIndex: 1, location: board[111])),
                new AiMilitaryUnit(new MilitaryUnit(11, ownerIndex: 1, location: board[478], movementType: MovementType.Airborne)) { Role = Role.Besieger },

                new AiMilitaryUnit(new MilitaryUnit(12, ownerIndex: 1, location: board[168])),
                };

            var computerPlayer = new ComputerPlayer(aiUnits);

            board.Units = aiUnits.Select(x => x.Unit).ToList();

            board.Units[0].TerrainTypeBattleModifier[TerrainType.Wetland] = 1;
            board.Units[1].TerrainTypeBattleModifier[TerrainType.Forest] = 1;

            board.Units[3].TerrainTypeBattleModifier[TerrainType.Wetland] = 1;
            board.Units[3].EdgeMovementCosts[EdgeType.River] = 0;

            var gameOver = false;

            var log = LogManager.GetCurrentClassLogger();

            //var computerPlayer = new ComputerPlayer();

            while (!gameOver)
            {


                computerPlayer.GenerateInfluenceMaps(board, numberOfPlayers);

                var labels = new string[board.Width, board.Height];
                var drawing2d = new GameRenderingEngine2D(board.Width, board.Height);
                GameRenderer.Render(drawing2d, RenderPipeline.Board, RenderPipeline.Units, board.Width, board.Height, board.Tiles, board.Edges, board.Structures, null, null, board.Units);
                
                //for (var i = 0; i < numberOfPlayers; i++)
                //{
                //    foreach (var mt in MilitaryUnit.MovementTypes)
                //    {
                //        board.Tiles.ToList().ForEach(y => labels[y.X, y.Y] = Math.Round(y.FriendlyStructureInfluence[mt][i], 1).ToString());
                //        Visualise.GameBoardRenderer.RenderLabelsAndSave($"FriendlyStructureInfluenceMap{mt.ToString()}Player{(i + 1)}Turn{board.Turn}.png", new Bitmap(bitmap), board.Height, labels);

                //        board.Tiles.ToList().ForEach(y => labels[y.X, y.Y] = Math.Round(y.EnemyStructureInfluence[mt][i], 1).ToString());
                //        Visualise.GameBoardRenderer.RenderLabelsAndSave($"EnemyStructureInfluenceMap{mt.ToString()}Player{(i + 1)}Turn{board.Turn}.png", new Bitmap(bitmap), board.Height, labels);
                //    }

                //    board.Tiles.ToList().ForEach(y => labels[y.X, y.Y] = Math.Round(y.FriendlyUnitInfluence[i], 1).ToString());
                //    Visualise.GameBoardRenderer.RenderLabelsAndSave($"FriendlyUnitInfluenceMapPlayer{i + 1}Turn{board.Turn}.png", new Bitmap(bitmap), board.Height, labels);

                //    board.Tiles.ToList().ForEach(y => labels[y.X, y.Y] = Math.Round(y.EnemyUnitInfluence[i], 1).ToString());
                //    Visualise.GameBoardRenderer.RenderLabelsAndSave($"EnemyUnitInfluenceMapPlayer{i + 1}Turn{board.Turn}.png", new Bitmap(bitmap), board.Height, labels);

                //    MilitaryUnit.Roles.ForEach(x => 
                //        {
                //            MilitaryUnit.MovementTypes.ForEach(z =>
                //            {
                //                board.Tiles.ToList().ForEach(y => labels[y.X, y.Y] = Math.Round(y.AggregateInfluence[new RoleMovementType(z, x)][i], 1).ToString());
                //                Visualise.GameBoardRenderer.RenderLabelsAndSave($"AggregateInfluenceMap{x.ToString()}Player{i + 1}Turn{board.Turn}.png", new Bitmap(bitmap), board.Height, labels);
                //            });
                //        });                   
                //}

                var unitsOrders = new List<IUnitOrder>();

                var units = board.Units.Where(x => x.IsAlive).ToList();
                computerPlayer.SetStrategicAction(board);
                unitsOrders = computerPlayer.CreateOrders(board, units);

                var lines = new List<Centreline>();
                unitsOrders.ForEach(x => lines.AddRange(Centreline.MoveOrderToCentrelines((MoveOrder)x)));

                GameRenderer.RenderAndSave(drawing2d, "MoveOrdersTurn" + board.Turn + ".png", board.Width, board.Height, board.Tiles, board.Edges, board.Structures, null, lines, board.Units);

                board.ResolveOrders(unitsOrders);
                for (var i = 0; i < numberOfPlayers; i++)
                {
                    board.ResolveStackLimits(i);
                }

                GameRenderer.RenderAndSave(drawing2d, "MovesResolvedTurn" + board.Turn + ".png", board.Width, board.Height, board.Tiles, board.Edges, board.Structures, null, null, board.Units);

                var battleReports = board.ConductBattles();
                battleReports.ForEach(x =>
                {
                    var structure = board.Structures.SingleOrDefault(y => y.Location == x.Tile);
                    var structureName = structure == null ? "" : " " + structure.StructureType.ToString();
                    log.Info($"Battle occurred at {x.Tile.ToString()}{structureName} on turn {x.Turn}.");
                    x.CasualtyLog.ForEach(y => log.Info(y.Text));
                });

                board.ChangeStructureOwners();

                if (battleReports.Any())
                {
                    GameRenderer.RenderAndSave(drawing2d, "BattlesConductedTurn" + board.Turn + ".png", board.Width, board.Height, board.Tiles, board.Edges, board.Structures, null, null, board.Units);
                }

                board.Turn++;

                gameOver = board.Units.GroupBy(x => x.OwnerIndex).Count() == 1 
                                    || board.Structures.GroupBy(x => x.OwnerIndex).Count() == 1 
                                    || board.Turn == 20;

            }
        }






    }
}
