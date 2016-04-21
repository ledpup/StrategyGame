using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameModel
{
    public class Stack
    {
        List<MilitaryUnit> Units;

        public Stack(List<MilitaryUnit> units)
        {
            Units = units;
        }

        public IEnumerable<Move> MoveList()
        {
            return MilitaryUnit.PossibleMoveList(Units.First());
        }
    }
}
