using Microsoft.VisualStudio.TestTools.UnitTesting;
using GameModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Visualise;

namespace Tests
{
    

    [TestClass]
    public class VisualiseTests
    {
        static string[] GameBoard = File.ReadAllLines("BasicBoard.txt");
        static string[] TileEdges = File.ReadAllLines("BasicBoardEdges.txt");
        static string[] Settlements = File.ReadAllLines("BasicBoardSettlements.txt");

        Func<PathFindTile, PathFindTile, double> distance = (node1, node2) => node1.MoveCost[node2.Hex];

        [TestMethod]
        public void VisualiseBoardTest()
        {
            var board = new GameState(new Board(GameBoard, TileEdges, Settlements));

            var labels = new string[board.Width * board.Height];
            board.Tiles.ToList().ForEach(x => labels[x.Index] = x.Index.ToString());
            GameBoardRenderer.RenderAndSave("Coords - index.png", board.Width, board.Height, board.Tiles, board.Edges, board.Settlements, labels);

            board.Tiles.ToList().ForEach(x => labels[x.Index] = x.ToOffsetCoordsString());
            GameBoardRenderer.RenderAndSave("Coords - offset.png", board.Width, board.Height, board.Tiles, board.Edges, board.Settlements, labels);

            board.Tiles.ToList().ForEach(x => labels[x.Index] = x.Hex.ToString());
            GameBoardRenderer.RenderAndSave("Coords - cube.png", board.Width, board.Height, board.Tiles, board.Edges, board.Settlements, labels);

            board.Tiles.ToList().ForEach(x => labels[x.Index] = x.ContiguousRegionId.ToString());
            GameBoardRenderer.RenderAndSave("ContiguousRegionIds.png", board.Width, board.Height, board.Tiles, board.Edges, board.Settlements, labels);
        }

        [TestMethod]
        public void VisualiseUnitOnBoardTest()
        {
            var board = new GameState(new Board(GameBoard, TileEdges, Settlements));

            var units = new List<MilitaryUnit>
            {
                new(new UnitTemplate()) { Location = board[1, 1] },
                new(new UnitTemplate()) { Location = board[1, 1] },
                new(new UnitTemplate()) { Location = board[1, 1] },
                new(new UnitTemplate()) { Location = board[1, 1], OwnerIndex = 2 }
            };

            GameBoardRenderer.RenderAndSave("BasicBoardWithUnits.png", board.Width, board.Height, board.Tiles, board.Edges, board.Settlements, units: units);
        }




    }
}
