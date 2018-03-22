using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameModel
{
    public class MilitaryUnitTemplate
    {
        public MilitaryUnitTemplate(string name, MovementType movementType = MovementType.Land, int movementPoints = 2, bool usesRoads = false, bool canTransport = false)
        {
            Name = name;
            MovementType = movementType;
            MovementPoints = movementPoints;
            CanTransport = canTransport;
            UsesRoads = usesRoads;
        }

        public string Name;
        public MovementType MovementType;
        public bool CanTransport;

        public bool UsesRoads { get; set; }
        public int MovementPoints { get; set; }
    }
}
