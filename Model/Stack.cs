using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class Stack
    {
        List<Unit> Units;

        public Stack(List<Unit> units)
        {
            Units = units;
        }

        public bool CanTransport(BaseUnitType transportUnitType)
        {
            if (transportUnitType.HasFlag(BaseUnitType.Land))
                throw new Exception("Transportation can only be undertaken by airborne or aquatic units.");

            var transporterSize = Units.Where(x => x.BaseUnitType.HasFlag(transportUnitType)).Sum(x => x.TransportSize);
            var transporteeSize = Units.Where(x => !x.BaseUnitType.HasFlag(transportUnitType)).Sum(x => x.TransportSize);
            return transporterSize >= transporteeSize;
        }

        public UnitType Transporting()
        {
            if (Units.Any(x => x.BaseUnitType == BaseUnitType.Aquatic))
            {
                if (CanTransport(BaseUnitType.Aquatic))
                    return UnitType.Aquatic;
            }
            else if (Units.Any(x => x.BaseUnitType == BaseUnitType.Airborne))
            {
                if (CanTransport(BaseUnitType.Airborne))
                    return UnitType.Airborne;
            }
            return UnitType.None;
        }

        public IEnumerable<Move> MoveList()
        {
            var units = Units.ToArray();

            var transporting = Transporting();
            if (transporting != UnitType.None)
            {
                units = Units.Where(x => x.UnitType == transporting).ToArray();
            }

            var moves = Unit.MoveList(units[0]);
            for (var i = 1; i < units.Length; i++)
            {
                moves = moves.Intersect(Unit.MoveList(units[i]));
            }

            return moves;
        }
    }
}
