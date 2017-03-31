using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using GameModel;

namespace Visualise
{
    public class GameBoardDrawing2D
    {
        Graphics _graphics;
        float _hexHeight;
        float _hexWidth;
        float _structureWidth;

        public GameBoardDrawing2D(Bitmap bitmap, int numberOfHexes)
        {
            _graphics = Graphics.FromImage(bitmap);
            _hexHeight = Convert.ToSingle(bitmap.Width) / (numberOfHexes - 2);
            _hexWidth = (float)(4 * (_hexHeight / 2 / Math.Sqrt(3)));
            _structureWidth = _hexHeight / 5;
        }

        public void DrawBoard(int width, int height, Dictionary<PointF, Brush> hexagonColours)
        {
            _graphics.SmoothingMode = SmoothingMode.AntiAlias;

            _graphics.FillRectangle(Brushes.White, 0, 0, width, height);

            foreach (PointF point in hexagonColours.Keys)
            {
                _graphics.FillPolygon(hexagonColours[point], HexToPoints(_hexWidth, _hexHeight, point.Y, point.X));
            }

            float xMax = width;
            float yMax = height;

            for (int row = 0; ; row++)
            {
                PointF[] points = HexToPoints(_hexWidth, _hexHeight, row, 0);

                if (points[4].Y > yMax)
                    break;

                for (int col = 0; ; col++)
                {
                    points = HexToPoints(_hexWidth, _hexHeight, row, col);

                    // If it doesn't fit horizontally,
                    // we're done with this row.
                    if (points[3].X > xMax)
                        break;

                    // If it fits vertically, draw it.
                    if (points[4].Y <= yMax)
                    {
                        _graphics.DrawPolygon(Pens.Black, points);
                    }
                }
            }

            //#if FIG34
            //            // Draw the selected rectangles for Figures 3 and 4.
            //            using (Pen pen = new Pen(Color.Red, 3))
            //            {
            //                pen.DashStyle = DashStyle.Dash;
            //                foreach (RectangleF rect in TestRects)
            //                {
            //                    e.Graphics.DrawRectangle(pen, Rectangle.Round(rect));
            //                }
            //            }
            //#endif
        }

