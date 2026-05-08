using GameModel.Commands;
using System.Collections.Generic;
using System.Linq;

namespace GameModel;

public class GameState
{
    public Board Board { get; }
    public List<MilitaryUnit> Units { get; set; }
    public int Turn { get; set; }
    public Dictionary<int, List<MoveCommand>> MoveCommands { get; }

    // Board pass-throughs
    public int Width => Board.Width;
    public int Height => Board.Height;
    public Tile[] Tiles => Board.TileArray;
    public List<Edge> Edges => Board.Edges;
    public List<Settlement> Settlements => Board.Settlements;
    public Tile this[int index] => Board[index];
    public Tile this[int x, int y] => Board[x, y];

    public GameState(Board board, List<MilitaryUnit> units = null, int turn = 0)
    {
        Board = board;
        Units = units ?? [];
        Turn = turn;
        MoveCommands = [];
        Board.CalculateTemperature(turn);
    }

    public void CalculateTemperature(int turn) => Board.CalculateTemperature(turn);
    public void InitialiseSupply() => Board.InitialiseSupply();

    public IEnumerable<MilitaryUnit> UnitsAt(Tile tile) => Units.Where(x => x.Location == tile);

    public bool OverStackLimit(Tile tile, int playerIndex) => tile.OverStackLimit(UnitsAt(tile), playerIndex);

    public void ResolveStackLimits(int playerIndex)
    {
        Tiles.ToList().ForEach(x =>
        {
            var tileUnits = UnitsAt(x).ToList();
            if (x.OverStackLimit(tileUnits, playerIndex))
            {
                var overStackLimitCount = x.OverStackLimitCount(tileUnits, playerIndex);
                tileUnits
                    .Where(y => y.IsAlive && y.OwnerIndex == playerIndex)
                    .ToList()
                    .ForEach(y => y.ChangeMorale(Turn, -.5 * overStackLimitCount, $"Units are over the stack limit of {x.StackLimit} by {overStackLimitCount} units"));
            }
        });
    }

    public void ResolveOrders(List<IUnitCommand> unitOrders) => new CommandResolver(this).ResolveCommands(unitOrders);

    public List<BattleReport> ConductBattles()
    {
        var battleReports = new List<BattleReport>();
        Tiles.ToList().ForEach(x =>
        {
            var tileUnits = UnitsAt(x).ToList();
            if (Tile.IsInConflict(tileUnits))
            {
                battleReports.Add(BattleResolver.ResolveBattle(x.ToString(), Turn, TerrainType.Mountain, Weather.Cold, tileUnits, 3, SettlementType.Fortress, 2));
            }
        });
        return battleReports;
    }

    public void ChangeSettlementOwners()
    {
        Settlements.ForEach(x =>
        {
            var unitsAtSettlementByOwner = Units.Where(y => y.IsAlive && y.Location == x.Location).GroupBy(y => y.OwnerIndex).ToList();
            if (unitsAtSettlementByOwner.Count == 1)
            {
                if (x.OwnerIndex == unitsAtSettlementByOwner.First().Key)
                    return;
                x.OwnerIndex = unitsAtSettlementByOwner.First().Key;
                var units = unitsAtSettlementByOwner.First().ToList();
                var numberOfUnits = units.Count;
                units.ForEach(y => y.ChangeMorale(Turn, 2D / numberOfUnits, $"Morale increase from pillaging {x.SettlementType}"));
            }
        });
    }
}
