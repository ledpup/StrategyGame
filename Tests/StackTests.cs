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
    public class StackTests
    {
        [TestMethod]
        public void CanTransport_Airborne_CanTransport()
        {
            var units = new List<Unit> {
                                            new LandUnit(UnitType.Cavalry),
                                            new AirborneUnit(),
                                        };

            var stack = new Stack(units);
            Assert.IsTrue(stack.CanTransport(UnitType.Airborne));
        }

        [TestMethod]
        public void CanTransport_AquaticAndAirborne_CanTransportAquatic()
        {
            var aquatic = UnitInitialValues.DefaultValues();
            aquatic.Size = 1f;

            var units = new List<Unit> {
                                            new LandUnit(UnitType.Cavalry),
                                            new AirborneUnit(),
                                            new AquaticUnit(aquatic),
                                        };

            var stack = new Stack(units);

            var transporting = stack.Transporting();
            Assert.AreEqual(UnitType.Aquatic, transporting);
        }

        [TestMethod]
        public void StackMoveList_LandUnits_ThreeMovesOnCorrectTerrain()
        {
            var board = Board.LoadBoard(BoardTests.GameBoard, BoardTests.TileEdges);

            var units = new List<Unit> 
            { 
                new LandUnit(UnitType.Melee),
                new AmphibiousUnit(),
            };
            units.ForEach(x => x.Location = board[1, 1]);


            var stack = new Stack(units);

            var amphibiousMoveList = Unit.MoveList(units[1]);
            Assert.AreEqual(4, amphibiousMoveList.Count());

            var moveList = stack.MoveList();
            Assert.AreEqual(3, moveList.Count());
        }

        [TestMethod]
        public void StackMoveList_TransportingUnits_ThreeMovesOnCorrectTerrain()
        {
            var board = Board.LoadBoard(BoardTests.GameBoard, BoardTests.TileEdges);

            var units = new List<Unit> 
            { 
                new LandUnit(UnitType.Melee),
                new AirborneUnit(),
            };
            units.ForEach(x => x.Location = board[1, 1]);

            var stack = new Stack(units);

            Assert.AreEqual(UnitType.Airborne, stack.Transporting());

            var moveList = stack.MoveList().ToList();

            Assert.AreEqual(6, moveList.Count);
            moveList.ForEach(x => Assert.IsFalse(x.BaseTerrainType.HasFlag(Terrain.All_Water)));
        }
    }
}
