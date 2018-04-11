using System.Collections.Generic;

namespace GameModel.Rendering
{
    public interface IGameModelForRendering
    {
        int Width { get; }
        int Height { get; }
        IEnumerable<Tile> Tiles { get; }
        List<GameModel.Edge> Edges { get; }
        List<Structure> Structures { get; }
        //string[] Labels { get; }
        //List<Centreline> Lines { get; }
        List<MilitaryUnit> Units { get; }
        //Tile Circles { get; }
    }
}