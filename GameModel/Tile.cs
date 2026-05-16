namespace GameModel;

using System;
using System.Collections.Generic;
using System.Linq;
using Hexagon;

public class Tile
{
    public int Index { get; private set; }

    public Hex Hex { get; private set; }

    public BaseTerrainType BaseTerrainType;
    public TerrainType TerrainType;
    public Weather Weather;

    public int ConnectedRegionId { get; set; }

    public int X, Y;

    public Tile(int x, int y, int width, TerrainType terrainType = TerrainType.Grassland, bool isEdgeOfMap = false)
    {
        X = x;
        Y = y;

        var offsetCoords = new OffsetCoord(x, y);
        Hex = offsetCoords.QoffsetToCube();
        Index = OffsetCoord.OffsetCoordsToIndex(x, y, width);

        BaseTerrainType = terrainType.HasFlag(TerrainType.Water) || terrainType.HasFlag(TerrainType.Reef) ? BaseTerrainType.Water : BaseTerrainType.Land;
        TerrainType = terrainType;
        IsEdgeOfMap = isEdgeOfMap;
    }

    public string ToOffsetCoordsString()
    {
        var offsetCoords = OffsetCoord.QoffsetFromCube(Hex);
        return string.Format($"{offsetCoords.col}, {offsetCoords.row}");
    }

    public override string ToString()
    {
        return string.Format($"{Index}: {Hex.ToString()} {TerrainType}");
    }

    public float? Supply { get; set; }

    public List<Edge> Neighbours { get; set; }

    public bool HasPort
    {
        get
        {
            return Neighbours.Any(x => x.EdgeType == EdgeType.Port);
        }
    }

    public bool IsCoast
    {
        get
        {
            if (searchedForCoast)
            {
                return field;
            }

            searchedForCoast = true;

            field = Terrain.AllWater.HasFlag(TerrainType) && Neighbours.Any(x => Terrain.AllLand.HasFlag(x.Destination.TerrainType));

            return field;
        }
    }

    private bool searchedForCoast;

    public bool IsSea
    {
        get
        {
            if (searchedForSea)
            {
                return field;
            }

            searchedForSea = true;

            field = Terrain.AllWater.HasFlag(TerrainType) && (Neighbours.Any(x => x.Destination.IsSea) || IsEdgeOfMap);

            return field;
        }
    }

    private bool searchedForSea;

    public bool IsLake
    {
        get
        {
            if (searchedForLake)
            {
                return field;
            }

            searchedForLake = true;

            field = Terrain.AllWater.HasFlag(TerrainType) && !IsEdgeOfMap && !Neighbours.Any(x => x.Destination.IsSea);

            return field;
        }
    }

    private bool searchedForLake;

    public TerrainType GetTerrainTypeByTemperature(double temperature)
    {
        switch (TerrainType)
        {
            case TerrainType.Mountain:
            case TerrainType.Hill:
            case TerrainType.Forest:
            case TerrainType.Reef:
                return TerrainType;

            case TerrainType.Grassland:
                if (temperature < 10)
                {
                    return TerrainType.Swamp;
                }

                if (temperature > 30)
                {
                    return TerrainType.Desert;
                }

                return TerrainType.Grassland;

            case TerrainType.Water:
                if (IsLake)
                {
                    if (temperature > 30)
                    {
                        return TerrainType.Swamp;
                    }
                }

                return TerrainType.Water;

            case TerrainType.Desert:
                if (temperature < 10)
                {
                    return TerrainType.Grassland;
                }

                return TerrainType.Desert;

            case TerrainType.Swamp:
                if (temperature < 10)
                {
                    return TerrainType.Water;
                }

                if (temperature > 30)
                {
                    return TerrainType.Grassland;
                }

                return TerrainType.Swamp;
        }

        throw new Exception("Can got resolve terrain type based on temperature given base type of " + TerrainType);
    }

    public int StackLimit
    {
        get
        {
            if (field == 0)
            {
                field = Terrain.TerrainStackLimit[TerrainType];
                if (Settlement != null)
                {
                    field++;
                }
            }

            return field;
        }
    } = 0;

    public bool OverStackLimit(IEnumerable<MilitaryUnit> tileUnits, Guid playerId)
    {
        return OverStackLimitCount(tileUnits, playerId) > 0;
    }

    internal int OverStackLimitCount(IEnumerable<MilitaryUnit> tileUnits, Guid playerId)
    {
        return tileUnits.Count(x => x.IsAlive && x.Owner.Id == playerId) - StackLimit;
    }

    internal static bool IsInConflict(IEnumerable<MilitaryUnit> tileUnits)
    {
        return tileUnits.Where(x => x.IsAlive).GroupBy(x => x.Owner.Id).Count() > 1;
    }

    public bool IsEdgeOfMap { get; private set; }

    public double Temperature { get; set; }

    public int DistanceFromWater { get; internal set; }

    public TerrainType TemperatureAdjustedTerrainType { get; set; }

    public bool IsSelected { get; set; }

    public Settlement? Settlement { get; set; }

    public Tile PortDestination
    {
        get
        {
            var edge = Neighbours.Single(x => x.EdgeType == EdgeType.Port);
            return edge.Destination;
        }
    }

    public override bool Equals(object obj)
    {
        if (obj is not Tile)
        {
            return false;
        }

        var tile = (Tile)obj;

        return Index == tile.Index;
    }

    public override int GetHashCode() => Index;
}
