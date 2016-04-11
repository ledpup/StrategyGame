// Code from http://www.redblobgames.com/grids/hexagons/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameModel;

namespace Tests
{
    [TestClass]
    class Tests
    {
        [TestMethod]
        static public void EqualHex(String name, Hex a, Hex b)
        {
            if (!(a.q == b.q && a.s == b.s && a.r == b.r))
            {
                Tests.Complain(name);
            }
        }


        [TestMethod]
        static public void EqualOffsetcoord(String name, OffsetCoord a, OffsetCoord b)
        {
            if (!(a.col == b.col && a.row == b.row))
            {
                Tests.Complain(name);
            }
        }


        [TestMethod]
        static public void EqualInt(String name, int a, int b)
        {
            if (!(a == b))
            {
                Tests.Complain(name);
            }
        }


        [TestMethod]
        static public void EqualHexArray(String name, List<Hex> a, List<Hex> b)
        {
            Tests.EqualInt(name, a.Count, b.Count);
            for (int i = 0; i < a.Count; i++)
            {
                Tests.EqualHex(name, a[i], b[i]);
            }
        }


        [TestMethod]
        static public void TestHexArithmetic()
        {
            Tests.EqualHex("hex_add", new Hex(4, -10, 6), Hex.Add(new Hex(1, -3, 2), new Hex(3, -7, 4)));
            Tests.EqualHex("hex_subtract", new Hex(-2, 4, -2), Hex.Subtract(new Hex(1, -3, 2), new Hex(3, -7, 4)));
        }


        [TestMethod]
        static public void TestHexDirection()
        {
            Tests.EqualHex("hex_direction", new Hex(0, -1, 1), Hex.Direction(2));
        }


        [TestMethod]
        static public void TestHexNeighbor()
        {
            Tests.EqualHex("hex_neighbor", new Hex(1, -3, 2), Hex.Neighbor(new Hex(1, -2, 1), 2));
        }


        [TestMethod]
        static public void TestHexDiagonal()
        {
            Tests.EqualHex("hex_diagonal", new Hex(-1, -1, 2), Hex.DiagonalNeighbor(new Hex(1, -2, 1), 3));
        }


        [TestMethod]
        static public void TestHexDistance()
        {
            Tests.EqualInt("hex_distance", 7, Hex.Distance(new Hex(3, -7, 4), new Hex(0, 0, 0)));
        }


        [TestMethod]
        static public void TestHexRound()
        {
            Hex a = new Hex(0, 0, 0);
            Hex b = new Hex(1, -1, 0);
            Hex c = new Hex(0, -1, 1);
            Tests.EqualHex("hex_round 1", new Hex(5, -10, 5), FractionalHex.HexRound(FractionalHex.HexLerp(new Hex(0, 0, 0), new Hex(10, -20, 10), 0.5)));
            Tests.EqualHex("hex_round 2", a, FractionalHex.HexRound(FractionalHex.HexLerp(a, b, 0.499)));
            Tests.EqualHex("hex_round 3", b, FractionalHex.HexRound(FractionalHex.HexLerp(a, b, 0.501)));
            Tests.EqualHex("hex_round 4", a, FractionalHex.HexRound(new FractionalHex(a.q * 0.4 + b.q * 0.3 + c.q * 0.3, a.r * 0.4 + b.r * 0.3 + c.r * 0.3, a.s * 0.4 + b.s * 0.3 + c.s * 0.3)));
            Tests.EqualHex("hex_round 5", c, FractionalHex.HexRound(new FractionalHex(a.q * 0.3 + b.q * 0.3 + c.q * 0.4, a.r * 0.3 + b.r * 0.3 + c.r * 0.4, a.s * 0.3 + b.s * 0.3 + c.s * 0.4)));
        }


        [TestMethod]
        static public void TestHexLinedraw()
        {
            Tests.EqualHexArray("hex_linedraw", new List<Hex> { new Hex(0, 0, 0), new Hex(0, -1, 1), new Hex(0, -2, 2), new Hex(1, -3, 2), new Hex(1, -4, 3), new Hex(1, -5, 4) }, FractionalHex.HexLinedraw(new Hex(0, 0, 0), new Hex(1, -5, 4)));
        }


