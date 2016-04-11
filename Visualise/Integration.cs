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
        public static void DrawHexagonImage(string fileName, IEnumerable<Tile> tiles, string[,] labels = null, List<Vector> lines = null, List<Structure> tileStructures = null, List<MilitaryUnit> units = null, int imageWidth = 1200, int imageHeight = 1000)
        {
            var hexagonColours = new Dictionary<PointF, Brush>();
            tiles.ToList().ForEach(x => hexagonColours.Add(new PointF(x.X, x.Y), GetBrush(x.TerrainType)));

            var vectors = new List<Vector>();
            Board.TileEdges.ForEach(x => vectors.Add(new Vector(x.Tiles[0].Location, x.Tiles[1].Location, EdgeToColour(x), x.BaseEdgeType) { EdgeType = x.EdgeType }));

            if (lines != null)
                vectors.AddRange(lines);


            var bitmap = new Bitmap(imageWidth, imageHeight);
            var graphics = Graphics.FromImage(bitmap);

            HexGrid.DrawBoard(graphics, bitmap.Width, bitmap.Height, hexagonColours, labels, tileStructures);

            vectors.ForEach(x => HexGrid.DrawLine(graphics, new GameModel.Point(x.Origin.X, x.Origin.Y), new GameModel.Point(x.Destination.X, x.Destination.Y), new Pen(Color.FromArgb(x.Colour.Alpha, x.Colour.Red, x.Colour.Green, x.Colour.Blue), x.EdgeType == EdgeType.Road ? 10 : 3), x.BaseEdgeType));

            if (tileStructures != null)
                tileStructures.ForEach(x => HexGrid.DrawRectangle(graphics, x.Location, new SolidBrush(Color.Red)));

            if (units != null)
            {
                var unitsByLocation = units.GroupBy(x => x.Tile);

                foreach (var group in unitsByLocation)
                {
                    var unitsAtLocation = group.OrderBy(x => x.OwnerId).ToList();

                    for (var i = 0; i < unitsAtLocation.Count; i ++)
                    {
                        HexGrid.DrawCircle(graphics, group.Key.Location, (float)(((i + 1) / (float)unitsAtLocation.Count) * Math.PI * 2), unitsAtLocation[i].OwnerId == 1 ? new SolidBrush(Color.Red) : new SolidBrush(Color.Blue));
                    }
                }

                
            }

            bitmap.Save(fileName);
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

        private static Brush GetBrush(TerrainType terrainType)
        {
            switch (terrainType)
            {
                case TerrainType.Grassland:
                    return Brushes.GreenYellow;
                case TerrainType.Steppe:
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
