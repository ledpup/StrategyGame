namespace GameModel;

public class UnitEvent(int turn, double value, string reason)
{
    public int Turn { get; set; } = turn;

    public double Value { get; set; } = value;

    public string Reason { get; set; } = reason;
}
