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

        public Board(string[] tiles, string[] edges = null, string[] roads = null, string[] structures = null, int turn = 0, Logger logger = null)
        {
            Width = tiles[0].Length;
            Height = tiles.Length;

            InitialiseTiles(Width, Height, tiles);
            FindNeighbours();
            CalculateTileDistanceFromTheSea();
            IntitaliseEdges(edges);
            IntitaliseRoads(roads);
            Structures = IntitaliseStructures(structures);
            InitialiseSupply();
            CalculateContiguousRegions();

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

        public void ResolveStackLimits(int playerIndex)
        {
            Tiles.Where(x => x.OverStackLimit(playerIndex))
                .ToList()
                .ForEach(x =>
                    {
                        var overStackLimitCount = x.OverStackLimitCount(playerIndex);
                        x.Units
                            .Where(y => y.IsAlive && y.OwnerIndex == playerIndex)
                            .ToList()
                            .ForEach(y => y.ChangeMorale(Turn, - .5 * overStackLimitCount, $"Units are over the stack limit of {x.StackLimit} by {overStackLimitCount} units"));
                    });
        }

        private void CalculateContiguousRegions()
        {
            var id = 0;
            foreach (var tile in _tiles)
            {
                if (tile.ContiguousRegionId == 0)
                {
                    id++;
                    tile.ContiguousRegionId = id;
                    AssignContiguousTilesToRegion(tile, id);
                }
            }
        }

        private void AssignContiguousTilesToRegion(Tile tile, int id)
        {
            tile.Neighbours
                .Where(x => x.ContiguousRegionId == 0 && x.BaseTerrainType == tile.BaseTerrainType)
                .ToList()
                .ForEach(x => 
                    {
                        x.ContiguousRegionId = id;
                        AssignContiguousTilesToRegion(x, id);
                    });
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
                        var tileEdge = Edges.SingleOrDefault(x => x.CrossesEdge(tile, neighbour));
                        if (tileEdge != null)
                        {
                            if (tileEdge.EdgeType != EdgeType.Mountain)
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
                var unitsAtStructureByOwner = Units.Where(y => y.IsAlive && y.Location == x.Location).GroupBy(y => y.OwnerIndex).ToList();
                if (unitsAtStructureByOwner.Count() == 1)
                {
                    if (x.OwnerIndex == unitsAtStructureByOwner.First().Key)
                    {
                        return;
                    }
                    x.OwnerIndex = unitsAtStructureByOwner.First().Key;

                    var units = unitsAtStructureByOwner.First().ToList();

                    var numberOfUnits = units.Count;

                    units.ForEach(y => y.ChangeMorale(Turn, 2D / numberOfUnits, $"Morale increase from pillaging {x.StructureType}"));
                }
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
                    }
                }

                tile.Neighbours = neighbours;
                
            }
        }

        private void IntitaliseEdges(string[] edges)
        {
            edges.ToList().ForEach(
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

                    var existingEdge = Edges.Where(y => y.CrossesEdge(t1, t2));

                    if (existingEdge.Any())
                        throw new Exception(string.Format("Can not create a tile edge between tile {0} and tile {1} because one already exists of type {2}.", t1.Index, t2.Index, existingEdge.Single().EdgeType.ToString()));

                    Edges.Add(new Edge(columns[2], t1, t2));
                }
            );
        }

        private void IntitaliseRoads(string[] roads)
        {
            roads.ToList().ForEach(
                x =>
                {
                    var columns = x.Split(',');

                    var tileIndexes = new List<int> { int.Parse(columns[0]), int.Parse(columns[1]) };

                    var firstTile = tileIndexes.Min();
                    var secondTile = tileIndexes.Max();

                    if (firstTile == secondTile)
                        throw new Exception("Must create an road between two different tiles");

                    var t1 = TileArray[firstTile];
                    var t2 = TileArray[secondTile];

                    if (!t1.Neighbours.Contains(t2))
                        throw new Exception(string.Format("Can not create a tile road between tile {0} and tile {1} because they are not neighbours", t1.Index, t2.Index));

                    var existingRoad = Roads.Where(y => y.CrossesRoad(t1, t2));

                    if (existingRoad.Any())
                        throw new Exception(string.Format("Can not create a tile road between tile {0} and tile {1} because one already exists", t1.Index, t2.Index));

                    Roads.Add(new Road(columns[2], t1, t2));
                }
            );
        }

        private void InitialiseTiles(int width, int height, string[] tileData)
        {
            _tiles = new Tile[width, height];
            _tiles1d = new Tile[width * height];

            for (ushort x = 0; x < width; x++)
            {
                for (ushort y = 0; y < height; y++)
                {
                    var terrainType = Terrain.ConvertCharToTerrainType(char.Parse(tileData[y].Substring(x, 1)));
                    var isEdgeOfMap = x == 0 || y == 0 || x == width || y == height ? true : false;
                    var tile = new Tile(Point.PointToIndex(x, y, width), x, y, terrainType, isEdgeOfMap);
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
        public List<Road> Roads;

        public List<PathFindTile> ValidMovesWithMoveCostsForUnit(MilitaryUnit unit)
        {
            var pathFindTiles = new List<PathFindTile>();
            Tiles.ToList().ForEach(x => pathFindTiles.Add(new PathFindTile(x.X, x.Y)));

            foreach (var pathFindTile in pathFindTiles)
            {
                var neighbours = new List<PathFindTile>();

                var originTile = this[pathFindTile.Point.X, pathFindTile.Point.Y];

                foreach(var validAdjacentMove in originTile.ValidAdjacentMoves(unit))
                {
                    var neighbour = pathFindTiles.Single(y => y.Point == validAdjacentMove.Point);

                    neighbours.Add(neighbour);
                    pathFindTile.Neighbours = neighbours;

                    pathFindTile.MoveCost[neighbour] = originTile.CalculateMoveCost(unit, validAdjacentMove);

                    neighbour.HasCumulativeCost = unit.MovementType == MovementType.Airborne && !unit.CanStopOn.HasFlag(validAdjacentMove.TerrainType);
                };
            }

            return pathFindTiles;
        }

        public void ResolveOrders(List<IUnitOrder> unitOrders)
        {
            ResolveTransportOrders(unitOrders);
            UnloadOrders(unitOrders);
            ResolveMoves(unitOrders);
            UnloadOrders(unitOrders);

            // Units can load onto transports after they have moved
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

        void ResolveMoves(List<IUnitOrder> unitOrders)
        {
            var moveOrders = unitOrders.OfType<MoveOrder>().ToList();

            if (moveOrders == null || moveOrders.Count == 0)
                return;

            MoveOrders[Turn] = moveOrders;

            //var movingUnits = moveOrders.Select(x => x.Unit).ToList();
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
                        throw new Exception($"Number of moves for {x.Unit} = { x.Moves.Length } exceeds the max number of moves permitted for the unit of {x.Unit.MovementPoints} moves with a road move bonus of {x.Unit.RoadMovementBonus}");
                }
            );

            var unitStepRate = new Dictionary<MilitaryUnit, int>();
            moveOrders.ForEach(x => unitStepRate.Add(x.Unit, (int)Math.Round(maxMovementPoints / (x.Moves.Length > x.Unit.MovementPoints ? (x.Unit.MovementPoints + x.Unit.RoadMovementBonus) : x.Unit.MovementPoints))));

            for (var step = 1; step <= maxMovementPoints; step++)
            {
                var unitStepMoves = MoveUnitsOneStep(moveOrders, unitStepRate, step);

                // Detect enemy adjacent units attempting to swap tiles
                var removeUnitMoves = new Dictionary<MilitaryUnit, Move>();
                foreach (var stepMove in unitStepMoves)
                {
                    if (unitStepMoves.Any(x => x.Value.Destination == stepMove.Value.Origin && x.Key.OwnerIndex != stepMove.Key.OwnerIndex))
                    {
                        var originStrength = stepMove.Value.Origin.Units.Where(x => x.OwnerIndex == stepMove.Key.OwnerIndex).Sum(x => x.Strength);
                        var destinationStrength = stepMove.Value.Destination.Units.Where(x => x.OwnerIndex != stepMove.Key.OwnerIndex).Sum(x => x.Strength);

                        if (originStrength <= destinationStrength)
                        {
                            removeUnitMoves.Add(stepMove.Key, stepMove.Value);
                        }
                    }
                }

                // Don't move a land unit onto a water time unless there is a transport there that can take it
                unitStepMoves.Where(x => x.Value.Destination.BaseTerrainType == BaseTerrainType.Water && x.Key.MovementType == MovementType.Land)
                    .ToList()
                    .ForEach(x => 
                    {
                        var transportedUnit = x.Key;
                        var transports = Units.Where(y => y.MovementType == MovementType.Water && x.Value.Destination.Point == y.Location.Point && y.CanTransport(transportedUnit)).OrderBy(y => y.TransportSize);
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

                // Move units
                foreach (var unitStepMove in unitStepMoves)
                {
                    var unit = unitStepMove.Key;

                    unit.Location = unitStepMove.Value.Destination;

                    // Take transported units along with you
                    unit.Transporting.ForEach(x => x.Location = unitStepMove.Key.Location);

                    if (unitStepMove.Value.MoveType != MoveType.Road)
                    {
                        if (unit.MoraleMoveCost[unit.BaseMovementPoints - unitStepMove.Value.MovesRemaining] > 0)
                        {
                            unit.ChangeMorale(Turn, -unit.MoraleMoveCost[unit.BaseMovementPoints - unitStepMove.Value.MovesRemaining], "Morale reduced during forced march");
                        }
                    }
                }

                // Remove conflicting units from move orders                
                var conflictedUnits = DetectConflictedUnits(moveOrders.Select(x => x.Unit).ToList(), Units.Where(x => x.IsAlive));
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
                        //moveOrder.Unit.Tile = moveOrder.Moves[moveIndex].Destination;
                }
            }
            return unitStepMoves;
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

                winnersToLosers[i].Units.Where(x => x.IsAlive).ToList().ForEach(x => x.ChangeMorale(turn, -(positionProportion + losesPenalty), "Morale change due to combat"));
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
    }
}
