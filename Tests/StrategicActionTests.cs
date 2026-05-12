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

    [TestMethod]
    public void EnemyNearNavelUnitSoDontDock()
    {
        var board = new GameState(new Board(GameBoard, TileEdges, Settlements));
        var labels = new string[board.Width, board.Height];

        board.Units =
        [
            new(new UnitTemplate { MovementType = MovementType.Waterbound, MovementPoints = 5, IsTransporter = true }, 0, location: board[20, 5]),
            new(new UnitTemplate { MovementType = MovementType.Waterbound, MovementPoints = 3, IsTransporter = true }, 1, 1, board[18, 7]),
        ];

        var computerPlayer = new ComputerPlayer(board.Units);
        computerPlayer.SetStrategicAction(board);

        Assert.AreEqual(OperationalAction.None, computerPlayer.GetUnitState(board.Units[0]).StrategicAction);
    }

    [TestMethod]
    public void NoEnemyNearNavelUnitSoDock()
    {
        var board = new GameState(new Board(GameBoard, TileEdges, Settlements));
        var labels = new string[board.Width, board.Height];

        var units = new List<MilitaryUnit>
        {
            new(new UnitTemplate { MovementType = MovementType.Waterbound, MovementPoints = 5, IsTransporter = true }, 0, location: board[20, 5]),
        };

        board.Units = units;

        var computerPlayer = new ComputerPlayer(board.Units);
        computerPlayer.SetStrategicAction(board);

        Assert.AreEqual(OperationalAction.Dock, computerPlayer.GetUnitState(units[0]).StrategicAction);
    }


    [TestMethod]
    public void EnemyNearAirborneUnitSoDontPickup()
    {
        var board = new GameState(new Board(GameBoard, TileEdges, Settlements));
        var labels = new string[board.Width, board.Height];

        var units = new List<MilitaryUnit>
        {
            new(new UnitTemplate { MovementType = MovementType.Airborne, MovementPoints = 4, IsTransporter = true }, 0, location: board[24, 11]),
            new(new UnitTemplate { TransportableBy = [MovementType.Airborne], RoadMovementBonus = 1 }, 1, location: board[22, 15]),

            new(new UnitTemplate { MovementType = MovementType.Airborne, MovementPoints = 4, IsTransporter = true }, 2, 1, board[25, 12]),
        };

        board.Units = units;

        var computerPlayer = new ComputerPlayer(board.Units);
        computerPlayer.SetStrategicAction(board);

        Assert.AreEqual(OperationalAction.None, computerPlayer.GetUnitState(units[0]).StrategicAction);
    }

    [TestMethod]
    public void EnemyNearAirborneUnitSoPickup()
    {
        var board = new GameState(new Board(GameBoard, TileEdges, Settlements));
        var labels = new string[board.Width, board.Height];

        var units = new List<MilitaryUnit>
        {
            new(new UnitTemplate { MovementType = MovementType.Airborne, MovementPoints = 4, IsTransporter = true }, 0, location: board[24, 11]),
            new(new UnitTemplate { TransportableBy = [MovementType.Airborne], RoadMovementBonus = 1 }, 1, location: board[22, 15]),
        };

        board.Units = units;

        var computerPlayer = new ComputerPlayer(board.Units);
        computerPlayer.SetStrategicAction(board);

        Assert.AreEqual(OperationalAction.Pickup, computerPlayer.GetUnitState(units[0]).StrategicAction);
    }
}
