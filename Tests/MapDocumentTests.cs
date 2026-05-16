using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StrategyGame;

namespace Tests;

[TestClass]
public class MapDocumentTests
{
    // ── helpers ───────────────────────────────────────────────────────

    static MapDocument Grassland(int width = 4, int height = 3) =>
        MapDocument.CreateDefault(width, height);

    static string Serialise(MapDocument doc)
    {
        var sw = new StringWriter();
        doc.WriteTo(sw);
        return sw.ToString();
    }

    static MapDocument Roundtrip(MapDocument doc) =>
        MapDocument.ParseFromLines(Serialise(doc).Split('\n'));

    // ── CreateDefault ─────────────────────────────────────────────────

    [TestMethod]
    public void CreateDefault_NewDocument_HasExpectedDimensions()
    {
        var doc = Grassland(5, 4);
        Assert.HasCount(4, doc.Tiles);
        Assert.IsTrue(doc.Tiles.All(r => r == "GGGGG"));
        Assert.IsEmpty(doc.Edges);
        Assert.IsEmpty(doc.Settlements);
        Assert.IsEmpty(doc.Units);
    }

    // ── WriteTo / ParseFromLines roundtrip ────────────────────────────

    [TestMethod]
    public void Roundtrip_WithTiles_PreservesTiles()
    {
        var doc = Grassland(3, 2);
        doc.Tiles[0] = "GMH";
        var rt = Roundtrip(doc);
        Assert.AreEqual("GMH", rt.Tiles[0]);
        Assert.AreEqual("GGG", rt.Tiles[1]);
    }

    [TestMethod]
    public void Roundtrip_WithEdges_PreservesEdges()
    {
        var doc = Grassland();
        doc.Edges.Add("0,1,River,false");
        doc.Edges.Add("1,2,None,true");
        var rt = Roundtrip(doc);
        CollectionAssert.AreEquivalent(doc.Edges, rt.Edges);
    }

    [TestMethod]
    public void Roundtrip_WithSettlements_PreservesSettlements()
    {
        var doc = Grassland();
        doc.Settlements.Add("5,City,1,100");
        var rt = Roundtrip(doc);
        CollectionAssert.AreEquivalent(doc.Settlements, rt.Settlements);
    }

    [TestMethod]
    public void Roundtrip_EmptyDocument_ProducesEmptyDocument()
    {
        var doc = Grassland(2, 2);
        var rt = Roundtrip(doc);
        Assert.HasCount(2, rt.Tiles);
        Assert.IsEmpty(rt.Edges);
    }

    // ── TerrainToChar ─────────────────────────────────────────────────

    [TestMethod]
    public void TerrainToChar_AllTerrainTypes_ReturnsCharacter()
    {
        var types = new[]
        {
            (TerrainType.Grassland, 'G'), (TerrainType.Desert, 'D'),
            (TerrainType.Forest, 'F'),    (TerrainType.Hill, 'H'),
            (TerrainType.Mountain, 'M'),  (TerrainType.Water, 'L'),
            (TerrainType.Swamp, 'W'),   (TerrainType.Reef, 'R'),
        };
        foreach (var (terrain, expected) in types)
            Assert.AreEqual(expected, MapDocument.TerrainToChar(terrain), terrain.ToString());
    }

    // ── FromBoard / ToBoard ───────────────────────────────────────────

    [TestMethod]
    public void FromBoard_BoardWithTiles_TileCountMatchesDimensions()
    {
        var board = new Board(
            File.ReadAllLines("BasicBoard.txt"),
            File.ReadAllLines("BasicBoardEdges.txt"),
            File.ReadAllLines("BasicBoardSettlements.txt"));
        var doc = MapDocument.FromBoard(board);
        Assert.HasCount(board.Height, doc.Tiles);
        Assert.IsTrue(doc.Tiles.All(r => r.Length == board.Width));
    }

    [TestMethod]
    public void ToBoardThenFromBoard_DocumentWithTiles_PreservesTileRows()
    {
        var doc = Grassland(4, 3);
        doc.Tiles[1] = "GHMG";
        var board = doc.ToBoard();
        var doc2 = MapDocument.FromBoard(board);
        CollectionAssert.AreEqual(doc.Tiles, doc2.Tiles);
    }

    [TestMethod]
    public void ToBoard_DocumentWithEdges_PreservesEdges()
    {
        var doc = Grassland(4, 3);
        // Add an edge between tile 0 and tile 1 (adjacent in a 4-wide map)
        doc.Edges.Add("0,1,River,false");
        var board = doc.ToBoard();
        var doc2 = MapDocument.FromBoard(board);
        Assert.HasCount(1, doc2.Edges);
        Assert.Contains("River", doc2.Edges[0]);
    }

    // ── ParseFromLines ignores blank lines ────────────────────────────

