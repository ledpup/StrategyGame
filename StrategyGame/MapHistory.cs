using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace StrategyGame;

// ─────────────────────────────────────────────────────────────────────────
// A single field-level change within one map document.
// ─────────────────────────────────────────────────────────────────────────

internal enum DeltaKind { TileRow, EdgeAdd, EdgeRemove, SettlementAdd, SettlementRemove, UnitAdd, UnitRemove }

/// <summary>
/// One atomic change: the kind of change, an optional row/key, the old value, and the new value.
/// For TileRow: Key = row index (string), OldValue/NewValue = full row strings.
/// For Edge/Settlement/Unit Add/Remove: OldValue (for Remove) or NewValue (for Add) is the line;
/// for a replacement both OldValue and NewValue are set and Kind is the Remove variant.
/// </summary>
internal sealed class MapDelta
{
    public DeltaKind Kind { get; init; }
    public string Key { get; init; }   // row index for TileRow; unused otherwise
    public string OldValue { get; init; }
    public string NewValue { get; init; }

    // ── serialisation ─────────────────────────────────────────────────

    // Format:  KIND [TAB] KEY [TAB] OLD [TAB] NEW
    // Missing fields are empty strings.
    public override string ToString()
    {
        var key = Key ?? "";
        var old = OldValue ?? "";
        var nw = NewValue ?? "";
        return $"{(int)Kind}\t{key}\t{old}\t{nw}";
    }

    public static MapDelta Parse(string line)
    {
        var parts = line.Split('\t', 4);
        return new MapDelta
        {
            Kind = (DeltaKind)int.Parse(parts[0]),
            Key = parts.Length > 1 ? parts[1] : "",
            OldValue = parts.Length > 2 ? parts[2] : "",
            NewValue = parts.Length > 3 ? parts[3] : "",
        };
    }

    // Returns the inverse delta (for undo).
    public MapDelta Inverse() => Kind switch
    {
        DeltaKind.TileRow => new MapDelta { Kind = DeltaKind.TileRow, Key = Key, OldValue = NewValue, NewValue = OldValue },
        DeltaKind.EdgeAdd => new MapDelta { Kind = DeltaKind.EdgeRemove, OldValue = NewValue },
        DeltaKind.EdgeRemove => new MapDelta { Kind = DeltaKind.EdgeAdd, NewValue = OldValue },
        DeltaKind.SettlementAdd => new MapDelta { Kind = DeltaKind.SettlementRemove, OldValue = NewValue },
        DeltaKind.SettlementRemove => new MapDelta { Kind = DeltaKind.SettlementAdd, NewValue = OldValue },
        DeltaKind.UnitAdd => new MapDelta { Kind = DeltaKind.UnitRemove, OldValue = NewValue },
        DeltaKind.UnitRemove => new MapDelta { Kind = DeltaKind.UnitAdd, NewValue = OldValue },
        _ => throw new InvalidOperationException($"Unknown delta kind {Kind}")
    };
}

// ─────────────────────────────────────────────────────────────────────────
// A named group of deltas representing one user action (one undo step).
// ─────────────────────────────────────────────────────────────────────────

internal sealed class MapEditGroup
{
    public string Description { get; init; }
    public List<MapDelta> Deltas { get; init; } = [];

    public bool IsEmpty => Deltas.Count == 0;

    // ── apply ──────────────────────────────────────────────────────────

    /// <summary>Applies all forward deltas to <paramref name="doc"/> in order.</summary>
    public void Apply(MapDocument doc) => ApplyDeltas(doc, Deltas);

    /// <summary>Applies all inverse deltas to <paramref name="doc"/> in reverse order (undo).</summary>
    public void Invert(MapDocument doc) => ApplyDeltas(doc, Deltas.AsEnumerable().Reverse().Select(d => d.Inverse()));

