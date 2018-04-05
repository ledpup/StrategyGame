using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameModel
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
    }
}
