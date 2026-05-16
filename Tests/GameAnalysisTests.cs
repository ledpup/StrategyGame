using ComputerOpponent;
using GameModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Tests;

[TestClass]
public class GameAnalysisTests
{
    [TestMethod]
    public void CalculateObjectiveFunction_BasicGameState_CompletesWithoutError()
    {
        var gameAnalysis = new GameAnalysis();

        var players = new[]
        {
            new Player(PlayerColour.Red, "Red"),
            new Player(PlayerColour.Blue, "Blue"),
        };

        var settlements = new List<Settlement>
        {
            new(SettlementType.Fortress, null, players[0], 1),
            new(SettlementType.Outpost, null, players[0], 1),

            new(SettlementType.City, null, players[1], 2),
        };

        var units = new List<MilitaryUnit>
        {
            new(new UnitTemplate { Quality = 2, Personnel = 300 }, players[0], null, "1st Infantry"),
            new(new UnitTemplate { Quality = 2, Personnel = 500 }, players[1], null, "1st Blue Infantry"),
        };

        gameAnalysis.CalculateObjectiveFunction(players, settlements, units);
    }
}


