﻿using GameModel;
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
using System.Windows.Shapes;

namespace ScenarioEditor
{
    public class GameRenderingWpf : IGameRenderingEngine, IReactiveGameRenderingEngine
    {
        float _structureWidth, _hexHeight;
        float _unitWidth;
        const float EdgeLength = 25;
        HexGrid _hexGrid;
        Canvas _canvas;
        Layout _layout;
        int _boardWidth, _boardHeight;
        List<Shape>[] _shapesAtLocation;

        public GameRenderingWpf(int boardWidth, int boardHeight)
        {
            _boardWidth = boardWidth;
            _boardHeight = boardHeight;
            _shapesAtLocation = new List<Shape>[_boardWidth * _boardHeight];
            for (var i = 0; i < _boardWidth * _boardHeight; i++)
                _shapesAtLocation[i] = new List<Shape>();

            var hexWidth = EdgeLength * 2;
            _hexHeight = (float)Math.Sqrt(3) / 2 * hexWidth;

            var imageWidth = (int)(hexWidth * (boardWidth + .4) * .75);
            var imageHeight = (int)(_hexHeight * (boardHeight + .6));

            _structureWidth = _hexHeight / 4;
            _unitWidth = _structureWidth * .8f;
            _layout = new Layout(Layout.flat, new PointD(EdgeLength, EdgeLength), new PointD(EdgeLength, _hexHeight / 2));

            _hexGrid = new HexGrid
            {
                ColumnCount = _boardWidth,
                RowCount = _boardHeight,
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

        public void DrawCentreline(Hex origin, Hex destination, ArgbColour colour, int width)
        {
            var point1 = Layout.HexToPixel(_layout, origin);
            var point2 = Layout.HexToPixel(_layout, destination);

            var line = new Line
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

        public void DrawCircle(int unitId, Hex location, float position, ArgbColour colour)
        {
            var circle = new Ellipse
            {
                Width = _unitWidth,
                Height = _unitWidth,
                Fill = new SolidColorBrush(ArgbColourToColor(colour)),
                Tag = unitId,
            };

            PositionShape(circle, location, position);

            var index = Hex.HexToIndex(location, _boardWidth, _boardHeight);
            _shapesAtLocation[index].Add(circle);

            _canvas.Children.Add(circle);
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
                    Foreground = new SolidColorBrush(ArgbColourToColor(colour)),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top
                };

                Canvas.SetLeft(label, (point1.X + point2.X) / 2 - 10);
                Canvas.SetTop(label, (point1.Y + point2.Y) / 2 - 15);
                _canvas.Children.Add(label);
            }
            else
            {
                var line = new Line
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

            Rectangle rectangle = null;

            foreach (UIElement uiElement in _hexGrid.Children)
            {
                if (uiElement is Rectangle)
                {
                    if ((int)uiElement.GetValue(Grid.ColumnProperty) == offsetCoord.col && (int)uiElement.GetValue(Grid.RowProperty) == offsetCoord.row)
                    {
                        rectangle = (Rectangle)uiElement;
                    }
                }
            }

            if (rectangle == null)
            {
                rectangle = new Rectangle
                {
                    Width = _structureWidth,
                    Height = _structureWidth,
                    IsHitTestVisible = false,
                };
                rectangle.SetValue(Grid.ColumnProperty, offsetCoord.col);
                rectangle.SetValue(Grid.RowProperty, offsetCoord.row);
                _hexGrid.Children.Add(rectangle);
            }

            rectangle.Fill = new SolidColorBrush(ArgbColourToColor(colour));
        }

        public void RemoveRectangle(Hex location)
        {
            var offsetCoord = OffsetCoord.QoffsetFromCube(location);

            Rectangle rectangle = null;

            foreach (UIElement uiElement in _hexGrid.Children)
            {
                if (uiElement is Rectangle)
                {
                    if ((int)uiElement.GetValue(Grid.ColumnProperty) == offsetCoord.col && (int)uiElement.GetValue(Grid.RowProperty) == offsetCoord.row) rectangle = (Rectangle)uiElement;
                }
            }

            if (rectangle != null)
            {
                _hexGrid.Children.Remove(rectangle);
            }
        }

        public void DrawTrapezium(int unitId, Hex location, float position, ArgbColour colour)
        {
            var polygon = new Polygon
            {
                Fill = new SolidColorBrush(ArgbColourToColor(colour)),
                Tag = unitId,
            };

            var pointCollection = new PointCollection(TrapeziumPoints(new PointD(0, 0)));
            polygon.Points = pointCollection;

            PositionShape(polygon, location, position);

            var index = Hex.HexToIndex(location, _boardWidth, _boardHeight);
            _shapesAtLocation[index].Add(polygon);

            _canvas.Children.Add(polygon);
        }

        private Point[] TrapeziumPoints(PointD topLeftCorner)
        {
            Point[] points =
            {
                new Point(topLeftCorner.X, topLeftCorner.Y),
                new Point(topLeftCorner.X + _unitWidth, topLeftCorner.Y),
                new Point(topLeftCorner.X + _unitWidth * .8F, topLeftCorner.Y + _unitWidth * .65F),
                new Point(topLeftCorner.X + _unitWidth * .2F, topLeftCorner.Y + _unitWidth * .65F),
            };
            return points;
        }

        public void DrawTriangle(int unitId, Hex location, float position, ArgbColour colour)
        {
            var polygon = new Polygon
            {
                Fill = new SolidColorBrush(ArgbColourToColor(colour)),
                Tag = unitId,
            };

            var pointCollection = new PointCollection(TrianglePoints(new PointD(0, 0)));
            polygon.Points = pointCollection;

            var index = Hex.HexToIndex(location, _boardWidth, _boardHeight);
            _shapesAtLocation[index].Add(polygon);

            PositionShape(polygon, location, position);

            _canvas.Children.Add(polygon);
        }

        private Point[] TrianglePoints(PointD topLeftCorner)
        {
            Point[] points =
            {
                new Point(topLeftCorner.X, topLeftCorner.Y),
                new Point(topLeftCorner.X + _unitWidth, topLeftCorner.Y),
                new Point(topLeftCorner.X + _unitWidth / 2, topLeftCorner.Y + _unitWidth)
            };
            return points;
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

        public void RepositionUnits(Hex location)
        {
            var index = Hex.HexToIndex(location, _boardWidth, _boardHeight);
            var numberOfUnits = _shapesAtLocation[index].Count;
            for (var j = 0; j <  numberOfUnits; j ++)
            {
                var position = (float)(((j + 1) / (float)numberOfUnits) * Math.PI * 2);
                PositionShape(_shapesAtLocation[index][j], location, position);
            }
        }

        void PositionShape(Shape shape, Hex location, float position)
        {
            var hexCentre = Layout.HexToPixel(_layout, location);
            var topLeftCorner = GameRenderer.UnitLocationTopLeftCorner(hexCentre, position, _hexHeight, _unitWidth);
            Canvas.SetLeft(shape, topLeftCorner.X);
            Canvas.SetTop(shape, topLeftCorner.Y);
        }

        internal void RemoveUnit(int id, int locationIndex)
        {
            Shape unitToRemove = null;
            foreach (UIElement uiElement in _canvas.Children)
            {
                if (uiElement is Shape && ((Shape)uiElement).Tag != null)
                {
                    if ((int)((Shape)uiElement).Tag == id)
                        unitToRemove = (Shape)uiElement;
                }
            }
            _canvas.Children.Remove(unitToRemove);
            _shapesAtLocation[locationIndex].Remove(unitToRemove);
        }

        public void PlaceUnit(MovementType movementType, int unitId, Hex hex, int position, ArgbColour colour)
        {
            switch (movementType)
            {
                case MovementType.Airborne:
                    DrawTriangle(unitId, hex, position, colour);
                    break;
                case MovementType.Land:
                    DrawCircle(unitId, hex, position, colour);
                    break;
                case MovementType.Water:
                    DrawTrapezium(unitId, hex, position, colour);
                    break;
            }
        }
    }
}
