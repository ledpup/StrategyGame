using Hexagon;
using PathFind;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameModel
{
     public class Tile
    {
        public int Index { get; private set; }
        public Point Point { get; private set; }
        public int X { get { return Point.X; } }
        public int Y { get { return Point.Y; } }
        public BaseTerrainType BaseTerrainType;
        public TerrainType TerrainType;
        public Weather Weather;
        public List<MilitaryUnit> Units;

        public Hex Hex;

        public Dictionary<MovementType, double[]> FriendlyStructureInfluence;
        public Dictionary<MovementType, double[]> EnemyStructureInfluence;
        public double[] FriendlyUnitInfluence;
        public double[] EnemyUnitInfluence;
        public Dictionary<RoleMovementType, double[]> AggregateInfluence;
        public Dictionary<int, double> TerrainAndWeatherInfluenceByUnit;

        public int ContiguousRegionId { get; set; }

        public Tile(int index, int x, int y, TerrainType terrainType = TerrainType.Grassland, bool isEdgeOfMap = false)
        {
            Units = new List<MilitaryUnit>();

            Index = index;
            Point = new Point(x, y);

            Hex = OffsetCoord.QoffsetToCube(new OffsetCoord(x, y));

            BaseTerrainType = terrainType.HasFlag(TerrainType.Water) || terrainType.HasFlag(TerrainType.Reef) ? BaseTerrainType.Water : BaseTerrainType.Land;
            TerrainType = terrainType;
            IsEdgeOfMap = isEdgeOfMap;

            AggregateInfluence = new Dictionary<RoleMovementType, double[]>();

            TerrainAndWeatherInfluenceByUnit = new Dictionary<int, double>();
        }

        public override string ToString()
        {
            return string.Format($"{Index} {Point.ToString()} {TerrainType}");
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
                if (_isTileSearchedForCoast)
                    return _isCoast;

                _isTileSearchedForCoast = true;

                _isCoast = Terrain.All_Water.HasFlag(TerrainType) && Neighbours.Any(x => Terrain.All_Land.HasFlag(x.Destination.TerrainType));

                return _isCoast;
            }
        }
        bool _isCoast;
        bool _isTileSearchedForCoast;

        public bool IsSea
        {
            get
            {
                if (_isTileSearchedForSea)
                    return _isSea;

                _isTileSearchedForSea = true;

                _isSea = Terrain.All_Water.HasFlag(TerrainType) && (Neighbours.Any(x => x.Destination.IsSea) || IsEdgeOfMap);

                return _isSea;
            }
        }
        bool _isSea;
        bool _isTileSearchedForSea;

        public bool IsLake
        {
            get
            {
                if (_isTileSearchedForLake)
                    return _isLake;

                _isTileSearchedForLake = true;

                _isLake = Terrain.All_Water.HasFlag(TerrainType) && !IsEdgeOfMap && !Neighbours.Any(x => x.Destination.IsSea);

                return _isLake;
            }
        }
        bool _isLake;
        bool _isTileSearchedForLake;

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
                        return TerrainType.Steppe;
                    }
                    return TerrainType.Grassland;

                case TerrainType.Water:
                    if (IsLake)
                    {
                        if (temperature > 30)
                            return TerrainType.Wetland;
                    }
                    return TerrainType.Water;

                case TerrainType.Steppe:
                    if (temperature < 10)
                        return TerrainType.Grassland;
                    return TerrainType.Steppe;

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
                if (_stackLimit == 0)
                {
                    _stackLimit = Terrain.TerrainStackLimit[TerrainType];
                    if (Structure != null)
                    {
                        _stackLimit++;
                    }
                }
                return _stackLimit;
            }
        }
        int _stackLimit = 0;
        

        public bool OverStackLimit(int playerIndex)
        {
            return OverStackLimitCount(playerIndex) > 0;
        }

        public int OverStackLimitCount(int playerIndex)
        {
            return Units.Count(x => x.IsAlive && x.OwnerIndex == playerIndex) - StackLimit;
        }

        public bool IsInConflict
        {
            get
            {
                var units = Units.Where(x => x.IsAlive).GroupBy(x => x.OwnerIndex);
                if (units.Count() > 1)
                    return true;
                return false;
            }
        }

        public bool IsEdgeOfMap { get; private set; }
        public double Temperature { get; set; }
        public int DistanceFromWater { get; internal set; }
        public TerrainType TemperatureAdjustedTerrainType { get; set; }
        public int? OwnerId { get; set; }
        public bool IsSelected { get; set; }
        public Structure Structure { get; set; }
        public Tile PortDestination
        {
            get
            {
                var edge = Neighbours.Single(x => x.EdgeType == EdgeType.Port);
                return edge.Destination;
            }
        }
    }
}
