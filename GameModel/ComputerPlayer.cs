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

        public static Move MoveOrderFromShortestPath(List<Move> moves, PathFindTile[] shortestPath)
        {
            Move furthestMove = null;
            var origin = shortestPath[0].Point;
            for (var i = 1; i < shortestPath.Length; i++)
            {
                var move = moves.SingleOrDefault(x => origin == x.Origin.Point && x.Destination.Point == shortestPath[i].Point);

                if (move == null)
                    return furthestMove;

                furthestMove = move;

                // Remove moves that we've considered
                moves.RemoveAll(x => x.Origin.Point == origin);
                origin = shortestPath[i].Point;
            }

            return furthestMove;
        }

        static Dictionary<Role, double> _enemyUnitInfluenceModifier;

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
        static Dictionary<Role, double> _enemyStructureInfluence;

        public static void GenerateInfluenceMaps(Board board, int numberOfPlayers)
        {
            var aliveUnits = board.Units.Where(x => x.IsAlive).ToList();

            board.Tiles.ToList().ForEach(x =>
            {
                x.FriendlyUnitInfluence = new double[numberOfPlayers];
                x.EnemyUnitInfluence = new double[numberOfPlayers];
                x.FriendlyStructureInfluence = new double[numberOfPlayers];
                x.EnemyStructureInfluence = new double[numberOfPlayers];

                x.AggregateInfluence = new Dictionary<Role, double[]>();
                MilitaryUnit.Roles.ForEach(y => x.AggregateInfluence.Add(y, new double[numberOfPlayers]));

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
                    for (var distance = 0; distance < 5; distance++)
                    {
                        var hexesInRing = Hex.HexRing(structure.Tile.Hex, distance);

                        hexesInRing.ForEach(y =>
                        {
                            var index = Hex.HexToIndex(y, board.Width);
                            if (index >= 0 && index < board.TileArray.Length)
                            {
                                if (structure.OwnerIndex == i)
                                    board[index].FriendlyStructureInfluence[i] += 1D / (distance + 1);
                                else
                                    board[index].EnemyStructureInfluence[i] += 1D / (distance + 1);
                            }
                        });
                    }  
                }


                board.Tiles.ToList().ForEach(x =>
                {
                    MilitaryUnit.Roles
                        .ForEach(y => CalculateAggregateInfluence(x, i, y));
                });
            }
        }

        private static void CalculateUnitInfluence(Board board, int numberOfPlayers, MilitaryUnit unit, int playerIndex)
        {
            for (var i = 0; i < 4; i++)
            {
                var hexesInRing = Hex.HexRing(unit.Location.Hex, i);

                hexesInRing.ForEach(x =>
                {
                    var index = Hex.HexToIndex(x, board.Width);
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

        private static void CalculateAggregateInfluence(Tile tile, int playerIndex, Role role)
        {
            tile.AggregateInfluence[role][playerIndex] = 
                  (tile.FriendlyUnitInfluence[playerIndex] * FriendlyUnitInfluenceModifier[role])
                + (tile.EnemyUnitInfluence[playerIndex] * EnemyUnitInfluenceModifier[role])
                + (tile.FriendlyStructureInfluence[playerIndex] * FriendlyStructureInfluence[role]) 
                + (tile.EnemyStructureInfluence[playerIndex] * EnemyStructureInfluence[role]);
        }

        public static MoveOrder FindBestMoveOrderForUnit(MilitaryUnit unit)
        {
            var possibleMoves = unit.PossibleMoves();

            var highestInfluence = possibleMoves.Max(y => y.Destination.AggregateInfluence[unit.Role][unit.OwnerIndex] - (1 * FriendlyUnitInfluenceModifier[unit.Role]) / (Hex.Distance(y.Destination.Hex, unit.Location.Hex) + 1));

            if (unit.Location.AggregateInfluence[unit.Role][unit.OwnerIndex] - (1 * FriendlyUnitInfluenceModifier[unit.Role]) < highestInfluence)
            {
                var moves = possibleMoves.Where(y => y.Destination.AggregateInfluence[unit.Role][unit.OwnerIndex] - (1 * FriendlyUnitInfluenceModifier[unit.Role]) / (Hex.Distance(y.Destination.Hex, unit.Location.Hex) + 1) == highestInfluence);

                var bestMove = moves.OrderByDescending(y => y.TerrainAndWeatherModifers(unit.Index)).ThenBy(y => y.Distance).First();

                var moveOrder = bestMove.GetMoveOrder(unit);

                moveOrder.Unit = unit;
                return moveOrder;
            }
            return null;
        }

        public static IEnumerable<PathFindTile> FindShortestPath(List<PathFindTile> pathFindTiles, Point origin, Point destination)
        {
            var ori = pathFindTiles.Single(x => x.X == origin.X && x.Y == origin.Y);
            var dest = pathFindTiles.Single(x => x.X == destination.X && x.Y == destination.Y);

            Func<PathFindTile, PathFindTile, double> distance = (node1, node2) => node1.MoveCost[node2];
            Func<PathFindTile, double> estimate = t => Math.Sqrt(Math.Pow(t.X - destination.X, 2) + Math.Pow(t.Y - destination.Y, 2));

            return PathFind.PathFind.FindPath(ori, dest, distance, estimate).Reverse();
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
