using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        public void LandUnitPathFind()
        {
            var board = new Board(GameBoard, TileEdges);

            var unit = new MilitaryUnit(location: board[1, 1]);

            var pathFindTiles = board.ValidMovesWithMoveCostsForUnit(unit);

            Assert.AreEqual(513, pathFindTiles.Count);

            var shortestPath = Board.FindShortestPath(pathFindTiles, new Hex(1, 1), new Hex(5, 5), unit.MovementPoints).ToArray();

            var lines = new List<Centreline>();
            lines.AddRange(Centreline.PathFindTilesToCentrelines(shortestPath));
            GameBoardRenderer.RenderAndSave("LandUnitPathFind.png", board.Width, board.Height, board.Tiles, board.Edges, board.Structures, null, lines);

            Assert.AreEqual(10, shortestPath.Length);

            Assert.AreEqual(shortestPath[0].Hex, new Hex(1, 1)); // Origin

            Assert.AreEqual(shortestPath[1].Hex, new Hex(2, 1));
            Assert.AreEqual(shortestPath[2].Hex, new Hex(3, 1));
            Assert.AreEqual(shortestPath[3].Hex, new Hex(4, 1));
            Assert.AreEqual(shortestPath[4].Hex, new Hex(5, 1)); // There is a road over the mountain
            Assert.AreEqual(shortestPath[5].Hex, new Hex(6, 1));
            Assert.AreEqual(shortestPath[6].Hex, new Hex(6, 2));
            Assert.AreEqual(shortestPath[7].Hex, new Hex(5, 3));
            Assert.AreEqual(shortestPath[8].Hex, new Hex(5, 4));

            Assert.AreEqual(shortestPath[9].Hex, new Hex(5, 5)); // Destination
        }

        [TestMethod]
        public void LandUnitNoPathToDestination()
        {
            var board = new Board(GameBoard, TileEdges);

            var unit = new MilitaryUnit(location: board[1, 1]);

            var pathFindTiles = board.ValidMovesWithMoveCostsForUnit(unit);
            var shortestPath = Board.FindShortestPath(pathFindTiles, unit.Location.Hex, new Hex(4, 7), unit.MovementPoints);

            Assert.IsNull(shortestPath);
        }

        [TestMethod]
        public void NavelUnitMoveToPortPathFind()
        {
            var board = new Board(GameBoard, TileEdges);

            var unit = new MilitaryUnit(location: board[20, 5], movementType: MovementType.Water, baseMovementPoints: 5, isTransporter: true);

            var pathFindTiles = board.ValidMovesWithMoveCostsForUnit(unit);
            var shortestPath = Board.FindShortestPath(pathFindTiles, unit.Location.Hex, new Hex(21, 0), unit.MovementPoints).ToArray();

            var lines = new List<Centreline>();
            lines.AddRange(Centreline.PathFindTilesToCentrelines(shortestPath));
            GameBoardRenderer.RenderAndSave("NavelUnitMoveToPortPathFind.png", board.Width, board.Height, board.Tiles, board.Edges, board.Structures, null, lines);

            Assert.AreEqual(shortestPath[0].Hex, unit.Location.Hex); // Origin

            Assert.AreEqual(shortestPath[1].Hex, new Hex(19, -4));
            Assert.AreEqual(shortestPath[2].Hex, new Hex(18, -3));
            Assert.AreEqual(shortestPath[3].Hex, new Hex(18, -2));
            Assert.AreEqual(shortestPath[4].Hex, new Hex(18, -1));
            Assert.AreEqual(shortestPath[5].Hex, new Hex(19, -1));
            Assert.AreEqual(shortestPath[6].Hex, new Hex(20, -1));
            Assert.AreEqual(shortestPath[7].Hex, new Hex(21, -1));

            Assert.AreEqual(shortestPath[8].Hex, new Hex(21, 0)); // Destination
        }

        [TestMethod]
        public void AirborneUnitMoveOverTerrainThatItCantStopOn()
        {
            var board = new Board(GameBoard, TileEdges);

            var unit = new MilitaryUnit(location: board[24, 15], movementType: MovementType.Airborne, baseMovementPoints: 3, isTransporter: true);

            var pathFindTiles = board.ValidMovesWithMoveCostsForUnit(unit);
            var shortestPath = Board.FindShortestPath(pathFindTiles, unit.Location.Hex, new Hex(14, 6), unit.MovementPoints).ToArray();

            var lines = new List<Centreline>();
            lines.AddRange(Centreline.PathFindTilesToCentrelines(shortestPath));
            GameBoardRenderer.RenderAndSave("AirborneUnitMoveOverTerrainThatItCantStopOn.png", board.Width, board.Height, board.Tiles, board.Edges, board.Structures, null, lines);

            Assert.AreEqual(unit.Location.Hex, shortestPath[0].Hex); // Origin

            Assert.AreEqual(new Hex(23, 3), shortestPath[1].Hex);
            Assert.AreEqual(new Hex(22, 3), shortestPath[2].Hex);
            Assert.AreEqual(new Hex(21, 4), shortestPath[3].Hex);
            Assert.AreEqual(new Hex(20, 4), shortestPath[4].Hex);
            Assert.AreEqual(new Hex(19, 4), shortestPath[5].Hex);
            Assert.AreEqual(new Hex(18, 4), shortestPath[6].Hex);
            Assert.AreEqual(new Hex(17, 5), shortestPath[7].Hex);
            Assert.AreEqual(new Hex(16, 5), shortestPath[8].Hex);
            Assert.AreEqual(new Hex(15, 6), shortestPath[9].Hex);

            Assert.AreEqual(new Hex(14, 6), shortestPath[10].Hex); // Destination
        }

        [TestMethod]
        public void AirborneUnitMoveOverTerrainThatItCantStopOnFromCoastLine()
        {
            var board = new Board(GameBoard, TileEdges);

            var unit = new MilitaryUnit(location: board[19, 13], movementType: MovementType.Airborne, baseMovementPoints: 3, isTransporter: true);

            var pathFindTiles = board.ValidMovesWithMoveCostsForUnit(unit);
            var shortestPath = Board.FindShortestPath(pathFindTiles, unit.Location.Hex, new Hex(14, 6), unit.MovementPoints).ToArray();

            var vectors = new List<Centreline>();
            vectors.AddRange(Centreline.PathFindTilesToCentrelines(shortestPath));
            Visualise.GameBoardRenderer.RenderAndSave("AirborneUnitMoveOverTerrainThatItCantStopOnFromCoastLine.png", board.Width, board.Height, board.Tiles, board.Edges, board.Structures, null, vectors);


            Assert.AreEqual(unit.Location.Hex, shortestPath[0].Hex); // Origin

            Assert.AreEqual(new Hex(18, 4), shortestPath[1].Hex);
            Assert.AreEqual(new Hex(17, 5), shortestPath[2].Hex);
            Assert.AreEqual(new Hex(16, 5), shortestPath[3].Hex);
            Assert.AreEqual(new Hex(15, 6), shortestPath[4].Hex);

            Assert.AreEqual(new Hex(14, 6), shortestPath[5].Hex); // Destination

            var moveOrder = unit.ShortestPathToMoveOrder(shortestPath);

            Assert.IsNotNull(moveOrder);
        }

        [TestMethod]
        public void AirborneUnitMoveOverWall()
        {
            var board = new Board(GameBoard, TileEdges);

            var unit = new MilitaryUnit(location: board[119], movementType: MovementType.Airborne, baseMovementPoints: 5, isTransporter: true);

            var pathFindTiles = board.ValidMovesWithMoveCostsForUnit(unit);
            var shortestPath = Board.FindShortestPath(pathFindTiles, unit.Location.Hex, new Hex(14, -4), unit.MovementPoints).ToArray();

            var vectors = new List<Centreline>();
            vectors.AddRange(Centreline.PathFindTilesToCentrelines(shortestPath));
            GameBoardRenderer.RenderAndSave("AirborneUnitMoveOverWallPathFind.png", board.Width, board.Height, board.Tiles, board.Edges, board.Structures, null, vectors);

            var moveOrder = unit.ShortestPathToMoveOrder(shortestPath);

            vectors = new List<Centreline>();
            vectors.AddRange(Centreline.MoveOrderToCentrelines(moveOrder));

            GameBoardRenderer.RenderAndSave($"AirborneUnitMoveOverWallMoveOrder.png", board.Width, board.Height, board.Tiles, board.Edges, board.Structures, units: board.Units, lines: vectors);


            var moves = unit.PossibleMoves();
            moves.ToList().ForEach(x => x.Edge.Destination.IsSelected = true);
            GameBoardRenderer.RenderAndSave("AirborneUnitMoveOverWallPossibleMoves.png", board.Width, board.Height, board.Tiles, board.Edges, board.Structures);


            Assert.AreEqual(unit.Location.Hex, shortestPath[0].Hex); // Origin

            Assert.AreEqual(new Hex(12, -2), shortestPath[1].Hex);
            Assert.AreEqual(new Hex(12, -3), shortestPath[2].Hex);
            Assert.AreEqual(new Hex(13, -4), shortestPath[3].Hex);

            Assert.AreEqual(new Hex(14, -4), shortestPath[4].Hex); // Destination

            Assert.AreEqual(new Hex(12, -2), moveOrder.Moves[0].Edge.Destination.Hex);
            Assert.AreEqual(new Hex(12, -3), moveOrder.Moves[1].Edge.Destination.Hex);
            Assert.AreEqual(new Hex(13, -4), moveOrder.Moves[2].Edge.Destination.Hex);
            Assert.AreEqual(new Hex(14, -4), moveOrder.Moves[3].Edge.Destination.Hex);
        }
    }
}
