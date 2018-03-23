using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameModel
{
    public class MilitaryUnitTemplate
    {
        public MilitaryUnitTemplate(string name, MovementType movementType = MovementType.Land, int movementPoints = 2, bool usesRoads = false, bool canTransport = false, int combatQuality = 1, int combatInitiative = 10)
        {
            Name = name;
            MovementType = movementType;
            MovementPoints = movementPoints;
            CanTransport = canTransport;
            UsesRoads = usesRoads;
            CombatQuality = combatQuality;
            CombatInitiative = combatInitiative;
        }

        public string Name;
        public MovementType MovementType;
        public bool CanTransport;

        public bool UsesRoads { get; set; }
        public int MovementPoints { get; set; }
        public int CombatQuality { get; set; }
        public int CombatInitiative { get; set; }
    }
}
