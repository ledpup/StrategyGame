namespace GameModel;

using System;
using System.Collections.Generic;
using System.Linq;

public class CombatantInBattle
{
    public Guid OwnerId { get; set; }

    public double UnitStrength { get; set; }

    public double StrengthDamage { get; set; }

    public Dictionary<UnitType, double> UnitStrengthByType { get; set; }

    public List<MilitaryUnit> Units { get; set; }

    public double UnitSurvivalProportion { get; set; }

    public List<MilitaryUnit> OpponentUnits { get; set; }

    public Dictionary<UnitType, int> OpponentUnitTypes { get; set; }

    public CombatantInBattle()
    {
        UnitStrengthByType = [];
        OpponentUnitTypes = [];
        foreach (UnitType unitType in Enum.GetValues<UnitType>())
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
