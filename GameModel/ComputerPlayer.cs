using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameModel
{
    public class ComputerPlayer
    {


        public ComputerPlayer()
        {
            //TerrainAndWeatherInfluenceByUnit = new Dictionary<int, double>();
        }

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
                    _enemyUnitInfluenceModifier[Role.Defensive] = -1;
                }
                return _enemyUnitInfluenceModifier;
            }
        }
        static Dictionary<Role, double> _enemyUnitInfluenceModifier;

        static Dictionary<Role, double> StructureInfluence
        {
            get
            {
                if (_structureInfluence == null)
                {
                    _structureInfluence = new Dictionary<Role, double>();
                    foreach (var role in Enum.GetValues(typeof(Role)))
                    {
                        _structureInfluence.Add((Role)role, 1);
                    }
                    _structureInfluence[Role.Defensive] = 0.5;
                    _structureInfluence[Role.Scout] = 2;
                    _structureInfluence[Role.Besieger] = 2;
                }
                return _structureInfluence;
            }
        }
        static Dictionary<Role, double> _structureInfluence;

        public static void GenerateInfluenceMaps(Board board, int numberOfPlayers)
        {
            var aliveUnits = board.Units.Where(x => x.IsAlive).ToList();

            board.Tiles.ToList().ForEach(x =>
            {
                x.UnitInfluence = new Dictionary<RoleAndMovementType, double[]>();

                //{
                //    { new RoleAndMovementType(Role. MovementType.Airborne, new double[numberOfPlayers] },
                //    { MovementType.Land, new double[numberOfPlayers] },
                //    { MovementType.Water, new double[numberOfPlayers] }
                //};
                //x.UnitStrengthInfluence = new Dictionary<MovementType, double[]>()
                //{
                //    { MovementType.Airborne, new double[numberOfPlayers] },
                //    { MovementType.Land, new double[numberOfPlayers] },
                //    { MovementType.Water, new double[numberOfPlayers] }
                //};
                x.AggregateInfluence = new Dictionary<RoleAndMovementType, double[]>();

                RoleAndMovementType.RolesAndMovementTypes.ForEach(y =>
                        {
                            x.UnitInfluence.Add(new RoleAndMovementType(y.Role, y.MovementType), new double[numberOfPlayers]);
                            x.AggregateInfluence.Add(new RoleAndMovementType(y.Role, y.MovementType), new double[numberOfPlayers]);
                        });


                //{
                //    { MovementType.Airborne, new double[numberOfPlayers] },
                //    { MovementType.Land, new double[numberOfPlayers] },
                //    { MovementType.Water, new double[numberOfPlayers] }
                //};
                aliveUnits.ForEach(y =>
                {
                    if (!x.TerrainAndWeatherInfluenceByUnit.ContainsKey(y.Index))
                        x.TerrainAndWeatherInfluenceByUnit.Add(y.Index, y.TerrainTypeBattleModifier[x.TerrainType] + y.WeatherBattleModifier[x.Weather]);
                });
                x.StructureInfluence = new Dictionary<Role, double[]>();

                MilitaryUnit
                    .Roles
                    .ForEach(y => x.StructureInfluence.Add(y, new double[numberOfPlayers]));
            });

            foreach (var unit in aliveUnits)
            {
                unit.CalculateStrength();
                var playerIndex = unit.OwnerIndex;

                var roleAndMovementType = new RoleAndMovementType(unit.Role, unit.MovementType);

                unit.Tile.UnitInfluence[roleAndMovementType][playerIndex] += 1;
                //unit.Tile.UnitStrengthInfluence[roleAndMovementType][playerIndex] += unit.Strength;

                for (var i = 1; i < 5; i++)
                {
                    var hexesInRing = Hex.HexRing(unit.Tile.Hex, i);

                    hexesInRing.ForEach(x =>
                    {
                        var index = Hex.HexToIndex(x, board.Width);
                        if (index >= 0 && index < board.TileArray.Length)
                        {
                            if (unit.CanStopOn.HasFlag(board[index].TerrainType))
                            {
                                board[index].UnitInfluence[roleAndMovementType][playerIndex] += Math.Round(1D / (i + 1), 1);
                                //board[index].UnitStrengthInfluence[roleAndMovementType][playerIndex] += Math.Round(unit.Strength / (i + 1), 0);
                            }
                        }
                    });
                }
            }

            for (var i = 0; i < numberOfPlayers; i++)
            {
                foreach (var structure in board.Structures)
                {
   

                    MilitaryUnit.Roles.ForEach(x =>
                            {
                                //structure.Tile.StructureInfluence[x][i] += structure.OwnerIndex == i ? StructureInfluenceModifier[x] : enemyStructureInfluence;
                                for (var j = 0; j < 5; j++)
                                {
                                    var hexesInRing = Hex.HexRing(structure.Tile.Hex, j);

                                    hexesInRing.ForEach(y =>
                                    {
                                        var index = Hex.HexToIndex(y, board.Width);
                                        if (index >= 0 && index < board.TileArray.Length)
                                            board[index].StructureInfluence[x][i] += (structure.OwnerIndex == i ? StructureInfluence[x] : -StructureInfluence[x]) / (j + 1);
                                    });
                                }
                            });

                    
                }

                board.Tiles.ToList().ForEach(x =>
                {
                    RoleAndMovementType.RolesAndMovementTypes
                        .ForEach(y => CalculateAggregateInfluenceMap(x, numberOfPlayers, i, y));
                });
            }
        }
        private static void CalculateAggregateInfluenceMap(Tile x, int numberOfPlayers, int i, RoleAndMovementType roleAndMovementType)
        {
            x.AggregateInfluence[roleAndMovementType][i] = x.UnitInfluence[roleAndMovementType][i] - x.StructureInfluence[roleAndMovementType.Role][i];
            for (var j = 0; j < numberOfPlayers; j++)
            {
                if (i == j)
                    continue;

                x.AggregateInfluence[roleAndMovementType][i] -= x.UnitInfluence[roleAndMovementType][j] * EnemyUnitInfluenceModifier[roleAndMovementType.Role];
            }
        }
    }
}
