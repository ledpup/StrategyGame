using ComputerOpponent;
using Hexagon;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Tests;

[TestClass]
public class BoardInfluenceMapTests
{
    [TestMethod]
    public void AddRadialInfluence_RadiusProvided_AddsExpectedFalloff()
    {
        var map = new BoardInfluenceMap(5, 5);
        var origin = new Hex(2, 1);

        map.AddRadialInfluence(origin, 12f, 2);

        var centerValue = map.GetValue(origin);
        var firstRingHex = Hex.Neighbor(origin, 0);
        var secondRingHex = Hex.Add(origin, Hex.Scale(Hex.Direction(0), 2));

        Assert.AreEqual(12f, centerValue, 0.0001f);
        Assert.AreEqual(6f, map.GetValue(firstRingHex), 0.0001f);
        Assert.AreEqual(4f, map.GetValue(secondRingHex), 0.0001f);
    }

    [TestMethod]
    public void Propagate_WithDecay_SpreadsInfluenceToNeighbors()
    {
        var map = new BoardInfluenceMap(5, 5);
        var origin = new Hex(2, 1);
        var originIndex = Hex.HexToIndex(origin, 5, 5);

        map.SetValue(originIndex, 6f);
        map.Propagate(0.6f, 1);

        var firstNeighbor = Hex.Neighbor(origin, 0);

        Assert.AreEqual(6f, map.GetValue(origin), 0.0001f);
        Assert.AreEqual(0.6f, map.GetValue(firstNeighbor), 0.0001f);
    }

    [TestMethod]
    public void Combine_WeightedMaps_MergesIntoDecisionField()
    {
        var mapA = new BoardInfluenceMap(5, 5);
        var mapB = new BoardInfluenceMap(5, 5);

        mapA.SetValue(0, 2f);
        mapB.SetValue(0, 4f);

        var combined = BoardInfluenceMap.Combine(5, 5,
        [
            (mapA, 1f),
            (mapB, 0.5f)
        ]);

        Assert.AreEqual(4f, combined.GetValue(0), 0.0001f);
    }

    [TestMethod]
    public void CopyValues_AfterCopy_ReturnsIndependentArray()
    {
        var map = new BoardInfluenceMap(4, 4);
        map.SetValue(3, 5f);

        var copy = map.CopyValues();
        copy[3] = 0f;

        Assert.AreEqual(5f, map.GetValue(3), 0.0001f);
    }
}


