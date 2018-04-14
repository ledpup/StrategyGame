using Hexagon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameModel.Rendering
{
    public interface IGameRenderingEngine
    {
        void DrawBoard(Dictionary<Hexagon.Hex, ArgbColour> hexagonColours);
        void DrawEdge(Hex origin, Hex destination, ArgbColour colour, bool isPort);
        void DrawCentreline(Hex origin, Hex destination, ArgbColour colour, int width);
        void DrawRectangle(Hex location, ArgbColour colour);
        void DrawCircle(int unitId, Hex location, float position, ArgbColour colour);
        void DrawTriangle(int unitId, Hex location, float position, ArgbColour colour);
        void DrawTrapezium(int unitId, Hex location, float position, ArgbColour colour);
        void LabelHexes(ArgbColour colour, float xMin, float yMin, string[] labels, int boardWidth);

        void SaveGameBoardToFile(string fileName);
        object GetBitmap();
    }
}
