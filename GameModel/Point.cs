﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameModel
{
    public struct Point
    {
        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X;
        public int Y;

        //public override int GetHashCode()
        //{
        //    return X * Y;
        //}

        public override string ToString()
        {
            return String.Format("({0}, {1})", X, Y);
        }

        public override bool Equals(object obj)
        {
            var otherPoint = (Point)obj;
            return X == otherPoint.X && Y == otherPoint.Y;
        }

        public static int PointToIndex(int x, int y, int width)
        {
            return y * width + x;
        }

        public static Point IndexToPoint(int index, int width)
        {
            return new Point(index % width, index / width);
        }
    }
}