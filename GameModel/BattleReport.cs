using System;
using System.Collections.Generic;

namespace GameModel
{
    public class BattleReport
    {
        public Dictionary<UnitType, int> CasualtiesByType { get; set; }
        public string Location { get; set; }
        public int Turn { get; set; }
        public List<CasualtyLogEntry> CasualtyLog { get; set; }

        public BattleReport()
        {
            CasualtiesByType = new Dictionary<UnitType, int>();
            foreach (UnitType unitType in Enum.GetValues(typeof(UnitType)))
            {
                CasualtiesByType.Add(unitType, 0);
            }

            CasualtyLog = new List<CasualtyLogEntry>();
        }
    }
}