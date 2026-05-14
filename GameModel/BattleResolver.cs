namespace GameModel;

using System;
using System.Collections.Generic;
using System.Linq;

public static class BattleResolver
{
    public static BattleReport ResolveBattle(string locationText, int turn, TerrainType terrainType, Weather weather, List<MilitaryUnit> units, int residentId = 0, SettlementType settlement = SettlementType.None, int siegeDuration = 1)
    {
        var groupedUnits = units.GroupBy(x => x.Owner.Id);
        if (groupedUnits.Count() == 1)
        {
            throw new Exception("Battle can not occur because all units in tile are owned by " + units[0].Owner);
        }

        units.ForEach(x =>
        {
            x.BattleQualityModifiers[BattleQualityModifier.Terrain] = x.TerrainTypeBattleModifier[terrainType];
            x.BattleQualityModifiers[BattleQualityModifier.Weather] = x.WeatherBattleModifier[weather];
            x.BattleQualityModifiers[BattleQualityModifier.Settlement] = settlement != SettlementType.None ? x.SettlementBattleModifier : 0;
        });

        var combatants = new List<CombatantInBattle>();
        foreach (var group in groupedUnits)
        {
            var combatantInBattle = new CombatantInBattle
            {
                OwnerId = group.Key,
                Units = group.ToList(),
                OpponentUnits = units.Where(x => x.Owner.Id != group.Key).ToList(),
            };

            var opponentUnitsCount = (double)combatantInBattle.OpponentUnits.Count;

            foreach (UnitType unitType in Enum.GetValues<UnitType>())
            {
                combatantInBattle.OpponentUnitTypes[unitType] = combatantInBattle.OpponentUnits.Count(x => x.UnitType == unitType);
                var proportion = combatantInBattle.OpponentUnitTypes[unitType] / opponentUnitsCount;

                combatantInBattle.Units.ForEach(x =>
                {
                    x.BattleQualityModifiers[BattleQualityModifier.UnitType] = x.OpponentUnitTypeBattleModifier[unitType] * proportion;
                });
            }

            foreach (var unit in combatantInBattle.Units)
            {
                unit.CalculateStrength();
                combatantInBattle.UnitStrengthByType[unit.UnitType] += unit.BattleStrength;
            }

            combatantInBattle.UnitStrength = group.Sum(x => x.BattleStrength);

            combatants.Add(combatantInBattle);
        }

        foreach (var combatant in combatants)
        {
            var numberOfSides = combatants.Count;
            var opponents = combatants.Where(x => x != combatant).ToList();

            combatant.StrengthDamage = opponents.Sum(x => x.UnitStrength) / (numberOfSides - 1) * .8;

            if (residentId == combatant.OwnerId && settlement != SettlementType.None)
            {
                var siegeUnitDamage = 0D;
                opponents.ForEach(x => siegeUnitDamage += x.UnitStrengthByType[UnitType.Siege]);

                combatant.StrengthDamage -= siegeUnitDamage;
                combatant.StrengthDamage *= (1 - Settlement.SettlementDefenceModifier(settlement)) + (.05 * siegeDuration);
                combatant.StrengthDamage += siegeUnitDamage;
            }
        }

        foreach (var combatant in combatants)
        {
            AssignCasualties(turn, combatant.Units, combatant.StrengthDamage);
        }

        var winnersToLosers = combatants.OrderByDescending(x => x.Outcome).ToArray();
        for (var i = 0; i < winnersToLosers.Length; i++)
        {
            var positionProportion = (i + 1) / (double)winnersToLosers.Length;
            var losesPenalty = 1 - winnersToLosers[i].UnitSurvivalProportion;

            winnersToLosers[i].Units.Where(x => x.IsAlive).ToList().ForEach(x => x.ChangeMorale(turn, -(positionProportion + losesPenalty), "Morale change due to combat"));
        }

        return CreateBattleReport(turn, units);
    }

    private static BattleReport CreateBattleReport(int turn, List<MilitaryUnit> units)
    {
        var numberOfPlayers = units.GroupBy(x => x.Owner.Id).Select(x => x.Key).Count();

        var battleReport = new BattleReport(numberOfPlayers)
        {
            Turn = turn,
        };

        foreach (UnitType unitType in Enum.GetValues<UnitType>())
        {
            units.Where(x => x.UnitType == unitType).ToList().ForEach(x => battleReport.CasualtiesByPlayerAndType[x.Owner.Id][unitType] += -x.Events.Where(y => y.Turn == turn && y.Reason == "Personnel change").Sum(z => (int)z.Value));
        }

        units.ForEach(x =>
        {
            var losses = -(int)x.Events.Where(y => y.Turn == turn && y.Reason == "Personnel loss in battle").Sum(y => y.Value);

            battleReport.CasualtyLog.Add(new CasualtyLogEntry
            {
                OwnerIndex = x.Owner.Id,
                Text = x.IsAlive ? losses > 1
                                    ? string.Format("{0} {1} loss{2}, {3} remain", x.Name, losses, losses > 1 ? "es" : string.Empty, x.Personnel)
                                    : string.Format("{0} no losses", x.Name)
                             : string.Format("{0} destroyed", x.Name),
            });
        });

        return battleReport;
    }

    private static void AssignCasualties(int turn, List<MilitaryUnit> units, double combatantStrengthDamage)
    {
        while (combatantStrengthDamage > 0)
        {
            var aliveUnits = units.Where(x => x.IsAlive).ToList();
            double assignedStrengthDamage = 0;

            if (aliveUnits.Count == 0)
            {
                combatantStrengthDamage = 0;
                continue;
            }

            var totalInitiative = aliveUnits.Sum(x => x.CombatInitiative);

            foreach (var unit in aliveUnits)
            {
                var casualityProportion = unit.CombatInitiative / totalInitiative;
                var strengthDamageToUnit = combatantStrengthDamage * casualityProportion;
                if (strengthDamageToUnit > unit.Strength)
                {
                    strengthDamageToUnit = unit.Strength;
                    unit.ChangePersonnel(turn, -unit.Personnel);
                }
                else
                {
                    var quantityDecrease = (int)Math.Ceiling(strengthDamageToUnit / unit.Quality);
                    unit.ChangePersonnel(turn, -quantityDecrease);
                }

                assignedStrengthDamage += strengthDamageToUnit;
            }

            combatantStrengthDamage -= assignedStrengthDamage;
            combatantStrengthDamage = Math.Round(combatantStrengthDamage, 0);
        }
    }
}
