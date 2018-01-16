using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hexagon
{
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

        public static int OffsetCoordsToIndex(int x, int y, int width)
        {
            return y * width + x;
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
}
