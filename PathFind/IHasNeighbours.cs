using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PathFind
{
    public interface IHasNeighbours<N>
    {
        List<N> Neighbours { get; }
        bool HasCumulativeCost { get; set; }
    }
}