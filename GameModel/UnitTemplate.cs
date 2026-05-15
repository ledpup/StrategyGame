namespace GameModel;

using System.Collections.Generic;

public record UnitTemplate(OperationalDomain MovementDomain = OperationalDomain.Land)
{
    public OperationalDomain OperationalDomain { get; set; } = MovementDomain;

    public UnitTemplateName? UnitTemplateName { get; set; }

    public string Name
    {
        get { return UnitTemplateName is null ? OperationalDomain.ToString() : UnitTemplateName.ToDisplayName(); }
    }

    public UnitType UnitType { get; set; }

    public int MovementPoints { get; set; } = 2;

    public int RoadMovementBonus
    {
        get
        {
            return field;
        }

        set
        {
            if (OperationalDomain == OperationalDomain.Land)
            {
                field = value;
            }
            else
            {
                field = 0;
            }
        }
    }

    public double Quality { get; set; } = 1;

    public int Personnel { get; set; } = 100;

    public double Size { get; set; } = 1;

    public bool IsTransporter { get; set; } = false;

    public List<OperationalDomain> TransportableBy { get; set; }

    public int CombatInitiative { get; set; } = 10;

    public double Morale { get; set; } = 5;

    public Dictionary<TerrainType, int> TerrainMovementCosts => OperationalDomain switch
    {
        OperationalDomain.Land => new Dictionary<TerrainType, int>
        {
            { TerrainType.Grassland, 1 },
            { TerrainType.Desert,    2 },
            { TerrainType.Forest,    2 },
            { TerrainType.Hill,      2 },
            { TerrainType.Mountain,  Terrain.Impassable },
            { TerrainType.Water,     Terrain.Impassable },
            { TerrainType.Swamp,     2 },
            { TerrainType.Reef,      Terrain.Impassable },
        },
        OperationalDomain.Airborne => new Dictionary<TerrainType, int>
        {
            { TerrainType.Grassland, 1 },
            { TerrainType.Desert,    1 },
            { TerrainType.Forest,    1 },
            { TerrainType.Hill,      1 },
            { TerrainType.Mountain,  1 },
            { TerrainType.Water,     1 },
            { TerrainType.Swamp,     1 },
            { TerrainType.Reef,      1 },
        },
        OperationalDomain.Waterbound => new Dictionary<TerrainType, int>
        {
            { TerrainType.Grassland, Terrain.Impassable },
            { TerrainType.Desert,    Terrain.Impassable },
            { TerrainType.Forest,    Terrain.Impassable },
            { TerrainType.Hill,      Terrain.Impassable },
            { TerrainType.Mountain,  Terrain.Impassable },
            { TerrainType.Water,     1 },
            { TerrainType.Swamp,     Terrain.Impassable },
            { TerrainType.Reef,      2 },
        },
        _ => [],
    };

    public Dictionary<EdgeType, int> EdgeMovementCosts => OperationalDomain switch
    {
        OperationalDomain.Land => new Dictionary<EdgeType, int>
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
        OperationalDomain.Airborne => new Dictionary<EdgeType, int>
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
        OperationalDomain.Waterbound => new Dictionary<EdgeType, int>
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

    public bool UsesRoads => OperationalDomain == OperationalDomain.Land;

    public TerrainType CanStopOn => OperationalDomain switch
    {
        OperationalDomain.Land => Terrain.NonMountainousLand,
        OperationalDomain.Airborne => Terrain.NonMountainousLand,
        OperationalDomain.Waterbound => Terrain.AllWater,
        _ => default,
    };
}
