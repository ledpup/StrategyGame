// Code from http://www.redblobgames.com/grids/hexagons/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using GameModel;
using System.IO;
using Hexagon;

namespace Tests;

[TestClass]
public class HexTests
{
    static string[] GameBoard = File.ReadAllLines("BasicBoard.txt");

    public static void EqualHex(String name, Hex a, Hex b)
    {
        if (!(a.q == b.q && a.s == b.s && a.r == b.r))
        {
            HexTests.Complain(name);
        }
    }


    public static void EqualOffsetcoord(String name, OffsetCoord a, OffsetCoord b)
    {
        if (!(a.col == b.col && a.row == b.row))
        {
            HexTests.Complain(name);
        }
    }


    public static void EqualInt(String name, int a, int b)
    {
        if (!(a == b))
        {
            HexTests.Complain(name);
        }
    }


    public static void EqualHexArray(String name, List<Hex> a, List<Hex> b)
    {
        HexTests.EqualInt(name, a.Count, b.Count);
        for (int i = 0; i < a.Count; i++)
        {
            HexTests.EqualHex(name, a[i], b[i]);
        }
    }


    [TestMethod]
    public void Hex_AddAndSubtract_ReturnsExpectedCoordinates()
    {
        HexTests.EqualHex("hex_add", new Hex(4, -10), Hex.Add(new Hex(1, -3), new Hex(3, -7)));
        HexTests.EqualHex("hex_subtract", new Hex(-2, 4), Hex.Subtract(new Hex(1, -3), new Hex(3, -7)));
    }


    [TestMethod]
    public void Hex_Direction_ReturnsExpectedNeighborVector()
    {
        HexTests.EqualHex("hex_direction", new Hex(0, -1), Hex.Direction(2));
    }


    [TestMethod]
    public void Hex_Neighbor_ReturnsAdjacentHex()
    {
        HexTests.EqualHex("hex_neighbor", new Hex(1, -3), Hex.Neighbor(new Hex(1, -2), 2));
    }


    [TestMethod]
    public void Hex_DiagonalNeighbor_ReturnsDiagonalHex()
    {
        HexTests.EqualHex("hex_diagonal", new Hex(-1, -1), Hex.DiagonalNeighbor(new Hex(1, -2), 3));
    }


    [TestMethod]
    public void Hex_Distance_ReturnsExpectedDistance()
    {
        HexTests.EqualInt("hex_distance", 7, Hex.Distance(new Hex(3, -7), new Hex(0, 0)));
    }


    [TestMethod]
    public void FractionalHex_HexRound_ReturnsNearestHex()
    {
        Hex a = new(0, 0);
        Hex b = new(1, -1);
        Hex c = new(0, -1);
        HexTests.EqualHex("hex_round 1", new Hex(5, -10), FractionalHex.HexRound(FractionalHex.HexLerp(new Hex(0, 0), new Hex(10, -20), 0.5)));
        HexTests.EqualHex("hex_round 2", a, FractionalHex.HexRound(FractionalHex.HexLerp(a, b, 0.499)));
        HexTests.EqualHex("hex_round 3", b, FractionalHex.HexRound(FractionalHex.HexLerp(a, b, 0.501)));
        HexTests.EqualHex("hex_round 4", a, FractionalHex.HexRound(new FractionalHex(a.q * 0.4 + b.q * 0.3 + c.q * 0.3, a.r * 0.4 + b.r * 0.3 + c.r * 0.3, a.s * 0.4 + b.s * 0.3 + c.s * 0.3)));
        HexTests.EqualHex("hex_round 5", c, FractionalHex.HexRound(new FractionalHex(a.q * 0.3 + b.q * 0.3 + c.q * 0.4, a.r * 0.3 + b.r * 0.3 + c.r * 0.4, a.s * 0.3 + b.s * 0.3 + c.s * 0.4)));
    }


    [TestMethod]
    public void FractionalHex_HexLinedraw_ReturnsLineHexes()
    {
        HexTests.EqualHexArray("hex_linedraw", [new(0, 0), new(0, -1), new(0, -2), new(1, -3), new(1, -4), new(1, -5)], FractionalHex.HexLinedraw(new Hex(0, 0), new Hex(1, -5)));
    }


