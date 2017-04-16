using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameModel;
using System.Drawing;
using System.Drawing.Drawing2D;

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
    public class GameBoardRenderer
    {
        public static Bitmap Render(Bitmap bitmap, RenderPipeline renderBegin, RenderPipeline renderUntil, int boardWidth, IEnumerable<Tile> tiles = null, IEnumerable<Edge> edges = null, List<Structure> structures = null, string[,] labels = null, List<Vector> lines = null, List<MilitaryUnit> units = null, Tile circles = null)
        {
            var drawing = new GameBoardDrawing2D(bitmap, boardWidth);

            if (renderBegin <= RenderPipeline.Board)
            {
                var hexagonColours = new Dictionary<GameModel.Point, Brush>();

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
                    hexagonColours.Add(new GameModel.Point(x.X, x.Y), new SolidBrush(Color.FromArgb(colour.Alpha, colour.Red, colour.Green, colour.Blue)));
                }
                );

                
                drawing.DrawBoard(bitmap.Width, bitmap.Height, hexagonColours);
            }
            if (renderUntil == RenderPipeline.Board)
                return bitmap;

            if (renderBegin <= RenderPipeline.Vectors)
            {
                var vectors = new List<Vector>();
                if (edges != null)
                {
                    edges.ToList().ForEach(x =>
                    {
                        if (x.EdgeType == EdgeType.Bridge)
                            vectors.Add(new Vector(x.Origin.Point, x.Destination.Point, EdgeToColour(EdgeType.River), BaseEdgeType.Hexside) { EdgeType = EdgeType.River });
                        vectors.Add(new Vector(x.Origin.Point, x.Destination.Point, EdgeToColour(x.EdgeType), x.BaseEdgeType) { EdgeType = x.EdgeType });
                    });


                    if (lines != null)
                        vectors.AddRange(lines);

                    vectors.ForEach(x => drawing.DrawEdge( 
                        new GameModel.Point(x.Origin.X, x.Origin.Y), 
                        new GameModel.Point(x.Destination.X, x.Destination.Y), 
                        new Pen(Color.FromArgb(x.Colour.Alpha, x.Colour.Red, x.Colour.Green, x.Colour.Blue), 
                        x.EdgeType == EdgeType.Road || x.EdgeType == EdgeType.Bridge ? 6 : 3), 
                        x.BaseEdgeType == BaseEdgeType.CentreToCentre ? true : false,
                        x.EdgeType == EdgeType.Port ? true : false));

                    //HexGrid.DrawCurvedRoads(graphics, vectors.Where(x => x.EdgeType == EdgeType.Road).ToList());


                    var ports = edges.Where(x => x.EdgeType == EdgeType.Port).ToList();

                }
            }
            if (renderUntil == RenderPipeline.Vectors)
                return bitmap;

            drawing.DrawEdge(new GameModel.Point(0,0), new GameModel.Point(0,0), 2, new Pen(Color.Red));

            if (renderBegin <= RenderPipeline.Structures)
            {
                if (structures != null)
                {
                    structures.ForEach(x =>
                    {
                        var colour = x.Colour;
                        drawing.DrawRectangle(x.Location.Point, new SolidBrush(Color.FromArgb(colour.Alpha, colour.Red, colour.Green, colour.Blue)));
                    });
                }
            }
            if (renderUntil == RenderPipeline.Structures)
                return bitmap;

            if (renderBegin <= RenderPipeline.Units)
            {
                if (units != null)
                {
                    var unitsByLocation = units.GroupBy(x => x.Location);

                    foreach (var group in unitsByLocation)
                    {
                        var unitsAtLocation = group.OrderBy(x => x.OwnerIndex).ToList();

                        for (var i = 0; i < unitsAtLocation.Count; i++)
                        {
                            var colour = Color.FromArgb(unitsAtLocation[i].UnitColour.Alpha, unitsAtLocation[i].UnitColour.Red, unitsAtLocation[i].UnitColour.Green, unitsAtLocation[i].UnitColour.Blue);
                            var brush = new SolidBrush(colour);
                            switch (unitsAtLocation[i].MovementType)
                            {
                                case MovementType.Airborne:
                                    drawing.DrawTriangle(group.Key.Point, (float)(((i + 1) / (float)unitsAtLocation.Count) * Math.PI * 2), brush);
                                    break;

                                case MovementType.Water:
                                    drawing.DrawTrapezium(group.Key.Point, (float)(((i + 1) / (float)unitsAtLocation.Count) * Math.PI * 2), brush);
                                    break;

                                case MovementType.Land:
                                    drawing.DrawCircle(group.Key.Point, (float)(((i + 1) / (float)unitsAtLocation.Count) * Math.PI * 2), brush);
                                    break;
                            }
                        }
                    }
                }
            }
            if (renderUntil == RenderPipeline.Units)
                return bitmap;

            if (renderBegin <= RenderPipeline.Labels)
            {
                drawing.LabelHexes(Pens.Black, 0, bitmap.Width, 0, bitmap.Height, labels);
            }
            
            return bitmap;
        }

        public static void RenderAndSave(string fileName, int boardWidth, IEnumerable<Tile> tiles, IEnumerable<Edge> edges = null, List<Structure> structures = null, string[,] labels = null, List<Vector> lines = null, List<MilitaryUnit> units = null, int imageWidth = 1200, int imageHeight = 1000, Tile circles = null)
        {
            var bitmap = new Bitmap(imageWidth, imageHeight);
            bitmap = Render(bitmap, RenderPipeline.Board, RenderPipeline.Labels, boardWidth, tiles, edges, structures, labels, lines, units, circles);
            bitmap.Save(fileName);
        }

        public static void RenderLabelsAndSave(string fileName, Bitmap bitmap, int boardWidth, string[,] labels)
        {
            bitmap = Render(bitmap, RenderPipeline.Labels, RenderPipeline.Labels, boardWidth, labels: labels);
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
                case EdgeType.Port:
                    return Colours.DarkBlue;
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
