using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GameModel;

namespace Tests
{
    [TestClass]
    public class StackTests
    {
        [TestMethod]
        public void CanTransport()
        {
            var inf = new MilitaryUnit();
            var air = new MilitaryUnit() { MovementType = MovementType.Airborne, IsTransporter = true };

            
            Assert.IsFalse(inf.CanTransport(air));
            Assert.IsTrue(air.CanTransport(inf));
        }

        //[TestMethod]
        //public void CanTransport_AquaticAndAirborne_CanTransportAquatic()
        //{
        //    var aquatic = UnitInitialValues.DefaultValues();
        //    aquatic.Size = 1f;

        //    var units = new List<Unit> {
        //                                    new Unit(BaseUnitType.Land),
        //                                    new Unit(BaseUnitType.Airborne),
        //                                    new Unit(BaseUnitType.Aquatic, aquatic),
        //                                };

        //    var stack = new Stack(units);

        //    var transporting = stack.Transporting();
        //    Assert.AreEqual(UnitType.Aquatic, transporting);
        //}

        //[TestMethod]
        //public void StackMoveList_LandUnits_ThreeMovesOnCorrectTerrain()
        //{
        //    var board = Board.LoadBoard(BoardTests.GameBoard, BoardTests.TileEdges);

        //    var units = new List<Unit> 
        //    { 
        //        new Unit(BaseUnitType.Land),
        //        new Unit(BaseUnitType.Land),
        //    };
        //    units.ForEach(x => x.Tile = board[1, 1]);


        //    var stack = new Stack(units);

        //    var amphibiousMoveList = Unit.MoveList(units[1]);
        //    Assert.AreEqual(5, amphibiousMoveList.Count());

        //    var moveList = stack.MoveList();
        //    Assert.AreEqual(3, moveList.Count());
        //}

        //[TestMethod]
        //public void StackMoveList_TransportingUnits_ThreeMovesOnCorrectTerrain()
        //{
        //    var board = Board.LoadBoard(BoardTests.GameBoard, BoardTests.TileEdges);

        //    var units = new List<Unit> 
        //    { 
        //        new Unit(BaseUnitType.Land),
        //        new Unit(BaseUnitType.Airborne),
        //    };
        //    units.ForEach(x => x.Tile = board[1, 1]);

        //    var stack = new Stack(units);

        //    Assert.AreEqual(UnitType.Airborne, stack.Transporting());

        //    var moveList = stack.MoveList().ToList();

        //    Assert.AreEqual(6, moveList.Count);
        //    moveList.ForEach(x => Assert.IsFalse(x.Destination.TerrainType.HasFlag(Terrain.AquaticTerrain)));
        //}
    }
}
