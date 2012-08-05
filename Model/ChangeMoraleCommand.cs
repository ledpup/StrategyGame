using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Model
{
    class ChangeMoraleCommand : AffectUnitCommand
    {
        public ChangeMoraleCommand(Unit unit, float value)
            : base(unit, value)
        {
            Action = ChangeMorale;
            MessageBase = "Unit morale reduced by {0}.";
        }

        public void ChangeMorale()
        {
            Unit.Morale += (short)Value;
        }
    }
}
