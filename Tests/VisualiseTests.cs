﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
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

        Func<PathFindTile, PathFindTile, double> distance = (node1, node2) => node1.MoveCost[node2];

        [TestMethod]
        public void VisualiseBoardTest()
        {
            var board = new Board(GameBoard, TileEdges, Structures);

            var labels = new string[board.Width, board.Height];
            board.Tiles.ToList().ForEach(x => labels[x.X, x.Y] = x.Index.ToString());
            Visualise.GameBoardRenderer.RenderAndSave("Coords - array.png", board.Height, board.Tiles, board.Edges, board.Structures, labels);

            board.Tiles.ToList().ForEach(x => labels[x.X, x.Y] = x.X + ", " + x.Y);
            Visualise.GameBoardRenderer.RenderAndSave("Coords - offset.png", board.Height, board.Tiles, board.Edges, board.Structures, labels);

            board.Tiles.ToList().ForEach(x => labels[x.X, x.Y] = x.Hex.ToString());
            Visualise.GameBoardRenderer.RenderAndSave("Coords - cube.png", board.Height, board.Tiles, board.Edges, board.Structures, labels);

            board.Tiles.ToList().ForEach(x => labels[x.X, x.Y] = x.ContiguousRegionId.ToString());
            Visualise.GameBoardRenderer.RenderAndSave("ContiguousRegionIds.png", board.Height, board.Tiles, board.Edges, board.Structures, labels);
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

            GameBoardRenderer.RenderAndSave("BasicBoardWithUnits.png", board.Height, board.Tiles, board.Edges, board.Structures, units: units);
        }

        [TestMethod]
        public void VisualisePathfind()
        {
            var board = new Board(GameBoard, TileEdges, Structures);

            var unit = new MilitaryUnit(location: board[1, 1]);

            var lines = new List<Centreline>();

            var pathFindTiles = board.ValidMovesWithMoveCostsForUnit(unit);

            lines.AddRange(Centreline.PathFindTilesToCentrelines(Board.FindShortestPath(pathFindTiles, new Point(1, 1), new Point(7, 7), unit.MovementPoints)));
            lines.AddRange(Centreline.PathFindTilesToCentrelines(Board.FindShortestPath(pathFindTiles, new Point(10, 3), new Point(13, 6), unit.MovementPoints)));

            var labels = new string[board.Width, board.Height];
            for (var x = 0; x < board.Width; x++)
            {
                for (var y = 0; y < board.Height; y++)
                {
                    labels[x, y] = board[x, y].Index.ToString();
                }
            }

            GameBoardRenderer.RenderAndSave("BasicBoardPathFind.png", board.Height, board.Tiles, board.Edges, board.Structures, labels, lines);
        }


    }
}
