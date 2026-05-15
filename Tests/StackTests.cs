using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GameModel;
using System.IO;

namespace Tests;

[TestClass]
public class StackTests
{
    static string[] GameBoard = File.ReadAllLines("BasicBoard.txt");
    static string[] Settlements = File.ReadAllLines("BasicBoardSettlements.txt");

    GameState gameState;

    [TestInitialize]
    public void TestInitialize()
    {
        gameState = new GameState(new Board(GameBoard));
        gameState.Board.ParseSettlements(Settlements, gameState.Players);
    }

    [TestMethod]
    public void StackLimits()
    {

        Assert.AreEqual(4, gameState[1, 2].StackLimit);
        Assert.AreEqual(5, gameState[1, 3].StackLimit);
        Assert.AreEqual(2, gameState[6, 1].StackLimit);
    }

    [TestMethod]
    public void OverStackLimit()
    {

        var units = new List<MilitaryUnit>
        {
            new(new UnitTemplate { RoadMovementBonus = 1 }, gameState.Players[0], location: gameState[6, 1]),
            new(new UnitTemplate(), gameState.Players[0], location: gameState[6, 1]),
            new(new UnitTemplate(), gameState.Players[0], location: gameState[6, 1]),
            new(new UnitTemplate(), gameState.Players[0], location: gameState[6, 1]),
        };
        gameState.Units.AddRange(units);

        Assert.AreEqual(2, gameState[6, 1].StackLimit);
        Assert.IsTrue(gameState.OverStackLimit(gameState[6, 1], gameState.Players[0].Id));

        gameState.ResolveStackLimits(gameState.Players[0].Id);

        units.ForEach(x => Assert.AreEqual(4, x.Morale));
    }

    [TestMethod]
    public void CanTransport()
    {
        var inf = new MilitaryUnit(new UnitTemplate { TransportableBy = [OperationalDomain.Airborne] }, gameState.Players[0]);
        var air = new MilitaryUnit(new UnitTemplate { OperationalDomain = OperationalDomain.Airborne, IsTransporter = true }, gameState.Players[0]);


        Assert.IsTrue(air.CanTransport(inf));
    }
}
