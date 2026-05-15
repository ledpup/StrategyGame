namespace ComputerOpponent;

public enum Role
{
    Balanced,
    Besieger,
    Offensive,
    Defensive,
    Scout,
}

public struct RoleMovementType(GameModel.OperationalDomain movementType, Role role)
{
    public GameModel.OperationalDomain MovementType = movementType;
    public Role Role = role;
}
