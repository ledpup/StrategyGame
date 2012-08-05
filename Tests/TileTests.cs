using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class TileTests
    {
        [TestMethod]
        public void BattleTest()
        { 
            var board = MoveTests.UnitsMoveIntoConflict();

            var battles = board.ConductBattles();

            Assert.AreEqual(1, battles.Count());
            Assert.AreEqual(6, battles[0].Events.Count);
        }
    }
}
