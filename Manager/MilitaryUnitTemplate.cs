using GameModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manager
{
    public class MilitaryUnitTemplate
    {
        public MilitaryUnitTemplate(string name)
        {
            Name = name;
            MovementType = MovementType.Land;
            MovementPoints = 2;
            UsesRoads = true;
            RoadMovementBonusPoints = 1;
            CanTransport = false;
            MovementTypesTransportableBy = new List<MovementType>() { MovementType.Water };
            CombatType = CombatType.Melee;
            CombatAbility = 1;
            DepletionOrder = 10;
            Members = 500;
            Size = 1;
            Morale = 5;

            TerrainMovements = new List<UnitTerrainMovement>();
            foreach (TerrainType terrainType in Enum.GetValues(typeof(TerrainType)))
            {
                TerrainMovements.Add(new UnitTerrainMovement(terrainType));
            }

            EdgeMovements = new List<UnitEdgeMovement>();
            foreach (EdgeType edgeType in Enum.GetValues(typeof(EdgeType)))
            {
                EdgeMovements.Add(new UnitEdgeMovement(edgeType));
            }
        }

        public string Name;
        public MovementType MovementType;
        public bool UsesRoads { get; set; }
        public int RoadMovementBonusPoints { get; set; }
        public bool CanTransport;
        public List<MovementType> MovementTypesTransportableBy { get; set; }
        public int MovementPoints { get; set; }
        public CombatType CombatType { get; set; }
        public int CombatAbility { get; set; }
        public int DepletionOrder { get; set; }
        public float Size { get; set; }
        public int Members { get; set; }
        public int Morale { get; set; }

        public List<UnitTerrainMovement> TerrainMovements { get; set; }

        public List<UnitEdgeMovement> EdgeMovements { get; set; }
    }
}
