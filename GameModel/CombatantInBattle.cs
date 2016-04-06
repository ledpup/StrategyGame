using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameModel
{
    public class CombatantInBattle
    {
        public int OwnerId { get; set; }
        public double UnitStrength { get; set; }
        public double StrengthDamage { get; set; }

        public Dictionary<UnitType, double> UnitStrengthByType { get; set; }
        public List<MilitaryUnit> Units { get; set; }
        public double UnitSurvivalProportion { get; set; }
        public List<MilitaryUnit> OpponentUnits { get; set; }

        public Dictionary<UnitType, int> OpponentUnitTypes { get; set; }

        public CombatantInBattle()
        {
            UnitStrengthByType = new Dictionary<UnitType, double>();
            OpponentUnitTypes = new Dictionary<UnitType, int>();
            foreach (UnitType unitType in Enum.GetValues(typeof(UnitType)))
            {
                UnitStrengthByType.Add(unitType, 0);
                OpponentUnitTypes.Add(unitType, 0);
            }
        }

        public double Outcome
        {
            get
            {
                UnitSurvivalProportion = Units.Count(x => x.IsAlive) / (double)Units.Count;

                return (UnitStrength - StrengthDamage) * UnitSurvivalProportion;
            }
        }
    }
}
