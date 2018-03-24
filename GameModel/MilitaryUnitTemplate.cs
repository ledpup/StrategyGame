using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameModel
{
    public class MilitaryUnitTemplate
    {
        public MilitaryUnitTemplate(string name, MovementType movementType = MovementType.Land, int movementPoints = 2, bool usesRoads = false, bool canTransport = false, List<MovementType> movementTypesTransportableBy = null, int members = 500, float size = 1, CombatType combatType = CombatType.Melee, int combatAbility = 1, int combatInitiative = 10, int morale = 5)
        {
            Name = name;
            MovementType = movementType;
            MovementPoints = movementPoints;
            UsesRoads = usesRoads;
            CanTransport = canTransport;
            MovementTypesTransportableBy = movementTypesTransportableBy == null ? new List<MovementType>() { MovementType.Water } : movementTypesTransportableBy;
            CombatType = combatType;
            CombatAbility = combatAbility;
            CombatInitiative = combatInitiative;
            Members = members;
            Size = size;
            Morale = morale;


        }

        public string Name;
        public MovementType MovementType;
        public bool UsesRoads { get; set; }
        public bool CanTransport;
        public List<MovementType> MovementTypesTransportableBy { get; set; }
        public int MovementPoints { get; set; }
        public CombatType CombatType { get; set; }
        public int CombatAbility { get; set; }
        public int CombatInitiative { get; set; }
        public float Size { get; set; }
        public int Members { get; set; }
        public int Morale { get; set; }

        
    }


}
