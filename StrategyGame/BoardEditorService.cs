using ComputerOpponent;
using GameModel;
using Hexagon;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StrategyGame
{
    internal static class BoardEditorService
    {
        public static Board CreateDefaultBoard(int width, int height)
        {
            return MapDocument.CreateDefault(width, height).ToBoard();
        }

        public static Board RebuildBoard(Board board)
        {
            return MapDocument.FromBoard(board).ToBoard();
        }

        public static Tile HitTest(Board board, int x, int y)
        {
            var point = new PointD(x, y);
            var hex = FractionalHex.HexRound(Layout.PixelToHex(EditorLayout.Layout, point));
            try
            {
                var index = Hex.HexToIndex(hex, board.Width, board.Height);
                return board[index];
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

        public static Board SetTerrain(Board board, Tile tile, TerrainType terrainType)
        {
            if (tile == null)
                return board;

            var document = MapDocument.FromBoard(board);
            var row = document.Tiles[tile.Y].ToCharArray();
            row[tile.X] = MapDocument.TerrainToChar(terrainType);
            document.Tiles[tile.Y] = new string(row);
            return document.ToBoard();
        }

        public static Board SetStructure(Board board, Tile tile, StructureType structureType, int ownerIndex = 0, int supply = 4)
        {
            if (tile == null)
                return board;

            var document = MapDocument.FromBoard(board);
            document.Structures.RemoveAll(x => x.StartsWith(tile.Index + ",", StringComparison.Ordinal));
            if (structureType != StructureType.None)
            {
                document.Structures.Add($"{tile.Index},{structureType},{ownerIndex},{supply}");
                document.Structures = document.Structures.OrderBy(x => x).ToList();
            }
            return document.ToBoard();
        }

        public static Board AddUnit(Board board, Tile tile, UnitType unitType, MovementType movementType, int ownerIndex = 0)
        {
            if (tile == null)
                return board;

            var document = MapDocument.FromBoard(board);
            var nextIndex = document.Units.Any() ? document.Units.Max(x => x.Index) + 1 : 0;
            var isTransporter = movementType != MovementType.Land;
            var transportableBy = new List<MovementType>();
            if (movementType == MovementType.Land)
            {
                transportableBy.Add(MovementType.Airborne);
                transportableBy.Add(MovementType.Water);
            }

            document.Units.Add(new UnitDocument
            {
                Index = nextIndex,
                Name = $"Unit {nextIndex} (owned by {ownerIndex})",
                OwnerIndex = ownerIndex,
                TileIndex = tile.Index,
                MovementType = movementType,
                BaseMovementPoints = movementType == MovementType.Airborne ? 4 : movementType == MovementType.Water ? 5 : 2,
                RoadMovementBonus = movementType == MovementType.Land ? 1 : 0,
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
            document.Units = document.Units.OrderBy(x => x.Index).ToList();
            return document.ToBoard();
        }

        public static Board RemoveUnits(Board board, Tile tile)
        {
            if (tile == null)
                return board;

            var document = MapDocument.FromBoard(board);
            document.Units.RemoveAll(x => x.TileIndex == tile.Index);
            return document.ToBoard();
        }

        public static Board EraseTileContent(Board board, Tile tile)
        {
            if (tile == null)
                return board;

            var document = MapDocument.FromBoard(board);
            document.Structures.RemoveAll(x => x.StartsWith(tile.Index + ",", StringComparison.Ordinal));
            document.Units.RemoveAll(x => x.TileIndex == tile.Index);
            return document.ToBoard();
        }

        public static Board SetEdge(Board board, Tile first, Tile second, EdgeType edgeType, bool hasRoad)
        {
            if (!AreAdjacent(first, second))
                return board;

            var document = MapDocument.FromBoard(board);
            var low = Math.Min(first.Index, second.Index);
            var high = Math.Max(first.Index, second.Index);
            document.Edges.RemoveAll(x => x.StartsWith($"{low},{high},", StringComparison.Ordinal));
            if (edgeType != EdgeType.None || hasRoad)
            {
                document.Edges.Add($"{low},{high},{edgeType},{hasRoad.ToString().ToLowerInvariant()}");
                document.Edges = document.Edges.OrderBy(x => x).ToList();
            }
            return document.ToBoard();
        }

        public static SimulationSession StartSimulation(Board board, int maxTurns = 50) =>
            new SimulationSession(board, maxTurns);
    }

    internal static class EditorLayout
    {
        public static readonly Layout Layout = new Layout(Layout.flat, new PointD(25, 25), new PointD(25, 21.650635094610966));
    }

    /// <summary>
    /// Holds a lazily-computed sequence of per-turn board states so the UI can
    /// step forward and backward through a simulation without re-running turns.
    /// </summary>
    internal class SimulationSession
    {
        private readonly int _maxTurns;
        private readonly int _numberOfPlayers;
        private readonly ComputerPlayer _computerPlayer;

        // Index 0 = initial board (turn 0, before any simulation).
        // Each subsequent entry is the board after that turn resolved.
        private readonly List<MapDocument> _snapshots = new();

        private int _currentIndex = 0;

        public int CurrentTurn => _currentIndex;
        public bool IsFinished { get; private set; }

        public bool CanStepForward =>
            !IsFinished || _currentIndex < _snapshots.Count - 1;

        public bool CanStepBack => _currentIndex > 0;

        public Board CurrentBoard { get; private set; }

        private readonly int _initialUnitOwners;
        private readonly int _initialStructureOwners;

        internal SimulationSession(Board board, int maxTurns)
        {
            _maxTurns = maxTurns;

            // Take a clean copy as turn-0 snapshot
            CurrentBoard = MapDocument.FromBoard(board).ToBoard();
            _snapshots.Add(MapDocument.FromBoard(CurrentBoard));

            var ownerCount = CurrentBoard.Units.Select(x => x.OwnerIndex).DefaultIfEmpty(0).Distinct().Count();
            _numberOfPlayers = Math.Max(2, ownerCount);
            _computerPlayer = new ComputerPlayer(CurrentBoard.Units);

            _initialUnitOwners      = CurrentBoard.Units.Where(x => x.IsAlive).Select(x => x.OwnerIndex).Distinct().Count();
            _initialStructureOwners = CurrentBoard.Structures.Select(x => x.OwnerIndex).Distinct().Count();
        }

        public string StatusLine()
        {
            var alive  = CurrentBoard.Units.Count(u => u.IsAlive);
            var owners = string.Join(", ",
                CurrentBoard.Structures
                    .GroupBy(s => s.OwnerIndex)
                    .OrderBy(g => g.Key)
                    .Select(g => $"P{g.Key}:{g.Count()}"));
            return $"Turn {CurrentTurn}  |  Units alive: {alive}  |  Structures: {owners}";
        }

        /// <summary>Advances one turn. Returns false if already at the end.</summary>
        public bool StepForward()
        {
            // If we already have the next snapshot cached, just move the pointer.
            if (_currentIndex < _snapshots.Count - 1)
            {
                _currentIndex++;
                CurrentBoard = _snapshots[_currentIndex].ToBoard();
                return true;
            }

            if (IsFinished) return false;

            // Compute and cache the next turn.
            var sim = _snapshots[_currentIndex].ToBoard();
            _computerPlayer.GenerateInfluenceMaps(sim, _numberOfPlayers);
            _computerPlayer.SetStrategicAction(sim);
            var orders = _computerPlayer.CreateOrders(sim, sim.Units.Where(x => x.IsAlive).ToList());
            sim.ResolveOrders(orders);
            for (var i = 0; i < _numberOfPlayers; i++)
                sim.ResolveStackLimits(i);
            sim.ConductBattles();
            sim.ChangeStructureOwners();
            sim.Turn++;

            _snapshots.Add(MapDocument.FromBoard(sim));
            _currentIndex++;
            CurrentBoard = sim;

            // Check end conditions — only stop early if sides have actually been eliminated
            var aliveOwners     = sim.Units.Where(x => x.IsAlive).Select(x => x.OwnerIndex).Distinct().Count();
            var structureOwners = sim.Structures.Select(x => x.OwnerIndex).Distinct().Count();
            if (sim.Turn >= _maxTurns
                || (_initialUnitOwners      > 1 && aliveOwners     <= 1)
                || (_initialStructureOwners > 1 && structureOwners <= 1))
            {
                IsFinished = true;
            }

            return true;
        }

        /// <summary>Steps back to the previous turn. Returns false if already at turn 0.</summary>
        public bool StepBack()
        {
            if (!CanStepBack) return false;
            _currentIndex--;
            CurrentBoard = _snapshots[_currentIndex].ToBoard();
            return true;
        }

        /// <summary>Jumps back to turn 0.</summary>
        public void Restart()
        {
            _currentIndex = 0;
            IsFinished = false;
            CurrentBoard = _snapshots[0].ToBoard();
        }
    }

    internal class SimulationResult
    {
        public Board Board { get; set; }
        public int TurnsCompleted { get; set; }
        public int RemainingUnits { get; set; }
        public Dictionary<int, int> StructuresByOwner { get; set; }
    }
}
