using System;
using System.Collections.Generic;

namespace GameModel
{
    public class BattleReport
    {
        public Dictionary<UnitType, int>[] CasualtiesByPlayerAndType { get; set; }
        public string Location { get; set; }
        public int Turn { get; set; }
        public List<CasualtyLogEntry> CasualtyLog { get; set; }

        public BattleReport(int numberOfCombatants)
        {
            CasualtiesByPlayerAndType = new Dictionary<UnitType, int>[numberOfCombatants];
            for (var i = 0; i < numberOfCombatants; i++)
            {
                CasualtiesByPlayerAndType[i] = new Dictionary<UnitType, int>();
                foreach (UnitType unitType in Enum.GetValues(typeof(UnitType)))
                {
                    CasualtiesByPlayerAndType[i].Add(unitType, 0);
                }
            }

            CasualtyLog = new List<CasualtyLogEntry>();
        }
    }
}