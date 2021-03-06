﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hexagon
{
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
