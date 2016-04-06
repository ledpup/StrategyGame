using GameModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Visualise
{
    public class HexGrid
    {
        const float HexHeight = 50;
        static float HexWidth = (float)(4 * (HexHeight / 2 / Math.Sqrt(3)));



        public static void DrawLine(Graphics graphics, GameModel.Point origin, GameModel.Point destination, Pen pen, int boardWidth, BaseEdgeType baseEdgeType)
        {
            PointF pt1, pt2;
            if (baseEdgeType == BaseEdgeType.CentreToCentre)
            {
                pt1 = HexCentre(HexWidth, HexHeight, origin.Y, origin.X);
                pt2 = HexCentre(HexWidth, HexHeight, destination.Y, destination.X);
            }
            else
            {
                var direction = new GameModel.Point(destination.X - origin.X, destination.Y - origin.Y);
                var points = HexToPoints(HexWidth, HexHeight, origin.Y, origin.X);
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
            }

            graphics.DrawLine(pen, pt1, pt2);
        }

        private static PointF HexCentre(float hexWidth, float hexHeight, float row, float col)
        {
            float y = hexHeight / 2;
            float x = 0;

            // Move down the required number of rows.
            y += row * hexHeight;

            // If the column is odd, move down half a hex more.
            if (col % 2 == 1) y += hexHeight / 2;

            // Move over for the column number.
            x += col * (hexWidth * 0.75f) + HexWidth / 2;

            return new PointF(x, y);
        }

        public static void DrawBoard(Graphics graphics, int width, int height, Dictionary<PointF, Brush> hexagonColours, int boardWidth)
        {
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            graphics.FillRectangle(Brushes.White, 0, 0, width, height);

            foreach (PointF point in hexagonColours.Keys)
            {
                graphics.FillPolygon(hexagonColours[point], HexToPoints(HexWidth, HexHeight, point.Y, point.X));
            }

            // Draw the grid.
            DrawHexGrid(graphics, Pens.Black, 0, width, 0, height, HexWidth, HexHeight, boardWidth);

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
        private static void DrawHexGrid(Graphics graphics, Pen pen, float xMin, float xMax, float yMin, float yMax, float hexWidth, float hexHeight, int boardWidth)
        {
            var font = new Font("Arial", (int)(HexHeight * .2));

            for (int row = 0; ; row++)
            {
                PointF[] points = HexToPoints(hexWidth, hexHeight, row, 0);

                if (points[4].Y > yMax)
                    break;

                // Draw the row.
                for (int col = 0; ; col++)
                {
                    points = HexToPoints(hexWidth, hexHeight, row, col);

                    // If it doesn't fit horizontally,
                    // we're done with this row.
                    if (points[3].X > xMax)
                        break;

                    // If it fits vertically, draw it.
                    if (points[4].Y <= yMax)
                    {
                        graphics.DrawPolygon(pen, points);

                        // Label the hexagon
                        using (StringFormat sf = new StringFormat())
                        {
                            sf.Alignment = StringAlignment.Center;
                            sf.LineAlignment = StringAlignment.Center;
                            var x = (points[0].X + points[3].X) / 2;
                            var y = ((points[1].Y + points[4].Y) / 2) - (hexHeight * .15f);
                            var label = (row * boardWidth + col).ToString();
                            //var label = "(" + col.ToString() + ", " + row.ToString() + ")";

                            graphics.DrawString(label, font, Brushes.Black, x, y, sf);
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
