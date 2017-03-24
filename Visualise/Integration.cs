using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameModel;
using System.Drawing;

namespace Visualise
{
    public enum RenderPipeline
    {
        Board,
        Vectors,
        Structures,
        Units,
        Labels
    }
    public class Integration
    {

        public static Bitmap Render(Bitmap bitmap, RenderPipeline renderBegin, RenderPipeline renderUntil, IEnumerable<Tile> tiles = null, IEnumerable<Edge> edges = null, List<Structure> structures = null, string[,] labels = null, List<Vector> lines = null, List<MilitaryUnit> units = null, Tile circles = null)
        {
            var graphics = Graphics.FromImage(bitmap);

            if (renderBegin <= RenderPipeline.Board)
            {
                var hexagonColours = new Dictionary<PointF, Brush>();

                var displaySelected = tiles.Any(x => x.IsSelected);

                tiles.ToList().ForEach(x =>
                {
                    var colour = GetColour(x.TerrainType);
                    if (displaySelected && !x.IsSelected)
                    {
                        colour.Red = (short)(colour.Red * .3);
                        colour.Green = (short)(colour.Green * .3);
                        colour.Blue = (short)(colour.Blue * .3);
                    }
                    hexagonColours.Add(new PointF(x.X, x.Y), new SolidBrush(Color.FromArgb(colour.Alpha, colour.Red, colour.Green, colour.Blue)));
                }
                );

                HexGrid.DrawBoard(graphics, bitmap.Width, bitmap.Height, hexagonColours);
            }
            if (renderUntil == RenderPipeline.Board)
                return bitmap;

            HexGrid.DrawCirclesAroundHex(graphics, circles);

            if (renderBegin <= RenderPipeline.Vectors)
            {
                var vectors = new List<Vector>();
                if (edges != null)
                {

                    edges.ToList().ForEach(x =>
                    {
                        if (x.EdgeType == EdgeType.Bridge)
                            vectors.Add(new Vector(x.Tiles[0].Location, x.Tiles[1].Location, EdgeToColour(EdgeType.River), BaseEdgeType.Hexside) { EdgeType = EdgeType.River });
                        vectors.Add(new Vector(x.Tiles[0].Location, x.Tiles[1].Location, EdgeToColour(x.EdgeType), x.BaseEdgeType) { EdgeType = x.EdgeType });
                    });
                }

                if (lines != null)
                    vectors.AddRange(lines);

                vectors.ForEach(x => HexGrid.DrawLine(graphics, new GameModel.Point(x.Origin.X, x.Origin.Y), new GameModel.Point(x.Destination.X, x.Destination.Y), new Pen(Color.FromArgb(x.Colour.Alpha, x.Colour.Red, x.Colour.Green, x.Colour.Blue), x.EdgeType == EdgeType.Road || x.EdgeType == EdgeType.Bridge ? 6 : 3), x.BaseEdgeType));
            }
            if (renderUntil == RenderPipeline.Vectors)
                return bitmap;

            if (renderBegin <= RenderPipeline.Structures)
            {
                if (structures != null)
                {
                    structures.ForEach(x =>
                    {
                        var colour = x.Colour;
                        HexGrid.DrawRectangle(graphics, x.Tile.Location, new SolidBrush(Color.FromArgb(colour.Alpha, colour.Red, colour.Green, colour.Blue)));
                    });
                }
            }
            if (renderUntil == RenderPipeline.Structures)
                return bitmap;

            if (renderBegin <= RenderPipeline.Units)
            {
                if (units != null)
                {
                    var unitsByLocation = units.GroupBy(x => x.Tile);

                    foreach (var group in unitsByLocation)
                    {
                        var unitsAtLocation = group.OrderBy(x => x.OwnerIndex).ToList();

                        for (var i = 0; i < unitsAtLocation.Count; i++)
                        {
                            var colour = Color.FromArgb(unitsAtLocation[i].UnitColour.Alpha, unitsAtLocation[i].UnitColour.Red, unitsAtLocation[i].UnitColour.Green, unitsAtLocation[i].UnitColour.Blue);
                            HexGrid.DrawCircle(graphics, group.Key.Location, (float)(((i + 1) / (float)unitsAtLocation.Count) * Math.PI * 2), new SolidBrush(colour));
                        }
                    }
                }
            }
            if (renderUntil == RenderPipeline.Units)
                return bitmap;

            if (renderBegin <= RenderPipeline.Labels)
            {
                HexGrid.LabelHexes(graphics, Pens.Black, 0, bitmap.Width, 0, bitmap.Height, HexGrid.HexWidth, HexGrid.HexHeight, labels);
            }
            
            return bitmap;
        }

        public static void RenderAndSave(string fileName, IEnumerable<Tile> tiles, IEnumerable<Edge> edges = null, List<Structure> structures = null, string[,] labels = null, List<Vector> lines = null, List<MilitaryUnit> units = null, int imageWidth = 1200, int imageHeight = 1000, Tile circles = null)
        {
            var bitmap = new Bitmap(imageWidth, imageHeight);
            bitmap = Render(bitmap, RenderPipeline.Board, RenderPipeline.Labels, tiles, edges, structures, labels, lines, units, circles);
            bitmap.Save(fileName);
        }

        private static ArgbColour EdgeToColour(EdgeType edgeType)
        {
            switch (edgeType)
            {
                case EdgeType.River:
                    return Colours.DodgerBlue;
                case EdgeType.Road:
                case EdgeType.Bridge:
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
