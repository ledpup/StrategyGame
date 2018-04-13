using GameModel;
using GameModel.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manager
{
    public class MilitaryUnitInstance : IMilitaryUnit
    {
        MilitaryUnitTemplate _militaryUnitTemplate;
        Faction _faction;

        public MilitaryUnitInstance(int id, string name, Faction faction, MilitaryUnitTemplate militaryUnitTemplate)
        {
            Id = id;
            Name = name;
            _faction = faction;
            _militaryUnitTemplate = militaryUnitTemplate;

            MovementType = _militaryUnitTemplate.MovementType;
            UsesRoads = _militaryUnitTemplate.UsesRoads;
            RoadMovementBonusPoints = _militaryUnitTemplate.RoadMovementBonusPoints;
            CanTransport = _militaryUnitTemplate.CanTransport;
            MovementTypesTransportableBy = _militaryUnitTemplate.MovementTypesTransportableBy;
            MovementPoints = _militaryUnitTemplate.MovementPoints;
            CombatType = _militaryUnitTemplate.CombatType;
            CombatAbility = _militaryUnitTemplate.CombatAbility;
            DepletionOrder = _militaryUnitTemplate.DepletionOrder;
            Size = _militaryUnitTemplate.Size;
            Members = _militaryUnitTemplate.Members;
            Morale = _militaryUnitTemplate.Morale;
            TerrainMovements = _militaryUnitTemplate.TerrainMovements;
            EdgeMovements = _militaryUnitTemplate.EdgeMovements;
        }

        public int Id { get; private set; }
        public string Name { get; set; }
        public ArgbColour FactionColour { get { return _faction.Colour; } }

        public MovementType MovementType { get; set; }
        public bool UsesRoads { get; set; }
        public int RoadMovementBonusPoints { get; set; }
        public bool CanTransport { get; set; }
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
 