using GameModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Tests;

[TestClass]
public class SupplyTests
{
    static string[] GameBoard = File.ReadAllLines("BasicBoard.txt");
    static string[] Edges = File.ReadAllLines("BasicBoardEdges.txt");
    static string[] Settlements = File.ReadAllLines("BasicBoardSettlements.txt");

    [TestMethod]
    public void SupplyTest()
    {
        var gameState = new GameState(new Board(GameBoard, Edges));
        gameState.Board.ParseSettlements(Settlements, gameState.Players);

        var settlementTile = gameState[82];
        settlementTile.Settlement.Owner = gameState.Players[1];
        gameState.Units = [new(new UnitTemplate(), gameState.Players[1], settlementTile)];
        gameState.InitialiseSupply();

        var labels = new string[gameState.Width * gameState.Height];
        gameState.Tiles.ToList().ForEach(x => labels[x.Index] = x.Supply.ToString());

        Visualise.GameBoardRenderer.RenderAndSave("BasicBoardWithSettlementsAndSupply.png", gameState.Width, gameState.Height, gameState.Tiles, gameState.Edges, gameState.Settlements, labels, null, gameState.Units);
    }
}
