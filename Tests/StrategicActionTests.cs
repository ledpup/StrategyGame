using ComputerOpponent;
using GameModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;

namespace Tests;

[TestClass]
public class StrategicActionTests
{
    public static string[] GameBoard = File.ReadAllLines("BasicBoard.txt");
    public static string[] TileEdges = File.ReadAllLines("BasicBoardEdges.txt");
    static string[] Settlements = File.ReadAllLines("BasicBoardSettlements.txt");

    GameState gameState;

    [TestInitialize]
    public void TestInitialize()
    {
        gameState = new GameState(new Board(GameBoard, TileEdges));
        gameState.Board.ParseSettlements(Settlements, gameState.Players);
    }

    [TestMethod]
    public void EnemyNearNavelUnitSoDontDock()
    {
        var labels = new string[gameState.Width, gameState.Height];

        gameState.Units =
        [
            new(new UnitTemplate { OperationalDomain = OperationalDomain.Waterbound, MovementPoints = 5, IsTransporter = true }, gameState.Players[0], location: gameState[20, 5]),
            new(new UnitTemplate { OperationalDomain = OperationalDomain.Waterbound, MovementPoints = 3, IsTransporter = true }, gameState.Players[0], location: gameState[18, 7]),
        ];

        var computerPlayer = new ComputerPlayer(gameState.Units);
        computerPlayer.SetStrategicAction(gameState);

        Assert.AreEqual(OperationalAction.None, computerPlayer.GetUnitState(gameState.Units[0]).OperationalAction);
    }

    [TestMethod]
    public void NoEnemyNearNavelUnitSoDock()
    {
        var labels = new string[gameState.Width, gameState.Height];

        var units = new List<MilitaryUnit>
        {
            new(new UnitTemplate { OperationalDomain = OperationalDomain.Waterbound, MovementPoints = 5, IsTransporter = true }, gameState.Players[0], location: gameState[20, 5]),
        };

        gameState.Units = units;

        var computerPlayer = new ComputerPlayer(gameState.Units);
        computerPlayer.SetStrategicAction(gameState);

        Assert.AreEqual(OperationalAction.Dock, computerPlayer.GetUnitState(units[0]).OperationalAction);
    }


    [TestMethod]
    public void EnemyNearAirborneUnitSoDontPickup()
    {
        var labels = new string[gameState.Width, gameState.Height];

        var units = new List<MilitaryUnit>
        {
            new(new UnitTemplate { OperationalDomain = OperationalDomain.Airborne, MovementPoints = 4, IsTransporter = true }, gameState.Players[0], location: gameState[24, 11]),
            new(new UnitTemplate { TransportableBy = [OperationalDomain.Airborne], RoadMovementBonus = 1 }, gameState.Players[0], location: gameState[22, 15]),

            new(new UnitTemplate { OperationalDomain = OperationalDomain.Airborne, MovementPoints = 4, IsTransporter = true }, gameState.Players[0], location: gameState[25, 12]),
        };

        gameState.Units = units;

        var computerPlayer = new ComputerPlayer(gameState.Units);
        computerPlayer.SetStrategicAction(gameState);

        Assert.AreEqual(OperationalAction.None, computerPlayer.GetUnitState(units[0]).OperationalAction);
    }

    [TestMethod]
    public void EnemyNearAirborneUnitSoPickup()
    {
        var labels = new string[gameState.Width, gameState.Height];

        var units = new List<MilitaryUnit>
        {
            new(new UnitTemplate { OperationalDomain = OperationalDomain.Airborne, MovementPoints = 4, IsTransporter = true }, gameState.Players[0], location: gameState[24, 11]),
            new(new UnitTemplate { TransportableBy = [OperationalDomain.Airborne], RoadMovementBonus = 1 }, gameState.Players[0], location: gameState[22, 15]),
        };

        gameState.Units = units;

        var computerPlayer = new ComputerPlayer(gameState.Units);
        computerPlayer.SetStrategicAction(gameState);

        Assert.AreEqual(OperationalAction.Pickup, computerPlayer.GetUnitState(units[0]).OperationalAction);
    }
}
