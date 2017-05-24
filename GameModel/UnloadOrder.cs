using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameModel
{
    public class UnloadOrder : IUnitOrder
    {
        public UnloadOrder(MilitaryUnit unit, bool executeBeforeMoveOrders)
        {
            Unit = unit;
            ExecuteBeforeMoveOrders = executeBeforeMoveOrders;
        }
        public MilitaryUnit Unit { get; set; }

        public bool ExecuteBeforeMoveOrders { get; set; }
    }
}
