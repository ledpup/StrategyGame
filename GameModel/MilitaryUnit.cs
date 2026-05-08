using System;
using System.Collections.Generic;
using System.Linq;

namespace GameModel
{

    public enum UnitType
    {
        Melee,
        Ranged,
        Cavalry,
        Siege,
    }

    public enum MovementType
    {
        Airborne,
        Land,
        Waterbound,
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

        public static List<MovementType> MovementTypes
        {
            get
            {
                if (movementTypes == null)
                {
                    movementTypes = [];
                    foreach (var role in Enum.GetValues<MovementType>())
                    {
                        movementTypes.Add(role);
                    }
                }
                return movementTypes;
            }
        }
        static List<MovementType> movementTypes;

        public int Index;
        public string Name { get; set; }
        public int OwnerIndex { get; set; }

        public UnitTemplate UnitTemplate { get; set; }
        public UnitType UnitType { get { return UnitTemplate.UnitType; } }
        public MovementType MovementType { get { return UnitTemplate.MovementType; } }
        public int BaseMovementPoints { get { return UnitTemplate.MovementPoints; } }
        public double BaseQuality
        {
            get { return UnitTemplate.Quality; }
        }
        public Dictionary<BattleQualityModifier, double> BattleQualityModifiers { get; set; }
        public double Quality { get; set; }
        public int Personnel { get; private set; }
        public double Strength { get; set; }
        public double BattleStrength { get; set; }
        public double Size { get { return UnitTemplate.Size; } }
        public List<UnitEvent> Events { get; set; }
        public double Morale { get; private set; }
        public double CombatInitiative { get; set; }
        public int Speed { get; set; }
        public bool IsAlive { get { return Personnel > 0; } }

        public Dictionary<TerrainType, double> TerrainTypeBattleModifier { get; set; }
        public Dictionary<Weather, double> WeatherBattleModifier { get; set; }
        public Dictionary<UnitType, double> OpponentUnitTypeBattleModifier { get; set; }

        public TerrainType CanStopOn;

        public Dictionary<TerrainType, int> TerrainMovementCosts { get; set; }
        public Dictionary<EdgeType, int> EdgeMovementCosts { get; set; }

        public bool IsTransporter { get { return UnitTemplate.IsTransporter; } }
        public List<MilitaryUnit> Transporting { get; set; }

        public int TurnCreated { get; set; }

        public override string ToString()
        {
            return MovementType.ToString() + " " +  Name + " (" + Strength + ") at " + Location.ToString();
        }
        public MilitaryUnit(UnitTemplate template, int index = 0, int ownerIndex = 0, Tile location = null, string name = null, int turnBuilt = 0, float[] moraleMoveCost = null)
        {
            UnitTemplate = template;

            BattleQualityModifiers = [];
            foreach (BattleQualityModifier battleQualityModifier in Enum.GetValues<BattleQualityModifier>())
            {
                BattleQualityModifiers.Add(battleQualityModifier, 0);
            }

            TerrainTypeBattleModifier = [];
            foreach (TerrainType terrainType in Enum.GetValues<TerrainType>())
            {
                TerrainTypeBattleModifier.Add(terrainType, 0);
            }

            WeatherBattleModifier = [];
            foreach (Weather weather in Enum.GetValues<Weather>())
            {
                WeatherBattleModifier.Add(weather, 0);
            }

            OpponentUnitTypeBattleModifier = [];
            foreach (UnitType unitTypeEnum in Enum.GetValues<UnitType>())
            {
                OpponentUnitTypeBattleModifier.Add(unitTypeEnum, 0);
            }

            TerrainMovementCosts = new Dictionary<TerrainType, int>(template.TerrainMovementCosts);
            EdgeMovementCosts = new Dictionary<EdgeType, int>(template.EdgeMovementCosts);
            UsesRoads = template.UsesRoads;
            CanStopOn = template.CanStopOn;

            Index = index;
            Name = name ?? template.Name ?? "Unit " + index + " (owned by " + ownerIndex + ")";
            OwnerIndex = ownerIndex;
            Location = location;
            RoadMovementBonus = template.RoadMovementBonus;
            TransportableBy = template.TransportableBy ?? [];
            Transporting = [];

            Quality = BaseQuality;
            CombatInitiative = template.CombatInitiative;
            TurnCreated = turnBuilt;

            Events = [];
            ChangeMorale(TurnCreated, UnitTemplate.Morale, "Initial morale");
            Personnel = InitialPersonnel;

            var templateMoraleMoveCost = moraleMoveCost ?? template.MoraleMoveCost;
            if (templateMoraleMoveCost == null)
            {
                MoraleMoveCost = new float[BaseMovementPoints];
                for (var i = 0; i < BaseMovementPoints; i++)
                    MoraleMoveCost[i] = 0;
            }
            else
            {
                MoraleMoveCost = templateMoraleMoveCost;
            }

            CalculateStrength();
        }



