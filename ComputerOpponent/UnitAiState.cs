using GameModel;

namespace ComputerOpponent
{
    public class UnitAiState(Role role = Role.Balanced)
    {
        public Role Role { get; set; } = role;
        public StrategicAction StrategicAction { get; set; } = StrategicAction.None;

        public RoleMovementType GetRoleMovementType(MilitaryUnit unit)
        {
            return new RoleMovementType(unit.MovementType, Role);
        }
    }
}
