using ComputerOpponent;
using GameModel;
using Hexagon;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Visualise;

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

            ComputerPlayer.GenerateInfluenceMaps(board, numberOfPlayers);

            var moveOrders = new List<IUnitOrder>();

            board.Units.Where(x => x.IsAlive).ToList().ForEach(x =>
            {
                var moveOrder = ComputerPlayer.FindBestMoveOrderForUnit(x, board);
                if (moveOrder != null)
                    moveOrders.Add(moveOrder);
            });

            var vectors = new List<Centreline>();
            moveOrders.ForEach(x => vectors.AddRange(Centreline.MoveOrderToCentrelines((MoveOrder)x)));

            Visualise.GameBoardRenderer.RenderAndSave("AggregateInfluenceMoveOrders.png", board.Width, board.Height, board.Tiles, board.Edges, board.Structures, null, vectors, board.Units);

            board.ResolveOrders(moveOrders);

            Visualise.GameBoardRenderer.RenderAndSave("AggregateInfluenceMovesResolved.png", board.Width, board.Height, board.Tiles, board.Edges, board.Structures, null, null, board.Units);
        }

        [TestMethod]
        public void SelectBestMoveFromInfluenceMap()
        {
            var board = new Board(GameBoard, Edges, Structures);

            var numberOfPlayers = 2;

            board.Units = new List<MilitaryUnit>
            {
                new MilitaryUnit(ownerIndex: 0, location: board[114], movementType: MovementType.Airborne, baseMovementPoints: 3),
                new MilitaryUnit(ownerIndex: 0, location: board[110], movementType: MovementType.Land, baseMovementPoints: 3),
                new MilitaryUnit(ownerIndex: 0, location: board[31], movementType: MovementType.Land),
                new MilitaryUnit(ownerIndex: 0, location: board[56], movementType: MovementType.Land),
                new MilitaryUnit(ownerIndex: 0, location: board[65], movementType: MovementType.Land),

                new MilitaryUnit(ownerIndex: 1, location: board[111]),
                new MilitaryUnit(ownerIndex: 1, location: board[111]),
                new MilitaryUnit(ownerIndex: 1, location: board[168]),
            };

            ComputerPlayer.GenerateInfluenceMaps(board, numberOfPlayers);

            var results = Hex.HexesWithinArea(board.Units[1].Location.Hex, 4, board.Width, board.Height);
            results.ToList().ForEach(x => board[Hex.HexToIndex(x, board.Width, board.Height)].IsSelected = true);

            Visualise.GameBoardRenderer.RenderAndSave("HexesConsideredForHighestInfluence.png", board.Width, board.Height, board.Tiles, board.Edges, board.Structures, null, null, board.Units);

            var tilesOrderedInfluence = board.Tiles
                .Where(x => results.Contains(x.Hex))
                .OrderByDescending(x => x.AggregateInfluence[board.Units[1].RoleMovementType][board.Units[1].OwnerIndex])
                .ToList();

            var pathFindTiles = board.ValidMovesWithMoveCostsForUnit(board.Units[1]);

            IEnumerable<PathFindTile> bestPossibleDestination = null;
            foreach (var tile in tilesOrderedInfluence)
            {
                bestPossibleDestination = Board.FindShortestPath(pathFindTiles, board.Units[1].Location.Hex, tile.Hex, board.Units[1].MovementPoints);
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
