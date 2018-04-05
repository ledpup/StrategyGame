using GameModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manager
{
    public class UnitEdgeMovement
    {
        public UnitEdgeMovement(EdgeType edgeType)
        {
            EdgeType = edgeType.ToString();
            Traversable = true;
            MovementCost = 0;
        }

        public string EdgeType { get; set; }
        public bool Traversable { get; set; }
        public int MovementCost { get; set; }
    }
}
