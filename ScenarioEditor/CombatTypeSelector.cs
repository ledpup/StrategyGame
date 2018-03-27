using GameModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScenarioEditor
{
    public class CombatTypeSelector
    {
        public CombatTypeSelector(CombatType combatType, bool isSelected)
        {
            Name = combatType.ToString();
            IsSelected = isSelected;
        }

        public string Name { get; set; }
        public bool IsSelected { get; set; }
    }
}
