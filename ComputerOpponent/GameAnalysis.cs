using GameModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ComputerOpponent
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
            SettlementCount,
            SettlementStrength,
            UnitStrength
        }

        public double[] CalculateObjectiveFunction(Player[] players, List<Settlement> settlements, List<MilitaryUnit> units)
        {
            ObjectiveFunctionValue = new double[players.Length];

            ObjectiveFunctionParameters = new Dictionary<ObjFuncParameter, double>[players.Length];
            ObjectiveFunctionWeightedParameters = new Dictionary<ObjFuncParameter, double>[players.Length];
            ObjectiveFunctionNormalisedParameters = new Dictionary<ObjFuncParameter, double>[players.Length];
            MaxParameterValue = new double[players.Length];

            var settlementsByPlayer = settlements.GroupBy(x => x.OwnerIndex).ToList();
            var unitsByPlayer = units.GroupBy(x => x.OwnerIndex).ToList();

            ObjectiveFunctionParameterWeight = new Dictionary<ObjFuncParameter, double>
            {
                { ObjFuncParameter.SettlementCount, 2 },
                { ObjFuncParameter.SettlementStrength, 1 },
                { ObjFuncParameter.UnitStrength, .001 },
            };

            for (var i = 0; i < players.Length; i++)
            {
                ObjectiveFunctionParameters[i] = [];
                ObjectiveFunctionWeightedParameters[i] = [];
                ObjectiveFunctionNormalisedParameters[i] = [];

                ObjectiveFunctionParameters[i].Add(ObjFuncParameter.SettlementCount, settlementsByPlayer[i].Count());
                ObjectiveFunctionParameters[i].Add(ObjFuncParameter.SettlementStrength, settlementsByPlayer[i].Sum(x => Settlement.SettlementDefenceModifier(x.SettlementType)));
                ObjectiveFunctionParameters[i].Add(ObjFuncParameter.UnitStrength, unitsByPlayer[i].Sum(x => x.Strength));

                foreach (ObjFuncParameter parameter in Enum.GetValues<ObjFuncParameter>())
                {
                    ObjectiveFunctionWeightedParameters[i][parameter] = ObjectiveFunctionParameters[i][parameter] * ObjectiveFunctionParameterWeight[parameter];
                }

                MaxParameterValue[i] = ObjectiveFunctionWeightedParameters[i].Values.Max();

                foreach (ObjFuncParameter parameter in Enum.GetValues<ObjFuncParameter>())
                {
                    ObjectiveFunctionNormalisedParameters[i].Add(parameter, ObjectiveFunctionWeightedParameters[i][parameter] / MaxParameterValue[i]);
                    ObjectiveFunctionValue[i] += ObjectiveFunctionNormalisedParameters[i][parameter] ;
                }
            }

            return ObjectiveFunctionValue;
        }
    }
}
