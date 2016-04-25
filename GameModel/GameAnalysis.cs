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

        public Dictionary<ObjFuncParameter, double>[] ObjectiveFunctionParameters;
        public Dictionary<ObjFuncParameter, double>[] ObjectiveFunctionWeightedParameters;
        public Dictionary<ObjFuncParameter, double>[] ObjectiveFunctionNormalisedParameters;
        public Dictionary<ObjFuncParameter, double> ObjectiveFunctionParameterWeight;
        public double[] MaxParameterValue;
        public enum ObjFuncParameter
        {
            StructureCount,
            StructureStrength,
            UnitStrength
        }

        public double[] CalculateObjectiveFunction(Player[] players, List<Structure> structures, List<MilitaryUnit> units)
        {
            ObjectiveFunctionValue = new double[players.Length];

            ObjectiveFunctionParameters = new Dictionary<ObjFuncParameter, double>[players.Length];
            ObjectiveFunctionWeightedParameters = new Dictionary<ObjFuncParameter, double>[players.Length];
            ObjectiveFunctionNormalisedParameters = new Dictionary<ObjFuncParameter, double>[players.Length];
            MaxParameterValue = new double[players.Length];

            var structuresByPlayer = structures.GroupBy(x => x.OwnerId).ToList();
            var unitsByPlayer = units.GroupBy(x => x.OwnerId).ToList();

            ObjectiveFunctionParameterWeight = new Dictionary<ObjFuncParameter, double>
            {
                { ObjFuncParameter.StructureCount, 2 },
                { ObjFuncParameter.StructureStrength, 1 },
                { ObjFuncParameter.UnitStrength, .001 },
            };

            for (var i = 0; i < players.Length; i++)
            {
                ObjectiveFunctionParameters[i] = new Dictionary<ObjFuncParameter, double>();
                ObjectiveFunctionWeightedParameters[i] = new Dictionary<ObjFuncParameter, double>();
                ObjectiveFunctionNormalisedParameters[i] = new Dictionary<ObjFuncParameter, double>();

                ObjectiveFunctionParameters[i].Add(ObjFuncParameter.StructureCount, structuresByPlayer[i].Count());
                ObjectiveFunctionParameters[i].Add(ObjFuncParameter.StructureStrength, structuresByPlayer[i].Sum(x => 1 - Structure.StructureDefenceModifiers[x.StructureType]));
                ObjectiveFunctionParameters[i].Add(ObjFuncParameter.UnitStrength, unitsByPlayer[i].Sum(x => x.Strength));

                foreach (ObjFuncParameter parameter in Enum.GetValues(typeof(ObjFuncParameter)))
                {
                    ObjectiveFunctionWeightedParameters[i][parameter] = ObjectiveFunctionParameters[i][parameter] * ObjectiveFunctionParameterWeight[parameter];
                }

                MaxParameterValue[i] = ObjectiveFunctionWeightedParameters[i].Values.Max();

                foreach (ObjFuncParameter parameter in Enum.GetValues(typeof(ObjFuncParameter)))
                {
                    ObjectiveFunctionNormalisedParameters[i].Add(parameter, ObjectiveFunctionWeightedParameters[i][parameter] / MaxParameterValue[i]);
                    ObjectiveFunctionValue[i] += ObjectiveFunctionNormalisedParameters[i][parameter] ;
                }
            }

            return ObjectiveFunctionValue;
        }
    }
}
