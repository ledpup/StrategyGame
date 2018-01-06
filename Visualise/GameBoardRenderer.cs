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
        Edges,
        Structures,
        Units,
        Labels
    }
    public class GameBoardRenderer
    {
        public static Bitmap Render(Bitmap bitmap, RenderPipeline renderBegin, RenderPipeline renderUntil, int boardHeight, IEnumerable<Tile> tiles = null, IEnumerable<GameModel.Edge> edges = null, List<Structure> structures = null, string[,] labels = null, List<Centreline> lines = null, List<MilitaryUnit> units = null, Tile circles = null)
        {
            var drawing = new GameBoardDrawing2D(bitmap, boardHeight);

            if (renderBegin <= RenderPipeline.Board)
            {
                var hexagonColours = new Dictionary<Hexagon.Point, Brush>();

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
                    hexagonColours.Add(new Hexagon.Point(x.X, x.Y), new SolidBrush(Color.FromArgb(colour.Alpha, colour.Red, colour.Green, colour.Blue)));
                }
                );

                
                drawing.DrawBoard(bitmap.Width, bitmap.Height, hexagonColours);
            }
            if (renderUntil == RenderPipeline.Board)
                return bitmap;

            if (renderBegin <= RenderPipeline.Edges)
            {
                var edgesToRender = new List<Edge>();
                var centrelines = new List<Centreline>();
                if (edges != null)
                {
                    edges.ToList().ForEach(x =>
                    {
                        if (x.HasRoad)
                        {
                            centrelines.Add(new Centreline(x.Origin.Point, x.Destination.Point, Colours.SaddleBrown, x.EdgeType == EdgeType.River));
                        }
                        else
                        {
                            edgesToRender.Add(new Edge(x.Origin.Point, x.Destination.Point, EdgeToColour(x.EdgeType), x.EdgeType == EdgeType.Port));
                        }
                    });


                    //if (lines != null)
                    //    edgesToRender.AddRange(lines);

                    edgesToRender.ForEach(x => drawing.DrawEdge(
                        x.Origin.Hex,
                        x.Destination.Hex,
                        x.Colour,
                        x.IsPort));

                    //HexGrid.DrawCurvedRoads(graphics, vectors.Where(x => x.EdgeType == EdgeType.Road).ToList());

                    centrelines.ForEach(x => drawing.DrawCentreline(x.Origin.Hex, x.Destination.Hex, x.Colour, x.Width));

                    var ports = edges.Where(x => x.EdgeType == EdgeType.Port).ToList();

                }
            }
            if (renderUntil == RenderPipeline.Edges)
                return bitmap;

            if (lines != null)
                lines.ForEach(x => drawing.DrawCentreline(x.Origin.Hex, x.Destination.Hex, Colours.Black, x.Width));



            if (renderBegin <= RenderPipeline.Structures)
            {
                if (structures != null)
                {
                    structures.ForEach(x =>
                    {
                        var colour = GameBoardRenderer.StructureColour(x);
                        drawing.DrawRectangle(x.Location.Point, colour);
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
                            var colour = UnitColour(unitsAtLocation[i]);
                            switch (unitsAtLocation[i].MovementType)
                            {
                                case MovementType.Airborne:
                                    drawing.DrawTriangle(group.Key.Point, (float)(((i + 1) / (float)unitsAtLocation.Count) * Math.PI * 2), colour);
                                    break;

                                case MovementType.Water:
                                    drawing.DrawTrapezium(group.Key.Point, (float)(((i + 1) / (float)unitsAtLocation.Count) * Math.PI * 2), colour);
                                    break;

                                case MovementType.Land:
                                    drawing.DrawCircle(group.Key.Point, (float)(((i + 1) / (float)unitsAtLocation.Count) * Math.PI * 2), colour);
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

        internal static ArgbColour PlayerColour(int playerIndex)
        {
            return playerIndex == 0 ? Colours.Blue : Colours.Red;
        }

        public static ArgbColour UnitColour(MilitaryUnit unit)
        {
            return unit.IsAlive ? PlayerColour(unit.OwnerIndex) : Colours.Black;
        }

        public static ArgbColour StructureColour(Structure structure)
        {
            return PlayerColour(structure.OwnerIndex);
        }

        public static void RenderAndSave(string fileName, int boardHeight, IEnumerable<Tile> tiles, IEnumerable<GameModel.Edge> edges = null, List<Structure> structures = null, string[,] labels = null, List<Centreline> lines = null, List<MilitaryUnit> units = null, int imageWidth = 1200, int imageHeight = 1000, Tile circles = null)
        {
            var bitmap = new Bitmap(imageWidth, imageHeight);
            bitmap = Render(bitmap, RenderPipeline.Board, RenderPipeline.Labels, boardHeight, tiles, edges, structures, labels, lines, units, circles);
            bitmap.Save(fileName);
        }

        public static void RenderLabelsAndSave(string fileName, Bitmap bitmap, int boardHeight, string[,] labels)
        {
            bitmap = Render(bitmap, RenderPipeline.Labels, RenderPipeline.Labels, boardHeight, labels: labels);
            bitmap.Save(fileName);
        }
        private static ArgbColour EdgeToColour(EdgeType edgeType)
        {
            switch (edgeType)
            {
                case EdgeType.River:
                    return Colours.DodgerBlue;
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
