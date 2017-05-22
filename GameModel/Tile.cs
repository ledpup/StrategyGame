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

        public Tile(int index, int x, int y, TerrainType terrainType = TerrainType.Grassland, bool isEdge = false)
        {
            Units = new List<MilitaryUnit>();
            Edges = new List<Edge>();

            Index = index;
            Point = new Point(x, y);

            Hex = OffsetCoord.QoffsetToCube(new OffsetCoord(x, y));

            BaseTerrainType = terrainType.HasFlag(TerrainType.Water) || terrainType.HasFlag(TerrainType.Reef) ? BaseTerrainType.Water : BaseTerrainType.Land;
            TerrainType = terrainType;
            IsEdgeOfMap = isEdge;

            AggregateInfluence = new Dictionary<RoleMovementType, double[]>();

            TerrainAndWeatherInfluenceByUnit = new Dictionary<int, double>();
        }

        public override string ToString()
        {
            var subTerrain = IsLake ? " (Lake)" : IsSea ? " (Sea)" : "";
            return Index + " " + Point.ToString() + " " + TerrainType + subTerrain + (Temperature < 0 ? " Frozen" : "");
        }

        public List<Edge> Edges
        {
            get; set;
        }

        public float? Supply { get; set; }

        public double CalculateMoveCostAStar(MilitaryUnit unit, Tile destination)
        {
            var cost = (double)CalculateMoveCost(unit, destination);
            if (!unit.CanStopOn.HasFlag(destination.TerrainType))
            {
                cost *= 3D;
            }
            return cost;
        }
        public int CalculateMoveCost(MilitaryUnit unit, Tile destination)
        {
            var costChanged = false;
            var cost = 100;

            var edge = Edges.Single(x => x.Destination == destination);

            // Movement by road or bridge always costs 1 regardless of terrain type
            if (unit.MovementType == MovementType.Land && (edge.EdgeType.HasFlag(EdgeType.Road) || edge.EdgeType.HasFlag(EdgeType.Bridge)))
            {
                return 1;
            }
            
            if (unit.EdgeMovementCosts[edge.EdgeType] != null)
            {
                costChanged = true;
                cost = (int)unit.EdgeMovementCosts[edge.EdgeType];
            }
            else
            {
                return cost;
            }

            if (unit.TerrainMovementCosts[destination.TerrainType] != null)
            {
                if (costChanged)
                {
                    cost += (int)unit.TerrainMovementCosts[destination.TerrainType];
                }
                else
                {
                    cost = (int)unit.TerrainMovementCosts[destination.TerrainType];
                }
            }
            

            if (cost == 0)
                throw new Exception();

            return cost;
        }

        public List<Tile> Neighbours { get; set; }

        public bool HasPort
        {
            get
            {
                return Edges.Any(x => x.EdgeType == EdgeType.Port);
            }
        }

        public bool IsCoast
        {
            get
            {
                if (_isTileSearchedForCoast)
                    return _isCoast;

                _isTileSearchedForCoast = true;

                _isCoast = Terrain.All_Water.HasFlag(TerrainType) && Neighbours.Any(x => Terrain.All_Land.HasFlag(x.TerrainType));

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

                _isSea = Terrain.All_Water.HasFlag(TerrainType) && (Neighbours.Any(x => x.IsSea) || IsEdgeOfMap);

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

                _isLake = Terrain.All_Water.HasFlag(TerrainType) && !IsEdgeOfMap && !Neighbours.Any(x => x.IsSea);

                return _isLake;
            }
        }
        bool _isLake;
        bool _isTileSearchedForLake;



        //public override bool Equals(object obj)
        //{
        //    return this.Location.Equals(((Tile)obj).Location);
        //}

        //public override int GetHasCode()
        //{
        //    return Location.GetHashCode();
        //}

        public IEnumerable<Tile> ValidAdjacentMoves(MilitaryUnit unit)
        {
            return ValidAdjacentMoves(unit, this);
        }

        public static IEnumerable<Tile> ValidAdjacentMoves(MilitaryUnit unit, Tile tile)
        {
            return tile
                    .Neighbours
                    .Where(x => unit.EdgeMovementCosts[Edge.GetEdge(tile, x).EdgeType] != null &&
                                (Edge.GetEdge(tile, x).BaseEdgeType == BaseEdgeType.CentreToCentre || unit.TerrainMovementCosts[x.TerrainType] != null));
        }

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
    }
}
