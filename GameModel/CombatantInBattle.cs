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

        public Dictionary<CombatType, double> UnitStrengthByCombatType { get; set; }
        public List<MilitaryUnit> Units { get; set; }
        public double UnitSurvivalProportion { get; set; }
        public List<MilitaryUnit> OpponentUnits { get; set; }

        public Dictionary<CombatType, int> OpponentCombatTypes { get; set; }

        public CombatantInBattle()
        {
            UnitStrengthByCombatType = new Dictionary<CombatType, double>();
            OpponentCombatTypes = new Dictionary<CombatType, int>();
            foreach (CombatType unitType in Enum.GetValues(typeof(CombatType)))
            {
                UnitStrengthByCombatType.Add(unitType, 0);
                OpponentCombatTypes.Add(unitType, 0);
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
