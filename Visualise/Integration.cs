using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameModel;
using System.Drawing;

namespace Visualise
{

    public class Integration
    {
        public static void DrawHexagonImage(string fileName, IEnumerable<Tile> tiles, string[,] labels = null, List<Vector> lines = null)
        {
            var hexagonColours = new Dictionary<PointF, Brush>();
            tiles.ToList().ForEach(x => hexagonColours.Add(new PointF(x.X, x.Y), GetBrush(x.TerrainType)));

            var vectors = new List<Vector>();
            Board.TileEdges.ForEach(x => vectors.Add(new Vector(x.Tiles[0].Location, x.Tiles[1].Location, EdgeToColour(x), x.BaseEdgeType) { EdgeType = x.EdgeType }));

            if (lines != null)
                vectors.AddRange(lines);


            DrawHexagonImage(fileName, hexagonColours, vectors, labels);
        }

        private static ArgbColour EdgeToColour(Edge x)
        {
            switch (x.EdgeType)
            {
                case EdgeType.River:
                    return ArgbColour.DodgerBlue;
                case EdgeType.Road:
                    return ArgbColour.SaddleBrown;
                case EdgeType.Forest:
                    return ArgbColour.DarkGreen;
                case EdgeType.Hill:
                    return ArgbColour.SandyBrown;
                case EdgeType.Mountain:
                    return ArgbColour.Brown;
                default:
                    return ArgbColour.Black;
            }
        }

        private static void DrawHexagonImage(string fileName, Dictionary<PointF, Brush> hexagonColours, List<Vector> vectors, string[,] labels, int imageWidth = 1200, int imageHeight = 1000)
        {
            var bitmap = new Bitmap(imageWidth, imageHeight);
            var graphics = Graphics.FromImage(bitmap);

            HexGrid.DrawBoard(graphics, bitmap.Width, bitmap.Height, hexagonColours, labels);

            vectors.ForEach(x => HexGrid.DrawLine(graphics, new GameModel.Point(x.Origin.X, x.Origin.Y), new GameModel.Point(x.Destination.X, x.Destination.Y), new Pen(Color.FromArgb(x.Colour.Alpha, x.Colour.Red, x.Colour.Green, x.Colour.Blue), x.EdgeType == EdgeType.Road ? 10 : 3), x.BaseEdgeType));


            bitmap.Save(fileName);
        }

        private static Brush GetBrush(TerrainType terrainType)
        {
            switch (terrainType)
            {
                case TerrainType.Grassland:
                    return Brushes.GreenYellow;
                case TerrainType.Desert:
                    return Brushes.Yellow;
                case TerrainType.Forest:
                    return Brushes.DarkGreen;
                case TerrainType.Hill:
                    return Brushes.SandyBrown;
                case TerrainType.Mountain:
                    return Brushes.Brown;
                case TerrainType.Water:
                    return Brushes.LightBlue;
                case TerrainType.Wetland:
                    return Brushes.DarkGray;
                case TerrainType.Reef:
                    return Brushes.DarkBlue;
            }
            return Brushes.Black;
        }
    }
}
