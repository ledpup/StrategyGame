using Microsoft.VisualStudio.TestTools.UnitTesting;
using ComputerOpponent;
using GameModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Visualise;

namespace Tests;



[TestClass]
public class VisualiseTests
{
    static string[] GameBoard = File.ReadAllLines("BasicBoard.txt");
    static string[] TileEdges = File.ReadAllLines("BasicBoardEdges.txt");
    static string[] Settlements = File.ReadAllLines("BasicBoardSettlements.txt");

    Func<PathFindTile, PathFindTile, double> distance = (node1, node2) => node1.MoveCost[node2.Hex];

    GameState gameState;

    [TestInitialize]
    public void TestInitialize()
    {
        gameState = new GameState(new Board(GameBoard, TileEdges));
        gameState.Board.ParseSettlements(Settlements, gameState.Players);
        ConnectedRegionCalculator.Calculate(gameState.Board);
    }

    [TestMethod]
    public void RenderAndSave_BasicBoard_RendersCoordinateMaps()
    {

        var labels = new string[gameState.Width * gameState.Height];
        gameState.Tiles.ToList().ForEach(x => labels[x.Index] = x.Index.ToString());
        GameBoardRenderer.RenderAndSave("Visualise/Coords - index.png", gameState.Width, gameState.Height, gameState.Tiles, gameState.Edges, gameState.Settlements, labels);

        gameState.Tiles.ToList().ForEach(x => labels[x.Index] = x.ToOffsetCoordsString());
        GameBoardRenderer.RenderAndSave("Visualise/Coords - offset.png", gameState.Width, gameState.Height, gameState.Tiles, gameState.Edges, gameState.Settlements, labels);

        gameState.Tiles.ToList().ForEach(x => labels[x.Index] = x.Hex.ToString());
        GameBoardRenderer.RenderAndSave("Visualise/Coords - cube.png", gameState.Width, gameState.Height, gameState.Tiles, gameState.Edges, gameState.Settlements, labels);

        gameState.Tiles.ToList().ForEach(x => labels[x.Index] = x.ConnectedRegionId.ToString());
        GameBoardRenderer.RenderAndSave("Visualise/ConnectedRegionIds.png", gameState.Width, gameState.Height, gameState.Tiles, gameState.Edges, gameState.Settlements, labels);
    }

    [TestMethod]
    public void RenderAndSave_UnitsOnBoard_RendersUnits()
    {

        var units = new List<MilitaryUnit>
        {
            new(new UnitTemplate(), gameState.Players[0], gameState[1, 1]),
            new(new UnitTemplate(), gameState.Players[0], gameState[1, 1]),
            new(new UnitTemplate(), gameState.Players[1], gameState[1, 1]),
            new(new UnitTemplate(), gameState.Players[2], gameState[1, 1])
        };

        GameBoardRenderer.RenderAndSave("Visualise/BasicBoardWithUnits.png", gameState.Width, gameState.Height, gameState.Tiles, gameState.Edges, gameState.Settlements, units: units);
    }
}

