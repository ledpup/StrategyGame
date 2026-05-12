namespace GameModel;

using System;
using System.Collections.Generic;

public enum BaseTerrainType
{
    Land,
    Water,
}

[Flags]
public enum TerrainType
{
    Grassland = 1 << 0,
    Desert = 1 << 1,
    Forest = 1 << 2,
    Hill = 1 << 3,
    Mountain = 1 << 4,
    Water = 1 << 5,
    Swamp = 1 << 6,
    Reef = 1 << 7,
}

[Flags]
public enum CalculatedTerrainType
{
    Ocean = 1 << 0,
    Coast = 1 << 1,
    Lake = 1 << 2,
    Frozen = 1 << 3, // Can apply to lake and wetland
    SnowCovered = 1 << 4, // Can apply to hills, mountains, grassland
}

public static class Terrain
{
    public const int Impassable = 100;

    public const TerrainType AllLand = TerrainType.Grassland | TerrainType.Forest | TerrainType.Desert | TerrainType.Hill | TerrainType.Mountain | TerrainType.Swamp;
    public const TerrainType AllWater = TerrainType.Water | TerrainType.Reef;
    public const TerrainType NonMountainousLand = AllLand ^ TerrainType.Mountain;
    public const TerrainType RoughLand = AllLand ^ TerrainType.Grassland;
    public const TerrainType AllTerrain = Terrain.AllLand ^ Terrain.AllWater;

    public static TerrainType ConvertCharToTerrainType(char character)
    {
        return character switch
        {
            'G' => TerrainType.Grassland,
            'F' => TerrainType.Forest,
            'D' => TerrainType.Desert,
            'M' => TerrainType.Mountain,
            'H' => TerrainType.Hill,
            'W' => TerrainType.Swamp,
            'L' => TerrainType.Water,
            'S' => TerrainType.Water,
            'R' => TerrainType.Reef,
            _ => throw new Exception(string.Format("{0} is an unknown terrain type.", character)),
        };
    }

    public static Dictionary<TerrainType, int> TerrainStackLimit
    {
        get
        {
            if (terrainStackLimit != null)
            {
                return terrainStackLimit;
            }

            terrainStackLimit = new Dictionary<TerrainType, int>
            {
                { TerrainType.Forest, 3 },
                { TerrainType.Grassland, 4 },
                { TerrainType.Hill, 3 },
                { TerrainType.Mountain, 2 },
                { TerrainType.Desert, 2 },
                { TerrainType.Swamp, 2 },

                { TerrainType.Reef, 2 },
                { TerrainType.Water, 4 },
            };
            return terrainStackLimit;
        }
    }

    public static Dictionary<TerrainType, int> terrainStackLimit;
}
