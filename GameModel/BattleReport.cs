using System;
using System.Collections.Generic;

namespace GameModel
{
    public class BattleReport
    {
        public Dictionary<CombatType, int>[] CasualtiesByPlayerAndType { get; set; }
        public Tile Tile { get; set; }
        public int Turn { get; set; }
        public List<CasualtyLogEntry> CasualtyLog { get; set; }

        public BattleReport(int numberOfCombatants)
        {
            CasualtiesByPlayerAndType = new Dictionary<CombatType, int>[numberOfCombatants];
            for (var i = 0; i < numberOfCombatants; i++)
            {
                CasualtiesByPlayerAndType[i] = new Dictionary<CombatType, int>();
                foreach (CombatType unitType in Enum.GetValues(typeof(CombatType)))
                {
                    CasualtiesByPlayerAndType[i].Add(unitType, 0);
                }
            }

            CasualtyLog = new List<CasualtyLogEntry>();
        }
    }
}