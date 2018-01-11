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
}