    [TestMethod]
    public void ParseFromLines_InputWithBlankLines_IgnoresBlankLines()
    {
        var lines = new[]
        {
            "", "[Tiles]", "", "GGG", "", "GGG", "",
            "[Edges]", "", "[Settlements]", "", "[Units]", ""
        };
        var doc = MapDocument.ParseFromLines(lines);
        Assert.HasCount(2, doc.Tiles);
    }
}

// ─────────────────────────────────────────────────────────────────────────

[TestClass]
public class MapDeltaTests
{
    [TestMethod]
    public void TileRow_Serialise_RoundTrips()
    {
        var d = new MapDelta { Kind = DeltaKind.TileRow, Key = "2", OldValue = "GGG", NewValue = "GMG" };
        var d2 = MapDelta.Parse(d.ToString());
        Assert.AreEqual(DeltaKind.TileRow, d2.Kind);
        Assert.AreEqual("2", d2.Key);
        Assert.AreEqual("GGG", d2.OldValue);
        Assert.AreEqual("GMG", d2.NewValue);
    }

    [TestMethod]
    public void EdgeAdd_Serialise_RoundTrips()
    {
        var d = new MapDelta { Kind = DeltaKind.EdgeAdd, NewValue = "0,1,River,false" };
        var d2 = MapDelta.Parse(d.ToString());
        Assert.AreEqual(DeltaKind.EdgeAdd, d2.Kind);
        Assert.AreEqual("0,1,River,false", d2.NewValue);
    }

    [TestMethod]
    public void TileRow_Inverse_SwapsOldAndNew()
    {
        var d = new MapDelta { Kind = DeltaKind.TileRow, Key = "1", OldValue = "GGG", NewValue = "GMG" };
        var inv = d.Inverse();
        Assert.AreEqual(DeltaKind.TileRow, inv.Kind);
        Assert.AreEqual("GMG", inv.OldValue);
        Assert.AreEqual("GGG", inv.NewValue);
    }

    [TestMethod]
    public void EdgeAdd_Inverse_ReturnsEdgeRemove()
    {
        var d = new MapDelta { Kind = DeltaKind.EdgeAdd, NewValue = "0,1,River,false" };
        var inv = d.Inverse();
        Assert.AreEqual(DeltaKind.EdgeRemove, inv.Kind);
        Assert.AreEqual("0,1,River,false", inv.OldValue);
    }

    [TestMethod]
    public void EdgeRemove_Inverse_ReturnsEdgeAdd()
    {
        var d = new MapDelta { Kind = DeltaKind.EdgeRemove, OldValue = "0,1,River,false" };
        var inv = d.Inverse();
        Assert.AreEqual(DeltaKind.EdgeAdd, inv.Kind);
        Assert.AreEqual("0,1,River,false", inv.NewValue);
    }

    [TestMethod]
    public void SettlementAdd_Inverse_ReturnsSettlementRemove()
    {
        var d = new MapDelta { Kind = DeltaKind.SettlementAdd, NewValue = "5,City,1,100" };
        var inv = d.Inverse();
        Assert.AreEqual(DeltaKind.SettlementRemove, inv.Kind);
        Assert.AreEqual("5,City,1,100", inv.OldValue);
    }

    [TestMethod]
    public void UnitAdd_Inverse_ReturnsUnitRemove()
    {
        var d = new MapDelta { Kind = DeltaKind.UnitAdd, NewValue = "some,unit,line" };
        var inv = d.Inverse();
        Assert.AreEqual(DeltaKind.UnitRemove, inv.Kind);
        Assert.AreEqual("some,unit,line", inv.OldValue);
    }
}

// ─────────────────────────────────────────────────────────────────────────

[TestClass]
public class MapHistoryTests
{
    static MapDocument Grassland(int w = 4, int h = 3) => MapDocument.CreateDefault(w, h);

    static MapDocument WithTerrain(MapDocument doc, int row, string value)
    {
        var clone = Clone(doc);
        clone.Tiles[row] = value;
        return clone;
    }

    static MapDocument Clone(MapDocument doc)
    {
        var sw = new StringWriter();
        doc.WriteTo(sw);
        return MapDocument.ParseFromLines(sw.ToString().Split('\n'));
    }

    // ── Commit ────────────────────────────────────────────────────────

    [TestMethod]
    public void Commit_NoChange_ReturnsFalse()
    {
        var h = new MapHistory();
        var doc = Grassland();
        Assert.IsFalse(h.Commit(doc, Clone(doc), "no-op"));
        Assert.IsFalse(h.CanUndo);
    }

    [TestMethod]
    public void Commit_WithChange_CanUndo()
    {
        var h = new MapHistory();
        var before = Grassland();
        var after = WithTerrain(before, 0, "GMMG");
        h.Commit(before, after, "paint");
        Assert.IsTrue(h.CanUndo);
        Assert.IsFalse(h.CanRedo);
    }

