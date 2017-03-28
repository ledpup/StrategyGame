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

        public static void GenerateInfluenceMaps(Board board, int numberOfPlayers)
        {
            var aliveUnits = board.Units.Where(x => x.IsAlive).ToList();

            board.Tiles.ToList().ForEach(x =>
            {
                x.UnitCountInfluence = new Dictionary<MovementType, double[]>()
                {
                    { MovementType.Airborne, new double[numberOfPlayers] },
                    { MovementType.Land, new double[numberOfPlayers] },
                    { MovementType.Water, new double[numberOfPlayers] }
                };
                x.UnitStrengthInfluence = new Dictionary<MovementType, double[]>()
                {
                    { MovementType.Airborne, new double[numberOfPlayers] },
                    { MovementType.Land, new double[numberOfPlayers] },
                    { MovementType.Water, new double[numberOfPlayers] }
                };
                x.AggregateInfluence = new Dictionary<MovementType, double[]>()
                {
                    { MovementType.Airborne, new double[numberOfPlayers] },
                    { MovementType.Land, new double[numberOfPlayers] },
                    { MovementType.Water, new double[numberOfPlayers] }
                };
                aliveUnits.ForEach(y =>
                {
                    if (!x.TerrainAndWeatherInfluenceByUnit.ContainsKey(y.Index))
                        x.TerrainAndWeatherInfluenceByUnit.Add(y.Index, y.TerrainTypeBattleModifier[x.TerrainType] + y.WeatherBattleModifier[x.Weather]);
                });
                x.StructureInfluence = new double[numberOfPlayers];
            });

            foreach (var unit in aliveUnits)
            {
                unit.CalculateStrength();
                var playerIndex = unit.OwnerIndex;
                unit.Tile.UnitCountInfluence[unit.MovementType][playerIndex] += 1;
                unit.Tile.UnitStrengthInfluence[unit.MovementType][playerIndex] += unit.Strength;

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
                                board[index].UnitCountInfluence[unit.MovementType][playerIndex] += Math.Round(1D / (i + 1), 1);
                                board[index].UnitStrengthInfluence[unit.MovementType][playerIndex] += Math.Round(unit.Strength / (i + 1), 0);
                            }
                        }
                    });
                }
            }

            for (var i = 0; i < numberOfPlayers; i++)
            {
                foreach (var structure in board.Structures)
                {
                    const double ownStructureInfluence = 1;
                    const double enemyStructureInfluence = 1.1;
                    structure.Tile.StructureInfluence[i] += structure.OwnerIndex == i ? ownStructureInfluence : enemyStructureInfluence;
                    for (var j = 1; j < 5; j++)
                    {
                        var hexesInRing = Hex.HexRing(structure.Tile.Hex, j);

                        hexesInRing.ForEach(x =>
                        {
                            var index = Hex.HexToIndex(x, board.Width);
                            if (index >= 0 && index < board.TileArray.Length)
                                board[index].StructureInfluence[i] += (structure.OwnerIndex == i ? ownStructureInfluence : enemyStructureInfluence) / (j + 1);
                        });
                    }
                }

                board.Tiles.ToList().ForEach(x =>
                {
                    CalculateAggregateInfluenceMap(x, numberOfPlayers, i, MovementType.Airborne);
                    CalculateAggregateInfluenceMap(x, numberOfPlayers, i, MovementType.Land);
                    CalculateAggregateInfluenceMap(x, numberOfPlayers, i, MovementType.Water);
                });
            }
        }
        private static void CalculateAggregateInfluenceMap(Tile x, int numberOfPlayers, int i, MovementType movementType)
        {
            x.AggregateInfluence[movementType][i] = x.UnitCountInfluence[movementType][i] - x.StructureInfluence[i];
            for (var j = 0; j < numberOfPlayers; j++)
            {
                if (i == j)
                    continue;

                x.AggregateInfluence[movementType][i] -= x.UnitCountInfluence[movementType][j];
            }
        }
    }
}
