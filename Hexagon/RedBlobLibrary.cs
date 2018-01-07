// Code from http://www.redblobgames.com/grids/hexagons/

using System;
using System.Collections.Generic;

namespace Hexagon
{
    public struct PointD
    {
        public PointD(double x, double y)
        {
            X = x;
            Y = y;
        }
        public readonly double X;
        public readonly double Y;

        double GetAngleFromPoints(PointD destination)
        {
            return Theta(this, destination);
        }

        public static double Theta(PointD origin, PointD destination)
        {
            var dy = -(destination.Y - origin.Y);
            var dx = (destination.X - origin.X);

            var PRECISION = 10;

            var rad = (Math.Round(dy, PRECISION) == 0 && Math.Round(dx, PRECISION) == 0) ? 0 : Math.Atan2(dy, dx);

            if (rad < 0)
            {
                rad = 2 * Math.PI + rad;
            }
            return 180 * rad / Math.PI;
        }

        PointD Offset(double dx, double dy)
        {
            return new PointD(X + dx, Y + dy);
        }
        public PointD Move(PointD target, double distance, double degreeOffset = 0)
        {
            var theta = Maths.DegreeToRadian(GetAngleFromPoints(target) + degreeOffset);
            return Offset(Math.Cos(theta) * distance, -Math.Sin(theta) * distance);
        }

        public static PointD EdgeToCentrePoint(PointD[] hexPoints, float edgeLength, int edge, double pixelOffset = 0)
        {
            var sourcePoint = hexPoints[edge];
            var targetPoint = hexPoints[(edge + 1) % 6];

            var offset = edgeLength / 2 + pixelOffset;
            return sourcePoint.Move(targetPoint, offset);
        }
    }

    public static class Maths
    {
        public static double Normalise(double value, double start, double end)
        {
            double width = end - start;
            double offsetValue = value - start;

            return (offsetValue - (Math.Floor(offsetValue / width) * width)) + start;
        }

        public static double DegreeToRadian(double angle)
        {
            angle = Normalise(angle, 0, 360);

            return Math.PI * angle / 180.0;
        }

        public static double RadianToDegree(double angle)
        {
            return angle * (180.0 / Math.PI);
        }
    }

    public struct Hex
    {
        public Hex(int q, int r, int s = 0)
        {
            this.q = q;
            this.r = r;
            this.s = -q - r;
        }
        public readonly int q;
        public readonly int r;
        public readonly int s;

        static public Hex Add(Hex a, Hex b)
        {
            return new Hex(a.q + b.q, a.r + b.r, a.s + b.s);
        }

        static public Hex Subtract(Hex a, Hex b)
        {
            return new Hex(a.q - b.q, a.r - b.r, a.s - b.s);
        }


        static public Hex Scale(Hex a, int k)
        {
            return new Hex(a.q * k, a.r * k, a.s * k);
        }

        static public List<Hex> Directions = new List<Hex> { new Hex(-1, 0, 1), new Hex(0, -1, 1), new Hex(1, -1, 0), new Hex(1, 0, -1), new Hex(0, 1, -1), new Hex(-1, 1, 0), };

        static public Hex Direction(int direction)
        {
            return Directions[direction];
        }

        public static int HexToIndex(Hex hex, int boardWidth, int boardHeight)
        {
            return HexToIndex(hex, OffsetCoord.ODD, boardWidth, boardHeight);
        }

        static int HexToIndex(Hex hex, int offset, int boardWidth, int boardHeight)
        {
            var hexOffset = hex.q / 2;

            if (hex.q < 0 || hex.q >= boardWidth || hex.r < -hexOffset || hex.r >= boardHeight - hexOffset)
                throw new Exception("Hex is outside the border of the board");

            int col = hex.q;
            int row = hex.r + ((hex.q + offset * (hex.q & 1)) / 2);

            var index = row * boardWidth + col;               

            return index;
        }

        public static List<Hex> Neighbours(Hex hex)
        {
            var neighbours = new List<Hex>();
            for (var i = 0; i < 6; i++)
            {
                neighbours.Add(Add(hex, Direction(i)));
            }
            return neighbours;
        }

        static public Hex Neighbor(Hex hex, int direction)
        {
            return Add(hex, Direction(direction));
        }

        static public List<Hex> Diagonals = new List<Hex> { new Hex(2, -1, -1), new Hex(1, -2, 1), new Hex(-1, -1, 2), new Hex(-2, 1, 1), new Hex(-1, 2, -1), new Hex(1, 1, -2) };

