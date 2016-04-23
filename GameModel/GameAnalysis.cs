using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameModel
{
    public class GameAnalysis
    {
        public double[] ObjectiveFunctionValue;
        public double[] StructureStrength;

        public double[] UnitStrength;
        public double[] ObjectiveFunction(int[] players, List<Structure> structures, List<MilitaryUnit> units)
        {
            ObjectiveFunctionValue = new double[players.Length];
            StructureStrength = new double[players.Length];

            UnitStrength = new double[players.Length];


            var structuresByPlayer = structures.GroupBy(x => x.OwnerId).ToList();
            var unitsByPlayer = units.GroupBy(x => x.OwnerId).ToList();


            for (var i = 0; i < players.Length; i++)
            {
                StructureStrength[i] = structuresByPlayer[i].Average(x => Board.StructureDefenceModifiers[x.StructureType]);

                UnitStrength[i] = unitsByPlayer[i].Average(x => x.Strength);

                ObjectiveFunctionValue[i] = StructureStrength[i] + UnitStrength[i] * .5;
            }

            return ObjectiveFunctionValue;
        }
    }
}
