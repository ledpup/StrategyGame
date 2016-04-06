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
            Board.TileEdges.ForEach(x => vectors.Add(new Vector { Origin = new GameModel.Point(x.Tiles.First().X, x.Tiles.First().Y), Destination = new GameModel.Point(x.Tiles.Last().X, x.Tiles.Last().Y) }));

            if (lines != null)
                vectors.AddRange(lines);

            DrawHexagonImage(fileName, hexagonColours, vectors, board.Width);
        }

        private static void DrawHexagonImage(string fileName, Dictionary<PointF, Brush> hexagonColours, List<Vector> vectors, int boardWidth, int imageWidth = 1200, int imageHeight = 1000)
        {
            var bitmap = new Bitmap(imageWidth, imageHeight);
            var graphics = Graphics.FromImage(bitmap);

            HexGrid.DrawBoard(graphics, bitmap.Width, bitmap.Height, hexagonColours, boardWidth);

            Pen blackPen = new Pen(Color.FromArgb(255, 0, 0, 0), 3);
            vectors.ForEach(x => HexGrid.DrawLine(graphics, new PointF(x.Origin.X, x.Origin.Y), new PointF(x.Destination.X, x.Destination.Y), blackPen, boardWidth));


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
                case TerrainType.Lake:
                    return Brushes.Blue;
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
