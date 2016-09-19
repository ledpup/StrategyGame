using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameModel
{
    public enum Weather
    {
        Fine,
        Dry,
        Wet,
        Cold,
    }
    public class Board
    {
        private Tile[,] _tiles;
        Tile[] _tiles1d;

        public int Width;
        public int Height;
        public List<MilitaryUnit> Units { get; set; }

        public Dictionary<int, List<MoveOrder>> MoveOrders;

        public static Logger Logger;

        public List<Structure> Structures;

        public int Turn;

        public Board(string[] tiles, string[] edges = null, string[] structures = null, int turn = 0, Logger logger = null)
        {
            Width = tiles[0].Length;
            Height = tiles.Length;

            InitialiseTiles(Width, Height, tiles);
            FindNeighbours();
            CalculateTileDistanceFromTheSea();
            Edges = IntitaliseTileEdges(edges);
            Structures = IntitaliseStructures(structures);
            InitialiseSupply();

            Logger = logger;
            if (Logger == null)
            {
                Logger = LogManager.GetCurrentClassLogger();
            }

            MoveOrders = new Dictionary<int, List<MoveOrder>>();

            TerrainTemperatureModifiers = new Dictionary<TerrainType, double>();
            foreach (TerrainType terrainType in Enum.GetValues(typeof(TerrainType)))
            {
                TerrainTemperatureModifiers.Add(terrainType, 0);
            }
            TerrainTemperatureModifiers[TerrainType.Mountain] = -10;
            TerrainTemperatureModifiers[TerrainType.Hill] = -5;

            Turn = turn;

            CalculateTemperature(Turn);
        }

        public void InitialiseSupply()
        {
            Tiles.ToList().ForEach(x => x.Supply = null);
            var supplyCalculated = new List<Tile>();
            foreach (var structure in Structures)
            {
                CalculateSupply(this[structure.Index], structure.OwnerIndex, structure.Supply, supplyCalculated);
            }
        }

        private void CalculateSupply(Tile tile, int ownerId, float supply, List<Tile> supplyCalculated)
        {
            if (supplyCalculated.Contains(tile))
            {
                if (tile.Supply < supply)
                {
                    tile.Supply = supply;
                }
                else
                {
                    return;
                }
            }
            else
            {
                supplyCalculated.Add(tile);
                tile.Supply = supply;
            }
            
            if (supply > 1)
            {
                foreach (var neighbour in tile.Neighbours)
                {
                    float neighbourSupply = 0;
                    if (neighbour.OwnerId == ownerId || neighbour.OwnerId == null)
                    {
                        var tileEdge = Edges.SingleOrDefault(x => x.Tiles.All(y => y == tile || y == neighbour));
                        if (tileEdge != null)
                        {
                            if (tileEdge.BaseEdgeType == BaseEdgeType.CentreToCentre && !Terrain.All_Water.HasFlag(neighbour.TerrainType))
                            {
                                neighbourSupply = supply - .5f;
                            }
                            else if (tileEdge.EdgeType != EdgeType.Mountain)
                            {
                                var edgeModifier = tileEdge.EdgeType == EdgeType.Wall ? 0 : 0.5f;
                                if (Terrain.Non_Mountainous_Land.HasFlag(neighbour.TerrainType))
                                {
                                    if (Terrain.Rough_Land.HasFlag(neighbour.TerrainType))
                                    {
                                        neighbourSupply = supply - 1.5f - edgeModifier;
                                    }
                                    else
                                    {
                                        neighbourSupply = supply - 1f - edgeModifier;
                                    }
                                }
                            }
                        }
                        else if (Terrain.Non_Mountainous_Land.HasFlag(neighbour.TerrainType))
                        {
                            if (Terrain.Rough_Land.HasFlag(neighbour.TerrainType))
                            {
                                neighbourSupply = supply - 1.5f;
                            }
                            else
                            {
                                neighbourSupply = supply - 1;
                            }
                        }
                        if (neighbourSupply >= 1)
                        {
                            CalculateSupply(neighbour, ownerId, neighbourSupply, supplyCalculated);
                        }
                    }
                }
            }
        }

        public void ChangeStructureOwners()
        {
            Structures.ForEach(x => 
            {
                var unitsAtStructureByOwner = Units.Where(y => y.IsAlive && y.Tile == x.Tile).GroupBy(y => y.OwnerIndex);
                if (unitsAtStructureByOwner.Count() == 1)
                    x.OwnerIndex = unitsAtStructureByOwner.First().Key;
            });
        }

        private List<Structure> IntitaliseStructures(string[] tilePoints)
        {
            var structures = new List<Structure>();

            if (tilePoints == null)
                return structures;
            foreach (var point in tilePoints)
            {
                var structureProperties = point.Split(',');
                var index = int.Parse(structureProperties[0]);
                var structureType = (StructureType)Enum.Parse(typeof(StructureType), structureProperties[2]);
                var structure = new Structure(index, structureType, TileArray[index], int.Parse(structureProperties[2]), int.Parse(structureProperties[3]));

                
                structures.Add(structure);
            }
            return structures;
        }

        private void CalculateTileDistanceFromTheSea()
        {
            Tiles.ToList().ForEach(x =>
            {
                var searched = new List<Tile>();
                x.DistanceFromWater = GetWaterDistanceToSea(x, 0, ref searched);
            });
        }

        private int GetWaterDistanceToSea(Tile tile, int distance, ref List<Tile> searched)
        {
            searched.Add(tile);
            if (tile.IsSea)
            {
                return distance;
            }
            distance += 1;
            if (tile.Neighbours.Any(x => x.IsSea))
            {
                return distance;
            }
                      
            var minDistance = 100;
            foreach (var neighbour in tile.Neighbours)
            {
                if (!searched.Contains(neighbour))
                {
                    var result = GetWaterDistanceToSea(neighbour, distance, ref searched);
                    if (result < minDistance)
                        minDistance = result;
                }
            }
            return minDistance;
        }

        public void CalculateTemperature(int? turnParameter = null)
        {
            var turn = Turn;
            if (turnParameter != null)
                turn = (int)turnParameter;

            for (var x = 0; x < Width; x++)
            {
                for (var y = 0; y < Height; y++)
                {
                    const double seasonRate = .3;
                    const double temperatureShiftPerMonth = 8;
                    
                    this[x, y].Temperature = y * .5 + 10 + TerrainTemperatureModifiers[this[x, y].TerrainType] - (this[x, y].DistanceFromWater * 2) + Math.Sin(turn * seasonRate) * temperatureShiftPerMonth;
                }
            }
        }

        public void FindNeighbours()
        {
            foreach (var tile in Tiles)
            {
                if (tile.Neighbours != null)
                    throw new Exception("Adjacent tiles have already be calculated");

                var neighbours = new List<Tile>();

                var potentialTiles = Hex.Neighbours(tile.Hex);

                foreach (var potientialTile in potentialTiles)
                {
                    var neighbourX = OffsetCoord.QoffsetFromCube(potientialTile).col;
                    var neighbourY = OffsetCoord.QoffsetFromCube(potientialTile).row;

                    if (neighbourX >= 0 && neighbourX < Width && neighbourY >= 0 && neighbourY < Height)
                    {
                        var neighbour = this[neighbourX, neighbourY];
                        neighbours.Add(neighbour);
                        tile.NeighbourEdges.Add(new Edge("Normal", new Tile[] { tile, neighbour }));
                    }
                }

                tile.Neighbours = neighbours;
                
            }
        }

        private List<Edge> IntitaliseTileEdges(string[] tilesEdges)
        {
            var tileEdgesList = new List<Edge>();

            if (tilesEdges == null)
                return tileEdgesList;

            tilesEdges.ToList().ForEach(
                x =>
                {
                    var columns = x.Split(',');

                    var tileIndexes = new List<int> { int.Parse(columns[0]), int.Parse(columns[1]) };

                    var firstTile = tileIndexes.Min();
                    var secondTile = tileIndexes.Max();

                    if (firstTile == secondTile)
                        throw new Exception("Must create an edge between two different tiles");

                    var t1 = TileArray[firstTile];
                    var t2 = TileArray[secondTile];

                    if (!t1.Neighbours.Contains(t2))
                        throw new Exception(string.Format("Can not create a tile edge between tile {0} and tile {1} because they are not neighbours", t1.Index, t2.Index));

                    var existingEdge = tileEdgesList.Where(y => y.Tiles.Contains(t1) && y.Tiles.Contains(t2));

                    if (existingEdge.Any())
                        throw new Exception(string.Format("Can not create a tile edge between tile {0} and tile {1} because one already exists of type {2}.", t1.Index, t2.Index, existingEdge.First().EdgeType.ToString()));

                    var tiles = new Tile[] { t1, t2 };
                    var edge = t1.NeighbourEdges.Single(y => y.Tiles.Contains(t2));
                    edge.SetEdgeType(columns[2]);

                    edge = t2.NeighbourEdges.Single(y => y.Tiles.Contains(t1));
                    edge.SetEdgeType(columns[2]);

                    tileEdgesList.Add(edge);
                }
            );
            return tileEdgesList;
        }

        private void InitialiseTiles(int width, int height, string[] data)
        {
            _tiles = new Tile[width, height];
            _tiles1d = new Tile[width * height];

            for (ushort x = 0; x < width; x++)
            {
                for (ushort y = 0; y < height; y++)
                {
                    var terrainType = Terrain.ConvertCharToTerrainType(char.Parse(data[y].Substring(x, 1)));
                    var isEdge = x == 0 || y == 0 || x == width || y == height ? true : false;
                    var tile = new Tile(Point.PointToIndex(x, y, width), x, y, terrainType, isEdge);
                    _tiles[x, y] = tile;
                    _tiles1d[y * width + x] = tile;
                }
            }
        }

        public Tile this[int index]
        {
            get
            {
                return _tiles1d[index];
            }
        }

        public Tile this[int x, int y]
        {
            get
            {
                return _tiles[x, y];
            }
            set
            {
                _tiles[x, y] = value;
            }
        }

        public Tile[] TileArray
        {
            get
            {
                return _tiles1d;
            }
        }

        public IEnumerable<Tile> Tiles
        {
            get 
            {   
                return _tiles1d;
            }
        }

        public Dictionary<TerrainType, double> TerrainTemperatureModifiers { get; private set; }

        public List<Edge> Edges;

        public static List<PathFindTile> GetValidMovesWithMoveCostsForUnit(Board board, MilitaryUnit unit)
        {
            var pathFindTiles = new List<PathFindTile>();
            board.Tiles.ToList().ForEach(x => pathFindTiles.Add(new PathFindTile(x.Index, x.X, x.Y)));

            foreach (var pathFindTile in pathFindTiles)
            {
                var neighbours = new List<PathFindTile>();

                var originTile = board[pathFindTile.Point.X, pathFindTile.Point.Y];

                originTile.ValidAdjacentMoves(unit).ToList().ForEach(x =>
                {
                    var neighbour = pathFindTiles.Single(y => y.Point.X == x.X && y.Point.Y == x.Y);

                    neighbours.Add(neighbour);
                    pathFindTile.Neighbours = neighbours;

                    pathFindTile.MoveCost[neighbour] = originTile.CalculateMoveCost(unit, x);

                });
            }

            return pathFindTiles;
        }

        public void ResolveMoves(List<MoveOrder> moveOrders)
        {
            if (moveOrders == null || moveOrders.Count == 0)
                return;

            MoveOrders[Turn] = moveOrders;

            var movingUnits = moveOrders.Select(x => x.Unit).ToList();
            float maxMovementPoints = 12;

            var invalidMoveOrders = moveOrders.Where(x => x.Moves[0].Origin != x.Unit.Tile);
            if (invalidMoveOrders.Count() > 0)
            {
                throw new Exception("The following units received orders to move from a location where they don't currently reside: " + string.Join(", ", invalidMoveOrders.Select(x => x.Unit + ". Ordered " + x.Moves[0])));
            }

            if (moveOrders.Max(x => x.Moves.Length) > maxMovementPoints)
                throw new Exception(string.Format("The max number of moves is capped at {0}. A move order has exceeded this limit.", maxMovementPoints));

            var unitStepRate = new Dictionary<MilitaryUnit, int>();
            movingUnits.ForEach(x => unitStepRate.Add(x, (int)Math.Round(maxMovementPoints / x.MovementPoints)));

            for (var step = 1; step <= maxMovementPoints; step++)
            {
                MoveUnitsOneStep(moveOrders, unitStepRate, step);

                // Remove conflicting units from move orders                
                var conflictedUnits = DetectConflictedUnits(movingUnits, Units.Where(x => x.IsAlive));
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

        private static void MoveUnitsOneStep(List<MoveOrder> moveOrders, Dictionary<MilitaryUnit, int> unitStepRate, int step)
        {
            foreach (var moveOrder in moveOrders)
            {
                if (step % unitStepRate[moveOrder.Unit] == 0)
                {
                    var moveIndex = step / unitStepRate[moveOrder.Unit] - 1;
                    if (moveOrder.Moves.Length > moveIndex)
                        moveOrder.Unit.Tile = moveOrder.Moves[moveIndex].Destination;
                }
            }
        }

        public List<BattleReport> ConductBattles()
        {
            var battleReports = new List<BattleReport>();
            Tiles.ToList().ForEach(x =>
            {
                if (x.IsInConflict)
                {
                    ResolveBattle(x.ToString(), Turn, TerrainType.Mountain, Weather.Cold, x.Units, 3, StructureType.Fortress, 2);
                    battleReports.Add(CreateBattleReport(x, Turn, x.Units));
                }
            });

            return battleReports;
        }

        public static void ResolveBattle(string locationText, int turn, TerrainType terrainType, Weather weather, List<MilitaryUnit> units, int residentId = 0, StructureType structure = StructureType.None , int siegeDuration = 1)
        {
            var groupedUnits = units.GroupBy(x => x.OwnerIndex);
            if (groupedUnits.Count() == 1)
            {
                throw new Exception("Battle can not occur because all units in tile are owned by " + units[0].OwnerIndex);
            }

            //Logger.Info("Battle between {0} combatants at {1} on {2} terrain in {3} weather on turn {4}", groupedUnits.Count(), location, terrainType, weather, turn);

            units.ForEach(x =>
            {
                x.BattleQualityModifiers[BattleQualityModifier.Terrain] = x.TerrainTypeBattleModifier[terrainType];
                x.BattleQualityModifiers[BattleQualityModifier.Weather] = x.WeatherBattleModifier[weather];
                x.BattleQualityModifiers[BattleQualityModifier.Structure] = structure != StructureType.None ? x.StructureBattleModifier : 0;
            });



            // Divide units in to their combatant group
            var combatants = new List<CombatantInBattle>();
            foreach (var group in groupedUnits)
            {
                var combatantInBattle = new CombatantInBattle
                {
                    OwnerId = group.Key,
                    Units = group.ToList(),
                    OpponentUnits = units.Where(x => x.OwnerIndex != group.Key).ToList(),
                };

                var opponentUnitsCount = (double)combatantInBattle.OpponentUnits.Count;

                foreach (UnitType unitType in Enum.GetValues(typeof(UnitType)))
                {
                    combatantInBattle.OpponentUnitTypes[unitType] = combatantInBattle.OpponentUnits.Count(x => x.UnitType == unitType);
                    var proportion = combatantInBattle.OpponentUnitTypes[unitType] / opponentUnitsCount;

                    combatantInBattle.Units.ForEach(x =>
                    {
                        x.BattleQualityModifiers[BattleQualityModifier.UnitType] = x.OpponentUnitTypeBattleModifier[unitType] * proportion;
                    });
                }

                foreach (var unit in combatantInBattle.Units)
                {
                    unit.CalculateStrength();
                    combatantInBattle.UnitStrengthByType[unit.UnitType] += unit.BattleStrength;
                }

                combatantInBattle.UnitStrength = group.Sum(x => x.BattleStrength);

                combatants.Add(combatantInBattle);
            }

            // Calculate combatant damage
            foreach (var combatant in combatants)
            {
                var numberOfSides = combatants.Count;
                var opponents = combatants.Where(x => x != combatant).ToList();

                combatant.StrengthDamage = opponents.Sum(x => x.UnitStrength) / (numberOfSides - 1) * .8;

                if (residentId == combatant.OwnerId && structure != StructureType.None)
                {
                    var siegeUnitDamage = 0D;
                    opponents.ForEach(x => siegeUnitDamage += x.UnitStrengthByType[UnitType.Siege]);

                    combatant.StrengthDamage -= siegeUnitDamage;
                    combatant.StrengthDamage *= (Structure.StructureDefenceModifiers[structure] + (.05 * siegeDuration));
                    combatant.StrengthDamage += siegeUnitDamage;
                }
            }

            var totalStrength = combatants.Sum(x => x.UnitStrength);
            var totalStrengthDamage = combatants.Sum(x => x.StrengthDamage);

            // Assign casualties to units
            foreach (var combatant in combatants)
            {
                AssignCasualties(turn, combatant.Units, combatant.StrengthDamage);
            }

            // Decrease morale of units
            var winnersToLosers = combatants.OrderByDescending(x => x.Outcome).ToArray();
            for (var i = 0; i < winnersToLosers.Length; i++)
            {
                var positionProportion = ((i + 1) / (double)winnersToLosers.Length);
                var losesPenalty = (1 - winnersToLosers[i].UnitSurvivalProportion);

                winnersToLosers[i].Units.Where(x => x.IsAlive).ToList().ForEach(x => x.ChangeMorale(turn, -(positionProportion + losesPenalty)));
            }

        }

        public static BattleReport CreateBattleReport(Tile tile, int turn, List<MilitaryUnit> units)
        {
            var numberOfPlayers = units.GroupBy(x => x.OwnerIndex).Select(x => x.Key).Count();

            var battleReport = new BattleReport(numberOfPlayers)
            {
                Tile = tile,
                Turn = turn,
            };

            foreach (UnitType unitType in Enum.GetValues(typeof(UnitType)))
            {
                units.Where(x => x.UnitType == unitType).ToList().ForEach(x => battleReport.CasualtiesByPlayerAndType[x.OwnerIndex][unitType] += -x.QuantityEvents.Where(y => y.Turn == turn).Sum(z => z.Quantity));
            }

            units.ForEach(x =>
            {
                var losses = -x.QuantityEvents.Where(y => y.Turn == turn).Sum(y => y.Quantity);

                battleReport.CasualtyLog.Add(new CasualtyLogEntry
                {
                    OwnerIndex = x.OwnerIndex,
                    Text = x.IsAlive ? losses > 1
                                        ? string.Format("{0} {1} loss{2}, {3} remain", x.Name, losses, losses > 1 ? "es" : "", x.Quantity)
                                        : string.Format("{0} no losses", x.Name)
                                 : string.Format("{0} destroyed", x.Name)
                });
            });

            return battleReport;
        }

        private static void AssignCasualties(int turn, List<MilitaryUnit> units, double combatantStrengthDamage)
        {
            while (combatantStrengthDamage > 0)
            {
                var aliveUnits = units.Where(x => x.IsAlive).ToList();
                double assignedStrengthDamage = 0;

                if (aliveUnits.Count == 0)
                {
                    combatantStrengthDamage = 0;
                    continue;
                }

                var totalInitiative = aliveUnits.Sum(x => x.CombatInitiative);

                foreach (var unit in aliveUnits)
                {
                    var casualityProportion = unit.CombatInitiative / totalInitiative;
                    var strengthDamageToUnit = combatantStrengthDamage * casualityProportion;
                    if (strengthDamageToUnit > unit.Strength)
                    {
                        strengthDamageToUnit = unit.Strength;
                        unit.ChangeQuantity(turn, -unit.Quantity);
                    }
                    else
                    {
                        var quantityDecrease = (int)Math.Ceiling(strengthDamageToUnit / unit.Quality);
                        unit.ChangeQuantity(turn, -quantityDecrease);
                    }
                    assignedStrengthDamage += strengthDamageToUnit;
                }
                combatantStrengthDamage -= assignedStrengthDamage;
                combatantStrengthDamage = Math.Round(combatantStrengthDamage, 0);
            }
        }

        public static void GenerateInfluenceMaps(Board board, int numberOfPlayers)
        {
            var aliveUnits = board.Units.Where(x => x.IsAlive).ToList();

            board.Tiles.ToList().ForEach(x =>
            {
                x.UnitCountInfluence = new double[numberOfPlayers];
                x.UnitStrengthInfluence = new double[numberOfPlayers];
                x.AggregateInfluence = new double[numberOfPlayers];
                aliveUnits.ForEach(y =>
                {
                    if (!x.TerrainAndWeatherInfluenceByUnit.ContainsKey(y.Index))
                        x.TerrainAndWeatherInfluenceByUnit.Add(y.Index, y.TerrainTypeBattleModifier[x.TerrainType] + y.WeatherBattleModifier[x.Weather]);
                });
                x.StructureInfluence = 0;
            });

            foreach (var unit in aliveUnits)
            {
                var playerIndex = unit.OwnerIndex;
                unit.Tile.UnitCountInfluence[playerIndex] += 1;
                unit.Tile.UnitStrengthInfluence[playerIndex] = unit.Strength;
                var moves = unit.PossibleMoves().GroupBy(x => x.Destination);
                foreach (var move in moves)
                {
                    var minDistance = move.Min(x => x.Distance) + 1;
                    board[move.Key.Index].UnitCountInfluence[playerIndex] += Math.Round(1D / minDistance, 1);
                    board[move.Key.Index].UnitStrengthInfluence[playerIndex] += Math.Round(unit.Strength / minDistance, 0);
                }
            }

            foreach (var structure in board.Structures)
            {
                const double structureInfluence = 2;
                structure.Tile.StructureInfluence += structureInfluence;
                for (var i = 1; i < 5; i++)
                {
                    var hexesInRing = Hex.HexRing(structure.Tile.Hex, i);

                    hexesInRing.ForEach(x =>
                    {
                        var index = Hex.HexToIndex(x, board.Width);
                        if (index >= 0 && index < board.TileArray.Length)
                            board[index].StructureInfluence += structureInfluence / (i + 1);
                    });
                }
            }
        }
    }
}
