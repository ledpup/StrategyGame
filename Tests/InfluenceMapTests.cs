using ComputerOpponent;
using GameModel;
using GameModel.Commands;
using Hexagon;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Visualise;

namespace Tests
{
    [TestClass]
    public class InfluenceMapTests
    {
      
        static string[] GameBoard = File.ReadAllLines("BasicBoard.txt");
        static string[] Edges = File.ReadAllLines("BasicBoardEdges.txt");
        static string[] Settlements = File.ReadAllLines("BasicBoardSettlements.txt");

        [TestMethod]
        public void DisplayInfluenceMap()
        {
            var board = new GameState(new Board(GameBoard, Edges, Settlements));

            var numberOfPlayers = 2;

            board.Units =
            [
                new(new UnitTemplate { MovementType = MovementType.Airborne, MovementPoints = 3 }, 0, 0, board[114], "1st Airborne"),
                new(new UnitTemplate { MovementPoints = 3 }, 1, 0, board[110], "1st Infantry"),
                new(new UnitTemplate(), 2, 0, board[31], "2nd Infantry"),
                new(new UnitTemplate(), 3, 0, board[56], "3rd Infantry"),
                new(new UnitTemplate(), 4, 0, board[65], "4th Infantry"),

                new(new UnitTemplate(), 5, 1, board[111], "1st Infantry"),
                new(new UnitTemplate(), 6, 1, board[111], "2nd Infantry"),

                new(new UnitTemplate(), 7, 1, board[168], "3rd Infantry"),
            ];

            board.Units[0].TerrainTypeBattleModifier[TerrainType.Wetland] = 1;
            board.Units[1].TerrainTypeBattleModifier[TerrainType.Forest] = 1;

            var computerPlayer = new ComputerPlayer(board.Units);

            computerPlayer.GenerateInfluenceMaps(board, numberOfPlayers);

            var moveOrders = new List<IUnitCommand>();

            board.Units.Where(x => x.IsAlive).ToList().ForEach(x =>
            {
                var moveOrder = computerPlayer.FindBestMoveOrderForUnit(x, board);
                if (moveOrder != null)
                    moveOrders.Add(moveOrder);
            });

            var vectors = new List<Centreline>();
            moveOrders.ForEach(x => vectors.AddRange(Centreline.MoveOrderToCentrelines((MoveCommand)x)));

            GameBoardRenderer.RenderAndSave("AggregateInfluenceMoveOrders.png", board.Width, board.Height, board.Tiles, board.Edges, board.Settlements, null, vectors, board.Units);

            board.ResolveOrders(moveOrders);

            GameBoardRenderer.RenderAndSave("AggregateInfluenceMovesResolved.png", board.Width, board.Height, board.Tiles, board.Edges, board.Settlements, null, null, board.Units);
        }

        [TestMethod]
        public void SelectBestMoveFromInfluenceMap()
        {
            var board = new GameState(new Board(GameBoard, Edges, Settlements));

            var numberOfPlayers = 2;

            board.Units =
            [
                new(new UnitTemplate { MovementType = MovementType.Airborne, MovementPoints = 3 }, 0, 0, board[114]),
                new(new UnitTemplate { MovementPoints = 3 }, 1, 0, board[110]),
                new(new UnitTemplate(), 2, 0, board[31]),
                new(new UnitTemplate(), 3, 0, board[56]),
                new(new UnitTemplate(), 4, 0, board[65]),

                new(new UnitTemplate(), 5, 1, board[111]),
                new(new UnitTemplate(), 6, 1, board[111]),
                new(new UnitTemplate(), 7, 1, board[168]),
            ];


            var computerPlayer = new ComputerPlayer(board.Units);
            computerPlayer.GenerateInfluenceMaps(board, numberOfPlayers);

            var results = Hex.HexesWithinArea(board.Units[1].Location.Hex, 4, board.Width, board.Height);
            results.ToList().ForEach(x => board[Hex.HexToIndex(x, board.Width, board.Height)].IsSelected = true);

            GameBoardRenderer.RenderAndSave("HexesConsideredForHighestInfluence.png", board.Width, board.Height, board.Tiles, board.Edges, board.Settlements, null, null, board.Units);

            var roleMovementType = computerPlayer.GetUnitState(board.Units[1]).GetRoleMovementType(board.Units[1]);
            var tilesOrderedInfluence = board.Tiles
                .Where(x => results.Contains(x.Hex))
                .OrderByDescending(x => computerPlayer.AggregateInfluence[x.Index][roleMovementType][board.Units[1].OwnerIndex])
                .ToList();

            IEnumerable<PathFindTile> bestPossibleDestination = null;
            foreach (var tile in tilesOrderedInfluence)
            {
                bestPossibleDestination = PathFinder.FindShortestPath(board.Units[1].Location, tile, board.Units[1]);
                if (bestPossibleDestination != null)
                    break;
            }

            if (bestPossibleDestination != null)
            {
                var moveOrder = board.Units[1].ShortestPathToMoveOrder(bestPossibleDestination.ToArray());
            }
        }
    }
}
