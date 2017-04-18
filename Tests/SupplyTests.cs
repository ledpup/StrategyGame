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
    public class SupplyTests
    {
        static string[] GameBoard = File.ReadAllLines("BasicBoard.txt");
        static string[] Edges = File.ReadAllLines("BasicBoardEdges.txt");
        static string[] Structures = File.ReadAllLines("BasicBoardStructures.txt");

        [TestMethod]
        public void SupplyTest()
        {
            var board = new Board(GameBoard, Edges, Structures);

            board[3, 4].OwnerId = 2;
            board.Units = new List<MilitaryUnit> { new MilitaryUnit(0, "1st Enemy", 2, board[3, 4]) };
            board.InitialiseSupply();

            var labels = new string[board.Width, board.Height];
            board.Tiles.ToList().ForEach(x => labels[x.X, x.Y] = x.Supply.ToString());

            Visualise.GameBoardRenderer.RenderAndSave("BasicBoardWithStructuresAndSupply.png", board.Height, board.Tiles, board.Edges, board.Structures, labels, null, board.Units);
        }
    }
}
