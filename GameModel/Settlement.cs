namespace GameModel;

public enum SettlementType
{
    None,
    Outpost,
    Fortress,
    City
}


public class Settlement
{
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

    public int Index;
    public Tile Location;
    public int OwnerIndex;
    public float Supply;

    public Settlement(int index, SettlementType settlementType, Tile tile, int ownerIndex = 0, int supply = 10)
    {
        Index = index;
        SettlementType = settlementType;
        Location = tile;
        OwnerIndex = ownerIndex;
        Supply = supply;

        Location?.Settlement = this;            
    }

    public SettlementType SettlementType { get; set;}
}
