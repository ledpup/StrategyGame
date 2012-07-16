﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
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

        public override bool Equals(object obj)
        {
            var otherPoint = (Point)obj;
            return X == otherPoint.X && Y == otherPoint.Y;
        }
    }
}
