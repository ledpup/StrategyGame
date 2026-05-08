using GameModel;
using Hexagon;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ComputerOpponent
{
    public enum StrategicAction
    {
        None,
        Dock,
        TransportToDestination,
        Embark,
        Disembark,
        Pickup,
        AirliftToDestination,
    }
    public class ComputerPlayer
    {
        public ComputerPlayer()
        {
            UnitStates = [];
        }

        public ComputerPlayer(List<MilitaryUnit> units)
            : this()
        {
            units.ForEach(unit => TrackUnit(unit));
        }

        public ComputerPlayer(Dictionary<MilitaryUnit, Role> unitRoles)
            : this()
        {
            foreach (var unitRole in unitRoles)
            {
                TrackUnit(unitRole.Key, unitRole.Value);
            }
        }
        Dictionary<Role, float> FriendlyUnitInfluenceModifier
        {
            get
            {
                if (_friendlyUnitInfluenceModifier == null)
                {
                    _friendlyUnitInfluenceModifier = [];
                    foreach (var role in Enum.GetValues<Role>())
                    {
                        _friendlyUnitInfluenceModifier.Add(role, 0.5f);
                    }
                    _friendlyUnitInfluenceModifier[Role.Defensive] = 1f;
                    _friendlyUnitInfluenceModifier[Role.Scout] = -0.5f;
                    _friendlyUnitInfluenceModifier[Role.Besieger] = -0.25f;
                }
                return _friendlyUnitInfluenceModifier;
            }
        }
        Dictionary<Role, float> _friendlyUnitInfluenceModifier;
        static Dictionary<Role, float> EnemyUnitInfluenceModifier
        {
            get
            {
                if (_enemyUnitInfluenceModifier == null)
                {
                    _enemyUnitInfluenceModifier = [];
                    foreach (var role in Enum.GetValues<Role>())
                    {
                        _enemyUnitInfluenceModifier.Add(role, 1f);
                    }
                    _enemyUnitInfluenceModifier[Role.Defensive] = -0.5f;
                    _enemyUnitInfluenceModifier[Role.Offensive] = 1.5f;
                    _enemyUnitInfluenceModifier[Role.Scout] = -0.5f;
                }
                return _enemyUnitInfluenceModifier;
            }
        }

        Dictionary<Role, float> FriendlyStructureInfluenceModifier
        {
            get
            {
                if (_friendlyStructureInfluence == null)
                {
                    _friendlyStructureInfluence = [];
                    foreach (var role in Enum.GetValues<Role>())
                    {
                        _friendlyStructureInfluence.Add(role, -1f);
                    }
                    _friendlyStructureInfluence[Role.Defensive] = 2f;
                    _friendlyStructureInfluence[Role.Scout] = -2f;
                    _friendlyStructureInfluence[Role.Besieger] = -2f;
                }
                return _friendlyStructureInfluence;
            }
        }
        Dictionary<Role, float> _friendlyStructureInfluence;

        static Dictionary<Role, float> EnemyStructureInfluenceModifier
        {
            get
            {
                if (_enemyStructureInfluence == null)
                {
                    _enemyStructureInfluence = [];
                    foreach (var role in Enum.GetValues<Role>())
                    {
                        _enemyStructureInfluence.Add(role, 1f);
                    }
                    _enemyStructureInfluence[Role.Besieger] = 2f;
                    _enemyStructureInfluence[Role.Defensive] = -2f;
                    _enemyStructureInfluence[Role.Scout] = 0.5f;
                }
                return _enemyStructureInfluence;
            }
        }

        public Dictionary<int, UnitAiState> UnitStates { get; set; }

        public Dictionary<int, Dictionary<RoleMovementType, float[]>> AggregateInfluence { get; private set; }
        public Dictionary<int, float[]> FriendlyUnitInfluence { get; private set; }
        public Dictionary<int, float[]> EnemyUnitInfluence { get; private set; }
        public Dictionary<int, Dictionary<MovementType, float[]>> FriendlyStructureInfluenceMap { get; private set; }
        public Dictionary<int, Dictionary<MovementType, float[]>> EnemyStructureInfluenceMap { get; private set; }

        public static List<Role> Roles
        {
            get
            {
                if (_roles == null)
                {
                    _roles = [.. Enum.GetValues<Role>()];
                }
                return _roles;
            }
        }
        static List<Role> _roles;

        public UnitAiState TrackUnit(MilitaryUnit unit, Role role = Role.Balanced)
        {
            var state = new UnitAiState(role);
            UnitStates[unit.Index] = state;
            return state;
        }

        public bool IsTracked(MilitaryUnit unit)
        {
            return unit != null && UnitStates.ContainsKey(unit.Index);
        }

        public UnitAiState GetUnitState(MilitaryUnit unit)
        {
            return UnitStates[unit.Index];
        }

        public UnitAiState TryGetUnitState(MilitaryUnit unit)
        {
            if (unit == null)
                return null;

            UnitStates.TryGetValue(unit.Index, out var state);
            return state;
        }
        public void SetStrategicAction(GameState board)
        {
            foreach (var unitState in UnitStates)
            {
                unitState.Value.StrategicAction = StrategicAction.None;

                var unit = board.Units.SingleOrDefault(x => x.Index == unitState.Key);
                if (unit == null || !unit.IsAlive)
                    continue;

                //var pathFindTiles = board.ValidMovesWithMoveCostsForUnit(unit);
                switch (unit.MovementType)
                {
                    case MovementType.Airborne:
                        // If there are any enemy land or airborne units that are nearby, don't do pickup or airlift
                        if (board.Units.Any(x => x.OwnerIndex != unit.OwnerIndex &&
                                    (x.MovementType == MovementType.Land || x.MovementType == MovementType.Airborne) &&
                                    (unit.Location == x.Location 
                                    || ShortestPathDistance(unit.Location, x.Location, unit) < unit.MovementPoints * 1.5)))
                        {
                            break;
                        }
                        if (!unit.Transporting.Any())
                        {
                            unitState.Value.StrategicAction = StrategicAction.Pickup;
                        }
                        else if (unit.Transporting.Any())
                        {
                            unitState.Value.StrategicAction = StrategicAction.AirliftToDestination;
                        }
                        break;
                    case MovementType.Land:
                        // Only embark if not already being transported, not in a defensive role, 
                        // and there are no enemy structures or units nearby
                        if (unit.TransportedBy == null &&
                                    unitState.Value.Role != Role.Defensive &&
                                    !board.Structures.Any(x => x.Location.ContiguousRegionId == unit.Location.ContiguousRegionId && x.OwnerIndex != unit.OwnerIndex) &&
                                    !board.Units.Any(x => x.Location.ContiguousRegionId == unit.Location.ContiguousRegionId && x.OwnerIndex != unit.OwnerIndex)
                                    )
                        {
                            unitState.Value.StrategicAction = StrategicAction.Embark;
                        }
                        else if (unit.TransportedBy != null)
                        {
                            unitState.Value.StrategicAction = StrategicAction.Disembark;
                        }
                        break;
                    case MovementType.Waterbound:
                        // If there are any enemy units nearby, don't dock or transport to destination
                        if (board.Units.Any(x => x.Location.ContiguousRegionId == unit.Location.ContiguousRegionId
                                            && x.OwnerIndex != unit.OwnerIndex
                                            && ShortestPathDistance(unit.Location, x.Location, unit) < unit.MovementPoints * 1.5))
                        {
                            break;
                        }
                        if (!unit.Transporting.Any())
                        {
                            unitState.Value.StrategicAction = StrategicAction.Dock;
                        }
                        else if (unit.Transporting.Any())
                        {
                            unitState.Value.StrategicAction = StrategicAction.TransportToDestination;
                        }
                        break;
                }
            }
        }

        public List<IUnitOrder> CreateOrders(GameState board, List<MilitaryUnit> units)
        {
            if (units.Any(x => !x.IsAlive))
                throw new Exception("Cannot assign orders to units that have been destroyed");

            var aiControlledUnits = units.Where(IsTracked).ToList();
            var unitOrders = new List<IUnitOrder>();

            var landAndWaterUnits = aiControlledUnits.Where(x => x.MovementType != MovementType.Airborne).ToList();
            landAndWaterUnits.ForEach(unit => unitOrders.AddRange(CreateOrdersForUnit(board, aiControlledUnits, null, unit)));

            var airborne = aiControlledUnits.Where(x => x.MovementType == MovementType.Airborne).ToList();
            airborne.ForEach(unit => unitOrders.AddRange(CreateOrdersForUnit(board, aiControlledUnits, unitOrders, unit)));

            return unitOrders;
        }

        private List<IUnitOrder> CreateOrdersForUnit(GameState board, List<MilitaryUnit> units, List<IUnitOrder> existingOrders, MilitaryUnit unit)
        {
            var unitOrders = new List<IUnitOrder>();

            var unitState = GetUnitState(unit);

            switch (unitState.StrategicAction)
            {
                case StrategicAction.None:
                    {
                        var moveOrder = FindBestMoveOrderForUnit(unit, board);
                        if (moveOrder != null)
                            unitOrders.Add(moveOrder);
                        break;
                    }
                case StrategicAction.Embark:
                    Func<MilitaryUnit, bool> airborneRule = (x) => x.MovementType == MovementType.Airborne && GetUnitState(x).StrategicAction == StrategicAction.Pickup;
                    var closestAvailableAirborneUnitPath = ClosestAvailableTransportPath(board, unit, units, airborneRule);

                    //Func<MilitaryUnit, bool> aquaticRule = (x) => x.MovementType == MovementType.Water && x.StrategicAction == StrategicAction.Dock;
                    //var closestAvailableWaterUnitPath = ClosestAvailableTransportPath(board, unit, units, aquaticRule);

                    var closestPortPath = ClosestPortPath(board, unit);

                    if (closestAvailableAirborneUnitPath != null)
                    {
                        if (closestPortPath == null || closestAvailableAirborneUnitPath.Path == null || closestAvailableAirborneUnitPath.Path.Count() < closestPortPath.Count())
                        {
                            // Transport by air
                            var transporter = closestAvailableAirborneUnitPath.Unit;

                            if (closestAvailableAirborneUnitPath.Path != null)
                            {
                                var pathToAirbornUnit = PathFinder.FindShortestPath(unit.Location, transporter.Location, unit);
                                Tile transporteeMoveOrderDesintation = null;
                                if (pathToAirbornUnit != null)
                                {
                                    var moveOrder = unit.ShortestPathToMoveOrder(pathToAirbornUnit.ToArray());
                                    transporteeMoveOrderDesintation = moveOrder.Moves.Last().Edge.Destination;
                                    unitOrders.Add(moveOrder);
                                }
                            }
                            unitOrders.Add(new TransportOrder(transporter, unit));
                            break;
                        }
                    }

                    if (unit.Location.HasPort)
                    {
                        var portEdges = board.Edges.Where(x => x.EdgeType == EdgeType.Port);

                        var transportingUnits = units.Where(x => x.IsAlive && x.IsTransporter && portEdges.Any(y => y.CrossesEdge(unit.Location, x.Location) && x.CanTransport(unit)))
                                                        .OrderByDescending(x => x.TransportSize);
                        var transportUnit = transportingUnits.FirstOrDefault();
                        if (transportUnit != null)
                        {
                            var moveToTransport = unit.PossibleMoves().SingleOrDefault(x => x.Edge.Destination == transportUnit.Location);

                            if (moveToTransport != null)
                                unitOrders.Add(moveToTransport.GetMoveOrder(unit));
                        }
                    }
                    else
                    {
                        var dest = board[Hex.HexToIndex(closestPortPath.Last().Hex, board.Width, board.Height)];
                        var moveOrder = unit.GetMoveOrderToDestination(dest);
                        if (moveOrder != null)
                            unitOrders.Add(moveOrder);
                    }
                    
                    break;
                case StrategicAction.Disembark:
                    if (unit.TransportedBy.MovementType == MovementType.Airborne)
                    {
                        if (board.Structures.Any(x => x.OwnerIndex != unit.OwnerIndex && x.Location.ContiguousRegionId == unit.Location.ContiguousRegionId))
                        {
                            unitOrders.Add(new UnloadOrder(unit));
                        }
                    }
                    if (unit.TransportedBy.MovementType == MovementType.Waterbound)
                    {
                        var tileEdges = Edge.GetEdges(board.Edges, unit.Location);
                        if (board.Structures.Any(y => tileEdges.Any(z => 
                                                                        z.EdgeType == EdgeType.Port 
                                                                        && (z.Destination.ContiguousRegionId == y.Location.ContiguousRegionId) || (z.Origin.ContiguousRegionId == y.Location.ContiguousRegionId))
                                                                        && y.OwnerIndex != unit.OwnerIndex))
                        {
                            unitOrders.Add(unit.PossibleMoves().First().GetMoveOrder(unit));
                            unit.TransportedBy.Transporting.Remove(unit);
                            unit.TransportedBy = null;
                        }
                    }
                    break;

                case StrategicAction.Dock:
                    {
                        if (!unit.Location.HasPort || !units.Any(x => x.Location.ContiguousRegionId == unit.Location.PortDestination.ContiguousRegionId && GetUnitState(x).StrategicAction == StrategicAction.Embark))
                        {
                            closestPortPath = ClosestPortPath(board, unit);

                            if (closestPortPath != null)
                            {
                                var dest = board[Hex.HexToIndex(closestPortPath.Last().Hex, board.Width, board.Height)];
                                var moveOrder = unit.GetMoveOrderToDestination(dest);
                                if (moveOrder != null)
                                    unitOrders.Add(moveOrder);
                            }
                        }
                        break;
                    }
                case StrategicAction.TransportToDestination:
                    {
                        // Find the closest port that has a region with one or more enemy structures
                        closestPortPath = ClosestPortPath(board, unit);

                        if (closestPortPath != null)
                        {
                            var dest = board[Hex.HexToIndex(closestPortPath.Last().Hex, board.Width, board.Height)];
                            var moveOrder = unit.GetMoveOrderToDestination(dest);
                            if (moveOrder != null)
                                unitOrders.Add(moveOrder);
                        }

                        break;
                    }
                case StrategicAction.Pickup:
                    {
                        var closestUnit = ClosestEmbarkingUnitPath(board, units, unit.Location);

                        if (closestUnit != null)
                        {
                            var destination = closestUnit.Location;

                            var transporteeMoveOrder = existingOrders.OfType<MoveOrder>().SingleOrDefault(x => x.Unit == closestUnit);
                            if (transporteeMoveOrder != null)
                            {
                                destination = transporteeMoveOrder.Moves.Last().Edge.Destination;
                            }

                            
                            if (unit.Location == destination)
                                break;

                            // Move transport unit to the destination of the transportee's move order or just to the transportee's location
                            var pathToTransporteesDestination = PathFinder.FindShortestPath(unit.Location, destination, unit);
                            if (pathToTransporteesDestination != null)
                                unitOrders.Add(unit.ShortestPathToMoveOrder(pathToTransporteesDestination.ToArray()));

                        }
                        break;
                    }
                case StrategicAction.AirliftToDestination:
                    {

                        var closestEnemyStructure = ClosestEnemyStructurePath(board, unit);
                        if (closestEnemyStructure != null)
                        {
                            var moveOrder = unit.ShortestPathToMoveOrder(closestEnemyStructure.ToArray());
                            if (moveOrder != null)
                                unitOrders.Add(moveOrder);

                            if (board.Structures.Any(x => x.OwnerIndex != unit.OwnerIndex && x.Location.ContiguousRegionId == moveOrder.Moves.Last().Edge.Destination.ContiguousRegionId))
                            {
                                unit.Transporting.ForEach(x => unitOrders.Add(new UnloadOrder(x, moveOrder.Moves.Last().Edge.Destination)));
                            }
                        }

                        break;
                    }
            }
            return unitOrders;
        }

        private MilitaryUnit ClosestEmbarkingUnitPath(GameState board, List<MilitaryUnit> units, Tile origin)
        {
            var closestUnit = units
                                    .Where(x => GetUnitState(x).StrategicAction == StrategicAction.Embark)
                                    .OrderBy(x => Hex.Distance(x.Location.Hex, origin.Hex))
                                    .FirstOrDefault();

            if (closestUnit == null)
                return null;

            return closestUnit;

            //foreach (var closestUnit in closestUnits)
            //{
            //    var pathFindTiles = board.ValidMovesWithMoveCostsForUnit(closestUnit);
            //    var shortestPath = FindShortestPath(pathFindTiles, origin.Point, closestUnit.Location.Point, closestUnit.MovementPoints);
            //    if (shortestPath != null)
            //    {
            //        return new UnitAndPath { Unit = closestUnit, Path = shortestPath };
            //    }
            //}

            //return null;
        }
        private static UnitAndPath ClosestAvailableTransportPath(GameState board, MilitaryUnit unit, List<MilitaryUnit> units, Func<MilitaryUnit, bool> rule)
        {
            var potentialPickupUnits = units
                                    .Where(x => rule(x) && x.CanTransport(unit))
                                    .OrderBy(x => Hex.Distance(x.Location.Hex, unit.Location.Hex));

            foreach(var potentialPickupUnit in potentialPickupUnits)
            {
                // Airborne unit already at the pickup location?
                if (unit.Location.Hex == potentialPickupUnit.Location.Hex)
                {
                    return new UnitAndPath { Unit = potentialPickupUnit };
                }

                var shortestPath = PathFinder.FindShortestPath(unit.Location, potentialPickupUnit.Location, potentialPickupUnit);
                if (shortestPath != null)
                {
                    return new UnitAndPath { Unit = potentialPickupUnit, Path = shortestPath };
                }
            }

            return null;
        }

        static Dictionary<Role, float> _enemyStructureInfluence;

        public static IEnumerable<PathFindTile> ClosestEnemyStructurePath(GameState board, MilitaryUnit unit)
        {
            var structures = board.Structures
                .Where(x => x.OwnerIndex != unit.OwnerIndex)
                .OrderBy(x => Hex.Distance(unit.Location.Hex, x.Location.Hex))
                .ToList();

            foreach (var enemyStructure in structures)
            { 
                var shortestPath = PathFinder.FindShortestPath(unit.Location, enemyStructure.Location, unit);
                if (shortestPath != null)
                {
                    return shortestPath;
                }
            }
            return null;
        }
        public IEnumerable<PathFindTile> ClosestPortPath(GameState board, MilitaryUnit unit)
        {
            var unitState = GetUnitState(unit);
            var closestPortDistance = int.MaxValue;
            IEnumerable<PathFindTile> closestPort = null;
            board.Tiles.ToList().ForEach(x =>
                {
                    if (x.ContiguousRegionId == unit.Location.ContiguousRegionId && x.HasPort)
                    {
                        switch (unitState.StrategicAction)
                        {
                            case StrategicAction.Dock:
                                // Only go to a port that has units that want to embark
                                if (!board.Units.Any(y => IsTracked(y) && x.Neighbours.Any(z => z.EdgeType == EdgeType.Port && z.Destination.ContiguousRegionId == y.Location.ContiguousRegionId) && GetUnitState(y).StrategicAction == StrategicAction.Embark))
                                    return;
                                break;
                            case StrategicAction.TransportToDestination:
                                // Only go to a port that has enemy structure(s)
                                if (!board.Structures.Any(y => x.Neighbours.Any(z => z.EdgeType == EdgeType.Port && z.Destination.ContiguousRegionId == y.Location.ContiguousRegionId) && y.OwnerIndex != unit.OwnerIndex))
                                    return;
                                break;
                        }

                        if (unit.Location == x)
                            return;

                        var shortestPath = PathFinder.FindShortestPath(unit.Location, x, unit);
                        if (shortestPath != null)
                        {
                            var distance = shortestPath.Count();
                            if (distance < closestPortDistance)
                            {
                                closestPortDistance = distance;
                                closestPort = shortestPath;
                            }
                        }
                    }
                });

            return closestPort;
        }



        static Dictionary<Role, float> _enemyUnitInfluenceModifier;



        public void GenerateInfluenceMaps(GameState board, int numberOfPlayers)
        {
            var aliveUnits = board.Units.Where(x => x.IsAlive).ToList();

            AggregateInfluence = [];
            FriendlyUnitInfluence = [];
            EnemyUnitInfluence = [];
            FriendlyStructureInfluenceMap = [];
            EnemyStructureInfluenceMap = [];

            board.Tiles.ToList().ForEach(x =>
            {
                FriendlyUnitInfluence[x.Index] = new float[numberOfPlayers];
                EnemyUnitInfluence[x.Index] = new float[numberOfPlayers];
                FriendlyStructureInfluenceMap[x.Index] = [];
                EnemyStructureInfluenceMap[x.Index] = [];

                MilitaryUnit.MovementTypes.ForEach(y => {
                    FriendlyStructureInfluenceMap[x.Index].Add(y, new float[numberOfPlayers]);
                    EnemyStructureInfluenceMap[x.Index].Add(y, new float[numberOfPlayers]);
                });

                var tileInfluence = new Dictionary<RoleMovementType, float[]>();
                Roles.ForEach(y => MilitaryUnit.MovementTypes.ForEach(z => tileInfluence.Add(new RoleMovementType(z, y), new float[numberOfPlayers])));
                AggregateInfluence[x.Index] = tileInfluence;
            });

            // Build reusable influence maps first, then copy values into the legacy dictionaries used by the AI decision code.
            var friendlyUnitMapsByPlayer = new BoardInfluenceMap[numberOfPlayers];
            var enemyUnitMapsByPlayer = new BoardInfluenceMap[numberOfPlayers];
            var friendlyStructureMapsByPlayer = new Dictionary<MovementType, BoardInfluenceMap>[numberOfPlayers];
            var enemyStructureMapsByPlayer = new Dictionary<MovementType, BoardInfluenceMap>[numberOfPlayers];

            for (var playerIndex = 0; playerIndex < numberOfPlayers; playerIndex++)
            {
                friendlyUnitMapsByPlayer[playerIndex] = new BoardInfluenceMap(board.Width, board.Height);
                enemyUnitMapsByPlayer[playerIndex] = new BoardInfluenceMap(board.Width, board.Height);
                friendlyStructureMapsByPlayer[playerIndex] = [];
                enemyStructureMapsByPlayer[playerIndex] = [];

                MilitaryUnit.MovementTypes.ForEach(movementType =>
                {
                    friendlyStructureMapsByPlayer[playerIndex].Add(movementType, new BoardInfluenceMap(board.Width, board.Height));
                    enemyStructureMapsByPlayer[playerIndex].Add(movementType, new BoardInfluenceMap(board.Width, board.Height));
                });
            }

            foreach (var unit in aliveUnits)
            {
                var unitMap = new BoardInfluenceMap(board.Width, board.Height);
                unitMap.AddRadialInfluence(unit.Location.Hex, 1f, 3);

                for (var index = 0; index < board.Tiles.Length; index++)
                {
                    if (!unit.CanStopOn.HasFlag(board[index].TerrainType))
                        continue;

                    var influence = unitMap.GetValue(index);
                    if (influence == 0f)
                        continue;

                    friendlyUnitMapsByPlayer[unit.OwnerIndex].AddValue(index, influence);

                    for (var playerIndex = 0; playerIndex < numberOfPlayers; playerIndex++)
                    {
                        if (playerIndex == unit.OwnerIndex)
                            continue;

                        enemyUnitMapsByPlayer[playerIndex].AddValue(index, influence);
                    }
                }
            }

            foreach (var structure in board.Structures)
            {
                var structureMap = new BoardInfluenceMap(board.Width, board.Height);
                structureMap.AddRadialInfluence(structure.Location.Hex, 1f, 5);

                for (var index = 0; index < board.Tiles.Length; index++)
                {
                    var influence = structureMap.GetValue(index);
                    if (influence == 0f)
                        continue;

                    for (var playerIndex = 0; playerIndex < numberOfPlayers; playerIndex++)
                    {
                        var isFriendlyForPlayer = structure.OwnerIndex == playerIndex;
                        var movementMapSet = isFriendlyForPlayer ? friendlyStructureMapsByPlayer[playerIndex] : enemyStructureMapsByPlayer[playerIndex];

                        // Air influence is always relevant, while land and water require same contiguous region.
                        movementMapSet[MovementType.Airborne].AddValue(index, influence);

                        if (structure.Location.ContiguousRegionId == board[index].ContiguousRegionId)
                        {
                            movementMapSet[MovementType.Land].AddValue(index, influence);
                            movementMapSet[MovementType.Waterbound].AddValue(index, influence);
                        }
                    }
                }
            }

            for (var playerIndex = 0; playerIndex < numberOfPlayers; playerIndex++)
            {
                foreach (var tile in board.Tiles)
                {
                    FriendlyUnitInfluence[tile.Index][playerIndex] = friendlyUnitMapsByPlayer[playerIndex].GetValue(tile.Index);
                    EnemyUnitInfluence[tile.Index][playerIndex] = enemyUnitMapsByPlayer[playerIndex].GetValue(tile.Index);

                    MilitaryUnit.MovementTypes.ForEach(movementType =>
                    {
                        FriendlyStructureInfluenceMap[tile.Index][movementType][playerIndex] = friendlyStructureMapsByPlayer[playerIndex][movementType].GetValue(tile.Index);
                        EnemyStructureInfluenceMap[tile.Index][movementType][playerIndex] = enemyStructureMapsByPlayer[playerIndex][movementType].GetValue(tile.Index);
                    });
                }

                Roles.ForEach(role =>
                {
                    MilitaryUnit.MovementTypes.ForEach(movementType =>
                    {
                        CalculateAggregateInfluence(
                            board,
                            playerIndex,
                            role,
                            movementType,
                            friendlyUnitMapsByPlayer[playerIndex],
                            enemyUnitMapsByPlayer[playerIndex],
                            friendlyStructureMapsByPlayer[playerIndex][movementType],
                            enemyStructureMapsByPlayer[playerIndex][movementType]);
                    });
                });
            }
        }

        private void CalculateAggregateInfluence(
            GameState board,
            int playerIndex,
            Role role,
            MovementType movementType,
            IInfluenceMap friendlyUnitMap,
            IInfluenceMap enemyUnitMap,
            IInfluenceMap friendlyStructureMap,
            IInfluenceMap enemyStructureMap)
        {
            var rmt = new RoleMovementType(movementType, role);

            // Combine the reusable influence layers with role-specific weights into one decision field.
            var combinedMap = BoardInfluenceMap.Combine(board.Width, board.Height,
            [
                (friendlyUnitMap, FriendlyUnitInfluenceModifier[role]),
                (enemyUnitMap, EnemyUnitInfluenceModifier[role]),
                (friendlyStructureMap, FriendlyStructureInfluenceModifier[role]),
                (enemyStructureMap, EnemyStructureInfluenceModifier[role])
            ]);

            foreach (var tile in board.Tiles)
            {
                AggregateInfluence[tile.Index][rmt][playerIndex] = combinedMap.GetValue(tile.Index);
            }
        }

        public MoveOrder FindBestMoveOrderForUnit(MilitaryUnit unit, GameState board)
        {
            var unitState = GetUnitState(unit);

            var distance = 4;
            if (unit.MovementPoints > distance)
                distance += 3;

            if (unit.MovementPoints > distance)
                throw new Exception("Movement points are greater than the search area for movement");

            var results = Hex.HexesWithinArea(unit.Location.Hex, distance, board.Width, board.Height);

            var roleMovementType = unitState.GetRoleMovementType(unit);
            var tilesOrderedInfluence = board.Tiles
                .Where(x => results.Contains(x.Hex))
                .OrderByDescending(x => AggregateInfluence[x.Index][roleMovementType][unit.OwnerIndex] - 1 * FriendlyUnitInfluenceModifier[unitState.Role] / (Hex.Distance(x.Hex, unit.Location.Hex) + 1))
                .ToList();

            IEnumerable<PathFindTile> bestPossibleDestination = null;
            foreach (var tile in tilesOrderedInfluence)
            {

                // Don't bother pathfinding if you're already there
                if (unit.Location.Equals(tile))
                    continue;

                // Don't attempt to pathfind to a location that the unit can't stop on
                if (!unit.CanStopOn.HasFlag(tile.TerrainType))
                    continue;

                bestPossibleDestination = PathFinder.FindShortestPath(unit.Location, tile, unit);

                if (bestPossibleDestination != null)
                    break;  
            }

            if (bestPossibleDestination != null)
            {
                var moveOrder = unit.ShortestPathToMoveOrder(bestPossibleDestination.ToArray());
                return moveOrder;
            }
            return null;
        }


        public static int ShortestPathDistance(Tile origin, Tile destination, MilitaryUnit unit)
        {
            var path = PathFinder.FindShortestPath(origin, destination, unit.MovementPoints, unit.UsesRoads, unit.IsBeingTransportedByWater, unit.EdgeMovementCosts, unit.TerrainMovementCosts, unit.CanStopOn);
            if (path == null)
                return int.MaxValue;
            return path.Count();
        }
    }
}
