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
    public class TemperatureTests
    {
        static string[] GameBoard = File.ReadAllLines("BasicBoard.txt");
        static string[] TileEdges = File.ReadAllLines("BasicBoardEdges.txt");

        [TestMethod]
        public void CorrectTempTest()
        {
            var board = new Board(GameBoard, TileEdges);

            for (var i = 0; i < 20; i++)
            {

                board.CalculateTemperature(i);

                var labels = new string[board.Width, board.Height];
                for (var x = 0; x < board.Width; x++)
                {
                    for (var y = 0; y < board.Height; y++)
                    {
                        labels[x, y] = Math.Round(board[x, y].Temperature, 1).ToString();
                    }
                }

                Visualise.Integration.DrawHexagonImage("BasicBoardTemp" + i + ".png", board, labels);
            }
        }
    }
}