        static public Hex DiagonalNeighbor(Hex hex, int direction)
        {
            return Hex.Add(hex, Hex.Diagonals[direction]);
        }

        static public int Length(Hex hex)
        {
            return (Math.Abs(hex.q) + Math.Abs(hex.r) + Math.Abs(hex.s)) / 2;
        }


        static public int Distance(Hex a, Hex b)
        {
            return Hex.Length(Hex.Subtract(a, b));
        }

        public override string ToString()
        {
            return q + ", " + r;
        }

        public static List<Hex> HexesWithinArea(Hex centreHex, int distance, int boardWidth, int boardHeight)
        {
            var results = new List<Hex>();
            for (var q = -distance; q <= distance; q++)
            {
                var adjustedQ = centreHex.q + q;
                if (adjustedQ < 0 || adjustedQ >= boardWidth)
                    continue;

                var offset = adjustedQ / 2;

                for (var r = Math.Max(-distance, -q - distance); r <= Math.Min(distance, -q + distance); r++)
                {
                    var adjustedR = centreHex.r + r;

                    if (adjustedR < -offset || adjustedR >= boardHeight - offset)
                        continue;

                    results.Add(Add(centreHex, new Hex(q, r)));
                }
            }
            return results;
        }

        public static List<Hex> HexRing(Hex centreHex, int radius, int boardWidth, int boardHeight)
        {
            if (radius < 1)
                return new List<Hex> { centreHex };

            var results = new List<Hex>();

            var hex = Add(centreHex, Scale(Direction(4), radius));
            
            for (var i = 0; i < 6; i++)
            {
                for (var j = 0; j < radius; j++)
                {
                    // Check borders
                    // I think you can set offset = q / 2 and then compare -offset <= r <= height - offset.
                    // In column q = 0, offset = 0. It'd be 0 <= r <= 18 - 0; in column q = 26, offset = 13 so it'd be -13 <= r <= 18 - 13.
                    var offset = hex.q / 2;

                    if ( !(hex.q < 0 || hex.q >= boardWidth || hex.r < -offset || hex.r >= boardHeight - offset) )
                        results.Add(hex);

                    hex = Neighbor(hex, i);
                }
            }
            return results;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Hex))
                return false;

            Hex hex = (Hex)obj;

