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

                var labels = new string[board.Width, board.Height];
                for (var x = 0; x < board.Width; x++)
                {
                    for (var y = 0; y < board.Height; y++)
                    {
                        labels[x, y] = Math.Round(board[x, y].Temperature, 1).ToString();
                    }
                }

                Visualise.TwoDimensionalVisualisation.RenderAndSave("BasicBoardTemp" + i + ".png", board.Width, tiles, board.Edges, null, labels);
            }
        }
    }
}
