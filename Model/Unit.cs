using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{

    [Flags]
    public enum BaseUnitType
    {
        Land = 1 << 0,
        Aquatic = 1 << 1,
        Airborne = 1 << 2,
    }

    [Flags]
    public enum UnitType
    {
        None = 0,
        Melee = 1 << 0,
        Cavalry = 1 << 1,
        Ranged = 1 << 2,
        Siege = 1 << 3,
        Airborne = 1 << 4,
        Aquatic = 1 << 5,
        Amphibious = 1 << 6,
    }

    public struct UnitInitialValues
    {
        public static UnitInitialValues DefaultValues()
        {
            var initialValues = new UnitInitialValues();

            initialValues.UnitModifiers = UnitModifier.None;

            initialValues.Size = 0.01f;
            initialValues.Quality = 0.01f;
            initialValues.Quantity = 500;
            initialValues.Stamina = 1f;
            initialValues.Morale = 1f;
            initialValues.MovementSpeed = 2;

            return initialValues;
        }

        public UnitType UnitType;
        public UnitModifier UnitModifiers;

        public float Size;
        public short Quantity;
        public float Quality;
        public float Stamina;
        public float Morale;
        public short MovementSpeed;

        public static UnitModifier Undead = UnitModifier.NoStamina | UnitModifier.NoMorale;
        public TerrainType TerrainCombatBonus;
        //public static UnitModifier Airborne = UnitModifier.CanCrossRivers;
        //public static UnitModifier Amphibious = UnitModifier.CanUseRoads | UnitModifier.CanCrossRivers;
        //public static UnitModifier LandUnit = UnitModifier.CanUseRoads;
    }

    public enum UnitModifier
    {
        None = 0,
        NoStamina = 1 << 0,
        NoMorale = 1 << 1,
        ColdResistant = 1 << 2,
        //Airborne = 1 << 4,
        //Amphibious = 1 << 5,
        //Aquatic = 1 << 6,
        //Undead = 1 << 7,
    }

    public class Unit
    {
        public Unit(BaseUnitType baseUnitType) : this(baseUnitType, UnitInitialValues.DefaultValues())
        {}

        public Unit(BaseUnitType baseUnitType, UnitInitialValues initialValues)
        {
            BaseUnitType = baseUnitType;
            InitialValues = initialValues;

            UnitType = initialValues.UnitType;
            UnitModifiers = initialValues.UnitModifiers;
            TerrainCombatBonus = initialValues.TerrainCombatBonus;
            Size = initialValues.Size;
            Quality = initialValues.Quality;
            Quantity = initialValues.Quantity;
            Stamina = initialValues.Stamina;
            Morale = initialValues.Morale;

            _movementSpeed = initialValues.MovementSpeed;

            switch (BaseUnitType)
            {
                case BaseUnitType.Land:
                    LandUnit();
                    break;
                case BaseUnitType.Airborne:
                    AirborneUnit();
                    break;
                case BaseUnitType.Aquatic:
                    AquaticUnit();
                    break;
            }

            switch (UnitType)
            {
                case UnitType.Amphibious:
                    AmphibiousUnit();
                    break;
            }
        }

        public static Func<Unit, Unit, bool> IsConflict = (p, o) => p.Player != o.Player && p.Tile == o.Tile && p.BaseUnitType == o.BaseUnitType;

        void LandUnit()
        {
            MoveOver = Terrain.All_Land_But_Mountain_And_Lake;
            MoveOverEdge = EdgeType.Road;
            StopOn = Terrain.All_Rough_Land;
            StopOver = Terrain.All_Land_But_Mountain_And_Lake;
        }

        void AmphibiousUnit()
        {
            MoveOver = Terrain.All_Land_But_Mountain;
            MoveOverEdge = EdgeType.Road | EdgeType.River;
            StopOn = Terrain.All_Rough_Land ^ TerrainType.Wetland;
            StopOver = Terrain.All_Land_But_Mountain;
        }

        void AirborneUnit()
        {
            MoveOver = Terrain.All_Terrain;
            MoveOverEdge = Edge.AllEdges;
            StopOn = Terrain.Nothing;
            StopOver = Terrain.All_Land_But_Mountain_And_Lake;
        }

        void AquaticUnit()
        {
            MoveOver = Terrain.All_Water;
            MoveOverEdge = EdgeType.Normal;
            StopOn = TerrainType.Reef;
            StopOver = Terrain.All_Water;
        }

        public int Id;
        public UnitInitialValues InitialValues;
        public Player Player;
        public BaseUnitType BaseUnitType;
        public UnitType UnitType;
        public UnitModifier UnitModifiers;
        public TerrainType MoveOver;
        public TerrainType StopOver;
        public TerrainType StopOn;
        public TerrainType TerrainCombatBonus;
        public EdgeType MoveOverEdge;

        public float Size;
        public short Quantity;
        public float Quality;
        public float Stamina;
        public float Morale;

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

        public short MovementSpeed
        {
            get
            {
                if (!UnitModifiers.HasFlag(UnitModifier.NoStamina) && Stamina == 0)
                    return 0;
                
                return _movementSpeed;
            }
        }
        short _movementSpeed;

        public bool IsTransporter;

        public float BaseCombatStrength
        {
            get
            {
                return Quality * Quantity * Stamina * Morale;
            }
        }

        public float CombatStrength
        {
            get 
            {
                var baseStrength = BaseCombatStrength;
                
                if (TerrainCombatBonus.HasFlag(Tile.TerrainType))
                    return baseStrength * 2f;
                
                return baseStrength;
            }
        }

        public float TransportSize
        {
            get { return Quantity * Size; }
        }

        public bool IsAlive
        {
            get { return (float)Quantity / InitialValues.Quantity > .1f; }
        }

        public static IEnumerable<Move> MoveList(Unit unit)
        {
            var moveList = new List<Move>();
            var exploringMoves = new List<Move> { new Move(null, unit.Tile) };
            var move = 0;

            while (move < unit.MovementSpeed)
            {
                var moves = GenerateMoves(unit, moveList, exploringMoves);
                var validMoves = moves.Where(x => unit.StopOver.HasFlag(x.Destination.TerrainType)).ToList();
                moveList.AddRange(validMoves);
                exploringMoves = moves.Where(x => !unit.StopOn.HasFlag(x.Destination.TerrainType) || Move.IsAllOnRoad(x)).ToList();

                move++;
            }

            // If all moves have been on road, you a bonus move
            if (unit.MoveOverEdge.HasFlag(EdgeType.Road))
            {
                var roadsToExplore = exploringMoves.Where(x => Move.IsAllOnRoad(x)).ToList();
                var roadMoves = GenerateMoves(unit, moveList, roadsToExplore);
                var validRoadMoves = roadMoves.Where(x => unit.StopOver.HasFlag(x.Destination.TerrainType) && Move.IsAllOnRoad(x)).ToList();
                moveList.AddRange(validRoadMoves);
            }
            
            return moveList;
        }

        private static List<Move> GenerateMoves(Unit unit, List<Move> moveList, List<Move> exploringMoves)
        {
            var potentialMoves = new List<Move>();

            foreach (var exploringMove in exploringMoves)
            {
                var origin = exploringMove.Destination;

                var destinations = origin.AdjacentTiles.Where(dest => dest != unit.Tile
                                        && !potentialMoves.Any(x => x.Destination == dest)
                                        && !moveList.Any(x => x.Destination == dest)
                                        && CanCrossEdge(unit.MoveOverEdge, origin, dest)).ToList();

                destinations.ForEach(dest => potentialMoves.Add(new Move(exploringMove, dest)));
            }

            return potentialMoves.Where(x => unit.MoveOver.HasFlag(x.Destination.TerrainType)).ToList();
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
