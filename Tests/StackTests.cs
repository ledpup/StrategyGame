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
        public void MovementTest()
        {
            var board = Board.LoadBoard(BoardTests.GameBoard);

            var unit = new LandUnit(UnitType.Melee);

            unit.Location = board[1, 1];

            var moveList = Stack.UnitMoveList(unit);

            Assert.IsTrue(board[1, 1].AdjacentTiles.Any(x => Terrain.All_Water.HasFlag(x.BaseTerrainType)));
            Assert.IsFalse(moveList.Any(x => Terrain.All_Water.HasFlag(x.BaseTerrainType)));
        }
    }
}
