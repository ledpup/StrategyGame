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
        public Dictionary<EdgeType, int?> EdgeMovementCosts { get; set; }

        public bool IsTransporter { get; set; }
        public bool IsTransporting { get; set; }

        public int TurnCreated { get; set; }

        public MilitaryUnit(string name = "Unamed unit", int ownerId = 1, Tile location = null, MovementType movementType = MovementType.Land, int baseMovementPoints = 2, UnitType unitType = UnitType.Melee, double baseQuality = 1, int initialQuantity = 100, double size = 1, int combatInitiative = 10, double initialMorale = 5, int turn = 0)
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
            EdgeMovementCosts = new Dictionary<EdgeType, int?>();
            foreach (EdgeType edgeType in Enum.GetValues(typeof(EdgeType)))
            {
                EdgeMovementCosts.Add(edgeType, null);
            }

            Name = name;
            OwnerId = ownerId;
            Tile = location;
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
            TerrainMovementCosts[TerrainType.Steppe] = 2;
            TerrainMovementCosts[TerrainType.Forest] = 2;
            TerrainMovementCosts[TerrainType.Hill] = 2;
            TerrainMovementCosts[TerrainType.Wetland] = 2;

            EdgeMovementCosts[EdgeType.Normal] = 0;
            EdgeMovementCosts[EdgeType.Road] = 1;
            EdgeMovementCosts[EdgeType.Forest] = 1;
            EdgeMovementCosts[EdgeType.Hill] = 1;

            RoadMoveBonus = 1;

            CanMoveOver = Terrain.Non_Mountainous_Land;
            CanMoveOverEdge = EdgeType.Road | EdgeType.Forest | EdgeType.Hill;
            MayStopOn = Terrain.Non_Mountainous_Land;
        }

        void AmphibiousUnit()
        {
            TerrainMovementCosts[TerrainType.Grassland] = 1;
            TerrainMovementCosts[TerrainType.Steppe] = 2;
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

            CanMoveOver = Terrain.All_Water;
            CanMoveOverEdge = EdgeType.Normal;
            MayStopOn = Terrain.All_Water;
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
                if (_tile != null)
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

        public ArgbColour UnitColour
        {
            get
            {
                return OwnerId == 1 ? Colours.Red : Colours.Blue;
            }
        }

        public int RoadMoveBonus { get; set; }

        public IEnumerable<Move> GetPossibleMoveList()
        {
            return PossibleMoveList(this);
        }

        public static IEnumerable<Move> PossibleMoveList(MilitaryUnit unit)
        {
            var movesConsidered = new List<Move>();

            var moves = GenerateStandardMoves(unit, unit.Tile, movesConsidered, unit.MovementPoints);

            if (unit.RoadMoveBonus > 0)
            {
                var roadMovesAlreadyConsidered = new List<Move>();
                var roadMoves = GenerateRoadBonusMoves(unit, unit.Tile, roadMovesAlreadyConsidered, unit.MovementPoints + unit.RoadMoveBonus);
                var notAlreadySeenRoadMoves = roadMoves.Where(x => !moves.Any(y => x.Origin == y.Origin && x.Destination == y.Destination));
                moves.AddRange(notAlreadySeenRoadMoves);
            }

            return moves;
        }

        private static List<Move> GenerateStandardMoves(MilitaryUnit unit, Tile tile, List<Move> movesConsidered, int movementPoints)
        {
            var potentialMoves = new List<Move>();

            potentialMoves.AddRange(tile.Neighbours.Where(dest => dest != unit.Tile
                                        && !movesConsidered.Any(x => x.Origin == tile && x.Destination == dest && x.MovesRemaining > movementPoints)
                                        && (unit.MovementType == MovementType.Airborne || (GetEdge(tile, dest).EdgeType == EdgeType.Road && EdgeMovementCost(unit, tile, dest) != null || (EdgeMovementCost(unit, tile, dest) != null && unit.TerrainMovementCosts[dest.TerrainType] != null)))
                                        ).Select(x => new Move(tile, x, movementPoints))
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
                else if (edge.EdgeType == EdgeType.Road && unit.EdgeMovementCosts[edge.EdgeType] != null)
                {
                    newMovementPoints = movementPoints - (int)unit.EdgeMovementCosts[edge.EdgeType];
                }
                else if (unit.EdgeMovementCosts[edge.EdgeType] != null)
                    newMovementPoints = movementPoints - (int)unit.TerrainMovementCosts[x.Destination.TerrainType] - (int)unit.EdgeMovementCosts[edge.EdgeType];                    

                if (newMovementPoints > 0)
                {
                    neighbourMoves.AddRange(GenerateStandardMoves(unit, x.Destination, movesConsidered, newMovementPoints));
                }

            });

            potentialMoves.AddRange(neighbourMoves);

            return potentialMoves.Where(x => unit.MayStopOn.HasFlag(x.Destination.TerrainType)).ToList();

        }

        private static List<Move> GenerateRoadBonusMoves(MilitaryUnit unit, Tile tile, List<Move> movesConsidered, int movementPoints)
        {
            var roadMoves = tile.AdjacentTileEdges.Where(x => x.EdgeType == EdgeType.Road && !movesConsidered.Any(y => (x.Tiles[0] == tile && x.Tiles[1] == y.Destination) || (x.Tiles[1] == tile && x.Tiles[0] == y.Destination)))
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
            var edge = origin.AdjacentTileEdges.SingleOrDefault(y => y.Tiles.Contains(destination));
            if (edge == null)
                return new Edge("Normal", new Tile[] { origin, destination });
            return edge;
        }

        private static float? EdgeMovementCost(MilitaryUnit unit, Tile origin, Tile dest)
        {
            var edge = GetEdge(origin, dest);

            return unit.EdgeMovementCosts[edge.EdgeType];
        }

        //public static IEnumerable<Move> PossibleMoveList(MilitaryUnit unit)
        //{
        //    var moveList = new List<Move>();
        //    var exploringMoves = new List<Move> { new Move(null, unit.Tile) };
        //    var movesTaken = 0;

        //    while (movesTaken < unit.MovementPoints)
        //    {
        //        var moves = GenerateMoves(unit, moveList, exploringMoves);
        //        var validDestinations = moves.Where(x => unit.MayStopOn.HasFlag(x.Destination.TerrainType)).ToList();
        //        moveList.AddRange(validDestinations);
        //        exploringMoves = moves.Where(x => movesTaken + unit.TerrainMovementCosts[x.Destination.TerrainType] <= unit.MovementPoints).ToList();

        //        movesTaken++;
        //    }

        //    //// If all moves have been on road, you get a bonus move
        //    //if (unit.CanMoveOverEdge.HasFlag(EdgeType.Road))
        //    //{
        //    //    var roadsToExplore = exploringMoves.Where(x => Move.IsAllOnRoad(x)).ToList();
        //    //    var roadMoves = GenerateMoves(unit, moveList, roadsToExplore);
        //    //    var validRoadMoves = roadMoves.Where(x => unit.MayStopOn.HasFlag(x.Destination.TerrainType) && Move.IsAllOnRoad(x)).ToList();
        //    //    moveList.AddRange(validRoadMoves);
        //    //}

        //    return moveList;
        //}

        //private static List<Move> GenerateMoves(MilitaryUnit unit, List<Move> moveList, List<Move> exploringMoves)
        //{
        //    var potentialMoves = new List<Move>();

        //    foreach (var exploringMove in exploringMoves)
        //    {
        //        var origin = exploringMove.Destination;

        //        var destinations = origin.Neighbours.Where(dest => dest != unit.Tile
        //                                && !potentialMoves.Any(x => x.Destination == dest)
        //                                && !moveList.Any(x => x.Destination == dest)
        //                                && CanCrossEdge(unit.CanMoveOverEdge, origin, dest)
        //                                && unit.CanMoveOver.HasFlag(dest.TerrainType)).ToList();

        //        destinations.ForEach(dest => potentialMoves.Add(new Move(exploringMove.Origin, dest)));
        //    }

        //    return potentialMoves;
        //}

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

