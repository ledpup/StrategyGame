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
            Assert.IsTrue(Terrain.All_Land.HasFlag(TerrainType.Steppe));
            Assert.IsTrue(Terrain.All_Land.HasFlag(TerrainType.Hill));

            Assert.IsTrue(!Terrain.Non_Mountainous_Land.HasFlag(TerrainType.Mountain));
        }

        [TestMethod]
        public void AirborneUnitCanStopOn()
        {
            var airborneUnit = new MilitaryUnit(movementType: MovementType.Airborne);

            Assert.IsTrue(airborneUnit.CanStopOn.HasFlag(TerrainType.Forest));

            Assert.IsFalse(airborneUnit.CanStopOn.HasFlag(TerrainType.Water));
            Assert.IsFalse(airborneUnit.CanStopOn.HasFlag(TerrainType.Reef));
            Assert.IsFalse(airborneUnit.CanStopOn.HasFlag(TerrainType.Water));
            Assert.IsFalse(airborneUnit.CanStopOn.HasFlag(TerrainType.Mountain));
        }

        [TestMethod]
        public void AirborneUnitValidMoves()
        {
            var board = new Board(BoardTests.GameBoard, BoardTests.TileEdges);

            var unit = new MilitaryUnit(movementType: MovementType.Airborne);

            unit.Location = board[1, 1];

            var moveList = unit.PossibleMoves();

            Assert.IsTrue(moveList.Any(x => x.Destination == board[1, 2]));
            Assert.IsTrue(moveList.Any(x => x.Destination == board[1, 3]));
            Assert.IsTrue(moveList.Any(x => x.Destination == board[2, 2]));
            Assert.IsTrue(moveList.Any(x => x.Destination == board[2, 1]));
            Assert.IsTrue(moveList.Any(x => x.Destination == board[3, 1]));
            Assert.IsTrue(moveList.Any(x => x.Destination == board[3, 2]));
            
        }
    }
}
