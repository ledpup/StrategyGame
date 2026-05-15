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
    public Settlement(SettlementType settlementType, Tile tile, Player owner, int supply = 10)
    {
        Id = Guid.NewGuid();
        SettlementType = settlementType;
        Location = tile;
        Owner = owner;
        Supply = supply;

        Location?.Settlement = this;
    }

    public Guid Id { get; set; }

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
}
