using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using GameModel.Rendering;
using Hexagon;

namespace GameRendering2D
{
    public class GameRenderingEngine2D : IGameRenderingEngine
    {
        Bitmap _bitmap;
        Graphics _graphics;
        float _hexWidth;
        float _hexHeight;
        const float EdgeLength = 25;
        float _structureWidth;
        float _unitWidth;
        Layout _layout;

        public GameRenderingEngine2D(int boardWidth, int boardHeight)
        {
            _hexWidth = EdgeLength * 2;
            _hexHeight = (float)Math.Sqrt(3) / 2 * _hexWidth;

            var imageWidth = (int)(_hexWidth * (boardWidth + .4) * .75);
            var imageHeight = (int)(_hexHeight * (boardHeight + .6));

            _bitmap = new Bitmap(imageWidth, imageHeight);

            _graphics = Graphics.FromImage(_bitmap);
            _graphics.SmoothingMode = SmoothingMode.AntiAlias;

            _structureWidth = _hexHeight / 4;
            _unitWidth = _structureWidth * .8f;
            _layout = new Layout(Layout.flat, new PointD(EdgeLength, EdgeLength), new PointD(EdgeLength, _hexHeight/2));
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

        private PointF PointDtoF(PointD point)
        {
            return new PointF((float)point.X, (float)point.Y);
        }
        private PointF[] PointDtoF(List<PointD> points)
        {
            return points
                .Select(x => new PointF((float)x.X, (float)x.Y))
                .ToArray();
        }

        public void DrawEdge(Hex origin, Hex destination, ArgbColour colour, bool isPort)
        {
            var pen = new Pen(Color.FromArgb(colour.Alpha, colour.Red, colour.Green, colour.Blue), 3);
            (PointF pt1, PointF pt2) points;

            var direction = Hex.Subtract(origin, destination);
            var index = Hex.Directions.IndexOf(direction);

            var vertices = Layout.PolygonCorners(_layout, origin);

            points.pt1 = PointDtoF(vertices[index]);
            points.pt2 = PointDtoF(vertices[(index + 1) % 6]);

            if (isPort)
            {
                using (StringFormat sf = new StringFormat())
                {
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Center;
                    var x = (points.pt1.X + points.pt2.X) / 2;
                    var y = (points.pt1.Y + points.pt2.Y) / 2;

                    var font = new Font("Arial", (int)(_hexHeight * .3));
                    _graphics.DrawString("P", font, pen.Brush, x, y, sf);
                }
            }
            else
            {
                _graphics.DrawLine(pen, points.pt1, points.pt2);
            }
        }

        public void DrawCentreline(Hex origin, Hex destination, ArgbColour colour, int width)
        {
            var pen = new Pen(Color.FromArgb(colour.Alpha, colour.Red, colour.Green, colour.Blue), width);
            (PointF pt1, PointF pt2) points;

            points.pt1 = PointDtoF(Layout.HexToPixel(_layout, origin));
            points.pt2 = PointDtoF(Layout.HexToPixel(_layout, destination));

            _graphics.DrawLine(pen, points.pt1, points.pt2);
        }

        public void DrawCircle(Hex location, float position, ArgbColour colour)
        {
            var brush = ArgbColourToBrush(colour);
            var topLeftCorner = UnitLocationTopLeftCorner(location, position);

            _graphics.FillEllipse(brush, topLeftCorner.xTopLeft, topLeftCorner.yTopLeft, _unitWidth, _unitWidth);
        }

        private (float xTopLeft, float yTopLeft) UnitLocationTopLeftCorner(Hex location, float position)
        {
            float Radius = _hexHeight / 4;

            var hexCentre = Layout.HexToPixel(_layout, location);

            var xOnCircle = (float)Math.Cos(position) * Radius + hexCentre.X;
            var yOnCircle = (float)Math.Sin(position) * Radius + hexCentre.Y;

            var xTopLeft = (float)(xOnCircle - (_unitWidth / 2));
            var yTopLeft = (float)(yOnCircle - (_unitWidth / 2));

            return (xTopLeft, yTopLeft);
        }

        public void DrawTrapezium(Hex location, float position, ArgbColour colour)
        {
            var brush = ArgbColourToBrush(colour);
            var topLeftCorner = UnitLocationTopLeftCorner(location, position);

            PointF[] points =
            {
                new PointF(topLeftCorner.xTopLeft, topLeftCorner.yTopLeft),
                new PointF(topLeftCorner.xTopLeft + _unitWidth, topLeftCorner.yTopLeft),
                new PointF(topLeftCorner.xTopLeft + _unitWidth * .8F, topLeftCorner.yTopLeft + _unitWidth * .65F),
                new PointF(topLeftCorner.xTopLeft + _unitWidth * .2F, topLeftCorner.yTopLeft + _unitWidth * .65F),
            };

            _graphics.FillPolygon(brush, points);
        }

        public void SaveGameBoardToFile(string fileName)
        {
            _bitmap.Save(fileName);
        }

        public object GetBitmap()
        {
            return _bitmap;
        }

        public void DrawTriangle(Hex location, float position, ArgbColour colour)
        {
            var brush = ArgbColourToBrush(colour);
            var topLeftCorner = UnitLocationTopLeftCorner(location, position);

            PointF[] points = 
            {
                new PointF(topLeftCorner.xTopLeft, topLeftCorner.yTopLeft),
                new PointF(topLeftCorner.xTopLeft + _unitWidth, topLeftCorner.yTopLeft),
                new PointF(topLeftCorner.xTopLeft + _unitWidth / 2, topLeftCorner.yTopLeft + _unitWidth)
            };

            _graphics.FillPolygon(brush, points);
        }

        public void DrawRectangle(Hex location, ArgbColour colour)
        {
            var brush = new SolidBrush(Color.FromArgb(colour.Alpha, colour.Red, colour.Green, colour.Blue));
            var hexCentre = Layout.HexToPixel(_layout, location);

            var x = (float)hexCentre.X - (_structureWidth / 2);
            var y = (float)hexCentre.Y - (_structureWidth / 2);

            _graphics.FillRectangle(brush, x, y, _structureWidth, _structureWidth);
        }

        public void LabelHexes(ArgbColour colour, float xMin, float yMin, string[] labels, int boardWidth)
        {
            var pen = new Pen(new SolidBrush(Color.FromArgb(colour.Alpha, colour.Red, colour.Green, colour.Blue)));

            var xMax = _bitmap.Width;
            var yMax = _bitmap.Height;

            var font = new Font("Arial", (int)(_hexHeight * .2));

            if (labels == null)
                return;

            using (StringFormat sf = new StringFormat())
            {
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;

                for (var i = 0; i < labels.Length; i++)
                {
                    var points = Layout.PolygonCorners(_layout, Hex.IndexToHex(i, boardWidth));
                    var worldX = (float)(points[0].X + points[3].X) / 2;
                    var worldY = (float)((points[1].Y + points[4].Y) / 2);
                    var label = labels[i];
                    _graphics.DrawString(label, font, Brushes.Black, worldX, worldY, sf);
                }
            }
        }


        static Color ArgbColourToColor(ArgbColour colour)
        {
            return Color.FromArgb(colour.Alpha, colour.Red, colour.Green, colour.Blue);
        }

        static SolidBrush ArgbColourToBrush(ArgbColour colour)
        {
            return new SolidBrush(ArgbColourToColor(colour));
        }
    }
}