    [TestMethod]
    public void Commit_WithNewChange_ClearsRedoStack()
    {
        var h = new MapHistory();
        var a = Grassland();
        var b = WithTerrain(a, 0, "GMMG");
        var c = WithTerrain(b, 1, "GHGG");
        h.Commit(a, b, "first");
        // Manually set up a redo entry by doing undo
        h.Undo(b);
        Assert.IsTrue(h.CanRedo);
        // Now commit a new edit — redo must be cleared
        h.Commit(a, c, "second");
        Assert.IsFalse(h.CanRedo);
    }

    // ── Undo ──────────────────────────────────────────────────────────

    [TestMethod]
    public void Undo_AfterTileRowChange_RestoresPreviousTileRow()
    {
        var h = new MapHistory();
        var before = Grassland();
        var after = WithTerrain(before, 0, "GMMG");
        h.Commit(before, after, "paint");

        var restored = h.Undo(after);
        Assert.AreEqual("GGGG", restored.Tiles[0]);
    }

    [TestMethod]
    public void Undo_AfterCommit_MovesGroupToRedoStack()
    {
        var h = new MapHistory();
        var a = Grassland();
        var b = WithTerrain(a, 0, "GMMG");
        h.Commit(a, b, "paint");
        h.Undo(b);
        Assert.IsFalse(h.CanUndo);
        Assert.IsTrue(h.CanRedo);
    }

    [TestMethod]
    public void Undo_MultipleEdits_RestoresInReverseOrder()
    {
        var h = new MapHistory();
        var a = Grassland();
        var b = WithTerrain(a, 0, "GMMG");
        var c = WithTerrain(b, 1, "HHHH");
        h.Commit(a, b, "first");
        h.Commit(b, c, "second");

        var afterUndo1 = h.Undo(c);
        Assert.AreEqual("GGGG", afterUndo1.Tiles[1], "row 1 should be back to grassland");

        var afterUndo2 = h.Undo(afterUndo1);
        Assert.AreEqual("GGGG", afterUndo2.Tiles[0], "row 0 should be back to grassland");
    }

    // ── Redo ──────────────────────────────────────────────────────────

    [TestMethod]
    public void Redo_AfterUndo_ReappliesForwardChange()
    {
        var h = new MapHistory();
        var before = Grassland();
        var after = WithTerrain(before, 0, "GMMG");
        h.Commit(before, after, "paint");

        var undone = h.Undo(after);
        Assert.AreEqual("GGGG", undone.Tiles[0]);

        var redone = h.Redo(undone);
        Assert.AreEqual("GMMG", redone.Tiles[0]);
    }

    [TestMethod]
    public void Redo_AfterUndo_MovesGroupBackToUndoStack()
    {
        var h = new MapHistory();
        var a = Grassland();
        var b = WithTerrain(a, 0, "GMMG");
        h.Commit(a, b, "paint");
        h.Undo(b);
        h.Redo(a);
        Assert.IsTrue(h.CanUndo);
        Assert.IsFalse(h.CanRedo);
    }

    [TestMethod]
    public void UndoRedo_MultipleTimes_RemainsStable()
    {
        var h = new MapHistory();
        var a = Grassland();
        var b = WithTerrain(a, 0, "GMMG");
        h.Commit(a, b, "paint");

        var state = b;
        for (var i = 0; i < 3; i++)
        {
            state = h.Undo(state);
            Assert.AreEqual("GGGG", state.Tiles[0]);
            state = h.Redo(state);
            Assert.AreEqual("GMMG", state.Tiles[0]);
        }
    }

    // ── Edge deltas ───────────────────────────────────────────────────

    [TestMethod]
    public void Undo_EdgeAdd_RemovesEdge()
    {
        var h = new MapHistory();
        var before = Grassland();
        var after = Clone(before);
        after.Edges.Add("0,1,River,false");
        h.Commit(before, after, "edge");

        var restored = h.Undo(after);
        Assert.IsEmpty(restored.Edges);
    }

    [TestMethod]
    public void Redo_EdgeAdd_ReAddsEdge()
    {
        var h = new MapHistory();
        var before = Grassland();
        var after = Clone(before);
        after.Edges.Add("0,1,River,false");
        h.Commit(before, after, "edge");

        var undone = h.Undo(after);
        var redone = h.Redo(undone);
        Assert.HasCount(1, redone.Edges);
        Assert.AreEqual("0,1,River,false", redone.Edges[0]);
    }

    // ── Settlement deltas ──────────────────────────────────────────────

    [TestMethod]
    public void Undo_SettlementAdd_RemovesSettlement()
    {
        var h = new MapHistory();
        var before = Grassland();
        var after = Clone(before);
        after.Settlements.Add("5,City,1,100");
        h.Commit(before, after, "settlement");

        var restored = h.Undo(after);
        Assert.IsEmpty(restored.Settlements);
    }

