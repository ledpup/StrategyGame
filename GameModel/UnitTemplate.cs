using System.Collections.Generic;

namespace GameModel;

public record UnitTemplate
{
    public string Name { get; set; }
    public UnitType UnitType { get; set; }
    public MovementType MovementType { get; set; } = MovementType.Land;
    public int MovementPoints { get; set; } = 2;
    public int RoadMovementBonus { get; set; } = 0;
    public double Quality { get; set; } = 1;
    public int Personnel { get; set; } = 100;
    public double Size { get; set; } = 1;
    public bool IsTransporter { get; set; } = false;
    public List<MovementType> TransportableBy { get; set; }
    public int CombatInitiative { get; set; } = 10;
    public double Morale { get; set; } = 5;
    public float[] MoraleMoveCost { get; set; }
}
