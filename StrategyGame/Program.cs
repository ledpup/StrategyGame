using GameModel;
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
                new MilitaryUnit(0, location: board[114], movementType: MovementType.Airborne, baseMovementPoints: 3, role: Role.Besieger),
                new MilitaryUnit(1, location: board[110], baseMovementPoints: 3, role: Role.Defensive),
                new MilitaryUnit(2, location: board[31], role: Role.Defensive),
                new MilitaryUnit(3, location: board[56], movementType: MovementType.Land, isAmphibious: true),
                new MilitaryUnit(4, location: board[65]),
                new MilitaryUnit(5, location: board[316], role: Role.Defensive),

                new MilitaryUnit(7, ownerIndex: 1, location: board[247], movementType: MovementType.Airborne, baseMovementPoints: 3),
                new MilitaryUnit(8, ownerIndex: 1, location: board[361], movementType: MovementType.Airborne, baseMovementPoints: 3),
                new MilitaryUnit(9, ownerIndex: 1, location: board[111]),
                new MilitaryUnit(10, ownerIndex: 1, location: board[111]),
                new MilitaryUnit(11, ownerIndex: 1, location: board[478], role: Role.Besieger),

                new MilitaryUnit(12, ownerIndex: 1, location: board[168]),
                };

            board.Units[0].TerrainTypeBattleModifier[TerrainType.Wetland] = 1;
            board.Units[1].TerrainTypeBattleModifier[TerrainType.Forest] = 1;

            var gameOver = false;

            var log = LogManager.GetCurrentClassLogger();

            //var computerPlayer = new ComputerPlayer();

            while (!gameOver)
            {


                ComputerPlayer.GenerateInfluenceMaps(board, numberOfPlayers);

                var labels = new string[board.Width, board.Height];
                var bitmap = new Bitmap(1920, 1450);
                Visualise.GameBoardRenderer.Render(bitmap, Visualise.RenderPipeline.Board, Visualise.RenderPipeline.Units, board.Height, board.Tiles, board.Edges, board.Structures, null, null, board.Units);
                
                for (var i = 0; i < numberOfPlayers; i++)
                {
                    foreach (var mt in MilitaryUnit.MovementTypes)
                    {
                        board.Tiles.ToList().ForEach(y => labels[y.X, y.Y] = Math.Round(y.FriendlyStructureInfluence[mt][i], 1).ToString());
                        Visualise.GameBoardRenderer.RenderLabelsAndSave($"FriendlyStructureInfluenceMap{mt.ToString()}Player{(i + 1)}Turn{board.Turn}.png", new Bitmap(bitmap), board.Height, labels);

                        board.Tiles.ToList().ForEach(y => labels[y.X, y.Y] = Math.Round(y.EnemyStructureInfluence[mt][i], 1).ToString());
                        Visualise.GameBoardRenderer.RenderLabelsAndSave($"EnemyStructureInfluenceMap{mt.ToString()}Player{(i + 1)}Turn{board.Turn}.png", new Bitmap(bitmap), board.Height, labels);
                    }

                    board.Tiles.ToList().ForEach(y => labels[y.X, y.Y] = Math.Round(y.FriendlyUnitInfluence[i], 1).ToString());
                    Visualise.GameBoardRenderer.RenderLabelsAndSave($"FriendlyUnitInfluenceMapPlayer{i + 1}Turn{board.Turn}.png", new Bitmap(bitmap), board.Height, labels);

                    board.Tiles.ToList().ForEach(y => labels[y.X, y.Y] = Math.Round(y.EnemyUnitInfluence[i], 1).ToString());
                    Visualise.GameBoardRenderer.RenderLabelsAndSave($"EnemyUnitInfluenceMapPlayer{i + 1}Turn{board.Turn}.png", new Bitmap(bitmap), board.Height, labels);

                    MilitaryUnit.Roles.ForEach(x => 
                        {
                            MilitaryUnit.MovementTypes.ForEach(z =>
                            {
                                board.Tiles.ToList().ForEach(y => labels[y.X, y.Y] = Math.Round(y.AggregateInfluence[new RoleMovementType(z, x)][i], 1).ToString());
                                Visualise.GameBoardRenderer.RenderLabelsAndSave($"AggregateInfluenceMap{x.ToString()}Player{i + 1}Turn{board.Turn}.png", new Bitmap(bitmap), board.Height, labels);
                            });
                        });                   
                }

                var moveOrders = new List<IUnitOrder>();

                board.Units.Where(x => x.IsAlive).ToList().ForEach(x =>
                {
                    var moveOrder = ComputerPlayer.FindBestMoveOrderForUnit(x);
                    if (moveOrder != null)
                        moveOrders.Add(moveOrder);
                });

                var vectors = new List<Vector>();
                moveOrders.ForEach(x => vectors.AddRange(((MoveOrder)x).Vectors));

                Visualise.GameBoardRenderer.RenderAndSave("MoveOrdersTurn" + board.Turn + ".png", board.Height, board.Tiles, board.Edges, board.Structures, null, vectors, board.Units);

                board.ResolveOrders(moveOrders);
                for (var i = 0; i < numberOfPlayers; i++)
                {
                    board.ResolveStackLimits(i);
                }

                Visualise.GameBoardRenderer.RenderAndSave("MovesResolvedTurn" + board.Turn + ".png", board.Height, board.Tiles, board.Edges, board.Structures, null, null, board.Units);

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
                    Visualise.GameBoardRenderer.RenderAndSave("BattlesConductedTurn" + board.Turn + ".png", board.Height, board.Tiles, board.Edges, board.Structures, null, null, board.Units);
                }

                board.Turn++;

                gameOver = board.Units.GroupBy(x => x.OwnerIndex).Count() == 1 
                                    || board.Structures.GroupBy(x => x.OwnerIndex).Count() == 1 
                                    || board.Turn == 20;

            }
        }






    }
}
