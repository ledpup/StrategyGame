namespace GameModel;

public enum PlayerColour
{
    Red = 1,
    Blue = 2,
    Green = 3,
    Black = 4,
}

public class Player(int id, string name)
{
    public int Id { get; set; } = id;

    public string Name { get; set; } = name;

    public PlayerColour Colour { get; set; } = (PlayerColour)id;

    public override int GetHashCode()
    {
        return Id;
    }

    public override string ToString()
    {
        return Name ?? Id.ToString();
    }
}
