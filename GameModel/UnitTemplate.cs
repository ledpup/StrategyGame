using System.Collections.Generic;

namespace GameModel;

public record UnitTemplate
{
    public string Name { get; set; }
    public UnitType UnitType { get; set; }
    public MovementType MovementType { get; set; } = MovementType.Land;
    public int MovementPoints { get; set; } = 2;
    public int RoadMovementBonus { get; set; } = 0;
    public double Quality { get; set; } = 1;
    public int Personnel { get; set; } = 100;
    public double Size { get; set; } = 1;
    public bool IsTransporter { get; set; } = false;
    public List<MovementType> TransportableBy { get; set; }
    public int CombatInitiative { get; set; } = 10;
    public double Morale { get; set; } = 5;

    public Dictionary<TerrainType, int> TerrainMovementCosts => MovementType switch
    {
        MovementType.Land => new Dictionary<TerrainType, int>
        {
            { TerrainType.Grassland, 1 },
            { TerrainType.Desert,    2 },
            { TerrainType.Forest,    2 },
            { TerrainType.Hill,      2 },
            { TerrainType.Mountain,  Terrain.Impassable },
            { TerrainType.Water,     Terrain.Impassable },
            { TerrainType.Swamp,   2 },
            { TerrainType.Reef,      Terrain.Impassable },
        },
        MovementType.Airborne => new Dictionary<TerrainType, int>
        {
            { TerrainType.Grassland, 1 },
            { TerrainType.Desert,    1 },
            { TerrainType.Forest,    1 },
            { TerrainType.Hill,      1 },
            { TerrainType.Mountain,  1 },
            { TerrainType.Water,     1 },
            { TerrainType.Swamp,   1 },
            { TerrainType.Reef,      1 },
        },
        MovementType.Waterbound => new Dictionary<TerrainType, int>
        {
            { TerrainType.Grassland, Terrain.Impassable },
            { TerrainType.Desert,    Terrain.Impassable },
            { TerrainType.Forest,    Terrain.Impassable },
            { TerrainType.Hill,      Terrain.Impassable },
            { TerrainType.Mountain,  Terrain.Impassable },
            { TerrainType.Water,     1 },
            { TerrainType.Swamp,   Terrain.Impassable },
            { TerrainType.Reef,      2 },
        },
        _ => [],
    };

    public Dictionary<EdgeType, int> EdgeMovementCosts => MovementType switch
    {
        MovementType.Land => new Dictionary<EdgeType, int>
        {
            { EdgeType.None,     0 },
            { EdgeType.River,    Terrain.Impassable },
            { EdgeType.Forest,   1 },
            { EdgeType.Hill,     1 },
            { EdgeType.Mountain, Terrain.Impassable },
            { EdgeType.Reef,     Terrain.Impassable },
            { EdgeType.Wall,     Terrain.Impassable },
            { EdgeType.Port,     1 },
        },
        MovementType.Airborne => new Dictionary<EdgeType, int>
        {
            { EdgeType.None,     0 },
            { EdgeType.River,    0 },
            { EdgeType.Forest,   0 },
            { EdgeType.Hill,     0 },
            { EdgeType.Mountain, 0 },
            { EdgeType.Reef,     0 },
            { EdgeType.Wall,     0 },
            { EdgeType.Port,     0 },
        },
        MovementType.Waterbound => new Dictionary<EdgeType, int>
        {
            { EdgeType.None,     0 },
            { EdgeType.River,    Terrain.Impassable },
            { EdgeType.Forest,   Terrain.Impassable },
            { EdgeType.Hill,     Terrain.Impassable },
            { EdgeType.Mountain, Terrain.Impassable },
            { EdgeType.Reef,     1 },
            { EdgeType.Wall,     Terrain.Impassable },
            { EdgeType.Port,     Terrain.Impassable },
        },
        _ => [],
    };

    public bool UsesRoads => MovementType == MovementType.Land;

    public TerrainType CanStopOn => MovementType switch
    {
        MovementType.Land     => Terrain.Non_Mountainous_Land,
        MovementType.Airborne => Terrain.Non_Mountainous_Land,
        MovementType.Waterbound => Terrain.All_Water,
        _ => default,
    };
}
