namespace GameModel;

using System;
using System.Collections.Generic;

public class BattleReport
{
    public Dictionary<Guid, Dictionary<UnitType, int>> CasualtiesByPlayerAndType { get; set; }

    public int Turn { get; set; }

    public List<CasualtyLogEntry> CasualtyLog { get; set; }

    public BattleReport(IEnumerable<Guid> playerIds)
    {
        CasualtiesByPlayerAndType = [];
        foreach (var playerId in playerIds)
        {
            CasualtiesByPlayerAndType[playerId] = [];
            foreach (UnitType unitType in Enum.GetValues<UnitType>())
            {
                CasualtiesByPlayerAndType[playerId].Add(unitType, 0);
            }
        }

        CasualtyLog = [];
    }
}