        public float[] MoraleMoveCost { get; set; }

        public int InitialPersonnel
        {
            get { return UnitTemplate.Personnel; }
        }

        public double InitialMorale
        {
            get { return UnitTemplate.Morale; }
        }

        public void CalculateStrength()
        {
            Strength = Quality * Personnel;

            var battleQuality = Quality + BattleQualityModifiers.Values.Sum();
            if (battleQuality < .1)
                battleQuality = .1;

            BattleStrength = battleQuality * Personnel;
        }

        public void ChangePersonnel(int turn, int quantity)
        {
            Events.Add(new UnitEvent(turn, quantity, "Personnel change"));
            Personnel += quantity;

            if (Personnel <= 0)
            {
                Strength = BattleStrength = 0;
                return;
            }

            CalculateStrength();
        }

        public void ChangeMorale(int turn, double moraleChange, string reason)
        {
            Morale += moraleChange;
            Events.Add(new UnitEvent(turn, moraleChange, reason));
        }


        public static Func<MilitaryUnit, MilitaryUnit, bool> IsInConflictDuringMovement = (p, o) => p.OwnerIndex != o.OwnerIndex && p.Location == o.Location && p.MovementType == o.MovementType;

        public Tile Location
        {
            get { return _location; }
            set { _location = value; }
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
                return false;

            if (!transportee.TransportableBy.Contains(MovementType))
                throw new Exception($"{transportee.Name} may not be transported by {MovementType} movement type");

            return TransportSize >= transportee.TransportSize + Transporting.Sum(x => x.TransportSize);
        }

        public int TransportSize
        {
            get { return (int)Math.Ceiling(Personnel * Size); }
        }

        public int RoadMovementBonus { get; set; }
        public double StructureBattleModifier { get; set; }
        public MilitaryUnit TransportedBy { get; set; }
        public List<MovementType> TransportableBy { get; private set; }
        public bool UsesRoads { get; private set; }
        public bool IsBeingTransportedByWater { get { return TransportedBy != null && TransportedBy.MovementType == MovementType.Waterbound; } }

        public IEnumerable<Move> PossibleMoves()
        {
            var possibleMoves = new List<Move>();

            var movesConsidered = new List<Move>();

            possibleMoves = GenerateStandardMoves(this, Location, null, movesConsidered, MovementPoints, 1);

            if (MovementType == MovementType.Land && TransportedBy == null)
            {
                var roadMovesAlreadyConsidered = new List<Move>();
                var roadMoves = GenerateRoadMoves(this, Location, null, roadMovesAlreadyConsidered, MovementPoints + RoadMovementBonus, 1);
                var notAlreadySeenRoadMoves = roadMoves.Where(x => !possibleMoves.Any(y => x.Origin == y.Origin && x.Edge.Destination == y.Edge.Destination));
                possibleMoves.AddRange(notAlreadySeenRoadMoves);
            }

            var searchForOnlyPassingThroughDestinations = true;
            while (searchForOnlyPassingThroughDestinations)
            {
                var removeOnlyPassingThroughDestinations = possibleMoves
                    .Where(x => x.MoveType == MoveType.OnlyPassingThrough && !possibleMoves.Any(y => y.Origin == x.Edge.Destination));

                searchForOnlyPassingThroughDestinations = removeOnlyPassingThroughDestinations.Any();
                removeOnlyPassingThroughDestinations.ToList().ForEach(x => possibleMoves.Remove(x));
            } 

            return possibleMoves;
        }