    [TestMethod]
    public void Layout_PixelRoundTrip_ReturnsOriginalHex()
    {
        Hex h = new(3, 4);
        Layout flat = new(Layout.flat, new PointD(10, 15), new PointD(35, 71));
        HexTests.EqualHex("layout", h, FractionalHex.HexRound(Layout.PixelToHex(flat, Layout.HexToPixel(flat, h))));
        Layout pointy = new(Layout.pointy, new PointD(10, 15), new PointD(35, 71));
        HexTests.EqualHex("layout", h, FractionalHex.HexRound(Layout.PixelToHex(pointy, Layout.HexToPixel(pointy, h))));
    }

    [TestMethod]
    public void HexRing_RadiusOne_ReturnsExpectedNeighbors()
    {
        var hex = new Hex(5, 0);
        var results = Hex.HexRing(hex, 1, 20, 20);

        var board = new GameState(new Board(GameBoard));

        Assert.Contains(x => x.Equals(new Hex(5, -1)), results);
        Assert.Contains(x => x.Equals(new Hex(6, -1)), results);
        Assert.Contains(x => x.Equals(new Hex(6, 0)), results);
        Assert.Contains(x => x.Equals(new Hex(5, 1)), results);
        Assert.Contains(x => x.Equals(new Hex(4, 1)), results);
        Assert.Contains(x => x.Equals(new Hex(4, 0)), results);

        // top-left corner
        hex = new Hex(0, 0);
        results = Hex.HexRing(hex, 1, 27, 19);
        results.ToList().ForEach(x => board[Hex.HexToIndex(x, board.Width, board.Height)].IsSelected = true);

        // top-right corner
        hex = new Hex(26, -13);
        results = Hex.HexRing(hex, 1, 27, 19);
        results.ToList().ForEach(x => board[Hex.HexToIndex(x, board.Width, board.Height)].IsSelected = true);

        // bottom-left corner
        hex = new Hex(0, 18);
        results = Hex.HexRing(hex, 1, 27, 19);
        results.ToList().ForEach(x => board[Hex.HexToIndex(x, board.Width, board.Height)].IsSelected = true);

        // bottom-right corner
        hex = new Hex(26, 5);
        results = Hex.HexRing(hex, 1, 27, 19);
        results.ToList().ForEach(x => board[Hex.HexToIndex(x, board.Width, board.Height)].IsSelected = true);

        Visualise.GameBoardRenderer.RenderAndSave("RedBlob/HexRingCorners.png", board.Width, board.Height, board.Tiles);
    }

    [TestMethod]
    public void HexesWithinArea_RadiusTwo_ReturnsExpectedTiles()
    {
        var board = new GameState(new Board(GameBoard));

        var hex = new Hex(14, -4);
        var results = Hex.HexesWithinArea(hex, 2, board.Width, board.Height);

        results.ToList().ForEach(x => board[Hex.HexToIndex(x, board.Width, board.Height)].IsSelected = true);
        Visualise.GameBoardRenderer.RenderAndSave("RedBlob/HexesInArea.png", board.Width, board.Height, board.Tiles);
        board.Tiles.ToList().ForEach(x => x.IsSelected = false);

        Assert.HasCount(19, results);

        Assert.Contains(x => x.Equals(hex), results);

        Assert.Contains(x => x.Equals(new Hex(14, -5)), results);
        Assert.Contains(x => x.Equals(new Hex(13, -4)), results);
        Assert.Contains(x => x.Equals(new Hex(13, -3)), results);
        Assert.Contains(x => x.Equals(new Hex(14, -3)), results);
        Assert.Contains(x => x.Equals(new Hex(14, -4)), results);
        Assert.Contains(x => x.Equals(new Hex(15, -5)), results);

        Assert.Contains(x => x.Equals(new Hex(15, -6)), results);
        Assert.Contains(x => x.Equals(new Hex(13, -5)), results);
        Assert.Contains(x => x.Equals(new Hex(12, -4)), results);
        Assert.Contains(x => x.Equals(new Hex(12, -3)), results);
        Assert.Contains(x => x.Equals(new Hex(13, -3)), results);
        Assert.Contains(x => x.Equals(new Hex(14, -3)), results);
        Assert.Contains(x => x.Equals(new Hex(14, -2)), results);
        Assert.Contains(x => x.Equals(new Hex(15, -3)), results);
        Assert.Contains(x => x.Equals(new Hex(16, -4)), results);
        Assert.Contains(x => x.Equals(new Hex(16, -5)), results);
        Assert.Contains(x => x.Equals(new Hex(16, -6)), results);
        Assert.Contains(x => x.Equals(new Hex(15, -6)), results);


        hex = new Hex(14, -7);
        results = Hex.HexesWithinArea(hex, 2, board.Width, board.Height);
        results.ToList().ForEach(x => board[Hex.HexToIndex(x, board.Width, board.Height)].IsSelected = true);

        hex = new Hex(14, 11);
        results = Hex.HexesWithinArea(hex, 2, board.Width, board.Height);
        results.ToList().ForEach(x => board[Hex.HexToIndex(x, board.Width, board.Height)].IsSelected = true);

        hex = new Hex(0, 10);
        results = Hex.HexesWithinArea(hex, 2, board.Width, board.Height);
        results.ToList().ForEach(x => board[Hex.HexToIndex(x, board.Width, board.Height)].IsSelected = true);

        hex = new Hex(26, -3);
        results = Hex.HexesWithinArea(hex, 2, board.Width, board.Height);
        results.ToList().ForEach(x => board[Hex.HexToIndex(x, board.Width, board.Height)].IsSelected = true);


        Visualise.GameBoardRenderer.RenderAndSave("RedBlob/HexesInAreaEdges.png", board.Width, board.Height, board.Tiles);
    }

