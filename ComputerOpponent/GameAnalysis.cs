using GameModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ComputerOpponent;

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

        var settlementsByPlayer = settlements.GroupBy(x => x.Owner.Id).ToDictionary(x => x.Key, x => x.ToList());
        var unitsByPlayer = units.GroupBy(x => x.Owner.Id).ToDictionary(x => x.Key, x => x.ToList());

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

            settlementsByPlayer.TryGetValue(players[i].Id, out var playerSettlements);
            unitsByPlayer.TryGetValue(players[i].Id, out var playerUnits);
            playerSettlements ??= [];
            playerUnits ??= [];

            ObjectiveFunctionParameters[i].Add(ObjFuncParameter.SettlementCount, playerSettlements.Count);
            ObjectiveFunctionParameters[i].Add(ObjFuncParameter.SettlementStrength, playerSettlements.Sum(x => Settlement.SettlementDefenceModifier(x.SettlementType)));
            ObjectiveFunctionParameters[i].Add(ObjFuncParameter.UnitStrength, playerUnits.Sum(x => x.Strength));

            foreach (ObjFuncParameter parameter in Enum.GetValues<ObjFuncParameter>())
            {
                ObjectiveFunctionWeightedParameters[i][parameter] = ObjectiveFunctionParameters[i][parameter] * ObjectiveFunctionParameterWeight[parameter];
            }

            MaxParameterValue[i] = ObjectiveFunctionWeightedParameters[i].Values.Max();

            foreach (ObjFuncParameter parameter in Enum.GetValues<ObjFuncParameter>())
            {
                ObjectiveFunctionNormalisedParameters[i].Add(parameter, ObjectiveFunctionWeightedParameters[i][parameter] / MaxParameterValue[i]);
                ObjectiveFunctionValue[i] += ObjectiveFunctionNormalisedParameters[i][parameter];
            }
        }

        return ObjectiveFunctionValue;
    }
}
