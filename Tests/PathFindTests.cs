﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using GameModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hexagon;
using ComputerOpponent;
using Visualise;

namespace Tests
{
    

    [TestClass]
    public class PathFindTests
    {
        static string[] GameBoard = File.ReadAllLines("BasicBoard.txt");
        static string[] TileEdges = File.ReadAllLines("BasicBoardEdges.txt");


        [TestMethod]
        public void LandUnitPath()
        {
            var board = new Board(GameBoard, TileEdges);

            var unit = new MilitaryUnit(location: board[1, 1]);

            var pathFindTiles = board.ValidMovesWithMoveCostsForUnit(unit);

            Assert.AreEqual(513, pathFindTiles.Count);

            var shortestPath = Board.FindShortestPath(pathFindTiles, new Point(1, 1), new Point(5, 7), unit.MovementPoints).ToArray();

            Assert.AreEqual(shortestPath[0].Point, new Point(1, 1)); // Origin

            Assert.AreEqual(shortestPath[1].Point, new Point(2, 2));
            Assert.AreEqual(shortestPath[2].Point, new Point(3, 2));
            Assert.AreEqual(shortestPath[3].Point, new Point(4, 3));
            Assert.AreEqual(shortestPath[4].Point, new Point(5, 3));
            Assert.AreEqual(shortestPath[5].Point, new Point(6, 4));
            Assert.AreEqual(shortestPath[6].Point, new Point(6, 5));
            Assert.AreEqual(shortestPath[7].Point, new Point(5, 5));
            Assert.AreEqual(shortestPath[8].Point, new Point(5, 6));

            Assert.AreEqual(shortestPath[9].Point, new Point(5, 7)); // Destination
        }

        [TestMethod]
        public void LandUnitNoPathToDestination()
        {
            var board = new Board(GameBoard, TileEdges);

            var unit = new MilitaryUnit(location: board[1, 1]);

            var pathFindTiles = board.ValidMovesWithMoveCostsForUnit(unit);
            var shortestPath = Board.FindShortestPath(pathFindTiles, unit.Location.Point, new Point(4, 9), unit.MovementPoints);

            Assert.IsNull(shortestPath);
        }

        [TestMethod]
        public void NavelUnitMoveToPort()
        {
            var board = new Board(GameBoard, TileEdges);

            var unit = new MilitaryUnit(location: board[20, 5], movementType: MovementType.Water, baseMovementPoints: 5, isTransporter: true);

            var pathFindTiles = board.ValidMovesWithMoveCostsForUnit(unit);
            var shortestPath = Board.FindShortestPath(pathFindTiles, unit.Location.Point, new Point(21, 10), unit.MovementPoints).ToArray();

            Assert.AreEqual(shortestPath[0].Point, unit.Location.Point); // Origin

            Assert.AreEqual(shortestPath[1].Point, new Point(19, 5));
            Assert.AreEqual(shortestPath[2].Point, new Point(18, 6));
            Assert.AreEqual(shortestPath[3].Point, new Point(18, 7));
            Assert.AreEqual(shortestPath[4].Point, new Point(18, 8));
            Assert.AreEqual(shortestPath[5].Point, new Point(19, 8));
            Assert.AreEqual(shortestPath[6].Point, new Point(20, 9));
            Assert.AreEqual(shortestPath[7].Point, new Point(21, 9));

            Assert.AreEqual(shortestPath[8].Point, new Point(21, 10)); // Destination
        }

        [TestMethod]
        public void AirborneUnitMoveOverTerrainThatItCantStopOn()
        {
            var board = new Board(GameBoard, TileEdges);

            var unit = new MilitaryUnit(location: board[24, 15], movementType: MovementType.Airborne, baseMovementPoints: 3, isTransporter: true);

            var pathFindTiles = board.ValidMovesWithMoveCostsForUnit(unit);
            var shortestPath = Board.FindShortestPath(pathFindTiles, unit.Location.Point, new Point(14, 13), unit.MovementPoints).ToArray();

            var vectors = new List<Centreline>();
            vectors.AddRange(Centreline.PathFindTilesToCentrelines(shortestPath));
            GameBoardRenderer.RenderAndSave("AirborneUnitMoveOverTerrainThatItCantStopOn.png", board.Height, board.Tiles, board.Edges, board.Structures, null, vectors);

            Assert.AreEqual(unit.Location.Point, shortestPath[0].Point); // Origin

            Assert.AreEqual(new Point(23, 14), shortestPath[1].Point);
            Assert.AreEqual(new Point(22, 14), shortestPath[2].Point);
            Assert.AreEqual(new Point(21, 14), shortestPath[3].Point);
            Assert.AreEqual(new Point(20, 14), shortestPath[4].Point);
            Assert.AreEqual(new Point(19, 13), shortestPath[5].Point);
            Assert.AreEqual(new Point(18, 13), shortestPath[6].Point);
            Assert.AreEqual(new Point(17, 13), shortestPath[7].Point);
            Assert.AreEqual(new Point(16, 13), shortestPath[8].Point);
            Assert.AreEqual(new Point(15, 13), shortestPath[9].Point);

            Assert.AreEqual(new Point(14, 13), shortestPath[10].Point); // Destination
        }

