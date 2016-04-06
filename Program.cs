using GameModel;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrategyWargame2016
{


    class Program
    {
        static void Main(string[] args)
        {

            Board.StructureDefenceModifiers = new Dictionary<string, double>();
            Board.StructureDefenceModifiers["Fort"] = .6;

            var units = new List<MilitaryUnit>
            {
                new MilitaryUnit()
                {
                    UnitType = UnitType.Melee,
                    BaseQuality = 2,
                    InitialQuantity = 347,
                    InitialMorale = 3,
                    TerrainTypeBattleModifier = new Dictionary<TerrainType, double> { { TerrainType.Mountain, 1 }, { TerrainType.Hill, 1 }, { TerrainType.Desert, -1 } }
                },
                new MilitaryUnit()
                {
                    UnitType = UnitType.Siege,
                    BaseQuality = 2,
                    InitialQuantity = 167,
                    InitialMorale = 3,
                    CombatInitiative = 5,
                    WeatherBattleModifier = new Dictionary<Weather, double> { { Weather.Cold, -1 } }
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
                    OwnerId = 2,
                    UnitType = UnitType.Melee,
                    BaseQuality = 3,
                    InitialQuantity = 245,
                    InitialMorale = 2,
                },
                new MilitaryUnit()
                {
                    OwnerId = 2,
                    UnitType = UnitType.Cavalry,
                    BaseQuality = 2,
                    InitialQuantity = 345,
                    CombatInitiative = 5,
                    WeatherBattleModifier = new Dictionary<Weather, double> { { Weather.Wet, -2 } },
                    OpponentUnitTypeBattleModifier = new Dictionary<UnitType, double> { { UnitType.Melee, 2 } }
                },


                new MilitaryUnit()                {
                    OwnerId = 3,
                    UnitType = UnitType.Cavalry,
                    BaseQuality = 2,
                    InitialQuantity = 165,
                    WeatherBattleModifier = new Dictionary<Weather, double> { { Weather.Wet, -2 } },
                    OpponentUnitTypeBattleModifier = new Dictionary<UnitType, double> { { UnitType.Melee, 2 } }
                },
                new MilitaryUnit() {
                    OwnerId = 3,
                    UnitType = UnitType.Ranged,
                    BaseQuality = 3,
                    InitialQuantity = 175,
                    CombatInitiative = 5,
                    OpponentUnitTypeBattleModifier = new Dictionary<UnitType, double> { { UnitType.Cavalry, 1 }, { UnitType.Melee, 1 } } },
            };

            var location = "Tzarian Castle";
            var turn = 1;

            Board.Logger = LogManager.GetCurrentClassLogger();

            Board.ResolveBattle(location, turn, TerrainType.Mountain, Weather.Cold, units, 3, "Fort", 2);
            var battleReport = Board.CreateBattleReport(location, turn, units);
        }

        
    }
}
