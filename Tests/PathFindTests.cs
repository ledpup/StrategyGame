using Microsoft.VisualStudio.TestTools.UnitTesting;
using GameModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hexagon;
using Visualise;

namespace Tests;

[TestClass]
public class PathFindTests
{
    static string[] GameBoard = File.ReadAllLines("BasicBoard.txt");
    static string[] TileEdges = File.ReadAllLines("BasicBoardEdges.txt");
    static string[] Settlements = File.ReadAllLines("BasicBoardSettlements.txt");

    GameState gameState;

    [TestInitialize]
    public void TestInitialize()
    {
        gameState = new GameState(new Board(GameBoard, TileEdges));
    }

    [TestMethod]
    public void RenderPathfind_ValidBoard_RendersPathsWithoutError()
    {
        var unit = new MilitaryUnit(new UnitTemplate(), 1, gameState.Players[0], location: gameState[1, 1]);

        var lines = new List<Centreline>();

        lines.AddRange(Centreline.PathFindTilesToCentrelines(PathFinder.FindShortestPath(gameState[28], gameState[196], unit)));
        lines.AddRange(Centreline.PathFindTilesToCentrelines(PathFinder.FindShortestPath(gameState[91], gameState[175], unit)));

        var labels = new string[gameState.Width * gameState.Height];
        for (var i = 0; i < gameState.Tiles.Length; i++)
        {
            labels[i] = i.ToString();
        }

        GameBoardRenderer.RenderAndSave("BasicBoardPathFind.png", gameState.Width, gameState.Height, gameState.Tiles, gameState.Edges, gameState.Settlements, labels, lines);
    }

