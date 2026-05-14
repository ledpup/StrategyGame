using GameModel;

namespace ComputerOpponent;

public class UnitAiState(Role role = Role.Balanced)
{
    public Role Role { get; set; } = role;
    public OperationalAction OperationalAction { get; set; } = OperationalAction.None;

    public RoleMovementType GetRoleMovementType(MilitaryUnit unit)
    {
        return new RoleMovementType(unit.MovementType, Role);
    }
}
