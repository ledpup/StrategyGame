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
using GameModel.Rendering;
using GameRendering2D;

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
            var drawing2d = new GameRenderingGdiPlus(board.Width, board.Height);
            GameRenderer.RenderAndSave(drawing2d, "Coords - index.png", board.Width, board.Height, board.Tiles, board.Edges, board.Structures, labels);

            board.Tiles.ToList().ForEach(x => labels[x.Index] = x.ToOffsetCoordsString());
            GameRenderer.RenderAndSave(drawing2d, "Coords - offset.png", board.Width, board.Height, board.Tiles, board.Edges, board.Structures, labels);

            board.Tiles.ToList().ForEach(x => labels[x.Index] = x.Hex.ToString());
            GameRenderer.RenderAndSave(drawing2d, "Coords - cube.png", board.Width, board.Height, board.Tiles, board.Edges, board.Structures, labels);

            board.Tiles.ToList().ForEach(x => labels[x.Index] = x.ContiguousRegionId.ToString());
            GameRenderer.RenderAndSave(drawing2d, "ContiguousRegionIds.png", board.Width, board.Height, board.Tiles, board.Edges, board.Structures, labels);
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

            var drawing2d = new GameRenderingGdiPlus(board.Width, board.Height);
            GameRenderer.RenderAndSave(drawing2d, "BasicBoardWithUnits.png", board.Width, board.Height, board.Tiles, board.Edges, board.Structures, units: units);
        }




    }
}