    private static void ApplyDeltas(MapDocument doc, IEnumerable<MapDelta> deltas)
    {
        foreach (var d in deltas)
        {
            switch (d.Kind)
            {
                case DeltaKind.TileRow:
                    int row = int.Parse(d.Key);
                    doc.Tiles[row] = d.NewValue;
                    break;

                case DeltaKind.EdgeAdd:
                    if (!doc.Edges.Contains(d.NewValue))
                        doc.Edges.Add(d.NewValue);
                    break;

                case DeltaKind.EdgeRemove:
                    doc.Edges.Remove(d.OldValue);
                    break;

                case DeltaKind.SettlementAdd:
                    if (!doc.Settlements.Contains(d.NewValue))
                        doc.Settlements.Add(d.NewValue);
                    break;

                case DeltaKind.SettlementRemove:
                    doc.Settlements.Remove(d.OldValue);
                    break;

                case DeltaKind.UnitAdd:
                    if (!doc.Units.Any(u => u.ToLine() == d.NewValue))
                        doc.Units.Add(UnitDocument.Parse(d.NewValue));
                    break;

                case DeltaKind.UnitRemove:
                    var toRemove = doc.Units.FirstOrDefault(u => u.ToLine() == d.OldValue);
                    if (toRemove != null) doc.Units.Remove(toRemove);
                    break;
            }
        }
    }

    // ── serialisation ─────────────────────────────────────────────────

    public void WriteTo(TextWriter w)
    {
        w.WriteLine($"[Edit] {Description}");
        foreach (var d in Deltas)
            w.WriteLine(d.ToString());
    }

    public static MapEditGroup ParseFrom(string description, List<string> lines)
    {
        var g = new MapEditGroup { Description = description };
        foreach (var line in lines)
            if (!string.IsNullOrWhiteSpace(line))
                g.Deltas.Add(MapDelta.Parse(line));
        return g;
    }
}

// ─────────────────────────────────────────────────────────────────────────
// Undo/redo stacks of edit groups.
// ─────────────────────────────────────────────────────────────────────────

internal sealed class MapHistory
{
    private readonly Stack<MapEditGroup> undoStack = new();
    private readonly Stack<MapEditGroup> redoStack = new();

    public bool CanUndo => undoStack.Count > 0;
    public bool CanRedo => redoStack.Count > 0;

    // ── diff & commit ─────────────────────────────────────────────────

    /// <summary>
    /// Computes the delta between <paramref name="before"/> and <paramref name="after"/>,
    /// pushes it onto the undo stack, and clears the redo stack.
    /// Returns false (and pushes nothing) if nothing changed.
    /// </summary>
    public bool Commit(MapDocument before, MapDocument after, string description)
    {
        var group = Diff(before, after, description);
        if (group.IsEmpty) return false;
        undoStack.Push(group);
        redoStack.Clear();
        return true;
    }

    // ── undo / redo ───────────────────────────────────────────────────

    // Both stacks store the same *forward* group (A→B).
    // Undo applies the inverse; Redo applies it forward.
    // The group simply moves between stacks unchanged.

    /// <summary>
    /// Applies the inverse of the top undo group to <paramref name="current"/>,
    /// moves the group to the redo stack, and returns the resulting document.
    /// </summary>
    public MapDocument Undo(MapDocument current)
    {
        if (!CanUndo) throw new InvalidOperationException("Nothing to undo.");
        var group = undoStack.Pop();
        var doc = Clone(current);
        group.Invert(doc);
        redoStack.Push(group);   // same forward group; Redo will re-apply it
        return doc;
    }

    /// <summary>
    /// Re-applies the top redo group to <paramref name="current"/>,
    /// moves the group back to the undo stack, and returns the resulting document.
    /// </summary>
    public MapDocument Redo(MapDocument current)
    {
        if (!CanRedo) throw new InvalidOperationException("Nothing to redo.");
        var group = redoStack.Pop();
        var doc = Clone(current);
        group.Apply(doc);
        undoStack.Push(group);   // same forward group; Undo will invert it again
        return doc;
    }

    public void Clear() { undoStack.Clear(); redoStack.Clear(); }

