using Hexagon;

namespace Visualise;

public class Edge(Hex origin, Hex destination, ArgbColour colour, bool isPort)
{
    public Hex Origin = origin;
    public Hex Destination = destination;
    public ArgbColour Colour = colour;
    public bool IsPort = isPort;
}