        [TestMethod]
        static public void TestLayout()
        {
            Hex h = new Hex(3, 4, -7);
            Layout flat = new Layout(Layout.flat, new PointD(10, 15), new PointD(35, 71));
            Tests.EqualHex("layout", h, FractionalHex.HexRound(Layout.PixelToHex(flat, Layout.HexToPixel(flat, h))));
            Layout pointy = new Layout(Layout.pointy, new PointD(10, 15), new PointD(35, 71));
            Tests.EqualHex("layout", h, FractionalHex.HexRound(Layout.PixelToHex(pointy, Layout.HexToPixel(pointy, h))));
        }


        [TestMethod]
        static public void TestConversionRoundtrip()
        {
            Hex a = new Hex(3, 4, -7);
            OffsetCoord b = new OffsetCoord(1, -3);
            Tests.EqualHex("conversion_roundtrip even-q", a, OffsetCoord.QoffsetToCube(OffsetCoord.EVEN, OffsetCoord.QoffsetFromCube(OffsetCoord.EVEN, a)));
            Tests.EqualOffsetcoord("conversion_roundtrip even-q", b, OffsetCoord.QoffsetFromCube(OffsetCoord.EVEN, OffsetCoord.QoffsetToCube(OffsetCoord.EVEN, b)));
            Tests.EqualHex("conversion_roundtrip odd-q", a, OffsetCoord.QoffsetToCube(OffsetCoord.ODD, OffsetCoord.QoffsetFromCube(OffsetCoord.ODD, a)));
            Tests.EqualOffsetcoord("conversion_roundtrip odd-q", b, OffsetCoord.QoffsetFromCube(OffsetCoord.ODD, OffsetCoord.QoffsetToCube(OffsetCoord.ODD, b)));
            Tests.EqualHex("conversion_roundtrip even-r", a, OffsetCoord.RoffsetToCube(OffsetCoord.EVEN, OffsetCoord.RoffsetFromCube(OffsetCoord.EVEN, a)));
            Tests.EqualOffsetcoord("conversion_roundtrip even-r", b, OffsetCoord.RoffsetFromCube(OffsetCoord.EVEN, OffsetCoord.RoffsetToCube(OffsetCoord.EVEN, b)));
            Tests.EqualHex("conversion_roundtrip odd-r", a, OffsetCoord.RoffsetToCube(OffsetCoord.ODD, OffsetCoord.RoffsetFromCube(OffsetCoord.ODD, a)));
            Tests.EqualOffsetcoord("conversion_roundtrip odd-r", b, OffsetCoord.RoffsetFromCube(OffsetCoord.ODD, OffsetCoord.RoffsetToCube(OffsetCoord.ODD, b)));
        }


        [TestMethod]
        static public void TestOffsetFromCube()
        {
            Tests.EqualOffsetcoord("offset_from_cube even-q", new OffsetCoord(1, 3), OffsetCoord.QoffsetFromCube(OffsetCoord.EVEN, new Hex(1, 2, -3)));
            Tests.EqualOffsetcoord("offset_from_cube odd-q", new OffsetCoord(1, 2), OffsetCoord.QoffsetFromCube(OffsetCoord.ODD, new Hex(1, 2, -3)));
        }


        [TestMethod]
        static public void TestOffsetToCube()
        {
            Tests.EqualHex("offset_to_cube even-", new Hex(1, 2, -3), OffsetCoord.QoffsetToCube(OffsetCoord.EVEN, new OffsetCoord(1, 3)));
            Tests.EqualHex("offset_to_cube odd-q", new Hex(1, 2, -3), OffsetCoord.QoffsetToCube(OffsetCoord.ODD, new OffsetCoord(1, 2)));
        }


        static public void Complain(String name)
        {
            Console.WriteLine("FAIL " + name);
        }

    }
}
