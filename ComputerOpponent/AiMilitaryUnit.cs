using GameModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputerOpponent
{
    public class AiMilitaryUnit
    {
        public AiMilitaryUnit(MilitaryUnit unit)
        {
            Unit = unit;
            Role = Role.Balanced;
            StrategicAction = StrategicAction.None;
        }
        public MilitaryUnit Unit;
        public Role Role
        {
            get { return _role; }
            set {
                _role = value;
                RoleMovementType = new RoleMovementType(Unit.MovementType, Role);
            }
        }
        Role _role;

        public StrategicAction StrategicAction;
        public RoleMovementType RoleMovementType { get; set; }
    }
}
