using System;
using System.Collections.Generic;
using System.Linq;

namespace GameModel
{
    public class OrderResolver
    {
        private readonly Board _board;

        public OrderResolver(Board board)
        {
            _board = board;
        }

        public void ResolveOrders(List<IUnitOrder> unitOrders)
        {
            ResolveTransportOrders(unitOrders);
            UnloadOrders(unitOrders);
            ResolveMoves(unitOrders);
            UnloadOrders(unitOrders);

            ResolveTransportOrders(unitOrders);
        }

        private static void UnloadOrders(List<IUnitOrder> unitOrders)
        {
            var unloadOrders = unitOrders
                .OfType<UnloadOrder>()
                .Where(x => x.Unit.TransportedBy != null && (x.Destination == null || x.Destination == x.Unit.Location))
                .ToList();

            unloadOrders.ForEach(x =>
            {
                x.Unit.TransportedBy.Transporting.Remove(x.Unit);
                x.Unit.TransportedBy = null;
            });
        }

        private static void ResolveTransportOrders(List<IUnitOrder> unitOrders)
        {
            var transportOrders = unitOrders.OfType<TransportOrder>().ToList();

            transportOrders.ForEach(x =>
            {
                if (x.Unit.Location == x.UnitToTransport.Location && x.Unit.CanTransport(x.UnitToTransport))
                {
                    x.Unit.Transporting.Add(x.UnitToTransport);
                    x.UnitToTransport.TransportedBy = x.Unit;
                }
            });
        }

        private void ResolveMoves(List<IUnitOrder> unitOrders)
        {
            var moveOrders = unitOrders.OfType<MoveOrder>().ToList();

            if (moveOrders == null || moveOrders.Count == 0)
                return;

            _board.MoveOrders[_board.Turn] = moveOrders;

            float maxMovementPoints = 12;

            var transportedUnitMoveOrder = moveOrders.FirstOrDefault(x => x.Unit.TransportedBy != null);
            if (transportedUnitMoveOrder != null)
            {
                throw new Exception($"Unit {transportedUnitMoveOrder.Unit.Name} is being transported and therefore may not submit move orders");
            }

            var invalidMoveOrders = moveOrders.Where(x => x.Moves[0].Origin != x.Unit.Location);
            if (invalidMoveOrders.Count() > 0)
            {
                throw new Exception("The following units received orders to move from a location where they don't currently reside: " + string.Join(", ", invalidMoveOrders.Select(x => x.Unit + ". Ordered " + x.Moves[0])));
            }

            if (moveOrders.Max(x => x.Moves.Length) > maxMovementPoints)
                throw new Exception(string.Format("The max number of moves is capped at {0}. A move order has exceeded this limit.", maxMovementPoints));

            moveOrders.ForEach(x =>
                {
                    if (x.Moves.Length > x.Unit.MovementPoints + x.Unit.RoadMovementBonus)
                        throw new Exception($"Number of moves for {x.Unit} = {x.Moves.Length} exceeds the max number of moves permitted for the unit of {x.Unit.MovementPoints} moves with a road move bonus of {x.Unit.RoadMovementBonus}");
                }
            );

            var unitStepRate = new Dictionary<MilitaryUnit, int>();
            moveOrders.ForEach(x => unitStepRate.Add(x.Unit, (int)Math.Round(maxMovementPoints / (x.Moves.Length > x.Unit.MovementPoints ? (x.Unit.MovementPoints + x.Unit.RoadMovementBonus) : x.Unit.MovementPoints))));

            for (var step = 1; step <= maxMovementPoints; step++)
            {
                var unitStepMoves = MoveUnitsOneStep(moveOrders, unitStepRate, step);

                var removeUnitMoves = new Dictionary<MilitaryUnit, Move>();
                foreach (var stepMove in unitStepMoves)
                {
                    if (unitStepMoves.Any(x => x.Value.Edge.Destination == stepMove.Value.Origin && x.Key.OwnerIndex != stepMove.Key.OwnerIndex))
                    {
                        var originStrength = _board.UnitsAt(stepMove.Value.Origin).Where(x => x.OwnerIndex == stepMove.Key.OwnerIndex).Sum(x => x.Strength);
                        var destinationStrength = _board.UnitsAt(stepMove.Value.Edge.Destination).Where(x => x.OwnerIndex != stepMove.Key.OwnerIndex).Sum(x => x.Strength);

                        if (originStrength <= destinationStrength)
                        {
                            removeUnitMoves.Add(stepMove.Key, stepMove.Value);
                        }
                    }
                }

                unitStepMoves.Where((KeyValuePair<MilitaryUnit, Move> x) => x.Value.Edge.Destination.BaseTerrainType == BaseTerrainType.Water && x.Key.MovementType == MovementType.Land)
                    .ToList()
                    .ForEach((KeyValuePair<MilitaryUnit, Move> x) =>
                    {
                        var transportedUnit = x.Key;
                        var transports = _board.Units.Where(y => y.MovementType == MovementType.Waterbound && x.Value.Edge.Destination.Hex == y.Location.Hex && y.CanTransport(transportedUnit)).OrderBy(y => y.TransportSize);
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

                    if (unitStepMove.Value.MoveType != MoveType.Road)
                    {
                        if (unit.MoraleMoveCost[unit.BaseMovementPoints - unitStepMove.Value.MovesRemaining] > 0)
                        {
                            unit.ChangeMorale(_board.Turn, -unit.MoraleMoveCost[unit.BaseMovementPoints - unitStepMove.Value.MovesRemaining], "Morale reduced during forced march");
                        }
                    }
                }

                var conflictedUnits = DetectConflictedUnits(moveOrders.Select(x => x.Unit).ToList(), _board.Units.Where(x => x.IsAlive));
                moveOrders.RemoveAll(x => conflictedUnits.Contains(x.Unit));
            }
        }

        public static IEnumerable<MilitaryUnit> DetectConflictedUnits(List<MilitaryUnit> setOfUnits, IEnumerable<MilitaryUnit> allUnits)
        {
            var conflictedUnits = new List<MilitaryUnit>();
            setOfUnits.ForEach(x =>
            {
                if (conflictedUnits.Contains(x))
                    return;

                if (allUnits.Any(u => MilitaryUnit.IsInConflictDuringMovement(u, x)))
                {
                    conflictedUnits.Add(x);
                }
            });

            return conflictedUnits;
        }

        private static Dictionary<MilitaryUnit, Move> MoveUnitsOneStep(List<MoveOrder> moveOrders, Dictionary<MilitaryUnit, int> unitStepRate, int step)
        {
            var unitStepMoves = new Dictionary<MilitaryUnit, Move>();
            foreach (var moveOrder in moveOrders)
            {
                if (step % unitStepRate[moveOrder.Unit] == 0)
                {
                    var moveIndex = step / unitStepRate[moveOrder.Unit] - 1;
                    if (moveOrder.Moves.Length > moveIndex)
                        unitStepMoves.Add(moveOrder.Unit, moveOrder.Moves[moveIndex]);
                }
            }
            return unitStepMoves;
        }
    }
}