    //[TestMethod]
    //static public void TestConversionRoundtrip()
    //{
    //    Hex a = new Hex(3, 4, -7);
    //    OffsetCoord b = new OffsetCoord(1, -3);
    //    Tests.EqualHex("conversion_roundtrip even-q", a, OffsetCoord.QoffsetToCube(OffsetCoord.EVEN, OffsetCoord.QoffsetFromCube(OffsetCoord.EVEN, a)));
    //    Tests.EqualOffsetcoord("conversion_roundtrip even-q", b, OffsetCoord.QoffsetFromCube(OffsetCoord.EVEN, OffsetCoord.QoffsetToCube(OffsetCoord.EVEN, b)));
    //    Tests.EqualHex("conversion_roundtrip odd-q", a, OffsetCoord.QoffsetToCube(OffsetCoord.ODD, OffsetCoord.QoffsetFromCube(OffsetCoord.ODD, a)));
    //    Tests.EqualOffsetcoord("conversion_roundtrip odd-q", b, OffsetCoord.QoffsetFromCube(OffsetCoord.ODD, OffsetCoord.QoffsetToCube(OffsetCoord.ODD, b)));
    //    Tests.EqualHex("conversion_roundtrip even-r", a, OffsetCoord.RoffsetToCube(OffsetCoord.EVEN, OffsetCoord.RoffsetFromCube(OffsetCoord.EVEN, a)));
    //    Tests.EqualOffsetcoord("conversion_roundtrip even-r", b, OffsetCoord.RoffsetFromCube(OffsetCoord.EVEN, OffsetCoord.RoffsetToCube(OffsetCoord.EVEN, b)));
    //    Tests.EqualHex("conversion_roundtrip odd-r", a, OffsetCoord.RoffsetToCube(OffsetCoord.ODD, OffsetCoord.RoffsetFromCube(OffsetCoord.ODD, a)));
    //    Tests.EqualOffsetcoord("conversion_roundtrip odd-r", b, OffsetCoord.RoffsetFromCube(OffsetCoord.ODD, OffsetCoord.RoffsetToCube(OffsetCoord.ODD, b)));
    //}


    //[TestMethod]
    //static public void TestOffsetFromCube()
    //{
    //    Tests.EqualOffsetcoord("offset_from_cube even-q", new OffsetCoord(1, 3), OffsetCoord.QoffsetFromCube(OffsetCoord.EVEN, new Hex(1, 2, -3)));
    //    Tests.EqualOffsetcoord("offset_from_cube odd-q", new OffsetCoord(1, 2), OffsetCoord.QoffsetFromCube(OffsetCoord.ODD, new Hex(1, 2, -3)));
    //}


    //[TestMethod]
    //static public void TestOffsetToCube()
    //{
    //    Tests.EqualHex("offset_to_cube even-", new Hex(1, 2, -3), OffsetCoord.QoffsetToCube(OffsetCoord.EVEN, new OffsetCoord(1, 3)));
    //    Tests.EqualHex("offset_to_cube odd-q", new Hex(1, 2, -3), OffsetCoord.QoffsetToCube(OffsetCoord.ODD, new OffsetCoord(1, 2)));
    //}


    static public void Complain(String name)
    {
        Console.WriteLine("FAIL " + name);
    }

}



