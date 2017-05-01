using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GameModel;
using System.IO;

namespace Tests
{
    [TestClass]
    public class StackTests
    {
        static string[] GameBoard = File.ReadAllLines("BasicBoard.txt");
        static string[] Structures = File.ReadAllLines("BasicBoardStructures.txt");

        [TestMethod]
        public void StackLimits()
        {
            var board = new Board(GameBoard, structures: Structures);

            Assert.AreEqual(4, board[1, 2].StackLimit);
            Assert.AreEqual(5, board[1, 3].StackLimit);
            Assert.AreEqual(2, board[6, 1].StackLimit);
        }

        [TestMethod]
        public void OverStackLimit()
        {
            var board = new Board(GameBoard, structures: Structures);

            var units = new List<MilitaryUnit>
            {
                new MilitaryUnit(location: board[6, 1], roadMovementBonus: 1),
                new MilitaryUnit(location: board[6, 1]),
                new MilitaryUnit(location: board[6, 1]),
                new MilitaryUnit(location: board[6, 1]),
            };

            Assert.AreEqual(2, board[6, 1].StackLimit);
            Assert.IsTrue(board[6, 1].OverStackLimit(0));

            board.ResolveStackLimits(0);

            units.ForEach(x => Assert.AreEqual(4, x.Morale));
        }

        [TestMethod]
        public void CanTransport()
        {
            var inf = new MilitaryUnit(transportableBy: new List<MovementType> { MovementType.Airborne });
            var air = new MilitaryUnit(movementType: MovementType.Airborne, isTransporter: true);

            
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
