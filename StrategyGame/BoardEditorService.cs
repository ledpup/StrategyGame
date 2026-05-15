using ComputerOpponent;
using GameModel;
using Hexagon;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StrategyGame;

internal static class BoardEditorService
{
    public static GameState CreateDefaultBoard(int width, int height)
    {
        return MapDocument.CreateDefault(width, height).ToGameState();
    }

    public static GameState RebuildBoard(GameState gameState)
    {
        return MapDocument.FromGameState(gameState).ToGameState(gameState.Turn);
    }

    public static Tile HitTest(GameState gameState, int x, int y)
    {
        var point = new PointD(x, y);
        var hex = FractionalHex.HexRound(Layout.PixelToHex(EditorLayout.Layout, point));
        try
        {
            var index = Hex.HexToIndex(hex, gameState.Width, gameState.Height);
            return gameState[index];
        }
        catch
        {
            return null;
        }
    }

    public static bool AreAdjacent(Tile first, Tile second)
    {
        if (first == null || second == null)
            return false;

        return first.Neighbours.Any(x => x.Destination == second);
    }

    public static void SetTerrainDirect(Tile tile, TerrainType terrainType)
    {
        tile.TerrainType = terrainType;
        tile.BaseTerrainType = terrainType.HasFlag(TerrainType.Water) || terrainType.HasFlag(TerrainType.Reef)
            ? BaseTerrainType.Water
            : BaseTerrainType.Land;
    }

    public static GameState SetTerrain(GameState gameState, Tile tile, TerrainType terrainType)
    {
        if (tile == null)
            return gameState;

        var document = MapDocument.FromGameState(gameState);
        var row = document.Tiles[tile.Y].ToCharArray();
        row[tile.X] = MapDocument.TerrainToChar(terrainType);
        document.Tiles[tile.Y] = new string(row);
        return document.ToGameState(gameState.Turn);
    }

    public static GameState SetSettlement(GameState gameState, Tile tile, SettlementType settlementType, int ownerIndex = 0, int supply = 4)
    {
        if (tile == null)
            return gameState;

        var document = MapDocument.FromGameState(gameState);
        document.Settlements.RemoveAll(x => x.StartsWith(tile.Index + ",", StringComparison.Ordinal));
        if (settlementType != SettlementType.None)
        {
            document.Settlements.Add($"{tile.Index},{settlementType},{ownerIndex},{supply}");
            document.Settlements = document.Settlements.OrderBy(x => x).ToList();
        }
        return document.ToGameState(gameState.Turn);
    }

    public static GameState AddUnit(GameState gameState, Tile tile, UnitType unitType, OperationalDomain movementType, int ownerIndex = 0)
    {
        if (tile == null)
            return gameState;

        var document = MapDocument.FromGameState(gameState);
        var nextIndex = document.Units.Count;
        var isTransporter = movementType != OperationalDomain.Land;
        var transportableBy = new List<OperationalDomain>();
        if (movementType == OperationalDomain.Land)
        {
            transportableBy.Add(OperationalDomain.Airborne);
            transportableBy.Add(OperationalDomain.Waterbound);
        }

        document.Units.Add(new UnitDocument
        {
            Name = $"Unit {nextIndex} (owned by {ownerIndex})",
            UnitTemplateName = UnitTemplateName.DwarvenInfantry,
            OwnerIndex = ownerIndex,
            TileIndex = tile.Index,
            MovementType = movementType,
            BaseMovementPoints = movementType == OperationalDomain.Airborne ? 4 : movementType == OperationalDomain.Waterbound ? 5 : 2,
            RoadMovementBonus = movementType == OperationalDomain.Land ? 1 : 0,
            UnitType = unitType,
            BaseQuality = 1,
            InitialQuantity = 100,
            Size = 1,
            IsTransporter = isTransporter,
            TransportableBy = transportableBy,
            CombatInitiative = 10,
            InitialMorale = 5,
            TurnBuilt = 0,
        });
        document.Units = document.Units.ToList();
        return document.ToGameState(gameState.Turn);
    }

    public static GameState RemoveUnits(GameState gameState, Tile tile)
    {
        if (tile == null)
            return gameState;

        var document = MapDocument.FromGameState(gameState);
        document.Units.RemoveAll(x => x.TileIndex == tile.Index);
        return document.ToGameState(gameState.Turn);
    }

    public static GameState EraseTileContent(GameState gameState, Tile tile)
    {
        if (tile == null)
            return gameState;

        var document = MapDocument.FromGameState(gameState);
        document.Settlements.RemoveAll(x => x.StartsWith(tile.Index + ",", StringComparison.Ordinal));
        document.Units.RemoveAll(x => x.TileIndex == tile.Index);
        return document.ToGameState(gameState.Turn);
    }

    public static GameState SetEdge(GameState gameState, Tile first, Tile second, EdgeType edgeType, bool hasRoad)
    {
        if (!AreAdjacent(first, second))
            return gameState;

        var document = MapDocument.FromGameState(gameState);
        var low = Math.Min(first.Index, second.Index);
        var high = Math.Max(first.Index, second.Index);
        document.Edges.RemoveAll(x => x.StartsWith($"{low},{high},", StringComparison.Ordinal));
        if (edgeType != EdgeType.None || hasRoad)
        {
            document.Edges.Add($"{low},{high},{edgeType},{hasRoad.ToString().ToLowerInvariant()}");
            document.Edges = document.Edges.OrderBy(x => x).ToList();
        }
        return document.ToGameState(gameState.Turn);
    }

