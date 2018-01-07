using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameModel
{
    public class MoveOrder : IUnitOrder
    {
        public MilitaryUnit Unit { get; set; }
        public Move[] Moves;

        public MoveOrder(Move[] moves, MilitaryUnit unit)
        {
            Moves = moves;
            Unit = unit;
        }


    }
}
