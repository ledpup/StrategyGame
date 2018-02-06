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
using GameModel.Rendering;
using GameRendering2D;

namespace Tests
{
    [TestClass]
    public class PathFindTests
    {
        static string[] GameBoard = File.ReadAllLines("BasicBoard.txt");
        static string[] TileEdges = File.ReadAllLines("BasicBoardEdges.txt");
        static string[] Structures = File.ReadAllLines("BasicBoardStructures.txt");

        [TestMethod]
        public void VisualisePathfind()
        {
            var board = new Board(GameBoard, TileEdges, Structures);

            var unit = new MilitaryUnit(location: board[1, 1]);

            var lines = new List<Centreline>();

            lines.AddRange(Centreline.PathFindTilesToCentrelines(Board.FindShortestPath(board[28], board[196], unit)));
            lines.AddRange(Centreline.PathFindTilesToCentrelines(Board.FindShortestPath(board[91], board[175], unit)));

            var labels = new string[board.Width * board.Height];
            for (var i = 0; i < board.TileArray.Length; i++)
            {
                labels[i] = i.ToString();
            }

            var drawing2d = new GameRenderingEngine2D(board.Width, board.Height);
            GameRenderer.RenderAndSave(drawing2d, "BasicBoardPathFind.png", board.Width, board.Height, board.Tiles, board.Edges, board.Structures, labels, lines);
        }

        [TestMethod]
        public void LandUnitPathFind()
        {
            var board = new Board(GameBoard, TileEdges);

            var unit = new MilitaryUnit(location: board[1, 1]);

            var shortestPath = Board.FindShortestPath(board[1, 1], board[194], unit).ToArray();

            var lines = new List<Centreline>();
            lines.AddRange(Centreline.PathFindTilesToCentrelines(shortestPath));
            var drawing2d = new GameRenderingEngine2D(board.Width, board.Height);
            GameRenderer.RenderAndSave(drawing2d, "LandUnitPathFind.png", board.Width, board.Height, board.Tiles, board.Edges, board.Structures, null, lines);

            Assert.AreEqual(10, shortestPath.Length);

            Assert.AreEqual(new Hex(1, 1), shortestPath[0].Hex); // Origin

            Assert.AreEqual(new Hex(2, 1), shortestPath[1].Hex);
            Assert.AreEqual(new Hex(3, 1), shortestPath[2].Hex);
            Assert.AreEqual(new Hex(4, 1), shortestPath[3].Hex);
            Assert.AreEqual(new Hex(5, 1), shortestPath[4].Hex); // There is a road over the mountain
            Assert.AreEqual(new Hex(6, 1), shortestPath[5].Hex);
            Assert.AreEqual(new Hex(6, 2), shortestPath[6].Hex);
            Assert.AreEqual(new Hex(5, 3), shortestPath[7].Hex);
            Assert.AreEqual(new Hex(5, 4), shortestPath[8].Hex);

            Assert.AreEqual(new Hex(5, 5), shortestPath[9].Hex); // Destination
        }

        [TestMethod]
        public void LandUnitNoPathToDestination()
        {
            var board = new Board(GameBoard, TileEdges);

            var unit = new MilitaryUnit(location: board[1, 1]);

            var shortestPath = Board.FindShortestPath(unit.Location, board[247], unit);

            Assert.IsNull(shortestPath);
        }

