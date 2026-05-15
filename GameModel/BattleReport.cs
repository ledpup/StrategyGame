namespace GameModel;

using System;
using System.Collections.Generic;

public class BattleReport
{
    public Dictionary<UnitType, Guid>[] CasualtiesByPlayerAndType { get; set; }

    public int Turn { get; set; }

    public List<CasualtyLogEntry> CasualtyLog { get; set; }

    public BattleReport(int numberOfCombatants)
    {
        CasualtiesByPlayerAndType = new Dictionary<UnitType, Guid>[numberOfCombatants];
        for (var i = 0; i < numberOfCombatants; i++)
        {
            CasualtiesByPlayerAndType[i] = [];
            foreach (UnitType unitType in Enum.GetValues<UnitType>())
            {
                CasualtiesByPlayerAndType[i].Add(unitType, Guid.Empty);
            }
        }

        CasualtyLog = [];
    }
}