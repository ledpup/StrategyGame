using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameModel
{
    [Flags]
    public enum TerrainType
    {
        Grassland = 1 << 0,
        Desert = 1 << 1,
        Forest = 1 << 2,
        Hill = 1 << 3,
        Mountain = 1 << 4,
        Water = 1 << 5,
        Wetland = 1 << 6,
        Reef = 1 << 7,
        Coastal = 1 << 10, // This is a calculated terrain type. Any land hex adjacent to sea or reef is coastal.
        Sea = 1 << 11, // This is a calculated terrain type. Any land hex adjacent to sea or reef is coastal.
        Lake = 1 << 12, // This is a calculated terrain type. Any land hex adjacent to sea or reef is coastal.
    }

    public static class Terrain
    {
        public static TerrainType All_Land = TerrainType.Grassland | TerrainType.Forest | TerrainType.Desert | TerrainType.Hill | TerrainType.Mountain | TerrainType.Wetland;
        public static TerrainType Aquatic_Terrain = TerrainType.Water | TerrainType.Reef | TerrainType.Coastal;
        public static TerrainType Water_Terrain = TerrainType.Water | TerrainType.Reef;
        public static TerrainType Non_Mountainous_Land = All_Land ^ TerrainType.Mountain;
        public static TerrainType Rough_Land = All_Land ^ TerrainType.Grassland;
        public static TerrainType All_Terrain = Terrain.All_Land ^ Terrain.Aquatic_Terrain;

        public static TerrainType ConvertCharToTerrainType(char character)
        {
            switch (character)
            {
                case 'G':
                    return TerrainType.Grassland;
                case 'F':
                    return TerrainType.Forest;
                case 'D':
                    return TerrainType.Desert;
                case 'M':
                    return TerrainType.Mountain;
                case 'H':
                    return TerrainType.Hill;
                case 'W':
                    return TerrainType.Wetland;
                case 'L':
                    return TerrainType.Water;
                case 'S':
                    return TerrainType.Water;
                case 'R':
                    return TerrainType.Reef;
            }
            throw new Exception(string.Format("{0} is an unknown terrain type.", character));
        }
    }
}
