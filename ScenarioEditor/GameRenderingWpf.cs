using Hexagon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScenarioEditor
{
    public class GameRenderingWpf
    {
        float _hexWidth;
        float _hexHeight;
        float _structureWidth;
        float _unitWidth;
        Layout _layout;

        public GameRenderingWpf(int boardWidth, int boardHeight)
        {

            _structureWidth = _hexHeight / 4;
            _unitWidth = _structureWidth * .8f;
            _layout = new Layout(Layout.flat, new PointD(EdgeLength, EdgeLength), new PointD(EdgeLength, _hexHeight / 2));
        }

        public void DrawBoard(Dictionary<Hex, ArgbColour> hexagonColours)
        {
            _graphics.FillRectangle(Brushes.White, 0, 0, _bitmap.Width, _bitmap.Height);

            foreach (var hex in hexagonColours.Keys)
            {
                var points = Layout.PolygonCorners(_layout, hex);
                var colour = hexagonColours[hex];
                var brush = new SolidBrush(Color.FromArgb(colour.Alpha, colour.Red, colour.Green, colour.Blue));
                _graphics.FillPolygon(brush, PointDtoF(points));
                _graphics.DrawPolygon(Pens.Black, PointDtoF(points));
            }
        }
    }


}
