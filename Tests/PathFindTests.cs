using Microsoft.VisualStudio.TestTools.UnitTesting;
using GameModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    

    [TestClass]
    public class PathFindTests
    {
        static string[] GameBoard = File.ReadAllLines("BasicBoard.txt");
        static string[] TileEdges = File.ReadAllLines("BasicBoardEdges.txt");


        [TestMethod]
        public void HasAPath()
        {
            var board = new Board(GameBoard, TileEdges);

            var unit = new MilitaryUnit() { Location = board[1, 1] };

            List<PathFindTile> pathFindTiles = Board.GetValidMovesWithMoveCostsForUnit(board, unit);

            var start = pathFindTiles.Single(x => x.Point.X == 1 && x.Point.Y == 1);
            var destination = pathFindTiles.Single(x => x.Point.X == 5 && x.Point.Y == 7);

            Func<PathFindTile, PathFindTile, double> distance = (node1, node2) => node1.MoveCost[node2];
            Func<PathFindTile, double> estimate = t => Math.Sqrt(Math.Pow(t.Point.X - destination.Point.X, 2) + Math.Pow(t.Point.Y - destination.Point.Y, 2));

            var path = PathFind.PathFind.FindPath(start, destination, distance, estimate);
        }


    }
}
