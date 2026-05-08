using System;
using System.Collections.Generic;

namespace GameModel
{
    public class BattleReport
    {
        public Dictionary<UnitType, int>[] CasualtiesByPlayerAndType { get; set; }
        public int Turn { get; set; }
        public List<CasualtyLogEntry> CasualtyLog { get; set; }

        public BattleReport(int numberOfCombatants)
        {
            CasualtiesByPlayerAndType = new Dictionary<UnitType, int>[numberOfCombatants];
            for (var i = 0; i < numberOfCombatants; i++)
            {
                CasualtiesByPlayerAndType[i] = [];
                foreach (UnitType unitType in Enum.GetValues<UnitType>())
                {
                    CasualtiesByPlayerAndType[i].Add(unitType, 0);
                }
            }

            CasualtyLog = [];
        }
    }
}