using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
