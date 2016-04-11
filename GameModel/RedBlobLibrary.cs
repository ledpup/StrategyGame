// Code from http://www.redblobgames.com/grids/hexagons/

using System;
using System.Linq;
using System.Collections.Generic;

namespace GameModel
{
    public struct PointD
    {
        public PointD(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
        public readonly double x;
        public readonly double y;
    }

    public struct Hex
    {
        public Hex(int q, int r, int s)
        {
            this.q = q;
            this.r = r;
            this.s = s;
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

        static public List<Hex> directions = new List<Hex> { new Hex(1, 0, -1), new Hex(1, -1, 0), new Hex(0, -1, 1), new Hex(-1, 0, 1), new Hex(-1, 1, 0), new Hex(0, 1, -1) };

        static public Hex Direction(int direction)
        {
            return Hex.directions[direction];
        }


        static public Hex Neighbor(Hex hex, int direction)
        {
            return Hex.Add(hex, Hex.Direction(direction));
        }

        static public List<Hex> diagonals = new List<Hex> { new Hex(2, -1, -1), new Hex(1, -2, 1), new Hex(-1, -1, 2), new Hex(-2, 1, 1), new Hex(-1, 2, -1), new Hex(1, 1, -2) };

        static public Hex DiagonalNeighbor(Hex hex, int direction)
        {
            return Hex.Add(hex, Hex.diagonals[direction]);
        }


        static public int Length(Hex hex)
        {
            return (int)((Math.Abs(hex.q) + Math.Abs(hex.r) + Math.Abs(hex.s)) / 2);
        }


        static public int Distance(Hex a, Hex b)
        {
            return Hex.Length(Hex.Subtract(a, b));
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

        static public OffsetCoord QoffsetFromCube(int offset, Hex h)
        {
            int col = h.q;
            int row = h.r + (int)((h.q + offset * (h.q & 1)) / 2);
            return new OffsetCoord(col, row);
        }


        static public Hex QoffsetToCube(int offset, OffsetCoord h)
        {
            int q = h.col;
            int r = h.row - (int)((h.col + offset * (h.col & 1)) / 2);
            int s = -q - r;
            return new Hex(q, r, s);
        }


        static public OffsetCoord RoffsetFromCube(int offset, Hex h)
        {
            int col = h.q + (int)((h.r + offset * (h.r & 1)) / 2);
            int row = h.r;
            return new OffsetCoord(col, row);
        }


        static public Hex RoffsetToCube(int offset, OffsetCoord h)
        {
            int q = h.col - (int)((h.row + offset * (h.row & 1)) / 2);
            int r = h.row;
            int s = -q - r;
            return new Hex(q, r, s);
        }

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
            double x = (M.f0 * h.q + M.f1 * h.r) * size.x;
            double y = (M.f2 * h.q + M.f3 * h.r) * size.y;
            return new PointD(x + origin.x, y + origin.y);
        }


        static public FractionalHex PixelToHex(Layout layout, PointD p)
        {
            Orientation M = layout.orientation;
            PointD size = layout.size;
            PointD origin = layout.origin;
            PointD pt = new PointD((p.x - origin.x) / size.x, (p.y - origin.y) / size.y);
            double q = M.b0 * pt.x + M.b1 * pt.y;
            double r = M.b2 * pt.x + M.b3 * pt.y;
            return new FractionalHex(q, r, -q - r);
        }


        static public PointD HexCornerOffset(Layout layout, int corner)
        {
            Orientation M = layout.orientation;
            PointD size = layout.size;
            double angle = 2.0 * Math.PI * (corner + M.start_angle) / 6;
            return new PointD(size.x * Math.Cos(angle), size.y * Math.Sin(angle));
        }


        static public List<PointD> PolygonCorners(Layout layout, Hex h)
        {
            List<PointD> corners = new List<PointD> { };
            PointD center = Layout.HexToPixel(layout, h);
            for (int i = 0; i < 6; i++)
            {
                PointD offset = Layout.HexCornerOffset(layout, i);
                corners.Add(new PointD(center.x + offset.x, center.y + offset.y));
            }
            return corners;
        }

    }



    // Tests


    

}