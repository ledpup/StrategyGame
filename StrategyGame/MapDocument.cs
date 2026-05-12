using GameModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StrategyGame;

internal class MapDocument
{
    public string[] Tiles { get; set; }
    public List<string> Edges { get; set; }
    public List<string> Settlements { get; set; }
    public List<UnitDocument> Units { get; set; }

    public static MapDocument CreateDefault(int width, int height)
    {
        var row = new string('G', width);
        return new MapDocument
        {
            Tiles = Enumerable.Range(0, height).Select(_ => row).ToArray(),
            Edges = [],
            Settlements = [],
            Units = [],
        };
    }

    public Board ToBoard() =>
        new Board(Tiles, Edges.ToArray(), Settlements.ToArray());

    public GameState ToGameState(int turn = 0)
    {
        var board = ToBoard();
        var units = Units.Select(ToMilitaryUnit(board)).ToList();
        return new GameState(board, units, turn);
    }

    public static MapDocument FromBoard(Board board) => FromGameState(new GameState(board));

    public static MapDocument FromGameState(GameState gameState)
    {
        var board = gameState.Board;
        var tiles = new string[board.Height];
        for (var y = 0; y < board.Height; y++)
        {
            var chars = new char[board.Width];
            for (var x = 0; x < board.Width; x++)
            {
                chars[x] = TerrainToChar(board[x, y].TerrainType);
            }
            tiles[y] = new string(chars);
        }

        return new MapDocument
        {
            Tiles = tiles,
            Edges = board.Edges
                .Select(x => $"{Math.Min(x.Origin.Index, x.Destination.Index)},{Math.Max(x.Origin.Index, x.Destination.Index)},{x.EdgeType},{x.HasRoad.ToString().ToLowerInvariant()}")
                .OrderBy(x => x)
                .ToList(),
            Settlements = board.Settlements
                .Select(x => $"{x.Index},{x.SettlementType},{x.OwnerIndex},{(int)x.Supply}")
                .OrderBy(x => x)
                .ToList(),
            Units = gameState.Units
                .Select(UnitDocument.FromMilitaryUnit)
                .OrderBy(x => x.Index)
                .ToList(),
        };
    }

    public void Save(string filePath)
    {
        using var writer = new StreamWriter(filePath);
        WriteTo(writer);
    }

    public void WriteTo(TextWriter writer)
    {
        writer.WriteLine("[Tiles]");
        foreach (var t in Tiles) writer.WriteLine(t);
        writer.WriteLine("[Edges]");
        foreach (var e in Edges) writer.WriteLine(e);
        writer.WriteLine("[Settlements]");
        foreach (var s in Settlements) writer.WriteLine(s);
        writer.WriteLine("[Units]");
        foreach (var u in Units) writer.WriteLine(u.ToLine());
    }

    public static MapDocument Load(string filePath) =>
        ParseFromLines(File.ReadAllLines(filePath));

    public static MapDocument ParseFromLines(IEnumerable<string> rawLines)
    {
        var sections = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        string currentSection = null;
        foreach (var rawLine in rawLines)
        {
            var line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line))
                continue;

            if (line.StartsWith("[") && line.EndsWith("]") && line != "[Snapshot]")
            {
                currentSection = line;
                if (!sections.ContainsKey(currentSection))
                    sections[currentSection] = [];
                continue;
            }

            if (currentSection != null)
                sections[currentSection].Add(line);
        }

        return new MapDocument
        {
            Tiles = GetSection(sections, "[Tiles]").ToArray(),
            Edges = GetSection(sections, "[Edges]"),
            Settlements = GetSection(sections, "[Settlements]"),
            Units = GetSection(sections, "[Units]").Select(UnitDocument.Parse).ToList(),
        };
    }

    private static List<string> GetSection(Dictionary<string, List<string>> sections, string name)
    {
        sections.TryGetValue(name, out var lines);
        return lines ?? [];
    }

    private static Func<UnitDocument, MilitaryUnit> ToMilitaryUnit(Board board)
    {
        return x =>
        {
            var template = new UnitTemplate
            {
                UnitTemplateName = x.UnitTemplateName,
                MovementType = x.MovementType,
                MovementPoints = x.BaseMovementPoints,
                RoadMovementBonus = x.RoadMovementBonus,
                UnitType = x.UnitType,
                Quality = x.BaseQuality,
                Personnel = x.InitialQuantity,
                Size = x.Size,
                IsTransporter = x.IsTransporter,
                TransportableBy = x.TransportableBy,
                CombatInitiative = x.CombatInitiative,
                Morale = x.InitialMorale,
            };
            return new MilitaryUnit(template, x.Index, x.OwnerIndex, board[x.TileIndex], turnBuilt: x.TurnBuilt);
        };
    }

    internal static char TerrainToChar(TerrainType terrainType)
    {
        return terrainType switch
        {
            TerrainType.Grassland => 'G',
            TerrainType.Desert => 'D',
            TerrainType.Forest => 'F',
            TerrainType.Hill => 'H',
            TerrainType.Mountain => 'M',
            TerrainType.Water => 'L',
            TerrainType.Swamp => 'W',
            TerrainType.Reef => 'R',
            _ => throw new InvalidOperationException($"Unsupported terrain type {terrainType}"),
        };
    }
}

