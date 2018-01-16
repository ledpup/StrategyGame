using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hexagon
{
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
}
