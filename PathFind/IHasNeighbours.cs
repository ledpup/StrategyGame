using System.Collections.Generic;

namespace PathFind
{
    public interface IHasNeighbours<N>
    {
        List<N> Neighbours { get; }
        bool HasCumulativeCost { get; set; }
    }
}