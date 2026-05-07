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
                new(new UnitTemplate(), 0, 1, null, "1st Infantry")
                {
                    BaseQuality = 2,
                    InitialQuantity = 300,
                },

                new(new UnitTemplate(), 1, 2, null, "1st Blue Infantry")
                {
                    BaseQuality = 2,
                    InitialQuantity = 500,
                },
            };

            gameAnalysis.CalculateObjectiveFunction(players, structures, units);
        }
    }
}
