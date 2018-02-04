using Microsoft.VisualStudio.TestTools.UnitTesting;
using GameModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ComputerOpponent;
using Hexagon;
using Visualise;

namespace Tests
{
    

    [TestClass]
    public class VisualiseTests
    {
        static string[] GameBoard = File.ReadAllLines("BasicBoard.txt");
        static string[] TileEdges = File.ReadAllLines("BasicBoardEdges.txt");
        static string[] Structures = File.ReadAllLines("BasicBoardStructures.txt");

        Func<PathFindTile, PathFindTile, double> distance = (node1, node2) => node1.MoveCost[node2.Hex];

        [TestMethod]
        public void VisualiseBoardTest()
        {
            var board = new Board(GameBoard, TileEdges, Structures);

            var labels = new string[board.Width * board.Height];
            board.Tiles.ToList().ForEach(x => labels[x.Index] = x.Index.ToString());
            GameBoardRenderer.RenderAndSave("Coords - index.png", board.Width, board.Height, board.Tiles, board.Edges, board.Structures, labels);

            board.Tiles.ToList().ForEach(x => labels[x.Index] = x.ToOffsetCoordsString());
            GameBoardRenderer.RenderAndSave("Coords - offset.png", board.Width, board.Height, board.Tiles, board.Edges, board.Structures, labels);

            board.Tiles.ToList().ForEach(x => labels[x.Index] = x.Hex.ToString());
            GameBoardRenderer.RenderAndSave("Coords - cube.png", board.Width, board.Height, board.Tiles, board.Edges, board.Structures, labels);

            board.Tiles.ToList().ForEach(x => labels[x.Index] = x.ContiguousRegionId.ToString());
            GameBoardRenderer.RenderAndSave("ContiguousRegionIds.png", board.Width, board.Height, board.Tiles, board.Edges, board.Structures, labels);
        }

        [TestMethod]
        public void VisualiseUnitOnBoardTest()
        {
            var board = new Board(GameBoard, TileEdges, Structures);

            var units = new List<MilitaryUnit>
            {
                new MilitaryUnit() { Location = board[1, 1] },
                new MilitaryUnit() { Location = board[1, 1] },
                new MilitaryUnit() { Location = board[1, 1] },
                new MilitaryUnit() { Location = board[1, 1], OwnerIndex = 2 }
            };

            GameBoardRenderer.RenderAndSave("BasicBoardWithUnits.png", board.Width, board.Height, board.Tiles, board.Edges, board.Structures, units: units);
        }




    }
}
