﻿using GameModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                new MilitaryUnit()
                {
                    CombatType = CombatType.Melee,
                    BaseCombatAbility = 2,
                    InitialQuantity = 347,
                    InitialMorale = 3,
                },
                new MilitaryUnit()
                {
                    CombatType = CombatType.Siege,
                    BaseCombatAbility = 2,
                    InitialQuantity = 167,
                    InitialMorale = 3,
                    CombatInitiative = 5,
                },
                new MilitaryUnit()
                {
                    CombatType = CombatType.Melee,
                    BaseCombatAbility = 1,
                    InitialQuantity = 256,
                    InitialMorale = 2,
                },



                new MilitaryUnit()
                {
                    FactionId = 1,
                    CombatType = CombatType.Melee,
                    BaseCombatAbility = 3,
                    InitialQuantity = 245,
                    InitialMorale = 2,
                },
                new MilitaryUnit()
                {
                    FactionId = 1,
                    CombatType = CombatType.Cavalry,
                    BaseCombatAbility = 3,
                    InitialQuantity = 345,
                    CombatInitiative = 5,
                    StructureCombatModifier = -1,
                },



                new MilitaryUnit()
                {
                    FactionId = 2,
                    CombatType = CombatType.Cavalry,
                    BaseCombatAbility = 3,
                    InitialQuantity = 165,
                    StructureCombatModifier = -1,
                },
                new MilitaryUnit() {
                    FactionId = 2,
                    CombatType = CombatType.Ranged,
                    BaseCombatAbility = 3,
                    InitialQuantity = 175,
                    CombatInitiative = 5,
                }
            };

            units[0].TerrainTypeCombatModifier[TerrainType.Mountain] = 1;
            units[0].TerrainTypeCombatModifier[TerrainType.Hill] = 1;
            units[0].TerrainTypeCombatModifier[TerrainType.Desert] = -1;

            units[1].WeatherCombatModifier[Weather.Cold] = -1;

            units[4].WeatherCombatModifier[Weather.Wet] = -2;
            units[4].EnemyCombatTypeCombatModifier[CombatType.Melee] = 2;

            units[5].WeatherCombatModifier[Weather.Wet] = -2;
            units[5].EnemyCombatTypeCombatModifier[CombatType.Melee] = 2;

            units[6].EnemyCombatTypeCombatModifier[CombatType.Cavalry] = 1;
            units[6].EnemyCombatTypeCombatModifier[CombatType.Melee] = 1;


            var location = "Tzarian Castle";
            var turn = 1;

            //Board.Logger = LogManager.GetCurrentClassLogger();

            var board = new Board(new[] { "S", "S" });

            Board.ResolveBattle(location, turn, TerrainType.Mountain, Weather.Cold, units, 3, StructureType.Fortress, 2);
            var battleReport = Board.CreateBattleReport(new Tile(0, 1, 1), turn, units);



            Assert.IsTrue(battleReport.CasualtiesByPlayerAndType[0][CombatType.Melee] > 50);
        }

        [TestMethod]
        public void BattleTest()
        {
            var board = new Board(BoardTests.GameBoard, BoardTests.TileEdges);

            board.Units = new List<MilitaryUnit>
            {
                new MilitaryUnit(0, "1st Infantry", 0, board[1, 1], baseMovementPoints: 4),
                new MilitaryUnit(1, "2nd Infantry", 1, board[2, 3]),
            };

            var moves1 = new Move[]
                    {
                        new Move(board[1, 1], board[1, 2], null, 2, 1),
                        new Move(board[1, 2], board[2, 2], null, 1, 2),
                        new Move(board[2, 2], board[3, 2], null, 0, 3),
                    };
            var moves2 = new Move[]
                    {
                        new Move(board[2, 3], board[2, 2], null, 1, 1),
                        new Move(board[2, 2], board[2, 1], null, 0, 2),
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
