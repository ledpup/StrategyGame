using GameModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace Tests
{
    [TestClass]
    public class BattleTests
    {
        [TestMethod]
        public void TwoEnemies_ResolveBattle_200Casualties()
        {
            var templateFactory = new UnitTemplateFactory();
            var unitFactory = new MilitaryUnitFactory(templateFactory);

            var units = new List<MilitaryUnit>
            {
                unitFactory.CreateNext(UnitTemplateName.DwarvenInfantry),
                unitFactory.CreateNext(UnitTemplateName.DwarvenInfantry, ownerIndex: 1),
            };

            var turn = 1;

            var battleReport = BattleResolver.ResolveBattle("BasicBattle", turn, TerrainType.Grassland, Weather.Fine, units, 1, StructureType.None, 0);

            Assert.AreEqual(200, battleReport.CasualtiesByPlayerAndType[0][UnitType.Melee]);
            Assert.AreEqual(200, battleReport.CasualtiesByPlayerAndType[1][UnitType.Melee]);
        }

        [TestMethod]
        public void TwoEnemies_MoveToSameDestination_BattleOccurs()
        {
            var board = new GameState(new Board(BoardTests.GameBoard, BoardTests.TileEdges));
            var unitFactory = new MilitaryUnitFactory(new UnitTemplateFactory());

            board.Units =
            [
                unitFactory.CreateNext(UnitTemplateName.DwarvenInfantry, ownerIndex: 0, location: board[1, 1]),
                unitFactory.CreateNext(UnitTemplateName.DwarvenInfantry, ownerIndex: 1, location: board[2, 3]),
            ];

            var moveOrders = new List<IUnitOrder>
            {
                board.Units[0].GetMoveOrderToDestination(board[2, 2]),
                board.Units[1].GetMoveOrderToDestination(board[2, 2]),
            };

            board.ResolveOrders(moveOrders);

            var battles = board.ConductBattles();

            Assert.AreEqual(1, battles.Count());
        }
    }
}