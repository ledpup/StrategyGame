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

        public MoveOrder(Move[] moves, MilitaryUnit unit = null)
        {
            Moves = moves;
            Unit = unit;
        }

        public List<Vector> Vectors
        {
            get
            {
                var colour = Unit == null ? Colours.Black : Unit.UnitColour;
                return Moves.Select(x => new Vector(x.Origin.Point, x.Destination.Point, colour, BaseEdgeType.CentreToCentre)).ToList();
            }
        }
    }
}
