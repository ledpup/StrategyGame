using GameModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScenarioEditor
{
    public class MovementTypeSelector
    {
        public MovementTypeSelector(MovementType movementType, bool isSelected)
        {
            Name = movementType.ToString();
            IsSelected = isSelected;
        }

        public string Name { get; set; }
        public bool IsSelected { get; set; }
    }
}