    // ── serialisation ─────────────────────────────────────────────────

    private const string HistoryExt = ".history";
    public static string HistoryPathFor(string mapPath) => Path.ChangeExtension(mapPath, HistoryExt);

    public void Save(string mapPath)
    {
        using var w = new StreamWriter(HistoryPathFor(mapPath), append: false, Encoding.UTF8);
        w.WriteLine("[UndoStack]");
        foreach (var g in ((IEnumerable<MapEditGroup>)undoStack).Reverse())
            g.WriteTo(w);
        w.WriteLine("[RedoStack]");
        foreach (var g in redoStack)   // top-first
            g.WriteTo(w);
    }

    public bool TryLoad(string mapPath)
    {
        var path = HistoryPathFor(mapPath);
        if (!File.Exists(path)) return false;

        undoStack.Clear();
        redoStack.Clear();

        string topSection = null;
        string editDescription = null;
        var editLines = new List<string>();

        void Flush()
        {
            if (editDescription == null || editLines.Count == 0) return;
            var g = MapEditGroup.ParseFrom(editDescription, editLines);
            if (topSection == "[UndoStack]") undoStack.Push(g);
            else if (topSection == "[RedoStack]") redoStack.Push(g);
            editLines = [];
            editDescription = null;
        }

        foreach (var raw in File.ReadAllLines(path))
        {
            var line = raw.TrimEnd();
            if (line == "[UndoStack]" || line == "[RedoStack]")
            {
                Flush();
                topSection = line;
            }
            else if (line.StartsWith("[Edit] "))
            {
                Flush();
                editDescription = line.Substring(7);
            }
            else
            {
                editLines.Add(line);
            }
        }
        Flush();

        return true;
    }

    // ── diff ──────────────────────────────────────────────────────────

    public static MapEditGroup Diff(MapDocument before, MapDocument after, string description)
    {
        var g = new MapEditGroup { Description = description };

        // Tile rows
        for (var row = 0; row < before.Tiles.Length && row < after.Tiles.Length; row++)
        {
            if (before.Tiles[row] != after.Tiles[row])
                g.Deltas.Add(new MapDelta { Kind = DeltaKind.TileRow, Key = row.ToString(), OldValue = before.Tiles[row], NewValue = after.Tiles[row] });
        }
        // Extra rows if height changed
        for (var row = before.Tiles.Length; row < after.Tiles.Length; row++)
            g.Deltas.Add(new MapDelta { Kind = DeltaKind.TileRow, Key = row.ToString(), OldValue = "", NewValue = after.Tiles[row] });

        // Edges – sets of lines
        DiffLines(before.Edges, after.Edges, DeltaKind.EdgeAdd, DeltaKind.EdgeRemove, g.Deltas);
        DiffLines(before.Settlements, after.Settlements, DeltaKind.SettlementAdd, DeltaKind.SettlementRemove, g.Deltas);

        var beforeUnitLines = before.Units.Select(u => u.ToLine()).ToList();
        var afterUnitLines = after.Units.Select(u => u.ToLine()).ToList();
        DiffLines(beforeUnitLines, afterUnitLines, DeltaKind.UnitAdd, DeltaKind.UnitRemove, g.Deltas);

        return g;
    }

    private static void DiffLines(
        List<string> before, List<string> after,
        DeltaKind addKind, DeltaKind removeKind,
        List<MapDelta> deltas)
    {
        var removed = before.Except(after);
        var added = after.Except(before);
        foreach (var v in removed)
            deltas.Add(new MapDelta { Kind = removeKind, OldValue = v });
        foreach (var v in added)
            deltas.Add(new MapDelta { Kind = addKind, NewValue = v });
    }

    // ── helpers ───────────────────────────────────────────────────────

    private static MapDocument Clone(MapDocument doc)
    {
        var sb = new StringBuilder();
        using (var w = new StringWriter(sb))
            doc.WriteTo(w);
        return MapDocument.ParseFromLines(sb.ToString().Split('\n'));
    }
}
