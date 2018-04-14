using GameModel;
using System.Collections.Generic;

namespace Manager
{ 
    public interface IMilitaryUnit
    {
        int Id { get; }
        string Name { get; set; }
        MovementType MovementType { get; set; }
        bool UsesRoads { get; set; }
        int RoadMovementBonusPoints { get; set; }
        bool CanTransport { get; set; }
        List<MovementType> MovementTypesTransportableBy { get; set; }
        int MovementPoints { get; set; }
        CombatType CombatType { get; set; }
        int CombatAbility { get; set; }
        int DepletionOrder { get; set; }
        float Size { get; set; }
        int Members { get; set; }
        int Morale { get; set; }

        List<UnitTerrainMovement> TerrainMovements { get; set; }

        List<UnitEdgeMovement> EdgeMovements { get; set; }
        NumberingConvention NumberingConvention { get; set; }
    }
}