        private static List<Move> GenerateStandardMoves(MilitaryUnit unit, Tile origin, Move previousMove, List<Move> movesConsidered, int movementPoints, int distance)
        {
            var potentialMoves = new List<Move>();

            foreach (var neighbour in origin.Neighbours)
            {
                if (PotentialMove(unit, origin, movesConsidered, movementPoints, neighbour))
                {
                    var move = new Move(origin, neighbour, previousMove, movementPoints, distance, GetMoveType(origin, neighbour.Destination, unit));

                    potentialMoves.Add(move);
                    movesConsidered.Add(move);
                }
            }

            var neighbourMoves = new List<Move>();

            var validMoves = new List<Move>();

            foreach (var move in potentialMoves)
            {
                var moveCost = move.Edge.MoveCost(unit.UsesRoads, unit.IsBeingTransportedByWater, unit.EdgeMovementCosts, unit.TerrainMovementCosts);
                var remainingMovementPoints = movementPoints - moveCost;
                if (remainingMovementPoints > 0)
                {
                    neighbourMoves.AddRange(GenerateStandardMoves(unit, move.Edge.Destination, move, movesConsidered, remainingMovementPoints, distance + 1));
                }
            }

            potentialMoves.AddRange(neighbourMoves);

            return potentialMoves.Where(x => ValidMove(unit, x)).ToList();

        }

        private static MoveType GetMoveType(Tile origin, Tile destination, MilitaryUnit unit)
        {
            if (unit.MovementType == MovementType.Land)
            {
                if (unit.TransportedBy == null && origin.Neighbours.Single(x => x.Destination == destination).EdgeType == EdgeType.Port)
                {
                    return MoveType.Embark;
                }
            }
            if (!unit.CanStopOn.HasFlag(destination.TerrainType))
                return MoveType.OnlyPassingThrough;

            return MoveType.Standard;
        }

        private static bool ValidMove(MilitaryUnit unit, Move x)
        {
            var validMove = x.MoveType == MoveType.OnlyPassingThrough || 
                                        x.MoveType == MoveType.Embark ||
                                        unit.CanStopOn.HasFlag(x.Edge.Destination.TerrainType);
            return validMove;
        }

        private static bool PotentialMove(MilitaryUnit unit, Tile origin, List<Move> movesConsidered, int movementPoints, Edge edge)
        {
            var potentialMove = edge.Destination != unit.Location 
                                        && !movesConsidered.Any(x => x.Origin == origin && x.MovesRemaining > movementPoints);

            if (!potentialMove)
                return false;

            potentialMove = unit.EdgeMovementCosts[edge.EdgeType] < Terrain.Impassable || (unit.UsesRoads && edge.HasRoad);

            if (!potentialMove)
                return false;

            potentialMove = unit.TransportedBy == null || edge.EdgeType == EdgeType.Port;

            return potentialMove;
        }

        private static List<Move> GenerateRoadMoves(MilitaryUnit unit, Tile tile, Move previousMove, List<Move> movesConsidered, int movementPoints, int distance)
        {
            var moves = tile.Neighbours.Where(x => x.HasRoad && !movesConsidered.Any(y => y.Origin == tile && x.Destination == y.Edge.Destination))
                                                .Select(x => new Move(tile, x, previousMove, movementPoints, distance, MoveType.Road)).ToList();

            //var moves = tile.Edges.Where(x => !movesConsidered.Any(y => Edge.CrossesEdge(x, tile, y.Destination)))  
            //                            .Select(x => new Move(x.Origin, x.Destination, x, previousMove, movementPoints, distance, MoveType.Road)).ToList();

            movesConsidered.AddRange(moves);

            var neighbourMoves = new List<Move>();

            foreach (var move in moves)
            {
                var cost = movementPoints - 1;
                if (cost > 0)
                    neighbourMoves.AddRange(GenerateRoadMoves(unit, move.Edge.Destination, move, movesConsidered, cost, distance + 1));
            }

            moves.AddRange(neighbourMoves);

            return moves;
        }

        public MoveOrder GetMoveOrderToDestination(Tile destination)
        {
            var shortestPath = PathFinder.FindShortestPath(Location, destination, this).ToArray();

            return ShortestPathToMoveOrder(shortestPath);
        }

        public MoveOrder ShortestPathToMoveOrder(PathFindTile[] shortestPath)
        {
            var moves = PathFinder.MovesFromShortestPath(PossibleMoves().ToList(), shortestPath);

            if (moves.Count == 0)
                return null;

            var moveOrders = new MoveOrder(moves.ToArray(), this);
            return moveOrders;
        }
    }
}

