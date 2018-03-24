using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameModel
{
    public enum BaseTerrainType
    {
        Land,
        Water,
    }

    [Flags]
    public enum TerrainType
    {
        Desert = 1 << 0,
        Forest = 1 << 1,
        FrozenWater = 1 << 2,
        Grassland = 1 << 3,
        Hill = 1 << 4,
        Mountain = 1 << 5,
        Water = 1 << 6,
        Wetland = 1 << 7,
        Reef = 1 << 8,
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

        public static TerrainType All_Land = TerrainType.Desert | TerrainType.Forest | TerrainType.FrozenWater | TerrainType.Grassland | TerrainType.Hill | TerrainType.Mountain | TerrainType.Wetland;
        public static TerrainType All_Water = TerrainType.Water | TerrainType.Reef;
        public static TerrainType Non_Mountainous_Land = All_Land ^ TerrainType.Mountain;
        public static TerrainType Rough_Land = All_Land ^ TerrainType.Grassland;
        public static TerrainType All_Terrain = Terrain.All_Land ^ Terrain.All_Water;

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

        public static Dictionary<TerrainType, int> TerrainStackLimit
        {
            get
            {
                if (_terrainStackLimit != null)
                    return _terrainStackLimit;

                _terrainStackLimit = new Dictionary<TerrainType, int>
                {
                    { TerrainType.Forest, 3 },
                    { TerrainType.FrozenWater, 2 },
                    { TerrainType.Grassland, 4 },
                    { TerrainType.Hill, 3 },
                    { TerrainType.Mountain, 2 },
                    { TerrainType.Desert, 2 },
                    { TerrainType.Wetland, 2 },

                    { TerrainType.Reef, 2 },
                    { TerrainType.Water, 4 }
                };
                return _terrainStackLimit;
            }
        }
        public static Dictionary<TerrainType, int> _terrainStackLimit;
    }
}
