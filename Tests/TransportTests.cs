using ComputerOpponent;
using GameModel;
using GameModel.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Visualise;

namespace Tests;

[TestClass]
public class TransportTests
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
    public void CreateOrders_WaterTransportScenario_FollowsExpectedPortRoute()
    {
        var numberOfPlayers = 2;
        var labels = new string[gameState.Width, gameState.Height];

        var units = new List<MilitaryUnit>
        {
            new(new UnitTemplate { OperationalDomain = OperationalDomain.Waterbound, MovementPoints = 5, IsTransporter = true }, gameState.Players[0], location: gameState[20, 5]),
            new(new UnitTemplate { OperationalDomain = OperationalDomain.Waterbound, MovementPoints = 3, IsTransporter = true }, gameState.Players[1], location: gameState[3, 10]),
            new(new UnitTemplate { TransportableBy = [OperationalDomain.Waterbound], RoadMovementBonus = 1 }, gameState.Players[0], location: gameState[24, 16]),
            new(new UnitTemplate { TransportableBy = [OperationalDomain.Waterbound] }, gameState.Players[1], location: gameState[1, 1]),
            new(new UnitTemplate { TransportableBy = [OperationalDomain.Waterbound] }, gameState.Players[0], location: gameState[1, 1]),
        };

        units[3].TerrainMovementCosts[TerrainType.Swamp] = 1;
        units[3].EdgeMovementCosts[EdgeType.River] = 0;

        gameState.Units = units;

        var computerPlayer = new ComputerPlayer(new Dictionary<MilitaryUnit, Role>
        {
            [units[0]] = Role.Besieger,
            [units[1]] = Role.Besieger,
            [units[2]] = Role.Balanced,
            [units[3]] = Role.Defensive,
            [units[4]] = Role.Besieger,
        });

        for (gameState.Turn = 0; gameState.Turn < 30; gameState.Turn++)
        {
            computerPlayer.GenerateInfluenceMaps(gameState, numberOfPlayers);

            GameBoardRenderer.Render(RenderPipeline.Board, RenderPipeline.Units, gameState.Width, gameState.Height, gameState.Tiles, gameState.Edges, gameState.Settlements, null, null, gameState.Units);

            // Remove any units that have been destroyed for the purposes of unit orders
            var aliveUnits = gameState.Units.Where(x => x.IsAlive).ToList();
            computerPlayer.SetStrategicAction(gameState);
            var moveOrders = computerPlayer.CreateOrders(gameState, aliveUnits);
            var lines = new List<Centreline>();
            moveOrders.ForEach(x => lines.AddRange(Centreline.MoveOrderToCentrelines((MoveCommand)x)));

            GameBoardRenderer.RenderAndSave($"Transport/PortsTurn{gameState.Turn}.png", gameState.Width, gameState.Height, gameState.Tiles, gameState.Edges, gameState.Settlements, null, lines, gameState.Units);

            gameState.ResolveOrders(moveOrders);
            gameState.ChangeSettlementOwners();

            switch (gameState.Turn)
            {
                case 0:
                    Assert.AreEqual(gameState[23, 13], units[2].Location);
                    break;
                case 1:
                    Assert.AreEqual(gameState[21, 11], units[2].Location);
                    break;
                case 2:
                    Assert.AreEqual(gameState[21, 10], units[0].Location);
                    Assert.AreEqual(gameState[21, 10], units[2].Location);
                    break;
                case 3:
                    Assert.AreEqual(gameState[18, 7], units[0].Location);
                    Assert.AreEqual(gameState[18, 7], units[2].Location);
                    break;
                case 5:
                    Assert.AreEqual(gameState[17, 2], units[0].Location);
                    Assert.AreEqual(gameState[21, 4], units[2].Location);
                    break;
            }
        }
    }

    [TestMethod]
    public void ResolveOrders_AirTransportCommand_TransportsUnitWithAirborneUnit()
    {

        var units = new List<MilitaryUnit>
        {
            new(new UnitTemplate { RoadMovementBonus = 1, TransportableBy = [OperationalDomain.Airborne] }, gameState.Players[0], location: gameState[1, 1]),
            new(new UnitTemplate { OperationalDomain = OperationalDomain.Airborne, MovementPoints = 3, IsTransporter = true }, gameState.Players[0], location: gameState[1, 1]),
        };

        gameState.Units = units;

        var moves = new Move[]
        {
            new(gameState[1, 1], gameState[2, 2], null, 2, 1),
            new(gameState[2, 2], gameState[3, 2], null, 1, 2),
        };

        var unitOrders = new List<IUnitCommand>
        {
            new MoveCommand(moves, units[1]),
            new TransportCommand(units[1], units[0]),

        };
        gameState.ResolveOrders(unitOrders);

        Assert.AreEqual(units[0], units[1].Transporting.Single());
        Assert.AreEqual(units[1], units[0].TransportedBy);

        Assert.AreEqual(gameState[3, 2], units[0].Location);
        Assert.AreEqual(gameState[3, 2], units[1].Location);
    }

    [TestMethod]
    public void ResolveOrders_TransportedUnitGivenMoveCommand_ThrowsException()
    {

        var units = new List<MilitaryUnit>
        {
            new(new UnitTemplate { RoadMovementBonus = 1 }, gameState.Players[0], location: gameState[1, 1]),
            new(new UnitTemplate { OperationalDomain = OperationalDomain.Airborne, IsTransporter = true }, gameState.Players[0], location: gameState[1, 1]),
        };

        gameState.Units = units;

        var moves = new Move[]
        {
            new(gameState[1, 1], gameState[2, 2], null, 2, 1),
            new(gameState[2, 2], gameState[3, 2], null, 1, 2),
        };

        var unitOrders = new List<IUnitCommand>
        {
            new TransportCommand(units[1], units[0]),
            new MoveCommand(moves, units[0]),
        };
        bool exceptionThrown = false;
        try { gameState.ResolveOrders(unitOrders); }
        catch (Exception) { exceptionThrown = true; }
        Assert.IsTrue(exceptionThrown, "Expected an Exception to be thrown.");
    }

    [TestMethod]
    public void ResolveOrders_UnloadTransportedUnit_RemovesTransportRelationship()
    {

        var units = new List<MilitaryUnit>
        {
            new(new UnitTemplate { RoadMovementBonus = 1, TransportableBy = [OperationalDomain.Airborne] }, gameState.Players[0], location: gameState[1, 1]),
            new(new UnitTemplate { OperationalDomain = OperationalDomain.Airborne, MovementPoints = 3, IsTransporter = true }, gameState.Players[0], location: gameState[1, 1]),
        };

        gameState.Units = units;

        var moves = new Move[]
        {
            new(gameState[1, 1], gameState[2, 2], null, 2, 1),
            new(gameState[2, 2], gameState[3, 2], null, 1, 2),
        };

        var unitOrders = new List<IUnitCommand>
        {
            new MoveCommand(moves, units[1]),
            new TransportCommand(units[1], units[0]),
        };
        gameState.ResolveOrders(unitOrders);

        Assert.AreEqual(units[0], units[1].Transporting.Single());
        Assert.AreEqual(units[1], units[0].TransportedBy);

        unitOrders =
        [
            new UnloadCommand(units[0]),
        ];
        gameState.ResolveOrders(unitOrders);

        Assert.IsEmpty(units[1].Transporting);
        Assert.IsNull(units[0].TransportedBy);
    }

    [TestMethod]
    public void CreateOrders_AirborneAirliftScenario_FollowsExpectedAirliftRoute()
    {
        var numberOfPlayers = 2;
        var labels = new string[gameState.Width, gameState.Height];

        var units = new List<MilitaryUnit>
        {
            new(new UnitTemplate { OperationalDomain = OperationalDomain.Airborne, MovementPoints = 4, IsTransporter = true }, gameState.Players[0], location: gameState[24, 11]),
            new(new UnitTemplate { TransportableBy = [OperationalDomain.Airborne], RoadMovementBonus = 1 }, gameState.Players[0], location: gameState[22, 15]),
            new(new UnitTemplate { TransportableBy = [OperationalDomain.Airborne] }, gameState.Players[0], location: gameState[1, 1]),
            new(new UnitTemplate { TransportableBy = [OperationalDomain.Airborne] }, gameState.Players[0], location: gameState[1, 1]),
        };

        units[2].TerrainTypeBattleModifier[TerrainType.Swamp] = 1;
        units[2].EdgeMovementCosts[EdgeType.River] = 0;

        gameState.Units = units;

        var computerPlayer = new ComputerPlayer(new Dictionary<MilitaryUnit, Role>
        {
            [units[0]] = Role.Balanced,
            [units[1]] = Role.Balanced,
            [units[2]] = Role.Defensive,
            [units[3]] = Role.Balanced,
        });

        for (var turn = 0; turn < 25; turn++)
        {
            computerPlayer.GenerateInfluenceMaps(gameState, numberOfPlayers);

            GameBoardRenderer.Render(RenderPipeline.Board, RenderPipeline.Units, gameState.Width, gameState.Height, gameState.Tiles, gameState.Edges, gameState.Settlements, null, null, gameState.Units);

            // Remove any units that have been destroyed for the purposes of unit orders
            units = units.Where(x => x.IsAlive).ToList();
            computerPlayer.SetStrategicAction(gameState);
            var unitOrders = computerPlayer.CreateOrders(gameState, units);

            var lines = new List<Centreline>();
            unitOrders.OfType<MoveCommand>().ToList().ForEach(x => lines.AddRange(Centreline.MoveOrderToCentrelines(x)));

            GameBoardRenderer.RenderAndSave($"Transport/CreateOrders_AirborneAirliftScenario_FollowsExpectedAirliftRoute{turn}.png", gameState.Width, gameState.Height, gameState.Tiles, gameState.Edges, gameState.Settlements, units: gameState.Units, lines: lines);

            gameState.ResolveOrders(unitOrders);
            gameState.ChangeSettlementOwners();

            gameState.Turn++;

            switch (gameState.Turn)
            {
                case 1:
                    Assert.AreEqual(429, units[0].Location.Index);
                    Assert.AreEqual(428, units[1].Location.Index);
                    break;
                case 2:
                    Assert.AreEqual(429, units[0].Location.Index);
                    Assert.AreEqual(429, units[1].Location.Index);
                    Assert.AreEqual(units[0], units[1].TransportedBy);
                    break;
                case 3:
                    Assert.AreEqual(399, units[0].Location.Index);
                    Assert.AreEqual(399, units[1].Location.Index);
                    Assert.AreEqual(units[0], units[1].TransportedBy);
                    break;
                case 4:
                    Assert.AreEqual(370, units[0].Location.Index);
                    Assert.AreEqual(370, units[1].Location.Index);
                    Assert.AreEqual(units[0], units[1].TransportedBy);
                    break;
                case 5:
                    Assert.AreEqual(340, units[0].Location.Index);
                    Assert.AreEqual(340, units[1].Location.Index);
                    Assert.IsNull(units[1].TransportedBy);
                    break;
                case 6:
                    Assert.AreEqual(311, units[0].Location.Index);
                    Assert.AreEqual(365, units[1].Location.Index);
                    Assert.IsNull(units[1].TransportedBy);
                    break;
                case 7:
                    Assert.AreEqual(338, units[0].Location.Index);
                    Assert.AreEqual(338, units[1].Location.Index);
                    Assert.AreEqual(units[0], units[1].TransportedBy);
                    break;
                case 8:
                    Assert.AreEqual(257, units[0].Location.Index);
                    Assert.AreEqual(257, units[1].Location.Index);
                    Assert.IsNull(units[1].TransportedBy);
                    break;
                case 9:
                    Assert.AreEqual(199, units[0].Location.Index);
                    Assert.AreEqual(203, units[1].Location.Index);
                    Assert.IsNull(units[1].TransportedBy);
                    break;
                case 10:
                    Assert.AreEqual(196, units[0].Location.Index);
                    Assert.AreEqual(150, units[1].Location.Index);
                    Assert.IsNull(units[1].TransportedBy);
                    Assert.AreEqual(units[0], units[3].TransportedBy);
                    break;
            }
        }
    }
}