        public void DrawEdge(GameModel.Point origin, GameModel.Point destination, Pen pen, bool isCentreToCentre, bool isPort)
        {
            (PointF pt1, PointF pt2) points;
            if (isCentreToCentre)
            {
                points.pt1 = HexCentre(_hexWidth, _hexHeight, origin.Y, origin.X);
                points.pt2 = HexCentre(_hexWidth, _hexHeight, destination.Y, destination.X);
            }
            else
            {
                points = HexSidePoints(origin, destination, _hexWidth, _hexHeight);
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

        private static (PointF pt1, PointF pt2) HexSidePoints(GameModel.Point origin, GameModel.Point destination, float hexWidth, float hexHeight)
        {
            var direction = new GameModel.Point(destination.X - origin.X, destination.Y - origin.Y);
            var points = HexToPoints(hexWidth, hexHeight, origin.Y, origin.X);
            PointF pt1, pt2;
            if (origin.X % 2 == 1)
            {
                if (direction.X == -1 && direction.Y == 0)
                {
                    pt1 = points[0];
                    pt2 = points[1];
                }
                else if (direction.X == 0 && direction.Y == -1)
                {
                    pt1 = points[1];
                    pt2 = points[2];
                }
                else if (direction.X == 1 && direction.Y == 0)
                {
                    pt1 = points[2];
                    pt2 = points[3];
                }
                else if (direction.X == 1 && direction.Y == 1)
                {
                    pt1 = points[3];
                    pt2 = points[4];
                }
                else if (direction.X == 0 && direction.Y == 1)
                {
                    pt1 = points[4];
                    pt2 = points[5];
                }
                else if (direction.X == -1 && direction.Y == 1)
                {
                    pt1 = points[5];
                    pt2 = points[0];
                }
                else
                {
                    throw new Exception("Vector has not been mapped to hexside"); // This should never happen.
                }
            }
            else
            {

                if (direction.X == -1 && direction.Y == 0)
                {
                    pt1 = points[5];
                    pt2 = points[0];
                }
                else if (direction.X == 0 && direction.Y == -1)
                {
                    pt1 = points[1];
                    pt2 = points[2];
                }
                else if (direction.X == 1 && direction.Y == 0)
                {
                    pt1 = points[3];
                    pt2 = points[4];
                }
                else if (direction.X == 1 && direction.Y == -1)
                {
                    pt1 = points[2];
                    pt2 = points[3];
                }
                else if (direction.X == 0 && direction.Y == 1)
                {
                    pt1 = points[4];
                    pt2 = points[5];
                }
                else if (direction.X == -1 && direction.Y == -1)
                {
                    pt1 = points[0];
                    pt2 = points[1];
                }
                else
                {
                    throw new Exception("Vector has not been mapped to hexside"); // This should never happen.
                }
            }
            return (pt1, pt2);
        }


        //internal void DrawCurvedRoads(Graphics graphics, List<Vector> roads)
        //{
        //    var tile = roads.First().Origin;

        //    var points = HexToPoints(_hexWidth, _hexHeight, tile.X, tile.Y);

        //    var radius = (points[2].X - points[1].X) / 2;

        //    //var pointsIncludingCentre = new List<PointF> { new PointF(points[0].X + (HexWidth / 2), points[0].Y) };
        //    //pointsIncludingCentre.AddRange(points);

        //    //points = pointsIncludingCentre.ToArray();

        //    //foreach (var point in points)
        //    //    graphics.DrawEllipse(Pens.Red, point.X - radius, point.Y - radius, 2 * radius, 2 * radius);
        //    //graphics.DrawEllipse(Pens.Red, points[0].X + (HexWidth / 2) - radius, points[0].Y - radius, 2 * radius, 2 * radius);

        //    StraightRoad(graphics, points, radius, 2);


        //    //graphics.DrawArc(new Pen(Color.Blue, 2), points[4].X - radius, points[4].Y - radius, 2 * radius, 2 * radius, 180, 60);
        //    //graphics.DrawArc(new Pen(Color.Blue, 2), points[3].X - radius, points[3].Y - radius, 2 * radius, 2 * radius, 180, 60);
        //}

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

            _graphics.FillEllipse(brush, topLeftCorner.xTopLeft, topLeftCorner.yTopLeft, _structureWidth, _structureWidth);
        }

        private (float xTopLeft, float yTopLeft) UnitLocationTopLeftCorner(GameModel.Point location, float position)
        {
            float Radius = _hexHeight / 4;

            var hexCentre = HexCentre(_hexWidth, _hexHeight, location.Y, location.X);

            var xOnCircle = (float)Math.Cos(position) * Radius + hexCentre.X;
            var yOnCircle = (float)Math.Sin(position) * Radius + hexCentre.Y;

            var xTopLeft = (xOnCircle - (_structureWidth / 2));
            var yTopLeft = (yOnCircle - (_structureWidth / 2));

            return (xTopLeft, yTopLeft);
        }

        internal void DrawTrapezium(GameModel.Point location, float position, SolidBrush brush)
        {
            var topLeftCorner = UnitLocationTopLeftCorner(location, position);

            PointF[] points =
            {
                new PointF(topLeftCorner.xTopLeft, topLeftCorner.yTopLeft),
                new PointF(topLeftCorner.xTopLeft + _structureWidth, topLeftCorner.yTopLeft),
                new PointF(topLeftCorner.xTopLeft + _structureWidth * .8F, topLeftCorner.yTopLeft + _structureWidth * .65F),
                new PointF(topLeftCorner.xTopLeft + _structureWidth * .2F, topLeftCorner.yTopLeft + _structureWidth * .65F),
                
            };

            _graphics.FillPolygon(brush, points);
        }

        public void DrawTriangle(GameModel.Point location, float position, SolidBrush brush)
        {
            var topLeftCorner = UnitLocationTopLeftCorner(location, position);

            PointF[] points = 
            {
                new PointF(topLeftCorner.xTopLeft, topLeftCorner.yTopLeft),
                new PointF(topLeftCorner.xTopLeft + _structureWidth, topLeftCorner.yTopLeft),
                new PointF(topLeftCorner.xTopLeft + _structureWidth / 2, topLeftCorner.yTopLeft + _structureWidth)
            };

            _graphics.FillPolygon(brush, points);
        }

        internal void DrawRectangle(GameModel.Point location, SolidBrush brush)
        {
            var hexCentre = HexCentre(_hexWidth, _hexHeight, location.Y, location.X);

            var x = hexCentre.X - (_structureWidth / 2);
            var y = hexCentre.Y - (_structureWidth / 2);

            _graphics.FillRectangle(brush, x, y, _structureWidth, _structureWidth);
        }

        private PointF HexCentre(float hexWidth, float hexHeight, float row, float col)
        {
            float y = hexHeight / 2;
            float x = 0;

            // Move down the required number of rows.
            y += row * hexHeight;

            // If the column is odd, move down half a hex more.
            if (col % 2 == 1) y += hexHeight / 2;

            // Move over for the column number.
            x += col * (hexWidth * 0.75f) + _hexWidth / 2;

            return new PointF(x, y);
        }

        public void LabelHexes(Pen pen, float xMin, float xMax, float yMin, float yMax, string[,] labels)
        {
            var font = new Font("Arial", (int)(_hexHeight * .2));

            for (int row = 0; ; row++)
            {
                PointF[] points = HexToPoints(_hexWidth, _hexHeight, row, 0);

                if (points[4].Y > yMax)
                    break;

                for (int col = 0; ; col++)
                {
                    points = HexToPoints(_hexWidth, _hexHeight, row, col);

                    // If it doesn't fit horizontally,
                    // we're done with this row.
                    if (points[3].X > xMax)
                        break;

                    // If it fits vertically, draw it.
                    if (points[4].Y <= yMax)
                    {
                        // Label the hexagon
                        if (labels != null)
                        {
                            using (StringFormat sf = new StringFormat())
                            {
                                sf.Alignment = StringAlignment.Center;
                                sf.LineAlignment = StringAlignment.Center;
                                var x = (points[0].X + points[3].X) / 2;
                                var y = ((points[1].Y + points[4].Y) / 2);

                                if (col <= labels.GetUpperBound(0) && row <= labels.GetUpperBound(1))
                                {
                                    var label = labels[col, row];
                                    _graphics.DrawString(label, font, Brushes.Black, x, y, sf);
                                }
                            }
                        }
                    }
                }
            }
        }

        // Return the points that define the indicated hexagon.
        private static PointF[] HexToPoints(float hexWidth, float hexHeight, float row, float col)
        {
            float y = hexHeight / 2;
            float x = 0;

            // Move down the required number of rows.
            y += row * hexHeight;

            // If the column is odd, move down half a hex more.
            if (col % 2 == 1) y += hexHeight / 2;

            // Move over for the column number.
            x += col * (hexWidth * 0.75f);

            // Generate the points.
            return new PointF[]
                {
                    new PointF(x, y),
                    new PointF(x + hexWidth * 0.25f, y - hexHeight / 2),
                    new PointF(x + hexWidth * 0.75f, y - hexHeight / 2),
                    new PointF(x + hexWidth, y),
                    new PointF(x + hexWidth * 0.75f, y + hexHeight / 2),
                    new PointF(x + hexWidth * 0.25f, y + hexHeight / 2),
                };
        }

        // Return the row and column of the hexagon at this point.
        //private void PointToHex(float x, float y, float height, out int row, out int col)
        //{
        //    // Find the test rectangle containing the point.
        //    float width = HexWidth(height);
        //    col = (int)(x / (width * 0.75f));

        //    if (col % 2 == 0)
        //        row = (int)(y / height);
        //    else
        //        row = (int)((y - height / 2) / height);

        //    // Find the test area.
        //    float testx = col * width * 0.75f;
        //    float testy = row * height;
        //    if (col % 2 == 1) testy += height / 2;

        //    // See if the point is above or
        //    // below the test hexagon on the left.
        //    bool is_above = false, is_below = false;
        //    float dx = x - testx;
        //    if (dx < width / 4)
        //    {
        //        float dy = y - (testy + height / 2);
        //        if (dx < 0.001)
        //        {
        //            // The point is on the left edge of the test rectangle.
        //            if (dy < 0) is_above = true;
        //            if (dy > 0) is_below = true;
        //        }
        //        else if (dy < 0)
        //        {
        //            // See if the point is above the test hexagon.
        //            if (-dy / dx > Math.Sqrt(3)) is_above = true;
        //        }
        //        else
        //        {
        //            // See if the point is below the test hexagon.
        //            if (dy / dx > Math.Sqrt(3)) is_below = true;
        //        }
        //    }

        //    // Adjust the row and column if necessary.
        //    if (is_above)
        //    {
        //        if (col % 2 == 0) row--;
        //        col--;
        //    }
        //    else if (is_below)
        //    {
        //        if (col % 2 == 1) row++;
        //        col--;
        //    }
        //}


    }
}