            return (q == hex.q) && (r == hex.r) && (s == hex.s);
        }
    }

    public struct FractionalHex
    {
        public FractionalHex(double q, double r, double s)
        {
            this.q = q;
            this.r = r;
            this.s = s;
        }
        public readonly double q;
        public readonly double r;
        public readonly double s;

        static public Hex HexRound(FractionalHex h)
        {
            int q = (int)(Math.Round(h.q));
            int r = (int)(Math.Round(h.r));
            int s = (int)(Math.Round(h.s));
            double q_diff = Math.Abs(q - h.q);
            double r_diff = Math.Abs(r - h.r);
            double s_diff = Math.Abs(s - h.s);
            if (q_diff > r_diff && q_diff > s_diff)
            {
                q = -r - s;
            }
            else
                if (r_diff > s_diff)
            {
                r = -q - s;
            }
            else
            {
                s = -q - r;
            }
            return new Hex(q, r, s);
        }


        static public FractionalHex HexLerp(Hex a, Hex b, double t)
        {
            return new FractionalHex(a.q + (b.q - a.q) * t, a.r + (b.r - a.r) * t, a.s + (b.s - a.s) * t);
        }


        static public List<Hex> HexLinedraw(Hex a, Hex b)
        {
            int N = Hex.Distance(a, b);
            List<Hex> results = new List<Hex> { };
            double step = 1.0 / Math.Max(N, 1);
            for (int i = 0; i <= N; i++)
            {
                results.Add(FractionalHex.HexRound(FractionalHex.HexLerp(a, b, step * i)));
            }
            return results;
        }
    }

    /// <summary>
    /// There are four offset types: odd-r, even-r, odd-q, even-q. The “r” types are used with with pointy top hexagons and the “q” types are used with flat top. Whether it’s even or odd can be encoded as +1 or -1.
    /// </summary>
    public struct OffsetCoord
    {
        public OffsetCoord(int col, int row)
        {
            this.col = col;
            this.row = row;
        }
        public readonly int col;
        public readonly int row;
        static public int EVEN = 1;
        static public int ODD = -1;

        public static OffsetCoord QoffsetFromCube(Hex h)
        {
            return QoffsetFromCube(ODD, h);
        }

        static OffsetCoord QoffsetFromCube(int offset, Hex h)
        {
            int col = h.q;
            int row = h.r + ((h.q + offset * (h.q & 1)) / 2);
            return new OffsetCoord(col, row);
        }

        public Hex QoffsetToCube()
        {
            return QoffsetToCube(ODD, this); // Always using the odd layout
        }

        public static Hex QoffsetToCube(OffsetCoord o)
        {
            return QoffsetToCube(ODD, o); // Always using the odd layout
        }

        static Hex QoffsetToCube(int offset, OffsetCoord o)
        {
            int q = o.col;
            int r = o.row - ((o.col + offset * (o.col & 1)) / 2);
            int s = -q - r;
            return new Hex(q, r, s);
        }

        // Not using pointy hex orientation for this game

        //static public OffsetCoord RoffsetFromCube(int offset, Hex h)
        //{
        //    int col = h.q + ((h.r + offset * (h.r & 1)) / 2);
        //    int row = h.r;
        //    return new OffsetCoord(col, row);
        //}

        //static public Hex RoffsetToCube(int offset, OffsetCoord h)
        //{
        //    int q = h.col - ((h.row + offset * (h.row & 1)) / 2);
        //    int r = h.row;
        //    int s = -q - r;
        //    return new Hex(q, r, s);
        //}
    }

    public struct Orientation
    {
        public Orientation(double f0, double f1, double f2, double f3, double b0, double b1, double b2, double b3, double start_angle)
        {
            this.f0 = f0;
            this.f1 = f1;
            this.f2 = f2;
            this.f3 = f3;
            this.b0 = b0;
            this.b1 = b1;
            this.b2 = b2;
            this.b3 = b3;
            this.start_angle = start_angle;
        }
        public readonly double f0;
        public readonly double f1;
        public readonly double f2;
        public readonly double f3;
        public readonly double b0;
        public readonly double b1;
        public readonly double b2;
        public readonly double b3;
        public readonly double start_angle;
    }

    public struct Layout
    {
        public Layout(Orientation orientation, PointD size, PointD origin)
        {
            this.orientation = orientation;
            this.size = size;
            this.origin = origin;
        }
        public readonly Orientation orientation;
        public readonly PointD size;
        public readonly PointD origin;
        static public Orientation pointy = new Orientation(Math.Sqrt(3.0), Math.Sqrt(3.0) / 2.0, 0.0, 3.0 / 2.0, Math.Sqrt(3.0) / 3.0, -1.0 / 3.0, 0.0, 2.0 / 3.0, 0.5);
        static public Orientation flat = new Orientation(3.0 / 2.0, 0.0, Math.Sqrt(3.0) / 2.0, Math.Sqrt(3.0), 2.0 / 3.0, 0.0, -1.0 / 3.0, Math.Sqrt(3.0) / 3.0, 0.0);

        static public PointD HexToPixel(Layout layout, Hex h)
        {
            Orientation M = layout.orientation;
            PointD size = layout.size;
            PointD origin = layout.origin;
            double x = (M.f0 * h.q + M.f1 * h.r) * size.X;
            double y = (M.f2 * h.q + M.f3 * h.r) * size.Y;
            return new PointD(x + origin.X, y + origin.Y);
        }


        static public FractionalHex PixelToHex(Layout layout, PointD p)
        {
            Orientation M = layout.orientation;
            PointD size = layout.size;
            PointD origin = layout.origin;
            PointD pt = new PointD((p.X - origin.X) / size.X, (p.Y - origin.Y) / size.Y);
            double q = M.b0 * pt.X + M.b1 * pt.Y;
            double r = M.b2 * pt.X + M.b3 * pt.Y;
            return new FractionalHex(q, r, -q - r);
        }


        static public PointD HexCornerOffset(Layout layout, int corner)
        {
            Orientation M = layout.orientation;
            PointD size = layout.size;
            double angle = 2.0 * Math.PI * (corner + M.start_angle) / 6;
            return new PointD(size.X * Math.Cos(angle), size.Y * Math.Sin(angle));
        }


        static public List<PointD> PolygonCorners(Layout layout, Hex h)
        {
            List<PointD> corners = new List<PointD> { };
            PointD center = HexToPixel(layout, h);
            for (int i = 0; i < 6; i++)
            {
                PointD offset = HexCornerOffset(layout, i);
                corners.Add(new PointD(center.X + offset.X, center.Y + offset.Y));
            }
            return corners;
        }
    }
}