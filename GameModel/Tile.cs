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
        public Point Location { get; private set; }
        public int X { get { return Location.X; } }
        public int Y { get { return Location.Y; } }
        public TerrainType TerrainType;
        public Weather Weather;
        public List<MilitaryUnit> Units;

        public Hex Hex;

        public double StructureInfluence;
        public Dictionary<MovementType, double[]> UnitCountInfluence;
        public Dictionary<MovementType, double[]> UnitStrengthInfluence;
        public Dictionary<MovementType, double[]> AggregateInfluence;
        public Dictionary<int, double> TerrainAndWeatherInfluenceByUnit;

        public Tile(int index, int x, int y, TerrainType terrainType = TerrainType.Grassland, bool isEdge = false)
        {
            Units = new List<MilitaryUnit>();
            NeighbourEdges = new List<Edge>();

            Index = index;
            Location = new Point(x, y);

            Hex = OffsetCoord.QoffsetToCube(new OffsetCoord(x, y));

            TerrainType = terrainType;
            IsEdgeOfMap = isEdge;

            TerrainAndWeatherInfluenceByUnit = new Dictionary<int, double>();
        }

        public override string ToString()
        {
            var subTerrain = IsLake ? " (Lake)" : IsSea ? " (Sea)" : "";
            return Index + " " + Location.ToString() + " " + TerrainType + subTerrain + (Temperature < 0 ? " Frozen" : "");
        }

        public List<Edge> NeighbourEdges
        {
            get; set;
        }

        public float? Supply { get; set; }

        public double CalculateMoveCost(MilitaryUnit unit, Tile destination)
        {
            var costChanged = false;
            var cost = 100D;

            var edge = NeighbourEdges.SingleOrDefault(x => x.Tiles.Contains(destination));
            if (edge != null)
            {
                if (unit.CanMoveOverEdge.HasFlag(edge.EdgeType))
                {
                    costChanged = true;
                    cost = 2;
                    if (edge.BaseEdgeType == BaseEdgeType.CentreToCentre)
                        return 1;
                }
                else
                {
                    return 100;
                }
            }

            if (unit.TerrainMovementCosts[TerrainType] != null)
            {
                if (unit.TerrainMovementCosts[destination.TerrainType] != null)
                {
                    if (costChanged)
                    {
                        cost += (double)unit.TerrainMovementCosts[destination.TerrainType];
                    }
                    else
                    {
                        cost = (double)unit.TerrainMovementCosts[destination.TerrainType];
                    }
                }
            }
            
            return cost;
        }

        public List<Tile> Neighbours { get; set; }

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
                    .Where(x => tile.NeighbourEdges.Any(y => unit.CanMoveOverEdge.HasFlag(y.EdgeType)) || unit.TerrainMovementCosts[x.TerrainType] != null);
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

        //public int StackLimit
        //{
        //    get
        //    {
        //        if (Structure != null)
        //            return stackLimit + 1;
        //        return stackLimit;
        //    }
        //}

        //private int stackLimit
        //{
        //    get { return TerrainType.StackLimit; }
        //}


        

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
    }
}
