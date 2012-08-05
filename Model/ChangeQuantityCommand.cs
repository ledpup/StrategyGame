using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class ChangeQuantityCommand : AffectUnitCommand
    {
        public ChangeQuantityCommand(Unit unit, float value)
            : base(unit, value / unit.Quality)
        {
            Action = DamageUnit;
            MessageBase = "Unit suffered {0} casulties.";
        }

        public void DamageUnit()
        {
            Unit.Quantity += (short)Value;
        }
    }
}
