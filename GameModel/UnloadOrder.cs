using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameModel
{
    public class UnloadOrder : IUnitOrder
    {
        public UnloadOrder(MilitaryUnit unit)
        {
            Unit = unit;
        }
        public MilitaryUnit Unit { get; set; }
    }
}
