using PathFind;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameModel
{

    public enum UnitType
    {
        Melee,
        Ranged,
        Cavalry,
        Siege,
        Aquatic,
    }

    public enum MovementType
    {
        Land,
        Water,
        Airborne,
        Amphibious,
    }

    public enum BattleQualityModifier
    {
        Terrain,
        Weather,
        UnitType,
        Structure,
    }
    public class MilitaryUnit
    {
        public int Index;
        public string Name { get; set; }
        public int OwnerIndex { get; set; }
        public UnitType UnitType { get; set; }
        public MovementType MovementType
        {
            get { return _movementType; }
            set
            {
                _movementType = value;
                switch (_movementType)
                {
                    case MovementType.Land:
                        LandUnit();
                        break;
                    case MovementType.Airborne:
                        AirborneUnit();
                        break;
                    case MovementType.Water:
                        WaterUnit();
                        break;
                    case MovementType.Amphibious:
                        AmphibiousUnit();
                        break;
                }
            }
        }
        MovementType _movementType;
        public int BaseMovementPoints { get; set; }
        public double BaseQuality
        {
            get { return _baseQuality; }
            set
            {
                _baseQuality = value;
                Quality = _baseQuality;
            }
        }
        private double _baseQuality;
        public Dictionary<BattleQualityModifier, double> BattleQualityModifiers { get; set; }
        public double Quality { get; set; }
        public int Quantity { get; private set; }
        public double Strength { get; set; }
        public double BattleStrength { get; set; }
        public double Size { get; set; }
        public List<QuantityChangeEvent> QuantityEvents { get; set; }
        public double Morale { get; set; }
        public List<MoraleChangeEvent> MoraleEvents { get; set; }
        public double CombatInitiative { get; set; }
        public int Speed { get; set; }
        public bool IsAlive { get; private set; }

        public Dictionary<TerrainType, double> TerrainTypeBattleModifier { get; set; }
        public Dictionary<Weather, double> WeatherBattleModifier { get; set; }
        public Dictionary<UnitType, double> OpponentUnitTypeBattleModifier { get; set; }

        public TerrainType CanMoveOver;
        public TerrainType MayStopOn;
        public EdgeType CanMoveOverEdge;

        public Dictionary<TerrainType, int?> TerrainMovementCosts { get; set; }
        public Dictionary<EdgeType, int?> EdgeMovementCosts { get; set; }

        public bool IsTransporter { get; set; }
        public bool IsTransporting { get; set; }

        public int TurnCreated { get; set; }

        public override string ToString()
        {
            return Name + " (" + Strength + ") at " + Tile.ToString();
        }
        public MilitaryUnit(int index = 0, string name = null, int ownerIndex = 0, Tile tile = null, MovementType movementType = MovementType.Land, int baseMovementPoints = 2, int roadMovementBonus = 0, UnitType unitType = UnitType.Melee, double baseQuality = 1, int initialQuantity = 100, double size = 1, int combatInitiative = 10, double initialMorale = 5, int turnBuilt = 0)
        {
            IsAlive = true;
            BattleQualityModifiers = new Dictionary<BattleQualityModifier, double>();
            foreach (BattleQualityModifier battleQualityModifier in Enum.GetValues(typeof(BattleQualityModifier)))
            {
                BattleQualityModifiers.Add(battleQualityModifier, 0);
            }

            TerrainTypeBattleModifier = new Dictionary<TerrainType, double>();
            foreach (TerrainType terrainType in Enum.GetValues(typeof(TerrainType)))
            {
                TerrainTypeBattleModifier.Add(terrainType, 0);
            }

            WeatherBattleModifier = new Dictionary<Weather, double>();
            foreach (Weather weather in Enum.GetValues(typeof(Weather)))
            {
                WeatherBattleModifier.Add(weather, 0);
            }

            OpponentUnitTypeBattleModifier = new Dictionary<UnitType, double>();
            foreach (UnitType unitTypeEnum in Enum.GetValues(typeof(UnitType)))
            {
                OpponentUnitTypeBattleModifier.Add(unitTypeEnum, 0);
            }

            TerrainMovementCosts = new Dictionary<TerrainType, int?>();
            foreach (TerrainType terrainType in Enum.GetValues(typeof(TerrainType)))
            {
                TerrainMovementCosts.Add(terrainType, null);
            }
            EdgeMovementCosts = new Dictionary<EdgeType, int?>();
            foreach (EdgeType edgeType in Enum.GetValues(typeof(EdgeType)))
            {
                EdgeMovementCosts.Add(edgeType, null);
            }

            Index = index;
            if (name == null)
            {
                name = "Unit " + index + " (owned by " + ownerIndex + ")";
            }
            Name = name;
            OwnerIndex = ownerIndex;
            Tile = tile;
            MovementType = movementType;
            BaseMovementPoints = baseMovementPoints;
            RoadMovementBonus = roadMovementBonus;
            UnitType = unitType;
            BaseQuality = baseQuality;
            Size = size;
            CombatInitiative = combatInitiative;
            TurnCreated = turnBuilt;

            InitialMorale = initialMorale;
            InitialQuantity = initialQuantity;
        }

        public int InitialQuantity
        {
            get { return _initialQuantity; }
            set
            {
                _initialQuantity = value;
                Quantity = _initialQuantity;
                QuantityEvents = new List<QuantityChangeEvent>();
            }
        }
        int _initialQuantity;

        public double InitialMorale
        {
            get { return _initialMorale; }
            set
            {
                MoraleEvents = new List<MoraleChangeEvent>();
                _initialMorale = value;
                ChangeMorale(TurnCreated, _initialMorale);
            }
        }
        double _initialMorale;



        public void CalculateStrength()
        {
            Strength = Quality * Quantity;

            var battleQuality = Quality + BattleQualityModifiers.Values.Sum();
            if (battleQuality < .1)
                battleQuality = .1;

            BattleStrength = battleQuality * Quantity;
        }

        public void ChangeQuantity(int turn, int quantity)
        {
            QuantityEvents.Add(new QuantityChangeEvent { Turn = turn, Quantity = quantity });
            Quantity += quantity;

            if (Quantity <= 0)
            {
                IsAlive = false;
                Strength = BattleStrength = 0;
                return;
            }

            CalculateStrength();
        }

        public void ChangeMorale(int turn, double morale)
        {
            Morale += morale;
            MoraleEvents.Add(new MoraleChangeEvent { Turn = turn, Morale = morale });
        }


        public static Func<MilitaryUnit, MilitaryUnit, bool> IsInConflictDuringMovement = (p, o) => p.OwnerIndex != o.OwnerIndex && p.Tile == o.Tile && p.MovementType == o.MovementType;

        void LandUnit()
        {
            TerrainMovementCosts[TerrainType.Grassland] = 1;
            TerrainMovementCosts[TerrainType.Steppe] = 2;
            TerrainMovementCosts[TerrainType.Forest] = 2;
            TerrainMovementCosts[TerrainType.Hill] = 2;
            TerrainMovementCosts[TerrainType.Wetland] = 2;

            EdgeMovementCosts[EdgeType.Normal] = 0;
            EdgeMovementCosts[EdgeType.Road] = 1;
            EdgeMovementCosts[EdgeType.Bridge] = 1;
            EdgeMovementCosts[EdgeType.Forest] = 1;
            EdgeMovementCosts[EdgeType.Hill] = 1;

            RoadMovementBonus = 1;

            CanMoveOver = Terrain.Non_Mountainous_Land;
            CanMoveOverEdge = EdgeType.Road | EdgeType.Bridge | EdgeType.Forest | EdgeType.Hill;
            MayStopOn = Terrain.Non_Mountainous_Land;
        }

        void AmphibiousUnit()
        {
            TerrainMovementCosts[TerrainType.Grassland] = 1;
            TerrainMovementCosts[TerrainType.Steppe] = 2;
            TerrainMovementCosts[TerrainType.Forest] = 2;
            TerrainMovementCosts[TerrainType.Hill] = 2;
            TerrainMovementCosts[TerrainType.Wetland] = 1;

            EdgeMovementCosts[EdgeType.Normal] = 0;
            EdgeMovementCosts[EdgeType.River] = 0;
            EdgeMovementCosts[EdgeType.Road] = 1;
            EdgeMovementCosts[EdgeType.Bridge] = 1;
            EdgeMovementCosts[EdgeType.Forest] = 1;
            EdgeMovementCosts[EdgeType.Hill] = 1;

            CanMoveOver = Terrain.Non_Mountainous_Land;
            CanMoveOverEdge = EdgeType.Road | EdgeType.River;
            MayStopOn = Terrain.Non_Mountainous_Land;
        }

        void AirborneUnit()
        {
            TerrainMovementCosts[TerrainType.Grassland] = 1;
            TerrainMovementCosts[TerrainType.Steppe] = 1;
            TerrainMovementCosts[TerrainType.Forest] = 1;
            TerrainMovementCosts[TerrainType.Hill] = 1;
            TerrainMovementCosts[TerrainType.Mountain] = 1;
            TerrainMovementCosts[TerrainType.Water] = 1;
            TerrainMovementCosts[TerrainType.Wetland] = 1;
            TerrainMovementCosts[TerrainType.Reef] = 1;

            CanMoveOver = Terrain.All_Terrain;
            CanMoveOverEdge = Edge.All_Edges;
            MayStopOn = Terrain.Non_Mountainous_Land;
        }

        void WaterUnit()
        {
            TerrainMovementCosts[TerrainType.Water] = 1;
            TerrainMovementCosts[TerrainType.Reef] = 2;

            EdgeMovementCosts[EdgeType.Normal] = 0;
            EdgeMovementCosts[EdgeType.Reef] = 1;

            CanMoveOver = Terrain.All_Water;
            CanMoveOverEdge = EdgeType.Normal;
            MayStopOn = Terrain.All_Water;
        }
        //public TerrainType StopOn;
        //public TerrainType TerrainCombatBonus;


        //public IEnumerable<Tile> ValidAdjacentMoves { get { return Tile.AdjacentTiles.Where(x => TerrainMovementCosts[x.TerrainType] != null) ; } }



        public Tile Tile
        {
            get { return _tile; }
            set
            {
                if (_tile != null)
                    _tile.Units.Remove(this);

                _tile = value;
                if (_tile != null)
                    _tile.Units.Add(this);
            }
        }
        Tile _tile;

        public override int GetHashCode()
        {
            return Index;
        }

        public int MovementPoints
        {
            get
            {
                if (Morale / InitialMorale < .5)
                {
                    return BaseMovementPoints - 1;
                }

                return BaseMovementPoints;
            }
        }


        public bool CanTransport(MilitaryUnit transportee)
        {
            if (!IsTransporter)
                return false;

            return TransportSize >= transportee.TransportSize;
        }

        public int TransportSize
        {
            get { return (int)Math.Ceiling(Quantity * Size); }
        }

        public ArgbColour UnitColour
        {
            get
            {
                return IsAlive ? Player.Colour(OwnerIndex) : Colours.Black;
            }
        }

        public int RoadMovementBonus { get; set; }
        public double StructureBattleModifier { get; set; }

        

        public IEnumerable<Move> PossibleMoves()
        {
            var possibleMoves = new List<Move>();

            var movesConsidered = new List<Move>();

            possibleMoves = GenerateStandardMoves(this, Tile, null, movesConsidered, MovementPoints, 1);

            if (RoadMovementBonus > 0)
            {
                var roadMovesAlreadyConsidered = new List<Move>();
                var roadMoves = GenerateRoadBonusMoves(this, Tile, roadMovesAlreadyConsidered, MovementPoints + RoadMovementBonus);
                var notAlreadySeenRoadMoves = roadMoves.Where(x => !possibleMoves.Any(y => x.Origin == y.Origin && x.Destination == y.Destination));
                possibleMoves.AddRange(notAlreadySeenRoadMoves);
            }

            return possibleMoves;
        }

        private static List<Move> GenerateStandardMoves(MilitaryUnit unit, Tile tile, Move previousMove, List<Move> movesConsidered, int movementPoints, int distance)
        {
            var potentialMoves = new List<Move>();

            potentialMoves.AddRange(tile.Neighbours.Where(dest => dest != unit.Tile
                                        && !movesConsidered.Any(x => x.Origin == tile && x.Destination == dest && x.MovesRemaining > movementPoints)
                                        && (unit.MovementType == MovementType.Airborne 
                                                || (GetEdge(tile, dest).BaseEdgeType == BaseEdgeType.CentreToCentre && unit.EdgeMovementCosts[GetEdge(tile, dest).EdgeType] != null || (unit.EdgeMovementCosts[GetEdge(tile, dest).EdgeType] != null && unit.TerrainMovementCosts[dest.TerrainType] != null)))
                                        ).Select(x => new Move(tile, x, previousMove, movementPoints, distance))
                                        .ToList());

            movesConsidered.AddRange(potentialMoves);

            var neighbourMoves = new List<Move>();

            potentialMoves.ForEach(x => {
                var edge = GetEdge(x.Origin, x.Destination);
                var newMovementPoints = 0;
                if (unit.MovementType == MovementType.Airborne)
                {
                    newMovementPoints = movementPoints - (int)unit.TerrainMovementCosts[x.Destination.TerrainType];
                }
                else if (edge.BaseEdgeType == BaseEdgeType.CentreToCentre && unit.EdgeMovementCosts[edge.EdgeType] != null)
                {
                    newMovementPoints = movementPoints - (int)unit.EdgeMovementCosts[edge.EdgeType];
                }
                else if (unit.EdgeMovementCosts[edge.EdgeType] != null)
                    newMovementPoints = movementPoints - (int)unit.TerrainMovementCosts[x.Destination.TerrainType] - (int)unit.EdgeMovementCosts[edge.EdgeType];                    

                if (newMovementPoints > 0)
                {
                    neighbourMoves.AddRange(GenerateStandardMoves(unit, x.Destination, x, movesConsidered, newMovementPoints, distance + 1));
                }

            });

            potentialMoves.AddRange(neighbourMoves);

            return potentialMoves.Where(x => unit.MayStopOn.HasFlag(x.Destination.TerrainType)).ToList();

        }

        private static List<Move> GenerateRoadBonusMoves(MilitaryUnit unit, Tile tile, List<Move> movesConsidered, int movementPoints)
        {
            var roadMoves = tile.NeighbourEdges.Where(x => x.BaseEdgeType == BaseEdgeType.CentreToCentre && !movesConsidered.Any(y => (x.Tiles[0] == tile && x.Tiles[1] == y.Destination) || (x.Tiles[1] == tile && x.Tiles[0] == y.Destination)))
                .Select(x => new Move(x.Tiles[0], x.Tiles[1])).ToList();

            movesConsidered.AddRange(roadMoves);

            var neighbourMoves = new List<Move>();

            foreach (var roadMove in roadMoves)
            {
                var cost = movementPoints - 1;
                if (cost > 0)
                    neighbourMoves.AddRange(GenerateRoadBonusMoves(unit, roadMove.Destination, movesConsidered, cost));
            }

            roadMoves.AddRange(neighbourMoves);

            return roadMoves;
        }

        private static Edge GetEdge(Tile origin, Tile destination)
        {
            return origin.NeighbourEdges.Single(y => y.Tiles.Contains(destination));
        }
    }
}

