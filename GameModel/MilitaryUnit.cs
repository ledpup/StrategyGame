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
        Airborne,
        Land,
        Water,
    }

    public enum BattleQualityModifier
    {
        Terrain,
        Weather,
        UnitType,
        Structure,
    }

    public enum Role
    {
        Balanced,
        Besieger,
        Offensive,
        Defensive,
        Scout,
    }

    public enum StrategicAction
    {
        None,
        Dock,
        Embark,
    }
    public class MilitaryUnit
    {

        public static List<Role> Roles
        {
            get
            {
                if (_roles == null)
                {
                    _roles = new List<Role>();
                    foreach (var role in Enum.GetValues(typeof(Role)))
                    {
                        _roles.Add((Role)role);
                    }
                }
                return _roles;
            }
        }
        static List<Role> _roles;

        public int Index;
        public string Name { get; set; }
        public int OwnerIndex { get; set; }
        public UnitType UnitType { get; set; }
        public MovementType MovementType { get; private set; }
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

        public bool IsAmphibious { get; set; }
        public Dictionary<TerrainType, double> TerrainTypeBattleModifier { get; set; }
        public Dictionary<Weather, double> WeatherBattleModifier { get; set; }
        public Dictionary<UnitType, double> OpponentUnitTypeBattleModifier { get; set; }

        public TerrainType CanStopOn;

        public Dictionary<TerrainType, int?> TerrainMovementCosts { get; set; }
        public Dictionary<EdgeType, int?> EdgeMovementCosts { get; set; }

        public bool IsTransporter { get; set; }
        public bool IsTransporting { get; set; }

        public int TurnCreated { get; set; }

        public Role Role { get; set; }

        public StrategicAction StrategicAction { get; set; }
        public Tile StrategicDestination { get; set; }
        public override string ToString()
        {
            return Name + " (" + Strength + ") at " + Location.ToString();
        }
        public MilitaryUnit(int index = 0, string name = null, int ownerIndex = 0, Tile location = null, MovementType movementType = MovementType.Land, int baseMovementPoints = 2, int roadMovementBonus = 0, UnitType unitType = UnitType.Melee, double baseQuality = 1, int initialQuantity = 100, double size = 1, bool isTransporter = false, int combatInitiative = 10, double initialMorale = 5, int turnBuilt = 0, bool isAmphibious = false, Role role = Role.Balanced, StrategicAction strategicAction = StrategicAction.None)
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
            Location = location;
            MovementType = movementType;
            BaseMovementPoints = baseMovementPoints;
            UnitType = unitType;
            BaseQuality = baseQuality;
            Size = size;
            IsTransporter = isTransporter;
            CombatInitiative = combatInitiative;
            TurnCreated = turnBuilt;

            InitialMorale = initialMorale;
            InitialQuantity = initialQuantity;
            IsAmphibious = isAmphibious;

            switch (MovementType)
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
            }

            if (IsAmphibious)
            {
                TerrainMovementCosts[TerrainType.Wetland] = 1;
                EdgeMovementCosts[EdgeType.River] = 0;
            }

            RoadMovementBonus = roadMovementBonus;
            Role = role;
            StrategicAction = strategicAction;

            CalculateStrength();
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


        public static Func<MilitaryUnit, MilitaryUnit, bool> IsInConflictDuringMovement = (p, o) => p.OwnerIndex != o.OwnerIndex && p.Location == o.Location && p.MovementType == o.MovementType;

        void LandUnit()
        {
            TerrainMovementCosts[TerrainType.Grassland] = 1;
            TerrainMovementCosts[TerrainType.Steppe] = 2;
            TerrainMovementCosts[TerrainType.Forest] = 2;
            TerrainMovementCosts[TerrainType.Hill] = 2;
            TerrainMovementCosts[TerrainType.Wetland] = 2;

            EdgeMovementCosts[EdgeType.Normal] = 0;
            EdgeMovementCosts[EdgeType.Road] = 0;
            EdgeMovementCosts[EdgeType.Bridge] = 0;
            EdgeMovementCosts[EdgeType.Forest] = 1;
            EdgeMovementCosts[EdgeType.Hill] = 1;
            EdgeMovementCosts[EdgeType.Port] = 1;

            CanStopOn = Terrain.Non_Mountainous_Land;
        }

        void AirborneUnit()
        {
            TerrainMovementCosts[TerrainType.Grassland] = 1;
            TerrainMovementCosts[TerrainType.Steppe] = 1;
            TerrainMovementCosts[TerrainType.Forest] = 1;
            TerrainMovementCosts[TerrainType.Hill] = 1;
            TerrainMovementCosts[TerrainType.Mountain] = 1;
            TerrainMovementCosts[TerrainType.Wetland] = 1;

            TerrainMovementCosts[TerrainType.Water] = 1;
            TerrainMovementCosts[TerrainType.Reef] = 1;

            EdgeMovementCosts[EdgeType.Normal] = 0;
            EdgeMovementCosts[EdgeType.Road] = 0;
            EdgeMovementCosts[EdgeType.Bridge] = 0;
            EdgeMovementCosts[EdgeType.Forest] = 0;
            EdgeMovementCosts[EdgeType.Hill] = 0;
            EdgeMovementCosts[EdgeType.Port] = 0;
            EdgeMovementCosts[EdgeType.River] = 0;
            EdgeMovementCosts[EdgeType.Reef] = 0;

            CanStopOn = Terrain.Non_Mountainous_Land;
        }

        void WaterUnit()
        {
            TerrainMovementCosts[TerrainType.Water] = 1;
            TerrainMovementCosts[TerrainType.Reef] = 2;

            EdgeMovementCosts[EdgeType.Normal] = 0;
            EdgeMovementCosts[EdgeType.Reef] = 1;

            CanStopOn = Terrain.All_Water;
        }
        //public TerrainType StopOn;
        //public TerrainType TerrainCombatBonus;


        //public IEnumerable<Tile> ValidAdjacentMoves { get { return Tile.AdjacentTiles.Where(x => TerrainMovementCosts[x.TerrainType] != null) ; } }



        public Tile Location
        {
            get { return _location; }
            set
            {
                if (_location != null)
                    _location.Units.Remove(this);

                _location = value;
                if (_location != null)
                    _location.Units.Add(this);
            }
        }
        Tile _location;

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

            possibleMoves = GenerateStandardMoves(this, Location, null, movesConsidered, MovementPoints, 1);

            if (RoadMovementBonus > 0)
            {
                var roadMovesAlreadyConsidered = new List<Move>();
                var roadMoves = GenerateRoadMoves(this, Location, null, roadMovesAlreadyConsidered, MovementPoints + RoadMovementBonus);
                var notAlreadySeenRoadMoves = roadMoves.Where(x => !possibleMoves.Any(y => x.Origin == y.Origin && x.Destination == y.Destination));
                possibleMoves.AddRange(notAlreadySeenRoadMoves);
            }

            return possibleMoves;
        }

        private static List<Move> GenerateStandardMoves(MilitaryUnit unit, Tile origin, Move previousMove, List<Move> movesConsidered, int movementPoints, int distance)
        {
            var potentialMoves = new List<Move>();

            potentialMoves.AddRange(origin.Neighbours.Where(dest => dest != unit.Location
                                        && !movesConsidered.Any(x => x.Origin == origin && x.Destination == dest && x.MovesRemaining > movementPoints)
                                        && (unit.EdgeMovementCosts[Edge.GetEdge(origin, dest).EdgeType] != null 
                                                        && (unit.TerrainMovementCosts[dest.TerrainType] != null || Edge.GetEdge(origin, dest).BaseEdgeType == BaseEdgeType.CentreToCentre || Edge.GetEdge(origin, dest).EdgeType == EdgeType.Port))
                                        ).Select(x => new Move(origin, x, previousMove, movementPoints, distance))
                                        .ToList());

            movesConsidered.AddRange(potentialMoves);

            var neighbourMoves = new List<Move>();

            potentialMoves.ForEach(x => {
                var edge = Edge.GetEdge(x.Origin, x.Destination);
                var newMovementPoints = 0;
                if (unit.TerrainMovementCosts[x.Destination.TerrainType] != null && unit.EdgeMovementCosts[edge.EdgeType] != null)
                {
                    if (unit.EdgeMovementCosts[edge.EdgeType] == 0)
                    {
                        newMovementPoints = movementPoints - 1;
                    }
                    else
                    {
                        newMovementPoints = movementPoints - (int)unit.TerrainMovementCosts[x.Destination.TerrainType] - (int)unit.EdgeMovementCosts[edge.EdgeType];
                    }
                    if (newMovementPoints == movementPoints)
                        throw new Exception("There always needs to be a cost to moving from one tile to another");
                }
                else if (unit.TerrainMovementCosts[x.Destination.TerrainType] == null && unit.EdgeMovementCosts[edge.EdgeType] != null)
                {
                    newMovementPoints = movementPoints - (unit.EdgeMovementCosts[edge.EdgeType] == 0 ? 1 : (int)unit.EdgeMovementCosts[edge.EdgeType]);
                    if (newMovementPoints == movementPoints)
                        throw new Exception("There always needs to be a cost to moving from one tile to another");
                } 
                else
                {
                    newMovementPoints = movementPoints - 1;
                }



                if (newMovementPoints > 0)
                {
                    neighbourMoves.AddRange(GenerateStandardMoves(unit, x.Destination, x, movesConsidered, newMovementPoints, distance + 1));
                }

            });

            potentialMoves.AddRange(neighbourMoves);

            return potentialMoves.Where(x => unit.CanStopOn.HasFlag(x.Destination.TerrainType) || x.Edge.EdgeType == EdgeType.Port).ToList();

        }

        private static List<Move> GenerateRoadMoves(MilitaryUnit unit, Tile tile, Move previousMove, List<Move> movesConsidered, int movementPoints)
        {
            var moves = tile.Edges.Where(x => x.BaseEdgeType == BaseEdgeType.CentreToCentre 
                                                            && !movesConsidered.Any(y => Edge.CrossesEdge(x, tile, y.Destination))
                                                     )
                .Select(x => new Move(x.Origin, x.Destination, x, previousMove)).ToList();

            movesConsidered.AddRange(moves);

            var neighbourMoves = new List<Move>();

            foreach (var move in moves)
            {
                var cost = movementPoints - 1;
                if (cost > 0)
                    neighbourMoves.AddRange(GenerateRoadMoves(unit, move.Destination, move, movesConsidered, cost));
            }

            moves.AddRange(neighbourMoves);

            return moves;
        }


    }
}

