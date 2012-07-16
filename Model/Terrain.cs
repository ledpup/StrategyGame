using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    [Flags]
    public enum TerrainType
    {
        Grassland = 1 << 0,
        Forest = 1 << 1,
        Desert = 1 << 2,
        Hill = 1 << 3,
        Mountain = 1 << 4,
        Wetland = 1 << 5,
        Lake = 1 << 6,
        Sea = 1 << 7,
        Reef = 1 << 8,
        Coastal = 1 << 9, // This is a calculated terrain type. Any land hex adjacent to sea or reef is coastal.
    }

    public static class Terrain
    {
        public static TerrainType All_Land = TerrainType.Grassland | TerrainType.Forest | TerrainType.Desert | TerrainType.Hill | TerrainType.Mountain | TerrainType.Wetland | TerrainType.Lake;
        public static TerrainType All_Water = TerrainType.Sea | TerrainType.Reef | TerrainType.Coastal;
        public static TerrainType All_Land_But_Mountain = All_Land ^ TerrainType.Mountain;
        public static TerrainType All_Land_But_Lake = All_Land ^ TerrainType.Lake;
        public static TerrainType All_Land_But_Mountain_And_Lake = All_Land ^ TerrainType.Mountain ^ TerrainType.Lake;
        public static TerrainType All_Rough_Land = All_Land_But_Lake ^ TerrainType.Grassland;
        public static TerrainType All_Terrain = Terrain.All_Land ^ Terrain.All_Water;
        public static TerrainType Nothing = 0;

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
                    return TerrainType.Lake;
                case 'S':
                    return TerrainType.Sea;
                case 'R':
                    return TerrainType.Reef;
            }
            throw new Exception(string.Format("{0} is an unknown terrain type.", character));
        }
    }
}
