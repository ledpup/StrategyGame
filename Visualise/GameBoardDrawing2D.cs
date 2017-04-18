using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using GameModel;
using System.Linq;

namespace Visualise
{
    public class GameBoardDrawing2D
    {
        Graphics _graphics;
        float _hexHeight;
        float _hexWidth;
        float _edgeLength;
        float _structureWidth;
        float _unitWidth;
        Layout _layout;

        public GameBoardDrawing2D(Bitmap bitmap, int numberOfHexesHigh)
        {
            _graphics = Graphics.FromImage(bitmap);
            _graphics.SmoothingMode = SmoothingMode.AntiAlias;
            
            _hexHeight = Convert.ToSingle(bitmap.Height) / (numberOfHexesHigh + 1);
            _edgeLength = (float)(_hexHeight / Math.Sqrt(3));
            _hexWidth = 2 * _edgeLength;
            
            _structureWidth = _hexHeight / 4;
            _unitWidth = _structureWidth * .8f;
            _layout = new Layout(Layout.flat, new PointD(_edgeLength, _edgeLength), new PointD(_edgeLength, _hexHeight/2));
        }

        public void DrawBoard(int width, int height, Dictionary<GameModel.Point, Brush> hexagonColours)
        {
            _graphics.FillRectangle(Brushes.White, 0, 0, width, height);

            foreach (var point in hexagonColours.Keys)
            {
                var points = Layout.PolygonCorners(_layout, new OffsetCoord(point.X, point.Y).QoffsetToCube());
                _graphics.FillPolygon(hexagonColours[point], PointDtoF(points));
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

        public void DrawEdge(Hex origin, Hex destination, Pen pen, bool isCentreToCentre, bool isPort)
        {
            (PointF pt1, PointF pt2) points;
            if (isCentreToCentre)
            {
                points.pt1 = PointDtoF(Layout.HexToPixel(_layout, origin));
                points.pt2 = PointDtoF(Layout.HexToPixel(_layout, destination));
            }
            else
            {
                return;
                //points = HexSidePoints(origin, destination, _hexWidth, _hexHeight);
            }

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
                _graphics.DrawLine(pen, points.pt1, points.pt2);
            //else
            //    graphics.DrawArc(pen, new RectangleF(pt1.X, pt1.Y, Math.Abs(pt2.X - pt1.X), Math.Abs(pt2.Y - pt1.Y)), 270, 90);
        }

        public void DrawEdge(Hex origin, Hex destination, int edge, Pen pen)
        {
           
            var points = Layout.PolygonCorners(_layout, new Hex(3, 4));

            var point1 = PointDtoF(Layout.HexToPixel(_layout, new Hex(3, 4)));
            var point2 = PointDtoF(Layout.HexToPixel(_layout, new Hex(4, 4)));

            foreach (var point in points)
            {
                //var points = Layout.PolygonCorners(_layout, new OffsetCoord(point.X, point.Y).QoffsetToCube());
                //graphics.FillPolygon(hexagonColours[point], PointDtoF(points));
                _graphics.DrawLine(Pens.Black, point1, point2);
            }

            //points.ForEach(x => _graphics.DrawEllipse(pen, (float)x.X - 2, (float)x.Y - 2, 4, 4));
            //var ROUTE_SIZE = 4;
            var sourceEdgeIndex = 0;
            //var sourceOffset = (sourceEdgeIndex * ROUTE_SIZE) - ((_edgeLength * ROUTE_SIZE) / 2) + (ROUTE_SIZE / 2);
            //var targetOffset = (1 * ROUTE_SIZE) - ((_edgeLength * ROUTE_SIZE) / 2) + (ROUTE_SIZE / 2);

            var points2 = new PointF[6];

            for (var i = 0; i < 6;i++)
            {
                var point = PointD.EdgeToCentrePoint(points.ToArray(), _edgeLength, i, 5);
                points2[i] = new PointF((float)point.X, (float)point.Y);
            }
            
            
            //(PointF pt1, PointF pt2) points;
            //if (isCentreToCentre)
            //{
            //    points.pt1 = HexCentre(_hexWidth, _hexHeight, origin.Y, origin.X);
            //    points.pt2 = HexCentre(_hexWidth, _hexHeight, destination.Y, destination.X);
            //}
            //else
            //{
            //    points = HexSidePoints(origin, destination, _hexWidth, _hexHeight);
            //}


            _graphics.DrawPolygon(pen, points2);

            //_graphics.DrawLine(pen, (float) point1.X, (float)point1.Y, (float)point2.X, (float)point2.Y);
            //_graphics.DrawLine(pen, (float)point1.X, (float)point1.Y, (float)point2.X, (float)point2.Y);

            //else
            //    graphics.DrawArc(pen, new RectangleF(pt1.X, pt1.Y, Math.Abs(pt2.X - pt1.X), Math.Abs(pt2.Y - pt1.Y)), 270, 90);
        }

        private void StraightRoad(PointF[] points, float radius, int tileIndex)
        {
            var degreesOffset = 0 * 60;

            //graphics.DrawArc(new Pen(Color.Brown, 2), points[1].X - radius, points[1].Y - radius, 2 * radius, 2 * radius, 0, 60);
            //graphics.DrawArc(new Pen(Color.Brown, 2), points[0].X + (HexWidth / 2) - radius, points[0].Y - radius, 2 * radius, 2 * radius, 120, 120);
            //graphics.DrawArc(new Pen(Color.Brown, 2), points[5].X - radius, points[5].Y - radius, 2 * radius, 2 * radius, 300, 60);


            //graphics.DrawArc(new Pen(Color.Brown, 2), points[2].X - radius, points[2].Y - radius, 2 * radius, 2 * radius, 60, 60);
            //graphics.DrawArc(new Pen(Color.Brown, 2), points[0].X + (HexWidth / 2) - radius, points[0].Y - radius, 2 * radius, 2 * radius, 180, 120);
            //graphics.DrawArc(new Pen(Color.Brown, 2), points[0].X - radius, points[0].Y - radius, 2 * radius, 2 * radius, 0, 60);

            _graphics.DrawArc(new Pen(Color.Brown, 2), points[3].X - radius, points[3].Y - radius, 2 * radius, 2 * radius, 120, 60);
            _graphics.DrawArc(new Pen(Color.Brown, 2), points[0].X + (_hexWidth / 2) - radius, points[0].Y - radius, 2 * radius, 2 * radius, 240, 120);
            _graphics.DrawArc(new Pen(Color.Brown, 2), points[1].X - radius, points[1].Y - radius, 2 * radius, 2 * radius, 60, 60);
        }

        public void DrawCircle(GameModel.Point location, float position, SolidBrush brush)
        {
            var topLeftCorner = UnitLocationTopLeftCorner(location, position);

            _graphics.FillEllipse(brush, topLeftCorner.xTopLeft, topLeftCorner.yTopLeft, _unitWidth, _unitWidth);
        }

        private (float xTopLeft, float yTopLeft) UnitLocationTopLeftCorner(GameModel.Point location, float position)
        {
            float Radius = _hexHeight / 4;

            var hexCentre = Layout.HexToPixel(_layout, new OffsetCoord(location.X, location.Y).QoffsetToCube());

            var xOnCircle = (float)Math.Cos(position) * Radius + hexCentre.X;
            var yOnCircle = (float)Math.Sin(position) * Radius + hexCentre.Y;

            var xTopLeft = (float)(xOnCircle - (_unitWidth / 2));
            var yTopLeft = (float)(yOnCircle - (_unitWidth / 2));

            return (xTopLeft, yTopLeft);
        }

        internal void DrawTrapezium(GameModel.Point location, float position, SolidBrush brush)
        {
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

        public void DrawTriangle(GameModel.Point location, float position, SolidBrush brush)
        {
            var topLeftCorner = UnitLocationTopLeftCorner(location, position);

            PointF[] points = 
            {
                new PointF(topLeftCorner.xTopLeft, topLeftCorner.yTopLeft),
                new PointF(topLeftCorner.xTopLeft + _unitWidth, topLeftCorner.yTopLeft),
                new PointF(topLeftCorner.xTopLeft + _unitWidth / 2, topLeftCorner.yTopLeft + _unitWidth)
            };

            _graphics.FillPolygon(brush, points);
        }

        internal void DrawRectangle(GameModel.Point location, SolidBrush brush)
        {
            var hexCentre = Layout.HexToPixel(_layout, new OffsetCoord(location.X, location.Y).QoffsetToCube());

            var x = (float)hexCentre.X - (_structureWidth / 2);
            var y = (float)hexCentre.Y - (_structureWidth / 2);

            _graphics.FillRectangle(brush, x, y, _structureWidth, _structureWidth);
        }

        public void LabelHexes(Pen pen, float xMin, float xMax, float yMin, float yMax, string[,] labels)
        {
            var font = new Font("Arial", (int)(_hexHeight * .2));

            if (labels == null)
                return;

            using (StringFormat sf = new StringFormat())
            {
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;

                for (var x = 0; x <= labels.GetUpperBound(0); x++)
                {
                    for (var y = 0; y <= labels.GetUpperBound(1); y++)
                    {
                        var points = Layout.PolygonCorners(_layout, new OffsetCoord(x, y).QoffsetToCube());
                        var worldX = (float)(points[0].X + points[3].X) / 2;
                        var worldY = (float)((points[1].Y + points[4].Y) / 2);
                        var label = labels[x, y];
                        _graphics.DrawString(label, font, Brushes.Black, worldX, worldY, sf);
                    }
                }
            }
        }


    }
}
