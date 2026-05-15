namespace GameModel;

using System;
using System.Collections.Generic;
using System.Linq;
using GameModel.Commands;

public class CommandResolver(GameState gameState)
{
    private readonly GameState gameState = gameState;

    public void ResolveCommands(List<IUnitCommand> unitCommands)
    {
        ResolveTransportCommands(unitCommands);
        UnloadCommands(unitCommands);
        ResolveMoves(unitCommands);
        UnloadCommands(unitCommands);

        ResolveTransportCommands(unitCommands);
    }

    private static void UnloadCommands(List<IUnitCommand> unitCommands)
    {
        var unloadCommands = unitCommands
            .OfType<UnloadCommand>()
            .Where(x => x.Unit.TransportedBy != null && (x.Destination == null || x.Destination == x.Unit.Location))
            .ToList();

        unloadCommands.ForEach(x =>
        {
            x.Unit.TransportedBy.Transporting.Remove(x.Unit);
            x.Unit.TransportedBy = null;
        });
    }

    private static void ResolveTransportCommands(List<IUnitCommand> unitCommands)
    {
        var transportCommands = unitCommands.OfType<TransportCommand>().ToList();

        transportCommands.ForEach(x =>
        {
            if (x.Unit.Location == x.UnitToTransport.Location && x.Unit.CanTransport(x.UnitToTransport))
            {
                x.Unit.Transporting.Add(x.UnitToTransport);
                x.UnitToTransport.TransportedBy = x.Unit;
            }
        });
    }

    private void ResolveMoves(List<IUnitCommand> unitCommands)
    {
        var moveCommands = unitCommands.OfType<MoveCommand>().ToList();

        if (moveCommands == null || moveCommands.Count == 0)
        {
            return;
        }

        gameState.MoveCommands[gameState.Turn] = moveCommands;

        float maxMovementPoints = 12;

        var transportedUnitMoveOrder = moveCommands.FirstOrDefault(x => x.Unit.TransportedBy != null);
        if (transportedUnitMoveOrder != null)
        {
            throw new Exception($"Unit {transportedUnitMoveOrder.Unit.Name} is being transported and therefore may not submit move orders");
        }

        var invalidMoveCommands = moveCommands.Where(x => x.Moves[0].Origin != x.Unit.Location);
        if (invalidMoveCommands.Count() > 0)
        {
            throw new Exception("The following units received commands to move from a location where they don't currently reside: " + string.Join(", ", invalidMoveCommands.Select(x => x.Unit + ". Ordered " + x.Moves[0])));
        }

        if (moveCommands.Max(x => x.Moves.Length) > maxMovementPoints)
        {
            throw new Exception(string.Format("The max number of moves is capped at {0}. A move command has exceeded this limit.", maxMovementPoints));
        }

        moveCommands.ForEach(x =>
            {
                if (x.Moves.Length > x.Unit.MovementPoints + x.Unit.RoadMovementBonus)
                {
                    throw new Exception($"Number of moves for {x.Unit} = {x.Moves.Length} exceeds the max number of moves permitted for the unit of {x.Unit.MovementPoints} moves with a road move bonus of {x.Unit.RoadMovementBonus}");
                }
            }
        );

        var unitStepRate = new Dictionary<MilitaryUnit, int>();
        moveCommands.ForEach(x => unitStepRate.Add(x.Unit, (int)Math.Round(maxMovementPoints / (x.Moves.Length > x.Unit.MovementPoints ? (x.Unit.MovementPoints + x.Unit.RoadMovementBonus) : x.Unit.MovementPoints))));

        for (var step = 1; step <= maxMovementPoints; step++)
        {
            var unitStepMoves = MoveUnitsOneStep(moveCommands, unitStepRate, step);
            var removeUnitMoves = new Dictionary<MilitaryUnit, Move>();
            foreach (var stepMove in unitStepMoves)
            {
                if (unitStepMoves.Any(x => x.Value.Edge.Destination == stepMove.Value.Origin && x.Key.Owner.Id != stepMove.Key.Owner.Id))
                {
                    var originStrength = gameState.UnitsAt(stepMove.Value.Origin).Where(x => x.Owner.Id == stepMove.Key.Owner.Id).Sum(x => x.Strength);
                    var destinationStrength = gameState.UnitsAt(stepMove.Value.Edge.Destination).Where(x => x.Owner.Id != stepMove.Key.Owner.Id).Sum(x => x.Strength);

                    if (originStrength <= destinationStrength)
                    {
                        removeUnitMoves.Add(stepMove.Key, stepMove.Value);
                    }
                }
            }

            unitStepMoves.Where((KeyValuePair<MilitaryUnit, Move> x) => x.Value.Edge.Destination.BaseTerrainType == BaseTerrainType.Water && x.Key.MovementType == OperationalDomain.Land)
                .ToList()
                .ForEach((KeyValuePair<MilitaryUnit, Move> x) =>
                {
                    var transportedUnit = x.Key;
                    var transports = gameState.Units.Where(y => y.MovementType == OperationalDomain.Waterbound && x.Value.Edge.Destination.Hex == y.Location.Hex && y.CanTransport(transportedUnit)).OrderBy(y => y.TransportSize);
                    var transport = transports.FirstOrDefault();
                    if (transport != null)
                    {
                        transport.Transporting.Add(transportedUnit);
                        transportedUnit.TransportedBy = transport;
                    }
                    else
                    {
                        removeUnitMoves.Add(x.Key, x.Value);
                    }
                });

            removeUnitMoves.Keys.ToList().ForEach(x => unitStepMoves.Remove(x));

            foreach (var unitStepMove in unitStepMoves)
            {
                var unit = unitStepMove.Key;

                unit.Location = unitStepMove.Value.Edge.Destination;

                unit.Transporting.ForEach(x => x.Location = unitStepMove.Key.Location);
            }

            var conflictedUnits = DetectConflictedUnits(moveCommands.Select(x => x.Unit).ToList(), gameState.Units.Where(x => x.IsAlive));
            moveCommands.RemoveAll(x => conflictedUnits.Contains(x.Unit));
        }
    }

    public static IEnumerable<MilitaryUnit> DetectConflictedUnits(List<MilitaryUnit> setOfUnits, IEnumerable<MilitaryUnit> allUnits)
    {
        var conflictedUnits = new List<MilitaryUnit>();
        setOfUnits.ForEach(x =>
        {
            if (conflictedUnits.Contains(x))
            {
                return;
            }

            if (allUnits.Any(u => MilitaryUnit.IsInConflictDuringMovement(u, x)))
            {
                conflictedUnits.Add(x);
            }
        });

        return conflictedUnits;
    }

    private static Dictionary<MilitaryUnit, Move> MoveUnitsOneStep(List<MoveCommand> moveCommands, Dictionary<MilitaryUnit, int> unitStepRate, int step)
    {
        var unitStepMoves = new Dictionary<MilitaryUnit, Move>();
        foreach (var moveCommand in moveCommands)
        {
            if (step % unitStepRate[moveCommand.Unit] == 0)
            {
                var moveIndex = (step / unitStepRate[moveCommand.Unit]) - 1;
                if (moveCommand.Moves.Length > moveIndex)
                {
                    unitStepMoves.Add(moveCommand.Unit, moveCommand.Moves[moveIndex]);
                }
            }
        }

        return unitStepMoves;
    }
}
