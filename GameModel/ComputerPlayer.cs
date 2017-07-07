using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameModel
{
    public class ComputerPlayer
    {

        static Dictionary<Role, double> FriendlyUnitInfluenceModifier
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
        static Dictionary<Role, double> _friendlyUnitInfluenceModifier;
        static Dictionary<Role, double> EnemyUnitInfluenceModifier
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

        static Dictionary<Role, double> FriendlyStructureInfluence
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
        static Dictionary<Role, double> _friendlyStructureInfluence;

        static Dictionary<Role, double> EnemyStructureInfluence
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

        public static void SetStrategicAction(Board board, List<MilitaryUnit> units)
        {
            units
                .ForEach(unit =>
                {
                    switch (unit.MovementType)
                    {
                        case MovementType.Airborne:
                            if (!unit.Transporting.Any() && !board.Units.Any(x => x.Location.ContiguousRegionId == unit.Location.ContiguousRegionId && x.OwnerIndex != unit.OwnerIndex))
                            {
                                unit.StrategicAction = StrategicAction.Pickup;
                            }
                            else if (unit.Transporting.Any())
                            {
                                unit.StrategicAction = StrategicAction.Airlift;
                            }
                            break;
                        case MovementType.Land:
                            if (unit.TransportedBy == null && unit.Role != Role.Defensive && !board.Structures.Any(x => x.Location.ContiguousRegionId == unit.Location.ContiguousRegionId && x.OwnerIndex != unit.OwnerIndex))
                            {
                                unit.StrategicAction = StrategicAction.Embark;
                            }
                            else if (unit.TransportedBy != null)
                            {
                                unit.StrategicAction = StrategicAction.Disembark;
                            }
                            else
                                unit.StrategicAction = StrategicAction.None;
                            break;
                        case MovementType.Water:
                            if ((!unit.Transporting.Any() && 
                                    !board.Units.Any(x => x.Location.ContiguousRegionId == unit.Location.ContiguousRegionId && x.OwnerIndex != unit.OwnerIndex))
                                    ||
                                    (unit.Location.HasPort && board.Structures.Any(x => x.Location.ContiguousRegionId == unit.Location.PortDestination.ContiguousRegionId && x.OwnerIndex != unit.OwnerIndex)))
                            {
                                unit.StrategicAction = StrategicAction.Dock;
                            }
                            else if (unit.Transporting.Any())
                            {
                                unit.StrategicAction = StrategicAction.Transport;
                            }
                            break;
                    }

                });
        }

        public static List<IUnitOrder> CreateOrders(Board board, List<MilitaryUnit> units)
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

        private static List<IUnitOrder> CreateOrdersForUnit(Board board, List<MilitaryUnit> units, List<IUnitOrder> existingOrders, MilitaryUnit unit)
        {
            var unitOrders = new List<IUnitOrder>();

            switch (unit.StrategicAction)
            {
                case StrategicAction.None:
                    {
                        var moveOrder = FindBestMoveOrderForUnit(unit, board);
                        if (moveOrder != null)
                            unitOrders.Add(moveOrder);
                        break;
                    }
                case StrategicAction.Embark:
                    Func<MilitaryUnit, bool> airborneRule = (x) => x.MovementType == MovementType.Airborne && x.StrategicAction == StrategicAction.Pickup;
                    var closestAvailableAirborneUnitPath = ClosestAvailableTransportPath(board, unit, units, airborneRule);

                    //Func<MilitaryUnit, bool> aquaticRule = (x) => x.MovementType == MovementType.Water && x.StrategicAction == StrategicAction.Dock;
                    //var closestAvailableWaterUnitPath = ClosestAvailableTransportPath(board, unit, units, aquaticRule);

                    var closestPortPath = ClosestPortPath(board, unit);

                    if (closestAvailableAirborneUnitPath != null)
                    {
                        if (closestPortPath == null || closestAvailableAirborneUnitPath.Path.Count() < closestPortPath.Count())
                        {
                            // Transport by air
                            var transporter = closestAvailableAirborneUnitPath.Unit;

                            var pathFindTiles = board.ValidMovesWithMoveCostsForUnit(unit);
                            var pathToAirbornUnit = FindShortestPath(pathFindTiles, unit.Location.Point, transporter.Location.Point, unit.MovementPoints);
                            Tile transporteeMoveOrderDesintation = null;
                            if (pathToAirbornUnit != null)
                            {
                                var moveOrder = unit.ShortestPathToMoveOrder(pathToAirbornUnit.ToArray());
                                transporteeMoveOrderDesintation = moveOrder.Moves.Last().Destination;
                                unitOrders.Add(moveOrder);
                            }
                            unitOrders.Add(new TransportOrder(transporter, unit));
                            break;
                        }
                    }

                    if (unit.Location.HasPort)
                    {
                        var waterTiles = unit.Location.Edges.Where(x => x.EdgeType == EdgeType.Port).Select(x => x.Destination).ToList();
                        var transportingUnits = units.Where(x => x.IsAlive && x.IsTransporter && waterTiles.Contains(x.Location) && x.CanTransport(unit))
                                                        .OrderByDescending(x => x.TransportSize);
                        var transportUnit = transportingUnits.FirstOrDefault();
                        if (transportUnit != null)
                        {
                            var moveToTransport = unit.PossibleMoves().SingleOrDefault(x => x.Destination == transportUnit.Location);

                            if (moveToTransport != null)
                                unitOrders.Add(moveToTransport.GetMoveOrder(unit));
                        }
                    }
                    else
                    {
                        var moveOrder = unit.GetMoveOrderToDestination(closestPortPath.Last().Point, board);
                        if (moveOrder != null)
                            unitOrders.Add(moveOrder);
                    }
                    
                    break;
                case StrategicAction.Disembark:
                    if (unit.TransportedBy.MovementType == MovementType.Airborne)
                    {
                        if (unit.TerrainMovementCosts[unit.Location.TerrainType] != null && board.Structures.Any(x => x.OwnerIndex != unit.OwnerIndex && x.Location.ContiguousRegionId == unit.Location.ContiguousRegionId))
                        {
                            unitOrders.Add(new UnloadOrder(unit));
                        }
                    }
                    if (unit.TransportedBy.MovementType == MovementType.Water)
                    {
                        if (board.Structures.Any(y => unit.Location.Edges.Any(z => z.EdgeType == EdgeType.Port && z.Destination.ContiguousRegionId == y.Location.ContiguousRegionId) && y.OwnerIndex != unit.OwnerIndex))
                        {
                            unitOrders.Add(unit.PossibleMoves().First().GetMoveOrder(unit));
                            unit.TransportedBy.Transporting.Remove(unit);
                            unit.TransportedBy = null;
                        }
                    }
                    break;

                case StrategicAction.Dock:
                    {
                        if (!unit.Location.HasPort || !units.Any(x => x.Location.ContiguousRegionId == unit.Location.PortDestination.ContiguousRegionId && x.StrategicAction == StrategicAction.Embark))
                        {
                            closestPortPath = ClosestPortPath(board, unit);

                            if (closestPortPath != null)
                            {
                                var moveOrder = unit.GetMoveOrderToDestination(closestPortPath.Last().Point, board);
                                if (moveOrder != null)
                                    unitOrders.Add(moveOrder);
                            }
                        }
                        break;
                    }
                case StrategicAction.Transport:
                    {
                        // Find the closest port that has a region with one or more enemy structures
                        closestPortPath = ClosestPortPath(board, unit);

                        if (closestPortPath != null)
                        {
                            var moveOrder = unit.GetMoveOrderToDestination(closestPortPath.Last().Point, board);
                            if (moveOrder != null)
                                unitOrders.Add(moveOrder);
                        }

                        break;
                    }
                case StrategicAction.Pickup:
                    {
                        var closestUnit = ClosestEmbarkingUnitPath(board, units, unit.Location);

                        var destination = closestUnit.Location.Point;

                        var transporteeMoveOrder = existingOrders.OfType<MoveOrder>().SingleOrDefault(x => x.Unit == closestUnit);
                        if (transporteeMoveOrder != null)
                        {
                            destination = transporteeMoveOrder.Moves.Last().Destination.Point;
                        }

                        // Move transport unit to the destination of the transportee's move order or just to the transportee's location
                        var pathFindTiles = board.ValidMovesWithMoveCostsForUnit(unit);
                        var pathToTransporteesDestination = FindShortestPath(pathFindTiles, unit.Location.Point, destination, unit.MovementPoints);
                        if (pathToTransporteesDestination != null)
                            unitOrders.Add(unit.ShortestPathToMoveOrder(pathToTransporteesDestination.ToArray()));

                        break;
                    }
                case StrategicAction.Airlift:
                    {

                        var closestEnemyStructure = ClosestEnemyStructurePath(board, unit);
                        if (closestEnemyStructure != null)
                        {
                            var moveOrder = unit.ShortestPathToMoveOrder(closestEnemyStructure.ToArray());
                            if (moveOrder != null)
                                unitOrders.Add(moveOrder);

                            if (board.Structures.Any(x => x.OwnerIndex != unit.OwnerIndex && x.Location.ContiguousRegionId == moveOrder.Moves.Last().Destination.ContiguousRegionId))
                            {
                                unit.Transporting.ForEach(x => unitOrders.Add(new UnloadOrder(x, moveOrder.Moves.Last().Destination)));
                            }
                        }

                        break;
                    }
            }
            return unitOrders;
        }

        private static MilitaryUnit ClosestEmbarkingUnitPath(Board board, List<MilitaryUnit> units, Tile origin)
        {
            var closestUnit = units
                                    .Where(x => x.StrategicAction == StrategicAction.Embark)
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
                var pathFindTiles = board.ValidMovesWithMoveCostsForUnit(potentialPickupUnit);
                var shortestPath = FindShortestPath(pathFindTiles, unit.Location.Point, potentialPickupUnit.Location.Point, potentialPickupUnit.MovementPoints);
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
                var pathFindTiles = board.ValidMovesWithMoveCostsForUnit(unit);
                var shortestPath = FindShortestPath(pathFindTiles, unit.Location.Point, enemyStructure.Location.Point, unit.MovementPoints);
                if (shortestPath != null)
                {
                    return shortestPath;
                }
            }
            return null;
        }
        public static IEnumerable<PathFindTile> ClosestPortPath(Board board, MilitaryUnit unit)
        {
            var closestPortDistance = int.MaxValue;
            IEnumerable<PathFindTile> closestPort = null;
            board.Tiles.ToList().ForEach(x =>
                {
                    if (x.ContiguousRegionId == unit.Location.ContiguousRegionId && x.HasPort)
                    {
                        switch (unit.StrategicAction)
                        {
                            case StrategicAction.Dock:
                                // Only go to a port that has units that want to embark
                                if (!board.Units.Any(y => x.Edges.Any(z => z.EdgeType == EdgeType.Port && z.Destination.ContiguousRegionId == y.Location.ContiguousRegionId) && y.StrategicAction == StrategicAction.Embark))
                                    return;
                                break;
                            case StrategicAction.Transport:
                                // Only go to a port that has enemy structure(s)
                                if (!board.Structures.Any(y => x.Edges.Any(z => z.EdgeType == EdgeType.Port && z.Destination.ContiguousRegionId == y.Location.ContiguousRegionId) && y.OwnerIndex != unit.OwnerIndex))
                                    return;
                                break;
                        }

                        var pathFindTiles = board.ValidMovesWithMoveCostsForUnit(unit);
                        var shortestPath = FindShortestPath(pathFindTiles, unit.Location.Point, x.Point, unit.MovementPoints);
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
                }
            );

            return closestPort;
        }

        public static List<Move> MovesFromShortestPath(List<Move> possibleMoves, PathFindTile[] shortestPath)
        {
            List<Move> moves = new List<Move>();
            Move furthestMove = null;
            var origin = shortestPath[0].Point;
            for (var i = 1; i < shortestPath.Length; i++)
            {
                var move = possibleMoves.FirstOrDefault(x => origin == x.Origin.Point && x.Destination.Point == shortestPath[i].Point && x.Distance == i);

                if (move == null)
                {
                    while (furthestMove != null && furthestMove.MoveType == MoveType.OnlyPassingThrough)
                    {
                        moves.Remove(furthestMove);
                        furthestMove = furthestMove.PreviousMove;
                    }
                    return moves;
                }

                moves.Add(move);
                furthestMove = move;

                // Remove moves that we've considered
                possibleMoves.RemoveAll(x => x.Origin.Point == origin);
                origin = shortestPath[i].Point;
            }

            return moves;
        }

        static Dictionary<Role, double> _enemyUnitInfluenceModifier;



        public static void GenerateInfluenceMaps(Board board, int numberOfPlayers)
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

        private static void CalculateAggregateInfluence(Tile tile, int playerIndex, Role role, MovementType movementType)
        {
            var rmt = new RoleMovementType(movementType, role);

            tile.AggregateInfluence[rmt][playerIndex] = 
                  (tile.FriendlyUnitInfluence[playerIndex] * FriendlyUnitInfluenceModifier[role])
                + (tile.EnemyUnitInfluence[playerIndex] * EnemyUnitInfluenceModifier[role])
                + (tile.FriendlyStructureInfluence[movementType][playerIndex] * FriendlyStructureInfluence[role]) 
                + (tile.EnemyStructureInfluence[movementType][playerIndex] * EnemyStructureInfluence[role]);
        }

        public static MoveOrder FindBestMoveOrderForUnit(MilitaryUnit unit, Board board)
        {
            var distance = 4;
            if (unit.MovementPoints > distance)
                distance += 3;

            if (unit.MovementPoints > distance)
                throw new Exception("Movement points are greater than the search area for movement");

            var results = Hex.HexesWithinArea(unit.Location.Hex, distance, board.Width, board.Height);

            var tilesOrderedInfluence = board.Tiles
                .Where(x => results.Contains(x.Hex))
                .OrderByDescending(x => x.AggregateInfluence[unit.RoleMovementType][unit.OwnerIndex] - (1 * FriendlyUnitInfluenceModifier[unit.Role]) / (Hex.Distance(x.Hex, unit.Location.Hex) + 1))
                .ToList();

            var pathFindTiles = board.ValidMovesWithMoveCostsForUnit(unit);

            IEnumerable<PathFindTile> bestPossibleDestination = null;
            foreach (var tile in tilesOrderedInfluence)
            {
                bestPossibleDestination = FindShortestPath(pathFindTiles, unit.Location.Point, tile.Point, unit.MovementPoints);
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

        public static IEnumerable<PathFindTile> FindShortestPath(List<PathFindTile> pathFindTiles, Point origin, Point destination, int maxCumulativeCost)
        {
            var ori = pathFindTiles.Single(x => x.X == origin.X && x.Y == origin.Y);
            var dest = pathFindTiles.Single(x => x.X == destination.X && x.Y == destination.Y);

            Func<PathFindTile, PathFindTile, double> distance = (node1, node2) => node1.MoveCost[node2];
            Func<PathFindTile, double> estimate = t => Math.Sqrt(Math.Pow(t.X - destination.X, 2) + Math.Pow(t.Y - destination.Y, 2));

            var path = PathFind.PathFind.FindPath(ori, dest, distance, estimate, maxCumulativeCost);

            return path == null || path.Count() == 1 ? null : path.Reverse();
        }

        public static List<Vector> PathFindTilesToVectors(IEnumerable<PathFindTile> path)
        {
            var pathArray = path.ToArray();

            var vectors = new List<Vector>();
            for (var i = 0; i < pathArray.Length - 1; i++)
            {
                vectors.Add(new Vector(pathArray[i].Point, pathArray[i + 1].Point, Colours.Black));
            }

            return vectors;
        }
    }
}
