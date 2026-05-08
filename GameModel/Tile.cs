using Hexagon;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameModel
{
     public class Tile
    {
        public int Index { get; private set; }
        public Hex Hex { get; private set; }

        public BaseTerrainType BaseTerrainType;
        public TerrainType TerrainType;
        public Weather Weather;

        public int ContiguousRegionId { get; set; }

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

        //public double CalculateMoveCostAStar(MilitaryUnit unit, Tile destination)
        //{
        //    var cost = (double)CalculateMoveCost(unit, destination);
        //    if (!unit.CanStopOn.HasFlag(destination.TerrainType))
        //    {
        //        cost *= 1.5D;
        //    }
        //    return cost;
        //}


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
                    return isCoast;

                searchedForCoast = true;

                isCoast = Terrain.All_Water.HasFlag(TerrainType) && Neighbours.Any(x => Terrain.All_Land.HasFlag(x.Destination.TerrainType));

                return isCoast;
            }
        }
        bool isCoast;
        bool searchedForCoast;

        public bool IsSea
        {
            get
            {
                if (searchedForSea)
                    return isSea;

                searchedForSea = true;

                isSea = Terrain.All_Water.HasFlag(TerrainType) && (Neighbours.Any(x => x.Destination.IsSea) || IsEdgeOfMap);

                return isSea;
            }
        }
        bool isSea;
        bool searchedForSea;

        public bool IsLake
        {
            get
            {
                if (searchedForLake)
                    return isLake;

                searchedForLake = true;

                isLake = Terrain.All_Water.HasFlag(TerrainType) && !IsEdgeOfMap && !Neighbours.Any(x => x.Destination.IsSea);

                return isLake;
            }
        }
        bool isLake;
        bool searchedForLake;

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
                        return TerrainType.Wetland;
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
                            return TerrainType.Wetland;
                    }
                    return TerrainType.Water;

                case TerrainType.Desert:
                    if (temperature < 10)
                        return TerrainType.Grassland;
                    return TerrainType.Desert;

                case TerrainType.Wetland:
                    if (temperature < 10)
                        return TerrainType.Water;
                    if (temperature > 30)
                        return TerrainType.Grassland;
                    return TerrainType.Wetland;
            }
            throw new Exception("Can got resolve terrain type based on temperature given base type of " + TerrainType);
        }

        public int StackLimit
        {
            get
            {
                if (stackLimit == 0)
                {
                    stackLimit = Terrain.TerrainStackLimit[TerrainType];
                    if (Settlement != null)
                    {
                        stackLimit++;
                    }
                }
                return stackLimit;
            }
        }
        int stackLimit = 0;
        

        public bool OverStackLimit(IEnumerable<MilitaryUnit> tileUnits, int playerIndex)
        {
            return OverStackLimitCount(tileUnits, playerIndex) > 0;
        }

        internal int OverStackLimitCount(IEnumerable<MilitaryUnit> tileUnits, int playerIndex)
        {
            return tileUnits.Count(x => x.IsAlive && x.OwnerIndex == playerIndex) - StackLimit;
        }

        internal static bool IsInConflict(IEnumerable<MilitaryUnit> tileUnits)
        {
            return tileUnits.Where(x => x.IsAlive).GroupBy(x => x.OwnerIndex).Count() > 1;
        }

        public bool IsEdgeOfMap { get; private set; }
        public double Temperature { get; set; }
        public int DistanceFromWater { get; internal set; }
        public TerrainType TemperatureAdjustedTerrainType { get; set; }
        public int? OwnerId { get; set; }
        public bool IsSelected { get; set; }
        public Settlement Settlement { get; set; }
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
                return false;

            var tile = (Tile)obj;

            return Index == tile.Index;
        }

        public override int GetHashCode() => Index;
    }
}
