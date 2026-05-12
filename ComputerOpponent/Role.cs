namespace ComputerOpponent;

public enum Role
{
    Balanced,
    Besieger,
    Offensive,
    Defensive,
    Scout,
}

public struct RoleMovementType(GameModel.MovementType movementType, Role role)
{
    public GameModel.MovementType MovementType = movementType;
    public Role Role = role;
}
