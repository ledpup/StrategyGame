using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Model
{
    class ChangeStaminaCommand : AffectUnitCommand
    {
        public ChangeStaminaCommand(Unit unit, float value)
            : base(unit, value)
        {
            Action = ChangeStamina;
            MessageBase = "Unit stamina reduced by {0}.";
        }

        public void ChangeStamina()
        {
            Unit.Stamina += (short)Value;
        }
    }
}
