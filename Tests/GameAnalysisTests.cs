using ComputerOpponent;
using GameModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Tests
{
    [TestClass]
    public class GameAnalysisTests
    {
        [TestMethod]
        public void BasicGameAnalysisTest()
        {
            var gameAnalysis = new GameAnalysis();

            var players = new Player[2];

            var structures = new List<Structure>
            {
                new(0, StructureType.Fortress, null, 1),
                new(0, StructureType.Outpost, null, 1),

                new(0, StructureType.City, null, 2),
            };

            var units = new List<MilitaryUnit>
            {
                new(new UnitTemplate { Quality = 2, Personnel = 300 }, 0, 1, null, "1st Infantry"),
                new(new UnitTemplate { Quality = 2, Personnel = 500 }, 1, 2, null, "1st Blue Infantry"),
            };

            gameAnalysis.CalculateObjectiveFunction(players, structures, units);
        }
    }
}
