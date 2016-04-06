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
    }
    public class MilitaryUnit
    {
        public int Id;
        public string Name { get; set; }
        public int OwnerId { get; set; }
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

        public bool IsTransporter { get; set; }
        public bool IsTransporting { get; set; }

        public int TurnCreated { get; set; }

        public MilitaryUnit(string name = "Unamed unit", int ownerId = 1, MovementType movementType = MovementType.Land, int baseMovementPoints = 2, UnitType unitType = UnitType.Melee, double baseQuality = 1, int initialQuantity = 100, double size = 1, int combatInitiative = 10, double initialMorale = 5, int turn = 0)
        {
            IsAlive = true;
            BattleQualityModifiers = new Dictionary<BattleQualityModifier, double>
            {
                { BattleQualityModifier.Terrain, 0 },
                { BattleQualityModifier.Weather, 0 },
                { BattleQualityModifier.UnitType, 0 },
            };
            TerrainTypeBattleModifier = new Dictionary<TerrainType, double>();
            WeatherBattleModifier = new Dictionary<Weather, double>();
            OpponentUnitTypeBattleModifier = new Dictionary<UnitType, double>();

            TerrainMovementCosts = new Dictionary<TerrainType, int?>();
            foreach (TerrainType terrainType in Enum.GetValues(typeof(TerrainType)))
            {
                TerrainMovementCosts.Add(terrainType, null);
            }

            Name = name;
            OwnerId = ownerId;
            MovementType = movementType;
            BaseMovementPoints = baseMovementPoints;
            UnitType = unitType;
            BaseQuality = baseQuality;
            Size = size;
            CombatInitiative = combatInitiative;
            TurnCreated = turn;

            InitialMorale = initialMorale;
            InitialQuantity = initialQuantity;
        }

        public int InitialQuantity
        {
            set
            {
                QuantityEvents = new List<QuantityChangeEvent>();
                ChangeQuantity(TurnCreated, value);
            }
        }

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


        public static Func<MilitaryUnit, MilitaryUnit, bool> IsInConflictDuringMovement = (p, o) => p.OwnerId != o.OwnerId && p.Tile == o.Tile && p.MovementType == o.MovementType;

        void LandUnit()
        {
            TerrainMovementCosts[TerrainType.Grassland] = 1;
            TerrainMovementCosts[TerrainType.Desert] = 2;
            TerrainMovementCosts[TerrainType.Forest] = 2;
            TerrainMovementCosts[TerrainType.Hill] = 2;
            TerrainMovementCosts[TerrainType.Wetland] = 2;

            CanMoveOver = Terrain.Non_Mountainous_Land;
            CanMoveOverEdge = EdgeType.Road | EdgeType.Forest | EdgeType.Hill;
            MayStopOn = Terrain.Non_Mountainous_Land;
        }

        void AmphibiousUnit()
        {
            TerrainMovementCosts[TerrainType.Grassland] = 1;
            TerrainMovementCosts[TerrainType.Desert] = 2;
            TerrainMovementCosts[TerrainType.Forest] = 2;
            TerrainMovementCosts[TerrainType.Hill] = 2;
            TerrainMovementCosts[TerrainType.Wetland] = 1;

            CanMoveOver = Terrain.Non_Mountainous_Land;
            CanMoveOverEdge = EdgeType.Road | EdgeType.River;
            MayStopOn = Terrain.Non_Mountainous_Land;
        }

        void AirborneUnit()
        {
            TerrainMovementCosts[TerrainType.Grassland] = 1;
            TerrainMovementCosts[TerrainType.Desert] = 1;
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

            CanMoveOver = Terrain.Aquatic_Terrain;
            CanMoveOverEdge = EdgeType.Normal;
            MayStopOn = Terrain.Aquatic_Terrain;
        }        
        //public TerrainType StopOn;
        public TerrainType TerrainCombatBonus;


        //public IEnumerable<Tile> ValidAdjacentMoves { get { return Tile.AdjacentTiles.Where(x => TerrainMovementCosts[x.TerrainType] != null) ; } }



        public Tile Tile 
        { 
            get { return _tile; } 
            set 
            {
                if (_tile != null)
                    _tile.Units.Remove(this);

                _tile = value;
                _tile.Units.Add(this);
            } 
        }
        Tile _tile;

        public override int GetHashCode()
        {
            return Id;
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

        public static IEnumerable<Move> MoveList(MilitaryUnit unit)
        {
            var moveList = new List<Move>();
            var exploringMoves = new List<Move> { new Move(null, unit.Tile) };
            var movesTaken = 0;

            while (movesTaken < unit.MovementPoints)
            {
                var moves = GenerateMoves(unit, moveList, exploringMoves);
                var validDestinations = moves.Where(x => unit.MayStopOn.HasFlag(x.Destination.TerrainType)).ToList();
                moveList.AddRange(validDestinations);
                exploringMoves = moves.Where(x => movesTaken + unit.TerrainMovementCosts[x.Destination.TerrainType] < unit.MovementPoints || Move.IsAllOnRoad(x)).ToList();

                movesTaken++;
            }

            // If all moves have been on road, you get a bonus move
            if (unit.CanMoveOverEdge.HasFlag(EdgeType.Road))
            {
                var roadsToExplore = exploringMoves.Where(x => Move.IsAllOnRoad(x)).ToList();
                var roadMoves = GenerateMoves(unit, moveList, roadsToExplore);
                var validRoadMoves = roadMoves.Where(x => unit.MayStopOn.HasFlag(x.Destination.TerrainType) && Move.IsAllOnRoad(x)).ToList();
                moveList.AddRange(validRoadMoves);
            }
            
            return moveList;
        }

        private static List<Move> GenerateMoves(MilitaryUnit unit, List<Move> moveList, List<Move> exploringMoves)
        {
            var potentialMoves = new List<Move>();

            foreach (var exploringMove in exploringMoves)
            {
                var origin = exploringMove.Destination;

                var destinations = origin.AdjacentTiles.Where(dest => dest != unit.Tile
                                        && !potentialMoves.Any(x => x.Destination == dest)
                                        && !moveList.Any(x => x.Destination == dest)
                                        && CanCrossEdge(unit.CanMoveOverEdge, origin, dest)).ToList();

                destinations.ForEach(dest => potentialMoves.Add(new Move(exploringMove, dest)));
            }

            return potentialMoves.Where(x => unit.CanMoveOver.HasFlag(x.Destination.TerrainType)).ToList();
        }

        private static bool CanCrossEdge(EdgeType moveOverEdge, Tile tile, Tile adjacentTile)
        {
            var adjacentTileEdge = tile.AdjacentTileEdges.SingleOrDefault(edge => edge.Tiles.Any(x => x.Id == adjacentTile.Id));

            if (adjacentTileEdge == null)
                return true;

            if (moveOverEdge.HasFlag(adjacentTileEdge.EdgeType))
                return true;

            if (adjacentTileEdge.EdgeType.HasFlag(EdgeType.River | EdgeType.Road))
            {
                if (moveOverEdge.HasFlag(EdgeType.Road))
                    return true;
            }

            return false;
        }
    }
}
