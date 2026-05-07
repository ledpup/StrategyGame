using GameModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests
{
    [TestClass]
    public class BattleTests
    {
        [TestMethod]
        public void BasicBattleTest()
        {
            var units = new List<MilitaryUnit>
            {
                new(new UnitTemplate())
                {
                    UnitType = UnitType.Melee,
                    BaseQuality = 2,
                    InitialQuantity = 347,
                    InitialMorale = 3,
                },
                new(new UnitTemplate())
                {
                    UnitType = UnitType.Siege,
                    BaseQuality = 2,
                    InitialQuantity = 167,
                    InitialMorale = 3,
                    CombatInitiative = 5,
                },
                new(new UnitTemplate())
                {
                    UnitType = UnitType.Melee,
                    BaseQuality = 1,
                    InitialQuantity = 256,
                    InitialMorale = 2,
                },



                new(new UnitTemplate())
                {
                    OwnerIndex = 1,
                    UnitType = UnitType.Melee,
                    BaseQuality = 3,
                    InitialQuantity = 245,
                    InitialMorale = 2,
                },
                new(new UnitTemplate())
                {
                    OwnerIndex = 1,
                    UnitType = UnitType.Cavalry,
                    BaseQuality = 3,
                    InitialQuantity = 345,
                    CombatInitiative = 5,
                    StructureBattleModifier = -1,
                },



                new(new UnitTemplate())
                {
                    OwnerIndex = 2,
                    UnitType = UnitType.Cavalry,
                    BaseQuality = 3,
                    InitialQuantity = 165,
                    StructureBattleModifier = -1,
                },
                new(new UnitTemplate()) {
                    OwnerIndex = 2,
                    UnitType = UnitType.Ranged,
                    BaseQuality = 3,
                    InitialQuantity = 175,
                    CombatInitiative = 5,
                }
            };

            units[0].TerrainTypeBattleModifier[TerrainType.Mountain] = 1;
            units[0].TerrainTypeBattleModifier[TerrainType.Hill] = 1;
            units[0].TerrainTypeBattleModifier[TerrainType.Desert] = -1;

            units[1].WeatherBattleModifier[Weather.Cold] = -1;

            units[4].WeatherBattleModifier[Weather.Wet] = -2;
            units[4].OpponentUnitTypeBattleModifier[UnitType.Melee] = 2;

            units[5].WeatherBattleModifier[Weather.Wet] = -2;
            units[5].OpponentUnitTypeBattleModifier[UnitType.Melee] = 2;

            units[6].OpponentUnitTypeBattleModifier[UnitType.Cavalry] = 1;
            units[6].OpponentUnitTypeBattleModifier[UnitType.Melee] = 1;


            var location = "Tzarian Castle";
            var turn = 1;

            //Board.Logger = LogManager.GetCurrentClassLogger();

            var board = new Board(new[] { "S", "S" });

            Board.ResolveBattle(location, turn, TerrainType.Mountain, Weather.Cold, units, 3, StructureType.Fortress, 2);
            var battleReport = Board.CreateBattleReport(new Tile(0, 1, 1), turn, units);



            Assert.IsTrue(battleReport.CasualtiesByPlayerAndType[0][UnitType.Melee] > 50);
        }

        [TestMethod]
        public void BattleTest()
        {
            var board = new Board(BoardTests.GameBoard, BoardTests.TileEdges);

            board.Units =
            [
                new(new UnitTemplate { MovementPoints = 4 }, 0, 0, board[1, 1], "1st Infantry"),
                new(new UnitTemplate(), 1, 1, board[2, 3], "2nd Infantry"),
            ];

            var moves1 = new Move[]
                    {
                        new(board[1, 1], board[1, 2], null, 2, 1),
                        new(board[1, 2], board[2, 2], null, 1, 2),
                        new(board[2, 2], board[3, 2], null, 0, 3),
                    };
            var moves2 = new Move[]
                    {
                        new(board[2, 3], board[2, 2], null, 1, 1),
                        new(board[2, 2], board[2, 1], null, 0, 2),
                    };

            var moveOrders = new List<IUnitOrder>
            {
                new MoveOrder(moves1, board.Units[0]),
                new MoveOrder(moves2, board.Units[1]),
            };

            board.ResolveOrders(moveOrders);

            var battles = board.ConductBattles();

            Assert.AreEqual(1, battles.Count());
        }
    }
}
