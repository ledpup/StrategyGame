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
    public class UnitTests
    {
        [TestMethod]
        public void TerrainTypeTests()
        {
            Assert.IsTrue(Terrain.All_Land.HasFlag(TerrainType.Desert));
            Assert.IsTrue(Terrain.All_Land.HasFlag(TerrainType.Hill));

            Assert.IsTrue(!Terrain.Non_Mountainous_Land.HasFlag(TerrainType.Mountain));
        }

        [TestMethod]
        public void NewUnit_Airborne_HasCorrectMovement()
        {
            var airborneUnit = new MilitaryUnit() { MovementType = MovementType.Airborne };

            Assert.IsTrue(airborneUnit.CanMoveOver.HasFlag(TerrainType.Forest));
            Assert.IsTrue(airborneUnit.CanMoveOver.HasFlag(TerrainType.Water));
            Assert.IsTrue(airborneUnit.CanMoveOver.HasFlag(TerrainType.Reef));

            Assert.IsFalse(airborneUnit.MayStopOn.HasFlag(TerrainType.Water));
            Assert.IsFalse(airborneUnit.MayStopOn.HasFlag(TerrainType.Mountain));

            Assert.IsTrue(airborneUnit.MayStopOn.HasFlag(TerrainType.Forest));
        }

        [TestMethod]
        public void UnitMoveList_LandUnit_ThreeMovesOnCorrectTerrain()
        {
            var board = new Board(BoardTests.GameBoard, BoardTests.TileEdges);

            var unit = new MilitaryUnit();

            unit.Tile = board[1, 1];

            var moveList = MilitaryUnit.MoveList(unit);

            Assert.AreEqual(4, moveList.Count());

            Assert.IsTrue(board[1, 1].AdjacentTiles.Any(x => Terrain.Aquatic_Terrain.HasFlag(x.TerrainType)));
            Assert.IsFalse(moveList.Any(x => Terrain.Water_Terrain.HasFlag(x.Destination.TerrainType)));

            moveList.ToList().ForEach(x => Assert.AreEqual(1, moveList.Count(t => t.Equals(x))));

            Assert.IsTrue(moveList.Any(x => x.Destination == board[1, 2]));
            Assert.IsTrue(moveList.Any(x => x.Destination == board[2, 2]));
            Assert.IsTrue(moveList.Any(x => x.Destination == board[3, 2]));
            Assert.IsTrue(moveList.Any(x => x.Destination == board[2, 1]));
        }

        [TestMethod]
        public void UnitMoveList_AirborneUnit_SixMovesOnCorrectTerrain()
        {
            var board = new Board(BoardTests.GameBoard, BoardTests.TileEdges);

            var unit = new MilitaryUnit() { MovementType = MovementType.Airborne };

            unit.Tile = board[1, 1];

            var moveList = MilitaryUnit.MoveList(unit);

            Assert.IsTrue(moveList.Any(x => x.Destination == board[1, 2]));
            Assert.IsTrue(moveList.Any(x => x.Destination == board[1, 3]));
            Assert.IsTrue(moveList.Any(x => x.Destination == board[2, 2]));
            Assert.IsTrue(moveList.Any(x => x.Destination == board[2, 1]));
            Assert.IsTrue(moveList.Any(x => x.Destination == board[3, 1]));
            Assert.IsTrue(moveList.Any(x => x.Destination == board[3, 2]));
            
        }
    }
}
