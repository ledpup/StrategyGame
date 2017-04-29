using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameModel
{
    public class TransportOrder : IUnitOrder
    {
        public TransportOrder(MilitaryUnit unit, MilitaryUnit unitToTransport)
        {
            Unit = unit;
            UnitToTransport = unitToTransport;
        }
        public MilitaryUnit Unit { get; set; }
        public MilitaryUnit UnitToTransport { get; set; }
    }
}