    [TestMethod]
    public void FindShortestPath_LandUnitWithReachableDestination_ReturnsExpectedPath()
    {

        var unit = new MilitaryUnit(new UnitTemplate(), 1, gameState.Players[0], location: gameState[1, 1]);

        var shortestPath = PathFinder.FindShortestPath(gameState[1, 1], gameState[194], unit).ToArray();

        var lines = new List<Centreline>();
        lines.AddRange(Centreline.PathFindTilesToCentrelines(shortestPath));
        GameBoardRenderer.RenderAndSave("LandUnitPathFind.png", gameState.Width, gameState.Height, gameState.Tiles, gameState.Edges, gameState.Settlements, null, lines);

        Assert.HasCount(10, shortestPath);

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
    public void FindShortestPath_LandUnitWithUnreachableDestination_ReturnsNull()
    {

        var unit = new MilitaryUnit(new UnitTemplate(), 1, gameState.Players[0], location: gameState[1, 1]);

        var shortestPath = PathFinder.FindShortestPath(unit.Location, gameState[247], unit);

        Assert.IsNull(shortestPath);
    }

    [TestMethod]
    public void FindShortestPath_WaterboundUnitMovingToPort_ReturnsExpectedPath()
    {

        var unit = new MilitaryUnit(new UnitTemplate { MovementType = MovementType.Waterbound, MovementPoints = 5, IsTransporter = true }, 1, gameState.Players[0], location: gameState[20, 5]);

        var shortestPath = PathFinder.FindShortestPath(unit.Location, gameState[291], unit).ToArray();

        var lines = new List<Centreline>();
        lines.AddRange(Centreline.PathFindTilesToCentrelines(shortestPath));
        GameBoardRenderer.RenderAndSave("NavelUnitMoveToPortPathFind.png", gameState.Width, gameState.Height, gameState.Tiles, gameState.Edges, gameState.Settlements, null, lines);

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
    public void FindShortestPath_AirborneUnitCrossingNonStoppableTerrain_ReturnsExpectedPath()
    {

        var unit = new MilitaryUnit(new UnitTemplate { MovementType = MovementType.Airborne, MovementPoints = 3, IsTransporter = true }, 1, gameState.Players[0], location: gameState[24, 15]);

        var shortestPath = PathFinder.FindShortestPath(unit.Location, gameState[365], unit).ToArray();

        var lines = new List<Centreline>();
        lines.AddRange(Centreline.PathFindTilesToCentrelines(shortestPath));
        GameBoardRenderer.RenderAndSave("AirborneUnitMoveOverTerrainThatItCantStopOn.png", gameState.Width, gameState.Height, gameState.Tiles, gameState.Edges, gameState.Settlements, null, lines);

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
    public void FindShortestPath_AirborneUnitFromCoastlineCrossingNonStoppableTerrain_ReturnsPathAndMoveOrder()
    {

        var unit = new MilitaryUnit(new UnitTemplate { MovementType = MovementType.Airborne, MovementPoints = 3, IsTransporter = true }, 1, gameState.Players[0], location: gameState[19, 13]);

        var shortestPath = PathFinder.FindShortestPath(unit.Location, gameState[365], unit).ToArray();

        var vectors = new List<Centreline>();
        vectors.AddRange(Centreline.PathFindTilesToCentrelines(shortestPath));
        Visualise.GameBoardRenderer.RenderAndSave("AirborneUnitMoveOverTerrainThatItCantStopOnFromCoastLine.png", gameState.Width, gameState.Height, gameState.Tiles, gameState.Edges, gameState.Settlements, null, vectors);


        Assert.AreEqual(unit.Location.Hex, shortestPath[0].Hex); // Origin

        Assert.AreEqual(new Hex(18, 4), shortestPath[1].Hex);
        Assert.AreEqual(new Hex(17, 4), shortestPath[2].Hex);
        Assert.AreEqual(new Hex(16, 4), shortestPath[3].Hex);
        Assert.AreEqual(new Hex(15, 5), shortestPath[4].Hex);

        Assert.AreEqual(new Hex(14, 6), shortestPath[5].Hex); // Destination

        var moveOrder = unit.ShortestPathToMoveCommand(shortestPath);

        Assert.IsNotNull(moveOrder);
    }

    [TestMethod]
    public void FindShortestPath_AirborneUnitCrossingWall_ReturnsValidWallCrossingPath()
    {

        var unit = new MilitaryUnit(new UnitTemplate { MovementType = MovementType.Airborne, MovementPoints = 5, IsTransporter = true }, 1, gameState.Players[0], location: gameState[119]);

        var shortestPath = PathFinder.FindShortestPath(unit.Location, gameState[95], unit).ToArray();

        var vectors = new List<Centreline>();
        vectors.AddRange(Centreline.PathFindTilesToCentrelines(shortestPath));
        GameBoardRenderer.RenderAndSave("AirborneUnitMoveOverWallPathFind.png", gameState.Width, gameState.Height, gameState.Tiles, gameState.Edges, gameState.Settlements, null, vectors);

        var moveOrder = unit.ShortestPathToMoveCommand(shortestPath);

        vectors = [.. Centreline.MoveOrderToCentrelines(moveOrder)];

        GameBoardRenderer.RenderAndSave($"AirborneUnitMoveOverWallMoveOrder.png", gameState.Width, gameState.Height, gameState.Tiles, gameState.Edges, gameState.Settlements, units: gameState.Units, lines: vectors);


        var moves = unit.PossibleMoves();
        moves.ToList().ForEach(x => x.Edge.Destination.IsSelected = true);
        GameBoardRenderer.RenderAndSave("AirborneUnitMoveOverWallPossibleMoves.png", gameState.Width, gameState.Height, gameState.Tiles, gameState.Edges, gameState.Settlements);


        Assert.AreEqual(unit.Location.Hex, shortestPath[0].Hex); // Origin
        Assert.AreEqual(new Hex(14, -4), shortestPath[^1].Hex); // Destination

        Assert.IsNotNull(moveOrder);
        Assert.HasCount(shortestPath.Length - 1, moveOrder.Moves);
        Assert.AreEqual(new Hex(14, -4), moveOrder.Moves[^1].Edge.Destination.Hex);

        // Airborne pathing should be able to cross wall edges.
        Assert.Contains(x => x.Edge.EdgeType == EdgeType.Wall, moveOrder.Moves);
    }
}
