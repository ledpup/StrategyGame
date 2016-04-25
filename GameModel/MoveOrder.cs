using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameModel
{
    public class MoveOrder
    {
        public MilitaryUnit Unit;
        public Move[] Moves;


        public MoveOrder()
        {

        }
        public MoveOrder(MilitaryUnit unit, Move[] moves)
        {
            Unit = unit;
            Moves = moves;
        }

        public List<Vector> Vectors
        {
            get
            {
                return Moves.Select(x => new Vector(x.Origin.Location, x.Destination.Location, Unit.UnitColour, BaseEdgeType.CentreToCentre)).ToList();
            }
        }
    }
}
