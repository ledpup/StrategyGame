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

        public bool CanTransport(UnitType transportUnitType)
        {
            if (!transportUnitType.HasFlag(UnitType.Airborne) && !transportUnitType.HasFlag(UnitType.Aquatic))
                throw new Exception("Transportation can only be undertaken by airborne or aquatic units.");

            var transporterSize = Units.Where(x => x.UnitType == transportUnitType).Sum(x => x.TransportSize);
            var transporteeSize = Units.Where(x => x.UnitType != transportUnitType).Sum(x => x.TransportSize);
            return transporterSize >= transporteeSize;
        }

        public UnitType Transporting()
        {
            if (Units.Any(x => x is AquaticUnit))
            {
                if (CanTransport(UnitType.Aquatic))
                    return UnitType.Aquatic;
            }
            else if (Units.Any(x => x is AirborneUnit))
            {
                if (CanTransport(UnitType.Airborne))
                    return UnitType.Airborne;
            }
            return UnitType.None;
        }
    }
}
