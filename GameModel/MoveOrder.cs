using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameModel
{
    public class MoveOrder
    {
        public int Turn;
        public MilitaryUnit Unit;
        public Move[] Moves;

        public List<Vector> Vectors
        {
            get
            {
                return Moves.Select(x => new Vector(x.Origin.Location, x.Destination.Location, Unit.UnitColour, BaseEdgeType.CentreToCentre)).ToList();
            }
        }
    }
}
