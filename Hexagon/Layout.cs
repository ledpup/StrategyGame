using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hexagon
{
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
