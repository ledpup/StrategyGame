﻿using GameModel;
using Hexagon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            AiUnits = new Dictionary<int, AiMilitaryUnit>();
        }

        public ComputerPlayer(List<MilitaryUnit> units)
        {
            AiUnits = new Dictionary<int, AiMilitaryUnit>();
            units.ForEach(x => AiUnits.Add(x.Index, new AiMilitaryUnit(x)));
        }

        public ComputerPlayer(List<AiMilitaryUnit> aiUnits)
        {
            AiUnits = new Dictionary<int, AiMilitaryUnit>();
            aiUnits.ForEach(x => AiUnits.Add(x.Unit.Index, x));
        }
        Dictionary<Role, double> FriendlyUnitInfluenceModifier
        {
            get
            {
                if (_friendlyUnitInfluenceModifier == null)
                {
                    _friendlyUnitInfluenceModifier = new Dictionary<Role, double>();
                    foreach (var role in Enum.GetValues(typeof(Role)))
                    {
                        _friendlyUnitInfluenceModifier.Add((Role)role, 0.5);
                    }
                    _friendlyUnitInfluenceModifier[Role.Defensive] = 1;
                    _friendlyUnitInfluenceModifier[Role.Scout] = -0.5;
                    _friendlyUnitInfluenceModifier[Role.Besieger] = -0.25;
                }
                return _friendlyUnitInfluenceModifier;
            }
        }
        Dictionary<Role, double> _friendlyUnitInfluenceModifier;
        Dictionary<Role, double> EnemyUnitInfluenceModifier
        {
            get
            {
                if (_enemyUnitInfluenceModifier == null)
                {
                    _enemyUnitInfluenceModifier = new Dictionary<Role, double>();
                    foreach (var role in Enum.GetValues(typeof(Role)))
                    {
                        _enemyUnitInfluenceModifier.Add((Role)role, 1);
                    }
                    _enemyUnitInfluenceModifier[Role.Defensive] = -0.5;
                    _enemyUnitInfluenceModifier[Role.Offensive] = 1.5;
                    _enemyUnitInfluenceModifier[Role.Scout] = -0.5;
                }
                return _enemyUnitInfluenceModifier;
            }
        }

        Dictionary<Role, double> FriendlyStructureInfluence
        {
            get
            {
                if (_friendlyStructureInfluence == null)
                {
                    _friendlyStructureInfluence = new Dictionary<Role, double>();
                    foreach (var role in Enum.GetValues(typeof(Role)))
                    {
                        _friendlyStructureInfluence.Add((Role)role, -1);
                    }
                    _friendlyStructureInfluence[Role.Defensive] = 2;
                    _friendlyStructureInfluence[Role.Scout] = -2;
                    _friendlyStructureInfluence[Role.Besieger] = -2;
                }
                return _friendlyStructureInfluence;
            }
        }
        Dictionary<Role, double> _friendlyStructureInfluence;

        Dictionary<Role, double> EnemyStructureInfluence
        {
            get
            {
                if (_enemyStructureInfluence == null)
                {
                    _enemyStructureInfluence = new Dictionary<Role, double>();
                    foreach (var role in Enum.GetValues(typeof(Role)))
                    {
                        _enemyStructureInfluence.Add((Role)role, 1);
                    }
                    _enemyStructureInfluence[Role.Besieger] = 2;
                    _enemyStructureInfluence[Role.Defensive] = -2;
                    _enemyStructureInfluence[Role.Scout] = 0.5;
                }
                return _enemyStructureInfluence;
            }
        }

        public Dictionary<int, AiMilitaryUnit> AiUnits { get; set; }
        public void SetStrategicAction(Board board)
        {
            foreach (var aiUnit in AiUnits.Values)
            {
                aiUnit.StrategicAction = StrategicAction.None;

                var unit = aiUnit.Unit;
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
                            aiUnit.StrategicAction = StrategicAction.Pickup;
                        }
                        else if (unit.Transporting.Any())
                        {
                            aiUnit.StrategicAction = StrategicAction.AirliftToDestination;
                        }
                        break;
                    case MovementType.Land:
                        // Only embark if not already being transported, not in a defensive role, 
                        // and there are no enemy structures or units nearby
                        if (unit.TransportedBy == null &&
                                    aiUnit.Role != Role.Defensive &&
                                    !board.Structures.Any(x => x.Location.ContiguousRegionId == unit.Location.ContiguousRegionId && x.OwnerIndex != unit.OwnerIndex) &&
                                    !board.Units.Any(x => x.Location.ContiguousRegionId == unit.Location.ContiguousRegionId && x.OwnerIndex != unit.OwnerIndex)
                                    )
                        {
                            aiUnit.StrategicAction = StrategicAction.Embark;
                        }
                        else if (unit.TransportedBy != null)
                        {
                            aiUnit.StrategicAction = StrategicAction.Disembark;
                        }
                        break;
                    case MovementType.Water:
                        // If there are any enemy units nearby, don't dock or transport to destination
                        if (board.Units.Any(x => x.Location.ContiguousRegionId == unit.Location.ContiguousRegionId
                                            && x.OwnerIndex != unit.OwnerIndex
                                            && ShortestPathDistance(unit.Location, x.Location, unit) < unit.MovementPoints * 1.5))
                        {
                            break;
                        }
                        if (!unit.Transporting.Any())
                        {
                            aiUnit.StrategicAction = StrategicAction.Dock;
                        }
                        else if (unit.Transporting.Any())
                        {
                            aiUnit.StrategicAction = StrategicAction.TransportToDestination;
                        }
                        break;
                }
            }
        }

        public List<IUnitOrder> CreateOrders(Board board, List<MilitaryUnit> units)
        {
            if (units.Any(x => !x.IsAlive))
                throw new Exception("Cannot assign orders to units that have been destroyed");

            var unitOrders = new List<IUnitOrder>();

            var landAndWaterUnits = units.Where(x => x.MovementType != MovementType.Airborne).ToList();
            landAndWaterUnits.ForEach(unit => unitOrders.AddRange(CreateOrdersForUnit(board, units, null, unit)));

            var airborne = units.Where(x => x.MovementType == MovementType.Airborne).ToList();
            airborne.ForEach(unit => unitOrders.AddRange(CreateOrdersForUnit(board, units, unitOrders, unit)));

            return unitOrders;
        }

        private List<IUnitOrder> CreateOrdersForUnit(Board board, List<MilitaryUnit> units, List<IUnitOrder> existingOrders, MilitaryUnit unit)
        {
            var unitOrders = new List<IUnitOrder>();

            var aiUnit = AiUnits[unit.Index];

            switch (aiUnit.StrategicAction)
            {
                case StrategicAction.None:
                    {
                        var moveOrder = FindBestMoveOrderForUnit(aiUnit, board);
                        if (moveOrder != null)
                            unitOrders.Add(moveOrder);
                        break;
                    }
                case StrategicAction.Embark:
                    Func<MilitaryUnit, bool> airborneRule = (x) => x.MovementType == MovementType.Airborne && AiUnits[x.Index].StrategicAction == StrategicAction.Pickup;
                    var closestAvailableAirborneUnitPath = ClosestAvailableTransportPath(board, unit, units, airborneRule);

                    //Func<MilitaryUnit, bool> aquaticRule = (x) => x.MovementType == MovementType.Water && x.StrategicAction == StrategicAction.Dock;
                    //var closestAvailableWaterUnitPath = ClosestAvailableTransportPath(board, unit, units, aquaticRule);

                    var closestPortPath = ClosestPortPath(board, aiUnit);

                    if (closestAvailableAirborneUnitPath != null)
                    {
                        if (closestPortPath == null || closestAvailableAirborneUnitPath.Path == null || closestAvailableAirborneUnitPath.Path.Count() < closestPortPath.Count())
                        {
                            // Transport by air
                            var transporter = closestAvailableAirborneUnitPath.Unit;

                            if (closestAvailableAirborneUnitPath.Path != null)
                            {
                                var pathToAirbornUnit = Board.FindShortestPath(unit.Location, transporter.Location, unit);
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
                        var moveOrder = unit.GetMoveOrderToDestination(dest, board);
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
                    if (unit.TransportedBy.MovementType == MovementType.Water)
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
                        if (!unit.Location.HasPort || !units.Any(x => x.Location.ContiguousRegionId == unit.Location.PortDestination.ContiguousRegionId && aiUnit.StrategicAction == StrategicAction.Embark))
                        {
                            closestPortPath = ClosestPortPath(board, aiUnit);

                            if (closestPortPath != null)
                            {
                                var dest = board[Hex.HexToIndex(closestPortPath.Last().Hex, board.Width, board.Height)];
                                var moveOrder = unit.GetMoveOrderToDestination(dest, board);
                                if (moveOrder != null)
                                    unitOrders.Add(moveOrder);
                            }
                        }
                        break;
                    }
                case StrategicAction.TransportToDestination:
                    {
                        // Find the closest port that has a region with one or more enemy structures
                        closestPortPath = ClosestPortPath(board, aiUnit);

                        if (closestPortPath != null)
                        {
                            var dest = board[Hex.HexToIndex(closestPortPath.Last().Hex, board.Width, board.Height)];
                            var moveOrder = unit.GetMoveOrderToDestination(dest, board);
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
                            var pathToTransporteesDestination = Board.FindShortestPath(unit.Location, destination, unit);
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

        private MilitaryUnit ClosestEmbarkingUnitPath(Board board, List<MilitaryUnit> units, Tile origin)
        {
            var closestUnit = units
                                    .Where(x => AiUnits[x.Index].StrategicAction == StrategicAction.Embark)
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
        private static UnitAndPath ClosestAvailableTransportPath(Board board, MilitaryUnit unit, List<MilitaryUnit> units, Func<MilitaryUnit, bool> rule)
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

                var shortestPath = Board.FindShortestPath(unit.Location, potentialPickupUnit.Location, potentialPickupUnit);
                if (shortestPath != null)
                {
                    return new UnitAndPath { Unit = potentialPickupUnit, Path = shortestPath };
                }
            }

            return null;
        }

        static Dictionary<Role, double> _enemyStructureInfluence;

        public static IEnumerable<PathFindTile> ClosestEnemyStructurePath(Board board, MilitaryUnit unit)
        {
            var structures = board.Structures
                .Where(x => x.OwnerIndex != unit.OwnerIndex)
                .OrderBy(x => Hex.Distance(unit.Location.Hex, x.Location.Hex))
                .ToList();

            foreach (var enemyStructure in structures)
            { 
                var shortestPath = Board.FindShortestPath(unit.Location, enemyStructure.Location, unit);
                if (shortestPath != null)
                {
                    return shortestPath;
                }
            }
            return null;
        }
        public IEnumerable<PathFindTile> ClosestPortPath(Board board, AiMilitaryUnit aiUnit)
        {
            var closestPortDistance = int.MaxValue;
            IEnumerable<PathFindTile> closestPort = null;
            board.Tiles.ToList().ForEach(x =>
                {
                    if (x.ContiguousRegionId == aiUnit.Unit.Location.ContiguousRegionId && x.HasPort)
                    {
                        switch (aiUnit.StrategicAction)
                        {
                            case StrategicAction.Dock:
                                // Only go to a port that has units that want to embark
                                if (!board.Units.Any(y => x.Neighbours.Any(z => z.EdgeType == EdgeType.Port && z.Destination.ContiguousRegionId == y.Location.ContiguousRegionId) && AiUnits[y.Index].StrategicAction == StrategicAction.Embark))
                                    return;
                                break;
                            case StrategicAction.TransportToDestination:
                                // Only go to a port that has enemy structure(s)
                                if (!board.Structures.Any(y => x.Neighbours.Any(z => z.EdgeType == EdgeType.Port && z.Destination.ContiguousRegionId == y.Location.ContiguousRegionId) && y.OwnerIndex != aiUnit.Unit.OwnerIndex))
                                    return;
                                break;
                        }

                        if (aiUnit.Unit.Location == x)
                            return;

                        var shortestPath = Board.FindShortestPath(aiUnit.Unit.Location, x, aiUnit.Unit);
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



        static Dictionary<Role, double> _enemyUnitInfluenceModifier;



        public void GenerateInfluenceMaps(Board board, int numberOfPlayers)
        {
            var aliveUnits = board.Units.Where(x => x.IsAlive).ToList();

            board.Tiles.ToList().ForEach(x =>
            {
                x.FriendlyUnitInfluence = new double[numberOfPlayers];
                x.EnemyUnitInfluence = new double[numberOfPlayers];
                x.FriendlyStructureInfluence = new Dictionary<MovementType, double[]>();
                x.EnemyStructureInfluence = new Dictionary<MovementType, double[]>();

                MilitaryUnit.MovementTypes.ForEach(y => {
                    x.FriendlyStructureInfluence.Add(y, new double[numberOfPlayers]);
                    x.EnemyStructureInfluence.Add(y, new double[numberOfPlayers]);
                });

                x.AggregateInfluence = new Dictionary<RoleMovementType, double[]>();
                MilitaryUnit.Roles.ForEach(y => MilitaryUnit.MovementTypes.ForEach(z => x.AggregateInfluence.Add(new RoleMovementType(z, y), new double[numberOfPlayers])));

                aliveUnits.ForEach(y =>
                {
                    if (!x.TerrainAndWeatherInfluenceByUnit.ContainsKey(y.Index))
                        x.TerrainAndWeatherInfluenceByUnit.Add(y.Index, y.TerrainTypeBattleModifier[x.TerrainType] + y.WeatherBattleModifier[x.Weather]);
                });

            });

            foreach (var unit in aliveUnits)
            {
                var playerIndex = unit.OwnerIndex;
                CalculateUnitInfluence(board, numberOfPlayers, unit, playerIndex);
            }

            for (var i = 0; i < numberOfPlayers; i++)
            {
                foreach (var structure in board.Structures)
                {
                    for (var distance = 0; distance < 6; distance++)
                    {
                        var hexesInRing = Hex.HexRing(structure.Location.Hex, distance, board.Width, board.Height);

                        hexesInRing.ForEach(y =>
                        {
                            var index = Hex.HexToIndex(y, board.Width, board.Height);
                            if (index >= 0 && index < board.TileArray.Length)
                            {
                                if (structure.OwnerIndex == i)
                                {
                                    board[index].FriendlyStructureInfluence[MovementType.Airborne][i] += 1D / (distance + 1);
                                    if (structure.Location.ContiguousRegionId == board[index].ContiguousRegionId)
                                    {
                                        board[index].FriendlyStructureInfluence[MovementType.Land][i] += 1D / (distance + 1);
                                        board[index].FriendlyStructureInfluence[MovementType.Water][i] += 1D / (distance + 1);
                                    }
                                }
                                else
                                {
                                    board[index].EnemyStructureInfluence[MovementType.Airborne][i] += 1D / (distance + 1);
                                    if (structure.Location.ContiguousRegionId == board[index].ContiguousRegionId)
                                    {
                                        board[index].EnemyStructureInfluence[MovementType.Land][i] += 1D / (distance + 1);
                                        board[index].EnemyStructureInfluence[MovementType.Water][i] += 1D / (distance + 1);
                                    }
                                }  
                            }
                        });
                    }  
                }


                board.Tiles.ToList().ForEach(x =>
                {
                    MilitaryUnit.Roles
                        .ForEach(y => 
                        {
                            MilitaryUnit.MovementTypes.ForEach(z => CalculateAggregateInfluence(x, i, y, z));
                        });
                });
            }
        }

        private static void CalculateUnitInfluence(Board board, int numberOfPlayers, MilitaryUnit unit, int playerIndex)
        {
            for (var i = 0; i < 4; i++)
            {
                var hexesInRing = Hex.HexRing(unit.Location.Hex, i, board.Width, board.Height);

                hexesInRing.ForEach(x =>
                {
                    var index = Hex.HexToIndex(x, board.Width, board.Height);
                    if (index >= 0 && index < board.TileArray.Length)
                    {
                        if (unit.CanStopOn.HasFlag(board[index].TerrainType))
                        {
                            board[index].FriendlyUnitInfluence[playerIndex] += 1D / (i + 1);
                            for (var j = 0; j < numberOfPlayers; j++)
                            {
                                if (playerIndex == j)
                                    continue;

                                board[index].EnemyUnitInfluence[j] += 1D / (i + 1);
                            }
                        }
                    }
                });
            }
        }

        private void CalculateAggregateInfluence(Tile tile, int playerIndex, Role role, MovementType movementType)
        {
            var rmt = new RoleMovementType(movementType, role);

            tile.AggregateInfluence[rmt][playerIndex] = 
                  (tile.FriendlyUnitInfluence[playerIndex] * FriendlyUnitInfluenceModifier[role])
                + (tile.EnemyUnitInfluence[playerIndex] * EnemyUnitInfluenceModifier[role])
                + (tile.FriendlyStructureInfluence[movementType][playerIndex] * FriendlyStructureInfluence[role]) 
                + (tile.EnemyStructureInfluence[movementType][playerIndex] * EnemyStructureInfluence[role]);
        }

        public MoveOrder FindBestMoveOrderForUnit(AiMilitaryUnit aiUnit, Board board)
        {
            var unit = aiUnit.Unit;

            var distance = 4;
            if (unit.MovementPoints > distance)
                distance += 3;

            if (unit.MovementPoints > distance)
                throw new Exception("Movement points are greater than the search area for movement");

            var results = Hex.HexesWithinArea(unit.Location.Hex, distance, board.Width, board.Height);

            var tilesOrderedInfluence = board.Tiles
                .Where(x => results.Contains(x.Hex))
                .OrderByDescending(x => x.AggregateInfluence[aiUnit.RoleMovementType][unit.OwnerIndex] - (1 * FriendlyUnitInfluenceModifier[aiUnit.Role]) / (Hex.Distance(x.Hex, unit.Location.Hex) + 1))
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

                bestPossibleDestination = Board.FindShortestPath(unit.Location, tile, unit);

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
            var path = Board.FindShortestPath(origin, destination, unit.MovementPoints, unit.UsesRoads, unit.IsBeingTransportedByWater, unit.EdgeMovementCosts, unit.TerrainMovementCosts, unit.CanStopOn);
            if (path == null)
                return int.MaxValue;
            return path.Count();
        }
    }
}