    // ── Diff ──────────────────────────────────────────────────────────

    [TestMethod]
    public void Diff_NoChange_ReturnsEmptyGroup()
    {
        var doc = Grassland();
        var g = MapHistory.Diff(doc, Clone(doc), "test");
        Assert.IsTrue(g.IsEmpty);
    }

    [TestMethod]
    public void Diff_SingleTileChange_ReturnsOneDelta()
    {
        var before = Grassland(3, 2);
        var after = Clone(before);
        after.Tiles[1] = "GMG";
        var g = MapHistory.Diff(before, after, "test");
        Assert.HasCount(1, g.Deltas);
        Assert.AreEqual(DeltaKind.TileRow, g.Deltas[0].Kind);
        Assert.AreEqual("1", g.Deltas[0].Key);
        Assert.AreEqual("GMG", g.Deltas[0].NewValue);
    }

    [TestMethod]
    public void Diff_EdgeAdded_ReturnsOneAddDelta()
    {
        var before = Grassland();
        var after = Clone(before);
        after.Edges.Add("0,1,River,false");
        var g = MapHistory.Diff(before, after, "test");
        Assert.HasCount(1, g.Deltas);
        Assert.AreEqual(DeltaKind.EdgeAdd, g.Deltas[0].Kind);
    }

    [TestMethod]
    public void Diff_EdgeRemoved_ReturnsOneRemoveDelta()
    {
        var before = Grassland();
        before.Edges.Add("0,1,River,false");
        var after = Clone(before);
        after.Edges.Clear();
        var g = MapHistory.Diff(before, after, "test");
        Assert.HasCount(1, g.Deltas);
        Assert.AreEqual(DeltaKind.EdgeRemove, g.Deltas[0].Kind);
    }

    // ── Clear ─────────────────────────────────────────────────────────

    [TestMethod]
    public void Clear_WithUndoAndRedo_EmptiesBothStacks()
    {
        var h = new MapHistory();
        var a = Grassland();
        var b = WithTerrain(a, 0, "GMMG");
        h.Commit(a, b, "paint");
        h.Undo(b);
        h.Clear();
        Assert.IsFalse(h.CanUndo);
        Assert.IsFalse(h.CanRedo);
    }

    // ── Serialise / TryLoad ───────────────────────────────────────────

    [TestMethod]
    public void SaveLoad_UndoStack_RoundTrips()
    {
        var h = new MapHistory();
        var a = Grassland();
        var b = WithTerrain(a, 0, "GMMG");
        var c = WithTerrain(b, 1, "HHHH");
        h.Commit(a, b, "first");
        h.Commit(b, c, "second");

        var mapPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".sgmap");
        try
        {
            File.WriteAllText(mapPath, "");   // create stub map file
            h.Save(mapPath);

            var h2 = new MapHistory();
            Assert.IsTrue(h2.TryLoad(mapPath));
            Assert.IsTrue(h2.CanUndo);
            Assert.IsFalse(h2.CanRedo);

            // Undo twice on the reloaded history should work
            var afterUndo1 = h2.Undo(c);
            Assert.AreEqual("GGGG", afterUndo1.Tiles[1]);
            var afterUndo2 = h2.Undo(afterUndo1);
            Assert.AreEqual("GGGG", afterUndo2.Tiles[0]);
        }
        finally
        {
            File.Delete(mapPath);
            var histPath = MapHistory.HistoryPathFor(mapPath);
            if (File.Exists(histPath)) File.Delete(histPath);
        }
    }

    [TestMethod]
    public void SaveLoad_RedoStack_RoundTrips()
    {
        var h = new MapHistory();
        var a = Grassland();
        var b = WithTerrain(a, 0, "GMMG");
        h.Commit(a, b, "paint");
        h.Undo(b);   // moves group to redo stack

        var mapPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".sgmap");
        try
        {
            File.WriteAllText(mapPath, "");
            h.Save(mapPath);

            var h2 = new MapHistory();
            h2.TryLoad(mapPath);
            Assert.IsFalse(h2.CanUndo);
            Assert.IsTrue(h2.CanRedo);

            var redone = h2.Redo(a);
            Assert.AreEqual("GMMG", redone.Tiles[0]);
        }
        finally
        {
            File.Delete(mapPath);
            var histPath = MapHistory.HistoryPathFor(mapPath);
            if (File.Exists(histPath)) File.Delete(histPath);
        }
    }

    [TestMethod]
    public void TryLoad_NoFile_ReturnsFalse()
    {
        var h = new MapHistory();
        Assert.IsFalse(h.TryLoad(Path.Combine(Path.GetTempPath(), "nonexistent_abc123.sgmap")));
    }
}


