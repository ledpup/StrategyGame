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

        public static SimulationResult Simulate(Board board, int maxTurns = 20)
        {
            var simulation = MapDocument.FromBoard(board).ToBoard();
            var ownerCount = simulation.Units.Select(x => x.OwnerIndex).DefaultIfEmpty(0).Distinct().Count();
            var numberOfPlayers = Math.Max(2, ownerCount);
            var computerPlayer = new ComputerPlayer(simulation.Units);

            while (simulation.Turn < maxTurns
                && simulation.Units.Where(x => x.IsAlive).Select(x => x.OwnerIndex).Distinct().Count() > 1
                && simulation.Structures.Select(x => x.OwnerIndex).Distinct().Count() > 1)
            {
                computerPlayer.GenerateInfluenceMaps(simulation, numberOfPlayers);
                computerPlayer.SetStrategicAction(simulation);
                var orders = computerPlayer.CreateOrders(simulation, simulation.Units.Where(x => x.IsAlive).ToList());
                simulation.ResolveOrders(orders);
                for (var i = 0; i < numberOfPlayers; i++)
                {
                    simulation.ResolveStackLimits(i);
                }
                simulation.ConductBattles();
                simulation.ChangeStructureOwners();
                simulation.Turn++;
            }

            return new SimulationResult
            {
                Board = simulation,
                TurnsCompleted = simulation.Turn,
                RemainingUnits = simulation.Units.Count(x => x.IsAlive),
                StructuresByOwner = simulation.Structures.GroupBy(x => x.OwnerIndex).ToDictionary(x => x.Key, x => x.Count()),
            };
        }
    }

    internal static class EditorLayout
    {
        public static readonly Layout Layout = new Layout(Layout.flat, new PointD(25, 25), new PointD(25, 21.650635094610966));
    }

    internal class SimulationResult
    {
        public Board Board { get; set; }
        public int TurnsCompleted { get; set; }
        public int RemainingUnits { get; set; }
        public Dictionary<int, int> StructuresByOwner { get; set; }
    }
}
