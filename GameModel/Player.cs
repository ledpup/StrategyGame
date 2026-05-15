namespace GameModel;

using System;

public enum PlayerColour
{
    Red,
    Blue,
    Green,
    Black,
}

public class Player(PlayerColour colour, string name)
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = name;

    public PlayerColour Colour { get; set; } = colour;

    public override int GetHashCode()
    {
        return Colour.GetHashCode();
    }

    public override string ToString()
    {
        return Name ?? Id.ToString();
    }
}