internal class UnitDocument
{
    public int Index { get; set; }
    public UnitTemplateName UnitTemplateName { get; set; }

    public string Name { get; set; }
    public int OwnerIndex { get; set; }
    public int TileIndex { get; set; }
    public MovementType MovementType { get; set; }
    public int BaseMovementPoints { get; set; }
    public int RoadMovementBonus { get; set; }
    public UnitType UnitType { get; set; }
    public double BaseQuality { get; set; }
    public int InitialQuantity { get; set; }
    public double Size { get; set; }
    public bool IsTransporter { get; set; }
    public List<MovementType> TransportableBy { get; set; }
    public int CombatInitiative { get; set; }
    public double InitialMorale { get; set; }
    public int TurnBuilt { get; set; }

    public static UnitDocument FromMilitaryUnit(MilitaryUnit unit)
    {
        return new UnitDocument
        {
            Index = unit.Index,
            UnitTemplateName = unit.UnitTemplate.UnitTemplateName,
            OwnerIndex = unit.OwnerIndex,
            TileIndex = unit.Location.Index,
            MovementType = unit.MovementType,
            BaseMovementPoints = unit.BaseMovementPoints,
            RoadMovementBonus = unit.RoadMovementBonus,
            UnitType = unit.UnitType,
            BaseQuality = unit.BaseQuality,
            InitialQuantity = unit.UnitTemplate.Personnel,
            Size = unit.Size,
            IsTransporter = unit.IsTransporter,
            TransportableBy = unit.TransportableBy.ToList(),
            CombatInitiative = (int)unit.CombatInitiative,
            InitialMorale = unit.InitialMorale,
            TurnBuilt = unit.TurnCreated,
        };
    }

    public string ToLine()
    {
        var transportableBy = string.Join('|', TransportableBy);
        return $"{Index},{UnitTemplateName},{OwnerIndex},{TileIndex},{MovementType},{BaseMovementPoints},{RoadMovementBonus},{UnitType},{BaseQuality},{InitialQuantity},{Size},{IsTransporter},{transportableBy},{CombatInitiative},{InitialMorale},{TurnBuilt}";
    }

    public static UnitDocument Parse(string line)
    {
        var columns = line.Split(',');
        var transportableBy = new List<MovementType>();
        if (columns.Length > 12 && !string.IsNullOrWhiteSpace(columns[12]))
        {
            transportableBy = columns[12]
                .Split('|')
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => Enum.Parse<MovementType>(x))
                .ToList();
        }

        return new UnitDocument
        {
            Index = int.Parse(columns[0]),
            UnitTemplateName = Enum.Parse<UnitTemplateName>(columns[1]),
            OwnerIndex = int.Parse(columns[2]),
            TileIndex = int.Parse(columns[3]),
            MovementType = Enum.Parse<MovementType>(columns[4]),
            BaseMovementPoints = int.Parse(columns[5]),
            RoadMovementBonus = int.Parse(columns[6]),
            UnitType = Enum.Parse<UnitType>(columns[7]),
            BaseQuality = double.Parse(columns[8]),
            InitialQuantity = int.Parse(columns[9]),
            Size = double.Parse(columns[10]),
            IsTransporter = bool.Parse(columns[11]),
            TransportableBy = transportableBy,
            CombatInitiative = int.Parse(columns[13]),
            InitialMorale = double.Parse(columns[14]),
            TurnBuilt = int.Parse(columns[15]),
        };
    }

    private static string Escape(string value)
    {
        return (value ?? string.Empty).Replace("\\", "\\\\").Replace(",", "\\,");
    }

    private static string Unescape(string value)
    {
        return value.Replace("\\,", ",").Replace("\\\\", "\\");
    }
}
