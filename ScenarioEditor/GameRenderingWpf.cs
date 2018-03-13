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
        float _structureWidth;
        float _unitWidth;
        const float EdgeLength = 25;
        HexGrid _hexGrid;
        Canvas _canvas;
        Layout _layout;

        public GameRenderingWpf(int boardWidth, int boardHeight)
        {
            var hexWidth = EdgeLength * 2;
            var hexHeight = (float)Math.Sqrt(3) / 2 * hexWidth;

            var imageWidth = (int)(hexWidth * (boardWidth + .4) * .75);
            var imageHeight = (int)(hexHeight * (boardHeight + .6));

            _structureWidth = hexHeight / 4;
            _unitWidth = _structureWidth * .8f;
            _layout = new Layout(Layout.flat, new PointD(EdgeLength, EdgeLength), new PointD(EdgeLength, hexHeight / 2));

            _hexGrid = new HexGrid
            {
                ColumnCount = boardWidth,
                RowCount = boardHeight,
                Orientation = System.Windows.Controls.Orientation.Horizontal,
                Width = imageWidth,
                Height = imageHeight,
            };
            _canvas = new Canvas();

            _canvas.Children.Add(_hexGrid);
        }

        public static Color ArgbColourToColor(ArgbColour colour)
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
                    hexItem.Background = new SolidColorBrush(ArgbColourToColor(hexagonColours[hex]));
                    _hexGrid.Children.Add(hexItem);
                }
            }
        }

        //private PointF[] PointDtoF(List<PointD> points)
        //{
        //    return points
        //        .Select(x => new PointF((float)x.X, (float)x.Y))
        //        .ToArray();
        //}

        public void DrawCentreline(Hex origin, Hex destination, ArgbColour colour, int width)
        {
            var point1 = Layout.HexToPixel(_layout, origin);
            var point2 = Layout.HexToPixel(_layout, destination);

            var line = new System.Windows.Shapes.Line
            {
                X1 = point1.X,
                Y1 = point1.Y,
                X2 = point2.X,
                Y2 = point2.Y,
                StrokeThickness = width,
                Stroke = new SolidColorBrush(ArgbColourToColor(colour))
            };

            _canvas.Children.Add(line);
        }

        public void DrawCircle(Hex location, float position, ArgbColour colour)
        {
            throw new NotImplementedException();
        }

        public void DrawEdge(Hex origin, Hex destination, ArgbColour colour, bool isPort)
        {            
            var direction = Hex.Subtract(origin, destination);
            var index = Hex.Directions.IndexOf(direction);

            var vertices = Layout.PolygonCorners(_layout, origin);

            var point1 = vertices[index];
            var point2 = vertices[(index + 1) % 6];

            if (isPort)
            {
                var label = new Label
                {
                    Content = "P",
                    FontWeight = FontWeights.Bold,
                    FontSize = 16,
                    Foreground = new SolidColorBrush(ArgbColourToColor(colour))
                };

                Canvas.SetLeft(label, (point1.X + point2.X) / 2 - 10);
                Canvas.SetTop(label, (point1.Y + point2.Y) / 2 - 10);
                _canvas.Children.Add(label);
            }
            else
            {
                var line = new System.Windows.Shapes.Line
                {
                    X1 = point1.X,
                    Y1 = point1.Y,
                    X2 = point2.X,
                    Y2 = point2.Y,
                    StrokeThickness = 3,
                    Stroke = new SolidColorBrush(ArgbColourToColor(colour))
                };
                _canvas.Children.Add(line);
            }
        }

        public void DrawRectangle(Hex location, ArgbColour colour)
        {
            var offsetCoord = OffsetCoord.QoffsetFromCube(location);

            var rectangle = new System.Windows.Shapes.Rectangle
            {
                Width = _structureWidth,
                Height = _structureWidth,
                Fill = new SolidColorBrush(ArgbColourToColor(colour))
            };
            rectangle.SetValue(Grid.ColumnProperty, offsetCoord.col);
            rectangle.SetValue(Grid.RowProperty, offsetCoord.row);
            _hexGrid.Children.Add(rectangle);
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
            return _canvas;
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