        [TestMethod]
        public void NavelUnitMoveToPortPathFind()
        {
            var board = new Board(GameBoard, TileEdges);

            var unit = new MilitaryUnit(location: board[20, 5], movementType: MovementType.Water, baseMovementPoints: 5, isTransporter: true);

            var shortestPath = Board.FindShortestPath(unit.Location, board[291], unit).ToArray();

            var lines = new List<Centreline>();
            lines.AddRange(Centreline.PathFindTilesToCentrelines(shortestPath));
            var drawing2d = new GameRenderingEngine2D(board.Width, board.Height);
            GameRenderer.RenderAndSave(drawing2d, "NavelUnitMoveToPortPathFind.png", board.Width, board.Height, board.Tiles, board.Edges, board.Structures, null, lines);

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

            var shortestPath = Board.FindShortestPath(unit.Location, board[365], unit).ToArray();

            var lines = new List<Centreline>();
            lines.AddRange(Centreline.PathFindTilesToCentrelines(shortestPath));
            var drawing2d = new GameRenderingEngine2D(board.Width, board.Height);
            GameRenderer.RenderAndSave(drawing2d, "AirborneUnitMoveOverTerrainThatItCantStopOn.png", board.Width, board.Height, board.Tiles, board.Edges, board.Structures, null, lines);

            Assert.AreEqual(unit.Location.Hex, shortestPath[0].Hex); // Origin

            Assert.AreEqual(new Hex(23, 3), shortestPath[1].Hex);
            Assert.AreEqual(new Hex(22, 3), shortestPath[2].Hex);
            Assert.AreEqual(new Hex(21, 4), shortestPath[3].Hex);
            Assert.AreEqual(new Hex(20, 4), shortestPath[4].Hex);
            Assert.AreEqual(new Hex(19, 4), shortestPath[5].Hex);
            Assert.AreEqual(new Hex(18, 4), shortestPath[6].Hex);
            Assert.AreEqual(new Hex(17, 4), shortestPath[7].Hex);
            Assert.AreEqual(new Hex(16, 4), shortestPath[8].Hex);
            Assert.AreEqual(new Hex(15, 5), shortestPath[9].Hex);

            Assert.AreEqual(new Hex(14, 6), shortestPath[10].Hex); // Destination
        }

        [TestMethod]
        public void AirborneUnitMoveOverTerrainThatItCantStopOnFromCoastLine()
        {
            var board = new Board(GameBoard, TileEdges);

            var unit = new MilitaryUnit(location: board[19, 13], movementType: MovementType.Airborne, baseMovementPoints: 3, isTransporter: true);

            var shortestPath = Board.FindShortestPath(unit.Location, board[365], unit).ToArray();

            var vectors = new List<Centreline>();
            vectors.AddRange(Centreline.PathFindTilesToCentrelines(shortestPath));
            var drawing2d = new GameRenderingEngine2D(board.Width, board.Height);
            GameRenderer.RenderAndSave(drawing2d, "AirborneUnitMoveOverTerrainThatItCantStopOnFromCoastLine.png", board.Width, board.Height, board.Tiles, board.Edges, board.Structures, null, vectors);


            Assert.AreEqual(unit.Location.Hex, shortestPath[0].Hex); // Origin

            Assert.AreEqual(new Hex(18, 4), shortestPath[1].Hex);
            Assert.AreEqual(new Hex(17, 4), shortestPath[2].Hex);
            Assert.AreEqual(new Hex(16, 4), shortestPath[3].Hex);
            Assert.AreEqual(new Hex(15, 5), shortestPath[4].Hex);

            Assert.AreEqual(new Hex(14, 6), shortestPath[5].Hex); // Destination

            var moveOrder = unit.ShortestPathToMoveOrder(shortestPath);

            Assert.IsNotNull(moveOrder);
        }

        [TestMethod]
        public void AirborneUnitMoveOverWall()
        {
            var board = new Board(GameBoard, TileEdges);

            var unit = new MilitaryUnit(location: board[119], movementType: MovementType.Airborne, baseMovementPoints: 5, isTransporter: true);

            var shortestPath = Board.FindShortestPath(unit.Location, board[95], unit).ToArray();

            var vectors = new List<Centreline>();
            vectors.AddRange(Centreline.PathFindTilesToCentrelines(shortestPath));
            var drawing2d = new GameRenderingEngine2D(board.Width, board.Height);
            GameRenderer.RenderAndSave(drawing2d, "AirborneUnitMoveOverWallPathFind.png", board.Width, board.Height, board.Tiles, board.Edges, board.Structures, null, vectors);

            var moveOrder = unit.ShortestPathToMoveOrder(shortestPath);

            vectors = new List<Centreline>();
            vectors.AddRange(Centreline.MoveOrderToCentrelines(moveOrder));

            GameRenderer.RenderAndSave(drawing2d, $"AirborneUnitMoveOverWallMoveOrder.png", board.Width, board.Height, board.Tiles, board.Edges, board.Structures, units: board.Units, lines: vectors);

            var moves = unit.PossibleMoves();
            moves.ToList().ForEach(x => x.Edge.Destination.IsSelected = true);
            
            GameRenderer.RenderAndSave(drawing2d, "AirborneUnitMoveOverWallPossibleMoves.png", board.Width, board.Height, board.Tiles, board.Edges, board.Structures);


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
