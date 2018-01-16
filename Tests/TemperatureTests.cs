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

            for (var i = 0; i < 30; i++)
            {

                board.CalculateTemperature(i);
                var tiles = new List<Tile>();
                board.Tiles.ToList().ForEach(x => tiles.Add(new Tile(x.Index, x.X, x.Y, x.GetTerrainTypeByTemperature(x.Temperature))));

                var labels = new string[board.Width * board.Height];
                for (var j = 0; j < board.TileArray.Length; j++)
                {
                    labels[j] = Math.Round(board[j].Temperature, 1).ToString();
                }

                Visualise.GameBoardRenderer.RenderAndSave("BasicBoardTemp" + i + ".png", board.Width, board.Height, tiles, board.Edges, null, labels);
            }
        }
    }
}
