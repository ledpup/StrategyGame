using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    [Flags]
    public enum TerrainType
    {
        Grassland = 1 << 0,
        Forest = 1 << 1,
        Arid = 1 << 2,
        Hill = 1 << 3,
        Mountain = 1 << 4,
        Wetland = 1 << 5,
        Lake = 1 << 6,
        Sea = 1 << 7,
        Reef = 1 << 8,
        Coastal = 1 << 9, // This is not a base terrain type but calculated. Any land hex adjacent to sea or reef is coastal.
    }

    public static class Terrain
    {
        public static TerrainType All_Land = TerrainType.Grassland | TerrainType.Forest | TerrainType.Arid | TerrainType.Hill | TerrainType.Mountain | TerrainType.Wetland | TerrainType.Lake;
        public static TerrainType All_Water = TerrainType.Sea | TerrainType.Reef | TerrainType.Coastal;
        public static TerrainType All_Land_But_Mountain = All_Land ^ TerrainType.Mountain;
        public static TerrainType All_Land_But_Lake = All_Land ^ TerrainType.Lake;
        public static TerrainType All_Land_But_Mountain_And_Lake = All_Land ^ TerrainType.Mountain ^ TerrainType.Lake;
        public static TerrainType All_Rough_Land = All_Land_But_Lake ^ TerrainType.Grassland;
        public static TerrainType All_Terrain = Terrain.All_Land ^ Terrain.All_Water;
        public static TerrainType Nothing = 0;
    }

    [Flags]
    public enum StopOver
    {
        All_Land = 0,
        All_Water = 1 << 0,
        Lake = 1 << 1,
        Coastal = 1 << 2,
    }

    [Flags]
    public enum StopOn
    {
        Nothing = 0,

        All_Rough = 1 << 0,
        All_Water = 1 << 1,
        Lake = 1 << 2,
        Coastal = 1 << 3,
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
        Amphibious = 1 << 5,
        Aquatic = 1 << 6,
    }

    public struct UnitInitialValues
    {
        public static UnitInitialValues DefaultValues()
        {
            var initialValues = new UnitInitialValues();

            initialValues.Size = 0.01f;
            initialValues.Quantity = 1f;
            initialValues.Stamina = 1f;
            initialValues.Morale = 1f;
            initialValues.MovementSpeed = 2;
            initialValues.ForcedMovementSpeed = 1;

            return initialValues;
        }

        public float Size;
        public float Quantity;
        public float Quality;
        public float Stamina;
        public float Morale;
        public short MovementSpeed;
        public short ForcedMovementSpeed;

        public static UnitModifier Undead = UnitModifier.NoStamina | UnitModifier.NoForcedMarch | UnitModifier.NoMorale;
    }

    public enum UnitModifier
    {
        None = 0,
        NoStamina = 1 << 0,
        NoMorale = 1 << 1,
        ColdResistant = 1 << 2,
        NoForcedMarch = 1 << 3,
        //Airborne = 1 << 4,
        //Amphibious = 1 << 5,
        //Aquatic = 1 << 6,
        //Undead = 1 << 7,
    }

    public abstract class Unit
    {
        public Unit(UnitType unitType)
            : this(unitType, UnitModifier.None, UnitInitialValues.DefaultValues())
        { 
        }

        public Unit(UnitType unitType, UnitModifier unitModifiers, UnitInitialValues initialValues)
        {
            UnitType = unitType;
            UnitModifiers = unitModifiers;
            Size = initialValues.Size;
            Quality = initialValues.Quality;
            Quantity = initialValues.Quantity;
            Stamina = initialValues.Stamina;
            Morale = initialValues.Morale;

            _movementSpeed = initialValues.MovementSpeed;
            _forcedMovementSpeed = initialValues.ForcedMovementSpeed;
        }

        public UnitType UnitType;
        public UnitModifier UnitModifiers;
        public TerrainType MoveOver;
        public TerrainType StopOver;
        public TerrainType StopOn;
        public TerrainType TerrainCombatBonus;

        public float Size;
        public float Quantity;
        public float Quality;
        public float Stamina;
        public float Morale;

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

        public short ForcedMovementSpeed
        {
            get
            {
                if (UnitModifiers.HasFlag(UnitModifier.NoForcedMarch))
                    return 0;

                if (Stamina == 0)
                    return 0;

                return _forcedMovementSpeed;
            }
        }
        short _forcedMovementSpeed;

        public bool IsTransporter;

        public float CombatStrength
        {
            get 
            {
                return Quality * Quantity * Stamina * Morale;
            }
        }

        public float TransportSize
        {
            get { return Quantity * Size; }
        }
    }

    public class LandUnit : Unit
    {
        public LandUnit(UnitType unitType) : this(unitType, UnitModifier.None, UnitInitialValues.DefaultValues())
        { 

        }

        public LandUnit(UnitType unitType, UnitModifier unitModifiers, UnitInitialValues initialValues)
            : base(unitType, unitModifiers, initialValues)
        {
            if (unitType.HasFlag(UnitType.Airborne) || unitType.HasFlag(UnitType.Amphibious) || unitType.HasFlag(UnitType.Aquatic))
                throw new ArgumentException(string.Format("Unit type '{0}' is not a land unit.", unitType.ToString()), "unitType");

            MoveOver = Terrain.All_Land_But_Mountain_And_Lake;
            StopOn = Terrain.All_Rough_Land;
            StopOver = Terrain.All_Land_But_Mountain_And_Lake;
        }
    }
    
    public class AirborneUnit : Unit
    {
        public AirborneUnit()
            : this(UnitInitialValues.DefaultValues())
        {

        }

        public AirborneUnit(UnitInitialValues initialValues)
            : base(UnitType.Airborne, UnitModifier.None, initialValues)
        {
            MoveOver = Terrain.All_Terrain;
            StopOn = Terrain.Nothing;
            StopOver = Terrain.All_Land_But_Mountain_And_Lake;
        }
    }

    public class AmphibiousUnit : Unit
    {
        public AmphibiousUnit()
            : base(UnitType.Airborne)
        {
            MoveOver = Terrain.All_Land_But_Mountain;
            StopOn = Terrain.All_Rough_Land ^ TerrainType.Wetland;
            StopOver = Terrain.All_Land_But_Mountain;
        }
    }

    public class AquaticUnit : Unit
    {
        public AquaticUnit(UnitInitialValues initialValues)
            : base(UnitType.Aquatic, UnitModifier.None, initialValues)
        {
            MoveOver = Terrain.All_Water;
            StopOn = TerrainType.Reef;
            StopOver = Terrain.All_Water;
        }
    }
}
