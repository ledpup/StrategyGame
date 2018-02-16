using GameModel.Rendering;
using Hexagon;
using HexGridControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ScenarioEditor
{
    public class GameRenderingWpf : IGameRenderingEngine
    {
        //float _hexWidth;
        //float _hexHeight;
        //float _structureWidth;
        //float _unitWidth;
        HexGrid _hexGrid;

        public GameRenderingWpf(int boardWidth, int boardHeight)
        {

            //_structureWidth = _hexHeight / 4;
            //_unitWidth = _structureWidth * .8f;
            //_layout = new Layout(Layout.flat, new PointD(EdgeLength, EdgeLength), new PointD(EdgeLength, _hexHeight / 2));

            _hexGrid = new HexGrid
            {
                ColumnCount = boardWidth,
                RowCount = boardHeight,
                Orientation = System.Windows.Controls.Orientation.Horizontal,
                Margin = new Thickness(10, 10, 427, 10),
            };
        }

        public static Color ColourToColor(ArgbColour colour)
        {
            return Color.FromArgb((byte)colour.Alpha, (byte)colour.Red, (byte)colour.Green, (byte)colour.Blue);
        }

        public void DrawBoard(Dictionary<Hex, ArgbColour> hexagonColours)
        {
            for (var x = 0; x < _hexGrid.ColumnCount; x++)
            {
                for (var y = 0; y < _hexGrid.RowCount; y++)
                {
                    var hex = OffsetCoord.QoffsetToCube(new OffsetCoord(x, y));

                    var hexItem = new HexItem();
                    hexItem.SetValue(Grid.ColumnProperty, x);
                    hexItem.SetValue(Grid.RowProperty, y);
                    hexItem.BorderThickness = new Thickness(.5);
                    hexItem.Background = new SolidColorBrush(ColourToColor(hexagonColours[hex]));
                    _hexGrid.Children.Add(hexItem);
                }
            }
        }

        public void DrawCentreline(Hex origin, Hex destination, ArgbColour colour, int width)
        {
            throw new NotImplementedException();
        }

        public void DrawCircle(Hex location, float position, ArgbColour colour)
        {
            throw new NotImplementedException();
        }

        public void DrawEdge(Hex origin, Hex destination, ArgbColour colour, bool isPort)
        {
            throw new NotImplementedException();
        }

        public void DrawRectangle(Hex location, ArgbColour colour)
        {
            throw new NotImplementedException();
        }

        public void DrawTrapezium(Hex location, float position, ArgbColour colour)
        {
            throw new NotImplementedException();
        }

        public void DrawTriangle(Hex location, float position, ArgbColour colour)
        {
            throw new NotImplementedException();
        }

        public object GetBitmap()
        {
            return _hexGrid;
        }

        public void LabelHexes(ArgbColour colour, float xMin, float yMin, string[] labels, int boardWidth)
        {
            throw new NotImplementedException();
        }

        public void SaveGameBoardToFile(string fileName)
        {
            throw new NotImplementedException();
        }
    }


}
