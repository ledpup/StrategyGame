using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrategyWargame2016
{
    public enum UnitType
    {
        Infantry,
        Ranged,
        Cavalry,
        Siege,
        Aquatic,
    }

    public enum MovementType
    {
        Land,
        Water,
        Airborne,
        Amphibious,
    }

    public enum BattleQualityModifier
    {
        Terrain,
        Weather,
        UnitType,
    }

    public class MilitaryUnit
    {
        public UnitType UnitType { get; set; }
        public MovementType MovementType { get; set; }
        public string Name { get; set; }
        public int OwnerId { get; set; }
        public double BaseQuality { get; set; }
        public Dictionary<BattleQualityModifier, double> BattleQualityModifiers { get; set; }
        public double Quality { get; set; }
        public int Quantity { get; set; }
        public double Strength { get; set; }

        public double BattleStrength { get; set; }
        public List<QuantityEvent> QuantityEvents { get; set; }
        public double Morale { get; set; }
        public double InitialMorale { get; set; }
        public List<MoraleEvent> MoraleEvents { get; set; }
        public double CombatInitiative { get; set; }
        public int Speed { get; set; }
        public bool IsAlive { get; private set; }

        public Dictionary<Terrain, double> TerrainModifier { get; set; }
        public Dictionary<Weather, double> WeatherModifier { get; set; }
        public Dictionary<UnitType, double> OpponentUnitTypeModifier { get; set; }

        public MilitaryUnit(string name, int ownerId, MovementType movementType, UnitType unitType, double baseQuality, int initialQuantity, int combatInitiative = 10, double initialMorale = 5, int turn = 0)
        {
            IsAlive = true;
            QuantityEvents = new List<QuantityEvent>();
            MoraleEvents = new List<MoraleEvent>();
            BattleQualityModifiers = new Dictionary<BattleQualityModifier, double>
            {
                { BattleQualityModifier.Terrain, 0 },
                { BattleQualityModifier.Weather, 0 },
                { BattleQualityModifier.UnitType, 0 },
            };
            TerrainModifier = new Dictionary<Terrain, double>();
            WeatherModifier = new Dictionary<Weather, double>();
            OpponentUnitTypeModifier = new Dictionary<UnitType, double>();
            MovementType = MovementType.Land;

            Name = name;
            OwnerId = ownerId;
            UnitType = unitType;
            Quality = BaseQuality = baseQuality;
            ChangeQuantity(turn, initialQuantity);
            CombatInitiative = combatInitiative;
            InitialMorale = initialMorale;
            ChangeMorale(turn, InitialMorale);

            CalculateStrength();
        }

        public void CalculateStrength()
        {
            Strength = Quality * Quantity;

            var battleQuality = Quality + BattleQualityModifiers.Values.Sum();
            if (battleQuality < 1)
                battleQuality = 1;

            BattleStrength = battleQuality * Quantity;
        }

        public void ChangeQuantity(int turn, int quantity)
        {
            QuantityEvents.Add(new QuantityEvent { Turn = turn, Quantity = quantity });
            Quantity += quantity;

            if (Quantity <= 0)
            {
                IsAlive = false;
                Strength = BattleStrength = 0;
                return;
            }

            CalculateStrength();
        }

        public void ChangeMorale(int turn, double morale)
        {
            Morale += morale;
            MoraleEvents.Add(new MoraleEvent { Turn = turn, Morale = morale });
        }
    }
}
