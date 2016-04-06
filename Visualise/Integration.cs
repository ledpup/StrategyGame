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
        public static void DrawHexagonImage(string fileName, Board board, List<Vector> lines = null)
        {
            var hexagonColours = new Dictionary<PointF, Brush>();
            board.Tiles.ToList().ForEach(x => hexagonColours.Add(new PointF(x.X, x.Y), GetBrush(x.TerrainType)));

            var vectors = new List<Vector>();
            Board.TileEdges.ForEach(x => vectors.Add(new Vector(x.Tiles[0].Location, x.Tiles[1].Location, EdgeToColour(x), x.BaseEdgeType)));

            if (lines != null)
                vectors.AddRange(lines);

            DrawHexagonImage(fileName, hexagonColours, vectors, board.Width);
        }

        private static ArgbColour EdgeToColour(Edge x)
        {
            switch (x.EdgeType)
            {
                case EdgeType.Road:
                    return ArgbColour.SaddleBrown;
                case EdgeType.River:
                    return ArgbColour.DodgerBlue;
                default:
                    return ArgbColour.Black;
            }
        }

        private static void DrawHexagonImage(string fileName, Dictionary<PointF, Brush> hexagonColours, List<Vector> vectors, int boardWidth, int imageWidth = 1200, int imageHeight = 1000)
        {
            var bitmap = new Bitmap(imageWidth, imageHeight);
            var graphics = Graphics.FromImage(bitmap);

            HexGrid.DrawBoard(graphics, bitmap.Width, bitmap.Height, hexagonColours, boardWidth);

            vectors.ForEach(x => HexGrid.DrawLine(graphics, new GameModel.Point(x.Origin.X, x.Origin.Y), new GameModel.Point(x.Destination.X, x.Destination.Y), new Pen(Color.FromArgb(x.Colour.Alpha, x.Colour.Red, x.Colour.Green, x.Colour.Blue), 3), boardWidth, x.BaseEdgeType));


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
