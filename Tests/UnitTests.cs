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
    }
}
