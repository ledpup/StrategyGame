using ComputerOpponent;
using GameModel;
using GameModel.Rendering;
using GameRendering2D;
using Hexagon;
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
    public class InfluenceMapTests
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
                new MilitaryUnit(2, "2nd Infantry", 0, board[31], MovementType.Land),
                new MilitaryUnit(3, "3rd Infantry", 0, board[56], MovementType.Land),
                new MilitaryUnit(4, "4th Infantry", 0, board[65], MovementType.Land),

                new MilitaryUnit(5, "1st Infantry", 1, board[111]),
                new MilitaryUnit(6, "2nd Infantry", 1, board[111]),

                new MilitaryUnit(7, "3rd Infantry", 1, board[168]),
            };

            board.Units[0].TerrainTypeBattleModifier[TerrainType.Wetland] = 1;
            board.Units[1].TerrainTypeBattleModifier[TerrainType.Forest] = 1;

            var computerPlayer = new ComputerPlayer(board.Units);

            computerPlayer.GenerateInfluenceMaps(board, numberOfPlayers);

            var moveOrders = new List<IUnitOrder>();

            board.Units.Where(x => x.IsAlive).ToList().ForEach(x =>
            {
                var moveOrder = computerPlayer.FindBestMoveOrderForUnit(computerPlayer.AiUnits[x.Index], board);
                if (moveOrder != null)
                    moveOrders.Add(moveOrder);
            });

            var vectors = new List<Centreline>();
            moveOrders.ForEach(x => vectors.AddRange(Centreline.MoveOrderToCentrelines((MoveOrder)x)));

            var drawing2d = new GameRenderingEngine2D(board.Width, board.Height);
            GameRenderer.RenderAndSave(drawing2d, "AggregateInfluenceMoveOrders.png", board.Width, board.Height, board.Tiles, board.Edges, board.Structures, null, vectors, board.Units);

            board.ResolveOrders(moveOrders);

            GameRenderer.RenderAndSave(drawing2d, "AggregateInfluenceMovesResolved.png", board.Width, board.Height, board.Tiles, board.Edges, board.Structures, null, null, board.Units);
        }

        [TestMethod]
        public void SelectBestMoveFromInfluenceMap()
        {
            var board = new Board(GameBoard, Edges, Structures);

            var numberOfPlayers = 2;

            board.Units = new List<MilitaryUnit>
            {
                new MilitaryUnit(0, ownerIndex: 0, location: board[114], movementType: MovementType.Airborne, baseMovementPoints: 3),
                new MilitaryUnit(1, ownerIndex: 0, location: board[110], movementType: MovementType.Land, baseMovementPoints: 3),
                new MilitaryUnit(2, ownerIndex: 0, location: board[31], movementType: MovementType.Land),
                new MilitaryUnit(3, ownerIndex: 0, location: board[56], movementType: MovementType.Land),
                new MilitaryUnit(4, ownerIndex: 0, location: board[65], movementType: MovementType.Land),

                new MilitaryUnit(5, ownerIndex: 1, location: board[111]),
                new MilitaryUnit(6, ownerIndex: 1, location: board[111]),
                new MilitaryUnit(7, ownerIndex: 1, location: board[168]),
            };


            var computerPlayer = new ComputerPlayer(board.Units);
            computerPlayer.GenerateInfluenceMaps(board, numberOfPlayers);

            var results = Hex.HexesWithinArea(board.Units[1].Location.Hex, 4, board.Width, board.Height);
            results.ToList().ForEach(x => board[Hex.HexToIndex(x, board.Width, board.Height)].IsSelected = true);

            var drawing2d = new GameRenderingEngine2D(board.Width, board.Height);
            GameRenderer.RenderAndSave(drawing2d, "HexesConsideredForHighestInfluence.png", board.Width, board.Height, board.Tiles, board.Edges, board.Structures, null, null, board.Units);

            var tilesOrderedInfluence = board.Tiles
                .Where(x => results.Contains(x.Hex))
                .OrderByDescending(x => x.AggregateInfluence[computerPlayer.AiUnits[1].RoleMovementType][board.Units[1].OwnerIndex])
                .ToList();

            IEnumerable<PathFindTile> bestPossibleDestination = null;
            foreach (var tile in tilesOrderedInfluence)
            {
                bestPossibleDestination = Board.FindShortestPath(board.Units[1].Location, tile, board.Units[1]);
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
