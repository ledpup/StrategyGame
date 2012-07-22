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
            Assert.IsTrue(Terrain.All_Land.HasFlag(TerrainType.Desert));
            Assert.IsTrue(Terrain.All_Land.HasFlag(TerrainType.Hill));

            Assert.IsFalse(Terrain.All_Land_But_Mountain.HasFlag(TerrainType.Mountain));
        }

        [TestMethod]
        public void NewUnit_Airborne_HasCorrectMovement()
        {
            var airborneUnit = new Unit(BaseUnitType.Airborne);

            Assert.IsTrue(airborneUnit.MoveOver.HasFlag(TerrainType.Forest));
            Assert.IsTrue(airborneUnit.MoveOver.HasFlag(TerrainType.Lake));
            Assert.IsTrue(airborneUnit.MoveOver.HasFlag(TerrainType.Reef));

            Assert.IsFalse(airborneUnit.StopOn.HasFlag(TerrainType.Coastal));

            Assert.IsFalse(airborneUnit.StopOver.HasFlag(TerrainType.Lake));
            Assert.IsFalse(airborneUnit.StopOver.HasFlag(TerrainType.Mountain));
            Assert.IsTrue(airborneUnit.StopOver.HasFlag(TerrainType.Forest));
        }

        [TestMethod]
        public void MovementSpeed_FatiguedLandUnit_CantMove()
        {
            var initialValues = new UnitInitialValues { MovementSpeed = 3, };

            var unit = new Unit(BaseUnitType.Land, initialValues);
            unit.Stamina = 0;
            Assert.AreEqual(0, unit.MovementSpeed);
            unit.Stamina = 1;
            Assert.AreEqual(3, unit.MovementSpeed);
        }

        [TestMethod]
        public void MovementSpeed_UndeadUnit_CanMove()
        {
            var initialValues = new UnitInitialValues
            { 
                UnitModifiers = UnitInitialValues.Undead,
                MovementSpeed = 3 
            };

            var unit = new Unit(BaseUnitType.Land, initialValues);
                        
            Assert.AreEqual(3, unit.MovementSpeed);
        }


        [TestMethod]
        public void UnitMoveList_LandUnit_ThreeMovesOnCorrectTerrain()
        {
            var board = Board.LoadBoard(BoardTests.GameBoard, BoardTests.TileEdges);

            var unit = new Unit(BaseUnitType.Land);

            unit.Location = board[1, 1];

            var moveList = Unit.MoveList(unit);

            Assert.AreEqual(4, moveList.Count());

            Assert.IsTrue(board[1, 1].AdjacentTiles.Any(x => Terrain.All_Water.HasFlag(x.BaseTerrainType)));
            Assert.IsFalse(moveList.Any(x => Terrain.All_Water.HasFlag(x.Destination.BaseTerrainType)));

            moveList.ToList().ForEach(x => Assert.AreEqual(1, moveList.Count(t => t.Equals(x))));

            Assert.IsTrue(moveList.Any(x => x.Destination == board[1, 2]));
                Assert.IsTrue(moveList.Any(x => x.Destination == board[2, 2]));
                    Assert.IsTrue(moveList.Any(x => x.Destination == board[3, 2]));
            Assert.IsTrue(moveList.Any(x => x.Destination == board[2, 1]));
        }

        [TestMethod]
        public void UnitMoveList_AirborneUnit_SixMovesOnCorrectTerrain()
        {
            var board = Board.LoadBoard(BoardTests.GameBoard, BoardTests.TileEdges);

            var unit = new Unit(BaseUnitType.Airborne);

            unit.Location = board[1, 1];

            var moveList = Unit.MoveList(unit);

            Assert.IsTrue(moveList.Any(x => x.Destination == board[1, 2]));
            Assert.IsTrue(moveList.Any(x => x.Destination == board[1, 3]));
            Assert.IsTrue(moveList.Any(x => x.Destination == board[2, 2]));
            Assert.IsTrue(moveList.Any(x => x.Destination == board[2, 1]));
            Assert.IsTrue(moveList.Any(x => x.Destination == board[3, 1]));
            Assert.IsTrue(moveList.Any(x => x.Destination == board[3, 2]));
            
        }
    }
}
