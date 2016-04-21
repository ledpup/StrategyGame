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
            tiles.ToList().ForEach(x => 
            {
                var colour = GetColour(x.TerrainType);
                if (!x.IsSelected)
                {
                    colour.Red = (short)(colour.Red * .3);
                    colour.Green = (short)(colour.Green * .3);
                    colour.Blue = (short)(colour.Blue * .3);
                }
                hexagonColours.Add(new PointF(x.X, x.Y), new SolidBrush(Color.FromArgb(colour.Alpha, colour.Red, colour.Green, colour.Blue)));
            }
            );

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
                        var colour = Color.FromArgb(unitsAtLocation[i].UnitColour.Alpha, unitsAtLocation[i].UnitColour.Red, unitsAtLocation[i].UnitColour.Green, unitsAtLocation[i].UnitColour.Blue);
                        HexGrid.DrawCircle(graphics, group.Key.Location, (float)(((i + 1) / (float)unitsAtLocation.Count) * Math.PI * 2), new SolidBrush(colour));
                    }
                }

                
            }

            HexGrid.LabelHexes(graphics, Pens.Black, 0, bitmap.Width, 0, bitmap.Height, HexGrid.HexWidth, HexGrid.HexHeight, labels);

            bitmap.Save(fileName);
        }

        private static ArgbColour EdgeToColour(Edge x)
        {
            switch (x.EdgeType)
            {
                case EdgeType.River:
                    return Colours.DodgerBlue;
                case EdgeType.Road:
                    return Colours.SaddleBrown;
                case EdgeType.Forest:
                    return Colours.DarkGreen;
                case EdgeType.Hill:
                    return Colours.SandyBrown;
                case EdgeType.Mountain:
                    return Colours.Brown;
                default:
                    return Colours.Black;
            }
        }

        private static ArgbColour GetColour(TerrainType terrainType)
        {
            switch (terrainType)
            {
                case TerrainType.Grassland:
                    return Colours.GreenYellow;
                case TerrainType.Steppe:
                    return Colours.Yellow;
                case TerrainType.Forest:
                    return Colours.DarkGreen;
                case TerrainType.Hill:
                    return Colours.SandyBrown;
                case TerrainType.Mountain:
                    return Colours.Brown;
                case TerrainType.Water:
                    return Colours.LightBlue;
                case TerrainType.Wetland:
                    return Colours.DarkGray;
                case TerrainType.Reef:
                    return Colours.DarkBlue;
            }
            return Colours.Black;
        }
    }
}
