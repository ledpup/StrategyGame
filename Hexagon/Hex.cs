using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hexagon
{
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

        public override bool Equals(object obj)
        {
            if (!(obj is Hex))
                return false;

            Hex hex = (Hex)obj;

            return (q == hex.q) && (r == hex.r) && (s == hex.s);
        }

        public static bool operator ==(Hex a, Hex b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Hex a, Hex b)
        {
            return !a.Equals(b);
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

                    if (!(hex.q < 0 || hex.q >= boardWidth || hex.r < -offset || hex.r >= boardHeight - offset))
                        results.Add(hex);

                    hex = Neighbor(hex, i);
                }
            }
            return results;
        }

        public static Hex IndexToHex(int index, int width)
        {
            var offsetCoords = new OffsetCoord(index % width, index / width);
            return offsetCoords.QoffsetToCube();
        }
    }
}
