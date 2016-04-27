using GameModel;
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
        public void BasicTest()
        {
            var units = new List<MilitaryUnit>
            {
                new MilitaryUnit()
                {
                    UnitType = UnitType.Melee,
                    BaseQuality = 2,
                    InitialQuantity = 347,
                    InitialMorale = 3,
                },
                new MilitaryUnit()
                {
                    UnitType = UnitType.Siege,
                    BaseQuality = 2,
                    InitialQuantity = 167,
                    InitialMorale = 3,
                    CombatInitiative = 5,
                },
                new MilitaryUnit()
                {
                    UnitType = UnitType.Melee,
                    BaseQuality = 1,
                    InitialQuantity = 256,
                    InitialMorale = 2,
                },



                new MilitaryUnit()
                {
                    OwnerIndex = 1,
                    UnitType = UnitType.Melee,
                    BaseQuality = 3,
                    InitialQuantity = 245,
                    InitialMorale = 2,
                },
                new MilitaryUnit()
                {
                    OwnerIndex = 1,
                    UnitType = UnitType.Cavalry,
                    BaseQuality = 3,
                    InitialQuantity = 345,
                    CombatInitiative = 5,
                    StructureBattleModifier = -1,
                },



                new MilitaryUnit()
                {
                    OwnerIndex = 2,
                    UnitType = UnitType.Cavalry,
                    BaseQuality = 3,
                    InitialQuantity = 165,
                    StructureBattleModifier = -1,
                },
                new MilitaryUnit() {
                    OwnerIndex = 2,
                    UnitType = UnitType.Ranged,
                    BaseQuality = 3,
                    InitialQuantity = 175,
                    CombatInitiative = 5,
                }
            };

            units[0].TerrainTypeBattleModifier[TerrainType.Mountain] = 1;
            units[0].TerrainTypeBattleModifier[TerrainType.Hill] = 1;
            units[0].TerrainTypeBattleModifier[TerrainType.Steppe] = -1;

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
            var battleReport = Board.CreateBattleReport(location, turn, units);



            Assert.IsTrue(battleReport.CasualtiesByPlayerAndType[0][UnitType.Melee] > 50);
        }
    }
}
