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
        Transport,
        Embark,
        Disembark,
    }

    public struct RoleMovementType
    {
        public MovementType MovementType;
        public Role Role;

        public RoleMovementType(MovementType movementType, Role role)
        {
            MovementType = movementType;
            Role = role;
        }
    }
    public class MilitaryUnit
    {

        public static List<MovementType> MovementTypes
        {
            get
            {
                if (_movementTypes == null)
                {
                    _movementTypes = new List<MovementType>();
                    foreach (var role in Enum.GetValues(typeof(MovementType)))
                    {
                        _movementTypes.Add((MovementType)role);
                    }
                }
                return _movementTypes;
            }
        }
        static List<MovementType> _movementTypes;

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
        public double Morale { get; private set; }
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
        public List<MilitaryUnit> Transporting { get; set; }

        public int TurnCreated { get; set; }

        public Role Role { get; set; }

        public StrategicAction StrategicAction { get; set; }
        public Tile StrategicDestination { get; set; }
        public override string ToString()
        {
            return Name + " (" + Strength + ") at " + Location.ToString();
        }
        public MilitaryUnit(int index = 0, string name = null, int ownerIndex = 0, Tile location = null, MovementType movementType = MovementType.Land, int baseMovementPoints = 2, int roadMovementBonus = 0, UnitType unitType = UnitType.Melee, double baseQuality = 1, int initialQuantity = 100, double size = 1, bool isTransporter = false, List<MovementType> transportableBy = null, int combatInitiative = 10, double initialMorale = 5, int turnBuilt = 0, bool isAmphibious = false, Role role = Role.Balanced, StrategicAction strategicAction = StrategicAction.None, float[] moraleMoveCost = null)
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
            TransportableBy = transportableBy;
            if (TransportableBy == null)
            {
                TransportableBy = new List<MovementType>();
            }
            Transporting = new List<MilitaryUnit>();

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

            if (moraleMoveCost == null)
            {
                MoraleMoveCost = new float[BaseMovementPoints];
                for (var i = 0; i < BaseMovementPoints; i++)
                    MoraleMoveCost[i] = 0;
            }
            else
            {
                MoraleMoveCost = moraleMoveCost;
            }

            if (IsAmphibious)
            {
                TerrainMovementCosts[TerrainType.Wetland] = 1;
                EdgeMovementCosts[EdgeType.River] = 0;
            }

            RoadMovementBonus = roadMovementBonus;
            Role = role;
            StrategicAction = strategicAction;
            RoleMovementType = new RoleMovementType(MovementType, Role);

            CalculateStrength();
        }

        public RoleMovementType RoleMovementType { get; set; }

        public float[] MoraleMoveCost { get; set; }

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
                ChangeMorale(TurnCreated, _initialMorale, "Initial morale");
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

        public void ChangeMorale(int turn, double moraleChange, string reason)
        {
            Morale += moraleChange;
            MoraleEvents.Add(new MoraleChangeEvent(turn, moraleChange, reason));
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
                var movementPoints = BaseMovementPoints;

                if (MovementType == MovementType.Airborne && Transporting.Any())
                {
                    return movementPoints -= 1;
                }
                if (Morale / InitialMorale < .5)
                {
                    return movementPoints -= 1;
                }

                return movementPoints;
            }
        }


        public bool CanTransport(MilitaryUnit transportee)
        {
            if (!IsTransporter)
                throw new Exception("Only transporters can transport units");

            if (!transportee.TransportableBy.Contains(MovementType))
                throw new Exception($"{transportee.Name} may not be transported by {MovementType} movement type");

            return TransportSize >= transportee.TransportSize + Transporting.Sum(x => x.TransportSize);
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
        public MilitaryUnit TransportedBy { get; set; }
        public List<MovementType> TransportableBy { get; private set; }

        public IEnumerable<Move> PossibleMoves()
        {
            var possibleMoves = new List<Move>();

            var movesConsidered = new List<Move>();

            possibleMoves = GenerateStandardMoves(this, Location, null, movesConsidered, MovementPoints, 1);

            if (MovementType == MovementType.Land && TransportedBy == null)
            {
                var roadMovesAlreadyConsidered = new List<Move>();
                var roadMoves = GenerateRoadMoves(this, Location, null, roadMovesAlreadyConsidered, MovementPoints + RoadMovementBonus, 1);
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
                                                        && (unit.TerrainMovementCosts[dest.TerrainType] != null 
                                                                    || Edge.GetEdge(origin, dest).BaseEdgeType == BaseEdgeType.CentreToCentre
                                                                    || (Edge.GetEdge(origin, dest).EdgeType == EdgeType.Port && unit.StrategicAction == StrategicAction.Embark)
                                                           )
                                                        && (unit.TransportedBy == null || Edge.GetEdge(origin, dest).EdgeType == EdgeType.Port))
                                        ).Select(x => new Move(origin, x, previousMove, movementPoints, distance))
                                        .ToList());

            movesConsidered.AddRange(potentialMoves);

            var neighbourMoves = new List<Move>();

            potentialMoves.ForEach(x =>
            {
                var remainingMovementPoints = movementPoints - x.Origin.CalculateMoveCost(unit, x.Destination);

                if (remainingMovementPoints > 0)
                {
                    neighbourMoves.AddRange(GenerateStandardMoves(unit, x.Destination, x, movesConsidered, remainingMovementPoints, distance + 1));
                }
            });

            potentialMoves.AddRange(neighbourMoves);

            return potentialMoves.Where(x => unit.CanStopOn.HasFlag(x.Destination.TerrainType) || (x.Edge.EdgeType == EdgeType.Port && unit.MovementType == MovementType.Land)).ToList();

        }

        private static List<Move> GenerateRoadMoves(MilitaryUnit unit, Tile tile, Move previousMove, List<Move> movesConsidered, int movementPoints, int distance)
        {
            var moves = tile.Edges.Where(x => x.BaseEdgeType == BaseEdgeType.CentreToCentre 
                                                            && !movesConsidered.Any(y => Edge.CrossesEdge(x, tile, y.Destination))
                                                     )
                .Select(x => new Move(x.Origin, x.Destination, x, previousMove, movementPoints, distance, true)).ToList();

            movesConsidered.AddRange(moves);

            var neighbourMoves = new List<Move>();

            foreach (var move in moves)
            {
                var cost = movementPoints - 1;
                if (cost > 0)
                    neighbourMoves.AddRange(GenerateRoadMoves(unit, move.Destination, move, movesConsidered, cost, distance + 1));
            }

            moves.AddRange(neighbourMoves);

            return moves;
        }

        public MoveOrder GetMoveOrderToDestination(Point destination, Board board)
        {
            var pathFindTiles = board.ValidMovesWithMoveCostsForUnit(this);
            var shortestPath = ComputerPlayer.FindShortestPath(pathFindTiles, Location.Point, destination).ToArray();

            return ShortestPathToMoveOrder(shortestPath);
        }

        public MoveOrder ShortestPathToMoveOrder(PathFindTile[] shortestPath)
        {
            var move = ComputerPlayer.MoveOrderFromShortestPath(PossibleMoves().ToList(), shortestPath);

            if (move == null)
                return null;

            var moveOrders = move.GetMoveOrder(this);
            return moveOrders;
        }
    }
}