        [TestMethod]
        public void AirborneUnitMoveOverTerrainThatItCantStopOnFromCoastLine()
        {
            var board = new Board(GameBoard, TileEdges);

            var unit = new MilitaryUnit(location: board[19, 13], movementType: MovementType.Airborne, baseMovementPoints: 3, isTransporter: true);

            var pathFindTiles = board.ValidMovesWithMoveCostsForUnit(unit);
            var shortestPath = Board.FindShortestPath(pathFindTiles, unit.Location.Point, new Point(14, 13), unit.MovementPoints).ToArray();

            var vectors = new List<Centreline>();
            vectors.AddRange(Centreline.PathFindTilesToCentrelines(shortestPath));
            Visualise.GameBoardRenderer.RenderAndSave("AirborneUnitMoveOverTerrainThatItCantStopOnFromCoastLine.png", board.Height, board.Tiles, board.Edges, board.Structures, null, vectors);


            Assert.AreEqual(unit.Location.Point, shortestPath[0].Point); // Origin

            Assert.AreEqual(new Point(18, 13), shortestPath[1].Point);
            Assert.AreEqual(new Point(17, 13), shortestPath[2].Point);
            Assert.AreEqual(new Point(16, 13), shortestPath[3].Point);
            Assert.AreEqual(new Point(15, 13), shortestPath[4].Point);

            Assert.AreEqual(new Point(14, 13), shortestPath[5].Point); // Destination

            var moveOrder = unit.ShortestPathToMoveOrder(shortestPath);

            Assert.IsNotNull(moveOrder);
        }

        [TestMethod]
        public void AirborneUnitMoveOverWall()
        {
            var board = new Board(GameBoard, TileEdges);

            var unit = new MilitaryUnit(location: board[119], movementType: MovementType.Airborne, baseMovementPoints: 5, isTransporter: true);

            var pathFindTiles = board.ValidMovesWithMoveCostsForUnit(unit);
            var shortestPath = Board.FindShortestPath(pathFindTiles, unit.Location.Point, new Point(14, 3), unit.MovementPoints).ToArray();

            var vectors = new List<Centreline>();
            vectors.AddRange(Centreline.PathFindTilesToCentrelines(shortestPath));
            Visualise.GameBoardRenderer.RenderAndSave("AirborneUnitMoveOverWallPathFind.png", board.Height, board.Tiles, board.Edges, board.Structures, null, vectors);

            var moveOrder = unit.ShortestPathToMoveOrder(shortestPath);

            vectors = new List<Centreline>();
            vectors.AddRange(Centreline.MoveOrderToCentrelines(moveOrder));

            Visualise.GameBoardRenderer.RenderAndSave($"AirborneUnitMoveOverWallMoveOrder.png", board.Height, board.Tiles, board.Edges, board.Structures, units: board.Units, lines: vectors);


            var moves = unit.PossibleMoves();
            moves.ToList().ForEach(x => x.Neighbour.Tile.IsSelected = true);
            Visualise.GameBoardRenderer.RenderAndSave("AirborneUnitMoveOverWallPossibleMoves.png", board.Height, board.Tiles, board.Edges, board.Structures);


            Assert.AreEqual(unit.Location.Point, shortestPath[0].Point); // Origin

            Assert.AreEqual(new Point(12, 4), shortestPath[1].Point);
            Assert.AreEqual(new Point(13, 4), shortestPath[2].Point);
            Assert.AreEqual(new Point(13, 3), shortestPath[3].Point);

            Assert.AreEqual(new Point(14, 3), shortestPath[4].Point); // Destination

            Assert.AreEqual(new Point(12, 4), moveOrder.Moves[0].Neighbour.Tile.Point);
            Assert.AreEqual(new Point(13, 4), moveOrder.Moves[1].Neighbour.Tile.Point);
            Assert.AreEqual(new Point(13, 3), moveOrder.Moves[2].Neighbour.Tile.Point);
            Assert.AreEqual(new Point(14, 3), moveOrder.Moves[3].Neighbour.Tile.Point);
        }
    }
}
