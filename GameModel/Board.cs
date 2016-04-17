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
        Dry,
        Fine,
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

        public static Dictionary<string, double> StructureDefenceModifiers;
        public static Logger Logger;

        public List<Structure> Structures;

        public Board(string[] tiles, string[] tilesEdges = null, string[] tilePoints = null, Logger logger = null)
        {
            Width = tiles[0].Length;
            Height = tiles.Length;

            InitialiseTiles(Width, Height, tiles);
            CalculateAdjacentTiles();
            CalculateTileDistanceFromTheSea();
            TileEdges = IntitaliseTileEdges(tilesEdges);
            Structures = IntitaliseTilePoints(tilePoints);
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
            

            CalculateTemperature();
        }

        public void InitialiseSupply()
        {
            Tiles.ToList().ForEach(x => x.Supply = null);
            var supplyCalculated = new List<Tile>();
            foreach (var structure in Structures)
            {
                CalculateSupply(this[structure.Id], structure.OwnerId, structure.Supply, supplyCalculated);
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
                        var tileEdge = TileEdges.SingleOrDefault(x => x.Tiles.All(y => y == tile || y == neighbour));
                        if (tileEdge != null)
                        {
                            if (tileEdge.EdgeType == EdgeType.Road && !Terrain.All_Water.HasFlag(neighbour.TerrainType))
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

        private List<Structure> IntitaliseTilePoints(string[] tilePoints)
        {
            var structures = new List<Structure>();

            if (tilePoints == null)
                return structures;
            foreach (var point in tilePoints)
            {
                var structureProperties = point.Split(',');
                var id = int.Parse(structureProperties[0]);
                var structure = new Structure(id, Point.IndexToPoint(id, Width), structureProperties[1], int.Parse(structureProperties[2]), int.Parse(structureProperties[3]));

                TileArray[structure.Id].Structure = structure;
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

        public void CalculateTemperature(int turn = 0)
        {
            for (var x = 0; x < Width; x++)
            {
                for (var y = 0; y < Height; y++)
                {
                    const double seasonRate = .3;
                    const double temperatureShiftPerMonth = 8;
                    
                    this[x, y].Temperature = y * .5 + 10 + TerrainTemperatureModifiers[this[x, y].TerrainType] - this[x, y].DistanceFromWater + Math.Sin(turn * seasonRate) * temperatureShiftPerMonth;
                }
            }
        }

        public void CalculateAdjacentTiles()
        {
            foreach (var tile in Tiles)
            {
                if (tile.Neighbours != null)
                    throw new Exception("Adjacent tiles have already be calculated");

                var neighbours = new List<Tile>();

                var potentialTiles = Hex.Neighbours(tile.Hex); // tile.X % 2 == 0 ? Tile.AdjacentEvenTiles : Tile.AdjacentOddTiles;

                foreach (var potientialTile in potentialTiles)
                {
                    //var neighbour = Hex.Add(tile.Hex, potientialTile);

                    var neighbourX = OffsetCoord.QoffsetFromCube(potientialTile).col;// tile.X + potientialTile.X;
                    var neighbourY = OffsetCoord.QoffsetFromCube(potientialTile).row;// tile.Y + potientialTile.Y;

                    if (neighbourX >= 0 && neighbourX < Width && neighbourY >= 0 && neighbourY < Height)
                        neighbours.Add(this[neighbourX, neighbourY]);
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
                        throw new Exception(string.Format("Can not create a tile edge between tile {0} and tile {2} because they are not neighbours", t1.Id, t2.Id));


                    var tiles = new Tile[] { t1, t2 };
                    var edge = new Edge(columns[2], tiles);

                    t1.AdjacentTileEdges.Add(edge);
                    t2.AdjacentTileEdges.Add(edge);

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

        public static List<Edge> TileEdges;

        public static List<PathFindTile> GetValidMovesWithMoveCostsForUnit(Board board, MilitaryUnit unit)
        {
            var pathFindTiles = new List<PathFindTile>();
            board.Tiles.ToList().ForEach(x => pathFindTiles.Add(new PathFindTile(x.Id, x.X, x.Y)));

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

        public static bool EdgeHasRoad(Tile tile, Tile adjacentTile)
        {
            var tileEdge = tile.AdjacentTileEdges.SingleOrDefault(edge => edge.Tiles.Any(x => x.Id == adjacentTile.Id));

            return tileEdge == null ? false : tileEdge.EdgeType == EdgeType.Road;
        }

        public void ResolveMoves(int turn, List<MoveOrder> moveOrders)
        {
            MoveOrders[turn] = moveOrders;

            if (moveOrders.Any(x => x.Turn != turn))
                throw new Exception(string.Format("Move order is not for the current turn ({0}).", turn));

            var movingUnits = moveOrders.Select(x => x.Unit).ToList();
            var initialMovementPoints = movingUnits.Max(x => x.MovementPoints);

            var unitStepRate = new Dictionary<MilitaryUnit, int>();
            movingUnits.ForEach(x => unitStepRate.Add(x, initialMovementPoints / x.MovementPoints));

            for (var step = 1; step <= initialMovementPoints; step++)
            {
                MoveUnitsOneStep(moveOrders, unitStepRate, step);

                // Remove conflicting units from move orders
                var conflictedUnits = DetectConflictedUnits(movingUnits, Units);
                moveOrders.RemoveAll(x => conflictedUnits.Contains(x.Unit));
            }
        }

        public static IEnumerable<MilitaryUnit> DetectConflictedUnits(List<MilitaryUnit> setOfUnits, List<MilitaryUnit> allUnits)
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
                var location = "Tzarian Castle";
                var turn = 1;

                if (x.IsInConflict)
                {
                    ResolveBattle(location, turn, TerrainType.Mountain, Weather.Cold, x.Units, 3, "Fort", 2);
                    battleReports.Add(CreateBattleReport(location, turn, x.Units));
                }
            });

            return battleReports;
        }

        public static void ResolveBattle(string location, int turn, TerrainType terrainType, Weather weather, List<MilitaryUnit> units, int residentId = 0, string structure = null, int siegeDuration = 1)
        {
            var groupedUnits = units.GroupBy(x => x.OwnerId);
            if (groupedUnits.Count() == 1)
            {
                throw new Exception("Battle can not occur because all units in tile are owned by " + units[0].OwnerId);
            }

            Logger.Info("Battle between {0} combatants at {1} on {2} terrain in {3} weather on turn {4}", groupedUnits.Count(), location, terrainType, weather, turn);

            units.ForEach(x =>
            {
                x.BattleQualityModifiers[BattleQualityModifier.Terrain] = x.TerrainTypeBattleModifier.ContainsKey(terrainType) ? x.TerrainTypeBattleModifier[terrainType] : 0;
                x.BattleQualityModifiers[BattleQualityModifier.Weather] = x.WeatherBattleModifier.ContainsKey(weather) ? x.WeatherBattleModifier[weather] : 0;
            });



            // Divide units in to their combatant group
            var combatants = new List<CombatantInBattle>();
            foreach (var group in groupedUnits)
            {
                var combatantInBattle = new CombatantInBattle
                {
                    OwnerId = group.Key,
                    Units = group.ToList(),
                    OpponentUnits = units.Where(x => x.OwnerId != group.Key).ToList(),
                };

                var opponentUnitsCount = (double)combatantInBattle.OpponentUnits.Count;

                foreach (UnitType unitType in Enum.GetValues(typeof(UnitType)))
                {
                    combatantInBattle.OpponentUnitTypes[unitType] = combatantInBattle.OpponentUnits.Count(x => x.UnitType == unitType);
                    var proportion = combatantInBattle.OpponentUnitTypes[unitType] / opponentUnitsCount;

                    combatantInBattle.Units.ForEach(x =>
                    {
                        x.BattleQualityModifiers[BattleQualityModifier.UnitType] = x.OpponentUnitTypeBattleModifier.ContainsKey(unitType) ? x.OpponentUnitTypeBattleModifier[unitType] * proportion : 0;
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

                if (residentId == combatant.OwnerId && !string.IsNullOrEmpty(structure))
                {
                    var siegeUnitDamage = 0D;
                    opponents.ForEach(x => siegeUnitDamage += x.UnitStrengthByType[UnitType.Siege]);

                    combatant.StrengthDamage -= siegeUnitDamage;
                    combatant.StrengthDamage *= (StructureDefenceModifiers[structure] + (.05 * siegeDuration));
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

        public static BattleReport CreateBattleReport(string location, int turn, List<MilitaryUnit> units)
        {
            var battleReport = new BattleReport
            {
                Location = location,
                Turn = turn,
            };

            foreach (UnitType unitType in Enum.GetValues(typeof(UnitType)))
            {
                units.Where(x => x.UnitType == unitType).ToList().ForEach(x => battleReport.CasualtiesByType[unitType] += -x.QuantityEvents.Where(y => y.Turn == turn).Sum(z => z.Quantity));
            }

            units.ForEach(x =>
            {
                var losses = -x.QuantityEvents.Where(y => y.Turn == turn).Sum(y => y.Quantity);

                battleReport.CasualtyLog.Add(new CasualtyLogEntry
                {
                    OwnerId = x.OwnerId,
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
                        var quantityDecrease = (int)(strengthDamageToUnit / unit.Quality);
                        unit.ChangeQuantity(turn, -quantityDecrease);
                    }
                    assignedStrengthDamage += strengthDamageToUnit;
                }
                combatantStrengthDamage -= assignedStrengthDamage;
                combatantStrengthDamage = Math.Round(combatantStrengthDamage, 0);
            }
        }
    }
}
