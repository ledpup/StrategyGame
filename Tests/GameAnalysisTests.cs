using ComputerOpponent;
using GameModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                new Structure(0, StructureType.Fortress, null, 1),
                new Structure(0, StructureType.Town, null, 1),
                new Structure(0, StructureType.City, null, 2),
            };

            var units = new List<MilitaryUnit>
            {
                new MilitaryUnit(0, "1st Infantry", 1)
                {
                    BaseQuality = 2,
                    InitialQuantity = 300,
                },

                new MilitaryUnit(1, "1st Blue Infantry", 2)
                {
                    BaseQuality = 2,
                    InitialQuantity = 500,
                },
            };

            gameAnalysis.CalculateObjectiveFunction(players, structures, units);
        }
    }
}
