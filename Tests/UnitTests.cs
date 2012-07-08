using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Model;

namespace Tests
{
    [TestClass]
    public class UnitTests
    {
        [TestMethod]
        public void TerrainTypeTests()
        {
            Assert.IsTrue(Terrain.All_Land.HasFlag(TerrainType.Arid));
            Assert.IsTrue(Terrain.All_Land.HasFlag(TerrainType.Hill));

            Assert.IsFalse(Terrain.All_Land_But_Mountain.HasFlag(TerrainType.Mountain));
        }

        [TestMethod]
        public void NewUnit_Airborne_HasCorrectMovement()
        {
            var airborneUnit = new AirborneUnit();

            Assert.IsTrue(airborneUnit.MoveOver.HasFlag(TerrainType.Forest));
            Assert.IsTrue(airborneUnit.MoveOver.HasFlag(TerrainType.Lake));
            Assert.IsTrue(airborneUnit.MoveOver.HasFlag(TerrainType.Reef));

            Assert.IsFalse(airborneUnit.StopOn.HasFlag(TerrainType.Coastal));

            Assert.IsFalse(airborneUnit.StopOver.HasFlag(TerrainType.Lake));
            Assert.IsFalse(airborneUnit.StopOver.HasFlag(TerrainType.Mountain));
            Assert.IsTrue(airborneUnit.StopOver.HasFlag(TerrainType.Forest));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void NewUnit_Land_InvalidUnitType()
        {
            new LandUnit(UnitType.Airborne);
        }

        [TestMethod]
        public void MovementSpeed_FatiguedLandUnit_CantMove()
        {
            var initialValues = new UnitInitialValues { MovementSpeed = 3, ForcedMovementSpeed = 1 };

            var unit = new LandUnit(UnitType.Ranged, UnitModifier.None, initialValues);
            unit.Stamina = 0;
            Assert.AreEqual(0, unit.MovementSpeed);
            Assert.AreEqual(0, unit.ForcedMovementSpeed);
            unit.Stamina = 1;
            Assert.AreEqual(3, unit.MovementSpeed);
            Assert.AreEqual(1, unit.ForcedMovementSpeed);
        }

        [TestMethod]
        public void MovementSpeed_UndeadUnit_CanMove()
        {
            var initialValues = new UnitInitialValues { MovementSpeed = 3 };

            var unit = new LandUnit(UnitType.Ranged, UnitInitialValues.Undead, initialValues);
                        
            Assert.AreEqual(3, unit.MovementSpeed);
            Assert.AreEqual(0, unit.ForcedMovementSpeed);
        }
    }
}
