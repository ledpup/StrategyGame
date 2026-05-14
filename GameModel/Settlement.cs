using System;
using System.Collections.Generic;
using System.Linq;

namespace GameModel;

public enum SettlementType
{
    None,
    Outpost,
    Fortress,
    City,
}

public class Settlement
{
    public Settlement(int id, SettlementType settlementType, Tile tile, Player owner, int supply = 10)
    {
        Id = id;
        SettlementType = settlementType;
        Location = tile;
        Owner = owner;
        Supply = supply;

        Location?.Settlement = this;
    }

    public int Id { get; set; }

    public SettlementType SettlementType { get; set; }

    public Tile Location { get; set; }

    public Player Owner { get; set; }

    public float Supply { get; set; }

    public static float SettlementDefenceModifier(SettlementType type)
    {
        return type switch
        {
            SettlementType.None => 0f,
            SettlementType.Outpost => .2f,
            SettlementType.Fortress => .4f,
            SettlementType.City => .6f,
            _ => 0f,
        };
    }

    public static List<Settlement> ParseSettlements(string[] tilePoints, List<Player> players)
    {
        var settlements = new List<Settlement>();

        if (tilePoints == null)
        {
            return settlements;
        }

        foreach (var point in tilePoints)
        {
            var settlementProperties = point.Split(',');
            var index = int.Parse(settlementProperties[0]);
            var settlementType = Enum.Parse<SettlementType>(settlementProperties[1]);
            var ownerId = int.Parse(settlementProperties[2]);
            var owner = players.FirstOrDefault(p => p.Id == ownerId) ?? new Player(ownerId, ownerId.ToString());
            var supply = int.Parse(settlementProperties[3]);
            var settlement = new Settlement(index, settlementType, TileArray[index], owner, supply);

            settlements.Add(settlement);
        }

        return settlements;
    }
}