    public static SimulationSession StartSimulation(GameState gameState, int maxTurns = 50) =>
        new(gameState, maxTurns);
}

internal static class EditorLayout
{
    public static readonly Layout Layout = new(Layout.flat, new PointD(25, 25), new PointD(25, 21.650635094610966));
}

/// <summary>
/// Holds a lazily-computed sequence of per-turn board states so the UI can
/// step forward and backward through a simulation without re-running turns.
/// </summary>
internal class SimulationSession
{
    private readonly int maxTurns;
    private readonly int numberOfPlayers;
    private readonly ComputerPlayer computerPlayer;

    // Index 0 = initial board (turn 0, before any simulation).
    // Each subsequent entry is the board after that turn resolved.
    private readonly List<MapDocument> snapshots = [];

    public int CurrentTurn { get; private set; } = 0;
    public bool IsFinished { get; private set; }

    public bool CanStepForward =>
        !IsFinished || CurrentTurn < snapshots.Count - 1;

    public bool CanStepBack => CurrentTurn > 0;

    public GameState CurrentBoard { get; private set; }

    private readonly int initialUnitOwners;
    private readonly int initialSettlementOwners;

    internal SimulationSession(GameState gameState, int maxTurns)
    {
        this.maxTurns = maxTurns;

        // Take a clean copy as turn-0 snapshot
        CurrentBoard = MapDocument.FromGameState(gameState).ToGameState(gameState.Turn);
        snapshots.Add(MapDocument.FromGameState(CurrentBoard));

        var ownerCount = CurrentBoard.Units.Select(x => x.Owner.Id).Distinct().Count();
        numberOfPlayers = Math.Max(2, ownerCount);
        computerPlayer = new ComputerPlayer(CurrentBoard.Units);

        initialUnitOwners = CurrentBoard.Units.Where(x => x.IsAlive).Select(x => x.Owner.Id).Distinct().Count();
        initialSettlementOwners = CurrentBoard.Settlements.Select(x => x.Owner.Id).Distinct().Count();
    }

    public string StatusLine()
    {
        var alive = CurrentBoard.Units.Count(u => u.IsAlive);
        var owners = string.Join(", ",
            CurrentBoard.Settlements
                .GroupBy(s => s.Owner.Id)
                .OrderBy(g => g.Key)
                .Select(g => $"P{g.Key}:{g.Count()}"));
        return $"Turn {CurrentTurn}  |  Units alive: {alive}  |  Settlements: {owners}";
    }

    /// <summary>Advances one turn. Returns false if already at the end.</summary>
    public bool StepForward()
    {
        // If we already have the next snapshot cached, just move the pointer.
        if (CurrentTurn < snapshots.Count - 1)
        {
            CurrentTurn++;
            CurrentBoard = snapshots[CurrentTurn].ToGameState();
            return true;
        }

        if (IsFinished) return false;

        // Compute and cache the next turn.
        var sim = snapshots[CurrentTurn].ToGameState(CurrentTurn);
        computerPlayer.GenerateInfluenceMaps(sim, numberOfPlayers);
        computerPlayer.SetStrategicAction(sim);
        var orders = computerPlayer.CreateOrders(sim, sim.Units.Where(x => x.IsAlive).ToList());
        sim.ResolveOrders(orders);
        foreach (var player in CurrentBoard.Players)
            sim.ResolveStackLimits(player.Id);
        sim.ConductBattles();
        sim.ChangeSettlementOwners();
        sim.Turn++;

        snapshots.Add(MapDocument.FromGameState(sim));
        CurrentTurn++;
        CurrentBoard = sim;

        // Check end conditions — only stop early if sides have actually been eliminated
        var aliveOwners = sim.Units.Where(x => x.IsAlive).Select(x => x.Owner.Id).Distinct().Count();
        var settlementOwners = sim.Settlements.Select(x => x.Owner.Id).Distinct().Count();
        if (sim.Turn >= maxTurns
            || (initialUnitOwners > 1 && aliveOwners <= 1)
            || (initialSettlementOwners > 1 && settlementOwners <= 1))
        {
            IsFinished = true;
        }

        return true;
    }

    /// <summary>Steps back to the previous turn. Returns false if already at turn 0.</summary>
    public bool StepBack()
    {
        if (!CanStepBack) return false;
        CurrentTurn--;
        CurrentBoard = snapshots[CurrentTurn].ToGameState();
        return true;
    }

    /// <summary>Jumps back to turn 0.</summary>
    public void Restart()
    {
        CurrentTurn = 0;
        IsFinished = false;
        CurrentBoard = snapshots[0].ToGameState();
    }
}

internal class SimulationResult
{
    public Board Board { get; set; }
    public int TurnsCompleted { get; set; }
    public int RemainingUnits { get; set; }
    public Dictionary<int, int> SettlementsByOwner { get; set; }
}
