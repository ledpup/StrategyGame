using GameModel;
using GameModel.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace Tests;

[TestClass]
public class BattleTests
{
    [TestMethod]
    public void ResolveBattle_TwoEnemies_AppliesExpectedCasualties()
    {
        var templateFactory = new UnitTemplateFactory();
        var unitFactory = new MilitaryUnitFactory(templateFactory);

        var units = new List<MilitaryUnit>
        {
            unitFactory.CreateNext(UnitTemplateName.DwarvenInfantry),
            unitFactory.CreateNext(UnitTemplateName.DwarvenInfantry, ownerIndex: 1),
        };

        var turn = 1;

        var battleReport = BattleResolver.ResolveBattle("BasicBattle", turn, TerrainType.Grassland, Weather.Fine, units, units[0].Owner.Id, SettlementType.None, 0);

        Assert.AreEqual(200, battleReport.CasualtiesByPlayerAndType[units[0].Owner.Id][UnitType.Melee]);
        Assert.AreEqual(200, battleReport.CasualtiesByPlayerAndType[units[1].Owner.Id][UnitType.Melee]);
    }

    [TestMethod]
    public void ResolveOrders_TwoEnemiesMoveToSameDestination_CreatesBattle()
    {
        var gameState = new GameState(new Board(BoardTests.GameBoard, BoardTests.TileEdges));
        var unitFactory = new MilitaryUnitFactory(new UnitTemplateFactory());

        gameState.Units =
        [
            unitFactory.CreateNext(UnitTemplateName.DwarvenInfantry, ownerIndex: 0, location: gameState[1, 1]),
            unitFactory.CreateNext(UnitTemplateName.DwarvenInfantry, ownerIndex: 1, location: gameState[2, 3]),
        ];

        var moveOrders = new List<IUnitCommand>
        {
            gameState.Units[0].GetMoveOrderToDestination(gameState[2, 2]),
            gameState.Units[1].GetMoveOrderToDestination(gameState[2, 2]),
        };

        gameState.ResolveOrders(moveOrders);

        var battles = gameState.ConductBattles();

        Assert.HasCount(1, battles);
    }
}


