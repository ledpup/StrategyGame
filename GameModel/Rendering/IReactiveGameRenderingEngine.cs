using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameModel.Rendering
{
    public interface IReactiveGameRenderingEngine
    {
        void RemoveRectangle(Hexagon.Hex location);

        void RepositionUnits(Hexagon.Hex location);
    }
}
