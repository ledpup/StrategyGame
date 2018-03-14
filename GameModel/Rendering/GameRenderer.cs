using Hexagon;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameModel.Rendering
{
    public enum RenderPipeline
    {
        Board,
        Edges,
        Structures,
        Units,
        Labels
    }
    public class GameRenderer
    {
        public static IGameRenderingEngine Render(IGameRenderingEngine gameRenderingEngine, RenderPipeline renderBegin, RenderPipeline renderUntil, int boardWidth, int boardHeight, IEnumerable<Tile> tiles = null, IEnumerable<GameModel.Edge> edges = null, List<Structure> structures = null, string[] labels = null, List<Centreline> lines = null, List<MilitaryUnit> units = null, Tile circles = null)
        {
            //var gameBoardDrawing2D = new GameBoardDrawing2D(boardWidth, boardHeight);

            if (renderBegin <= RenderPipeline.Board)
            {
                var hexagonColours = new Dictionary<Hexagon.Hex, ArgbColour>();

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
                    hexagonColours.Add(x.Hex, colour);
                }
                );


                gameRenderingEngine.DrawBoard(hexagonColours);
            }
            if (renderUntil == RenderPipeline.Board)
                return gameRenderingEngine;

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
                            centrelines.Add(new Centreline(x.Origin.Hex, x.Destination.Hex, Colours.SaddleBrown, x.EdgeType == EdgeType.River));
                        }

                        if (!x.HasRoad || (x.HasRoad && x.EdgeType == EdgeType.River))
                        {
                            edgesToRender.Add(new Edge(x.Origin.Hex, x.Destination.Hex, EdgeToColour(x.EdgeType), x.EdgeType == EdgeType.Port));
                        }
                    });


                    //if (lines != null)
                    //    edgesToRender.AddRange(lines);

                    edgesToRender.ForEach(x => gameRenderingEngine.DrawEdge(
                        x.Origin,
                        x.Destination,
                        x.Colour,
                        x.IsPort));

                    //HexGrid.DrawCurvedRoads(graphics, vectors.Where(x => x.EdgeType == EdgeType.Road).ToList());

                    centrelines.ForEach(x => gameRenderingEngine.DrawCentreline(x.Origin, x.Destination, x.Colour, x.Width));

                    var ports = edges.Where(x => x.EdgeType == EdgeType.Port).ToList();

                }
            }
            if (renderUntil == RenderPipeline.Edges)
                return gameRenderingEngine;

            if (lines != null)
                lines.ForEach(x => gameRenderingEngine.DrawCentreline(x.Origin, x.Destination, x.Colour, x.Width));



            if (renderBegin <= RenderPipeline.Structures)
            {
                if (structures != null)
                {
                    structures.ForEach(x =>
                    {
                        var colour = GameRenderer.StructureColour(x);
                        gameRenderingEngine.DrawRectangle(x.Location.Hex, colour);
                    });
                }
            }
            if (renderUntil == RenderPipeline.Structures)
                return gameRenderingEngine;

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
                                    gameRenderingEngine.DrawTriangle(group.Key.Hex, (float)(((i + 1) / (float)unitsAtLocation.Count) * Math.PI * 2), colour);
                                    break;

                                case MovementType.Water:
                                    gameRenderingEngine.DrawTrapezium(group.Key.Hex, (float)(((i + 1) / (float)unitsAtLocation.Count) * Math.PI * 2), colour);
                                    break;

                                case MovementType.Land:
                                    gameRenderingEngine.DrawCircle(group.Key.Hex, (float)(((i + 1) / (float)unitsAtLocation.Count) * Math.PI * 2), colour);
                                    break;
                            }
                        }
                    }
                }
            }
            if (renderUntil == RenderPipeline.Units)
                return gameRenderingEngine;

            if (renderBegin <= RenderPipeline.Labels)
            {
                gameRenderingEngine.LabelHexes(Colours.Black, 0, 0, labels, boardWidth);
            }
            
            return gameRenderingEngine;
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

        public static void RenderAndSave(IGameRenderingEngine gameRenderingEngine, string fileName, int boardWidth, int boardHeight, IEnumerable<Tile> tiles, IEnumerable<GameModel.Edge> edges = null, List<Structure> structures = null, string[] labels = null, List<Centreline> lines = null, List<MilitaryUnit> units = null, Tile circles = null)
        {
            var gameBoardDrawing2D = Render(gameRenderingEngine, RenderPipeline.Board, RenderPipeline.Labels, boardWidth, boardHeight, tiles, edges, structures, labels, lines, units, circles);
            gameBoardDrawing2D.SaveGameBoardToFile(fileName);
        }

        public static void RenderLabelsAndSave(IGameRenderingEngine gameRenderingEngine, string fileName, int boardWidth, int boardHeight, string[] labels)
        {
            var gameBoardDrawing2D = Render(gameRenderingEngine, RenderPipeline.Labels, RenderPipeline.Labels, boardWidth, boardHeight, labels: labels);
            gameBoardDrawing2D.SaveGameBoardToFile(fileName);
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

        public static ArgbColour GetColour(TerrainType terrainType)
        {
            switch (terrainType)
            {
                case TerrainType.Grassland:
                    return Colours.GreenYellow;
                case TerrainType.Desert:
                    return Colours.YellowNcs;
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

        public static PointD UnitLocationTopLeftCorner(PointD hexCentre, float position, float hexHeight, float unitWidth)
        {
            float Radius = hexHeight / 4;

            var xOnCircle = (float)Math.Cos(position) * Radius + hexCentre.X;
            var yOnCircle = (float)Math.Sin(position) * Radius + hexCentre.Y;

            var xTopLeft = (float)(xOnCircle - (unitWidth / 2));
            var yTopLeft = (float)(yOnCircle - (unitWidth / 2));

            return new PointD(xTopLeft, yTopLeft);
        }
    }
}
