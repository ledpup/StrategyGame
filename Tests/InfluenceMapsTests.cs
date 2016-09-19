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
                new MilitaryUnit(2, "2nd Infantry", 0, board[31], MovementType.Land),
                new MilitaryUnit(3, "3rd Infantry", 0, board[56], MovementType.Land),
                new MilitaryUnit(4, "4th Infantry", 0, board[65], MovementType.Land),

                new MilitaryUnit(5, "1st Infantry", 1, board[111]),
                new MilitaryUnit(6, "2nd Infantry", 1, board[111]),

                new MilitaryUnit(7, "3rd Infantry", 1, board[168]),
            };

            board.Units[0].TerrainTypeBattleModifier[TerrainType.Wetland] = 1;
            board.Units[1].TerrainTypeBattleModifier[TerrainType.Forest] = 1;

            Board.GenerateInfluenceMaps(board, numberOfPlayers);

            var moveOrders = new List<MoveOrder>();

            board.Units.ForEach(x =>
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

            Visualise.Integration.DrawHexagonImage("AggregateInfluenceMoveOrders.png", board.Tiles, board.Edges, board.Structures, null, vectors, board.Units);

            board.ResolveMoves(moveOrders);

            Visualise.Integration.DrawHexagonImage("AggregateInfluenceMovesResolved.png", board.Tiles, board.Edges, board.Structures, null, null, board.Units);
        }
    }
}
