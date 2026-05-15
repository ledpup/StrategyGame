using System;
using System.Collections.Generic;
using System.Linq;
using GameModel;

namespace Visualise;

public enum RenderPipeline
{
    Board,
    Edges,
    Settlements,
    Units,
    Labels
}
public class GameBoardRenderer
{
    public static GameBoardDrawing2D Render(RenderPipeline renderBegin, RenderPipeline renderUntil, int boardWidth, int boardHeight, IEnumerable<Tile> tiles = null, IEnumerable<GameModel.Edge> edges = null, List<Settlement> settlements = null, string[] labels = null, List<Centreline> lines = null, List<MilitaryUnit> units = null, Tile circles = null)
    {
        var gameBoardDrawing2D = new GameBoardDrawing2D(boardWidth, boardHeight);

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


            gameBoardDrawing2D.DrawBoard(hexagonColours);
        }
        if (renderUntil == RenderPipeline.Board)
            return gameBoardDrawing2D;

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

                edgesToRender.ForEach(x => gameBoardDrawing2D.DrawEdge(
                    x.Origin,
                    x.Destination,
                    x.Colour,
                    x.IsPort));

                //HexGrid.DrawCurvedRoads(graphics, vectors.Where(x => x.EdgeType == EdgeType.Road).ToList());

                centrelines.ForEach(x => gameBoardDrawing2D.DrawCentreline(x.Origin, x.Destination, x.Colour, x.Width));

                var ports = edges.Where(x => x.EdgeType == EdgeType.Port).ToList();

            }
        }
        if (renderUntil == RenderPipeline.Edges)
            return gameBoardDrawing2D;

        if (lines != null)
            lines.ForEach(x => gameBoardDrawing2D.DrawCentreline(x.Origin, x.Destination, x.Colour, x.Width));



        if (renderBegin <= RenderPipeline.Settlements)
        {
            if (settlements != null)
            {
                settlements.ForEach(x =>
                {
                    var colour = GameBoardRenderer.SettlementColour(x);
                    gameBoardDrawing2D.DrawRectangle(x.Location.Hex, colour);
                });
            }
        }
        if (renderUntil == RenderPipeline.Settlements)
            return gameBoardDrawing2D;

        if (renderBegin <= RenderPipeline.Units)
        {
            if (units != null)
            {
                var unitsByLocation = units.GroupBy(x => x.Location);

                foreach (var group in unitsByLocation)
                {
                    var unitsAtLocation = group.OrderBy(x => x.Owner.Colour).ToList();

                    for (var i = 0; i < unitsAtLocation.Count; i++)
                    {
                        var colour = UnitColour(unitsAtLocation[i]);
                        switch (unitsAtLocation[i].MovementType)
                        {
                            case OperationalDomain.Airborne:
                                gameBoardDrawing2D.DrawTriangle(group.Key.Hex, (float)((i + 1) / (float)unitsAtLocation.Count * Math.PI * 2), colour);
                                break;

                            case OperationalDomain.Waterbound:
                                gameBoardDrawing2D.DrawTrapezium(group.Key.Hex, (float)((i + 1) / (float)unitsAtLocation.Count * Math.PI * 2), colour);
                                break;

                            case OperationalDomain.Land:
                                gameBoardDrawing2D.DrawCircle(group.Key.Hex, (float)((i + 1) / (float)unitsAtLocation.Count * Math.PI * 2), colour);
                                break;
                        }
                    }
                }
            }
        }
        if (renderUntil == RenderPipeline.Units)
            return gameBoardDrawing2D;

        if (renderBegin <= RenderPipeline.Labels)
        {
            gameBoardDrawing2D.LabelHexes(Colours.Black, 0, 0, labels, boardWidth);
        }

        return gameBoardDrawing2D;
    }

    internal static ArgbColour PlayerColour(int playerIndex)
    {
        return playerIndex == 0 ? Colours.Blue : Colours.Red;
    }

    internal static ArgbColour PlayerColour(PlayerColour playerColour)
    {
        return playerColour == GameModel.PlayerColour.Red ? Colours.Blue : Colours.Red;
    }

    public static ArgbColour UnitColour(MilitaryUnit unit)
    {
        return unit.IsAlive ? PlayerColour(unit.Owner.Colour) : Colours.Black;
    }

    public static ArgbColour SettlementColour(Settlement settlement)
    {
        return PlayerColour(settlement.Owner.Colour);
    }

    public static void RenderAndSave(string fileName, int boardWidth, int boardHeight, IEnumerable<Tile> tiles, IEnumerable<GameModel.Edge> edges = null, List<Settlement> settlements = null, string[] labels = null, List<Centreline> lines = null, List<MilitaryUnit> units = null, Tile circles = null)
    {
        var gameBoardDrawing2D = Render(RenderPipeline.Board, RenderPipeline.Labels, boardWidth, boardHeight, tiles, edges, settlements, labels, lines, units, circles);
        gameBoardDrawing2D.Save(fileName);
    }

    public static void RenderLabelsAndSave(string fileName, int boardWidth, int boardHeight, string[] labels)
    {
        var gameBoardDrawing2D = Render(RenderPipeline.Labels, RenderPipeline.Labels, boardWidth, boardHeight, labels: labels);
        gameBoardDrawing2D.Save(fileName);
    }
    private static ArgbColour EdgeToColour(EdgeType edgeType)
    {
        return edgeType switch
        {
            EdgeType.River => Colours.DodgerBlue,
            EdgeType.Forest => Colours.DarkGreen,
            EdgeType.Hill => Colours.SandyBrown,
            EdgeType.Mountain => Colours.Brown,
            EdgeType.Port => Colours.DarkBlue,
            _ => Colours.Black,
        };
    }

    private static ArgbColour GetColour(TerrainType terrainType)
    {
        return terrainType switch
        {
            TerrainType.Grassland => Colours.GreenYellow,
            TerrainType.Desert => Colours.Yellow,
            TerrainType.Forest => Colours.DarkGreen,
            TerrainType.Hill => Colours.SandyBrown,
            TerrainType.Mountain => Colours.Brown,
            TerrainType.Water => Colours.LightBlue,
            TerrainType.Swamp => Colours.DarkGray,
            TerrainType.Reef => Colours.DarkBlue,
            _ => Colours.Black,
        };
    }
}
