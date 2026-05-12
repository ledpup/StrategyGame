using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GameModel;
using ComputerOpponent;
using Visualise;
using GameModel.Commands;

namespace Tests;

[TestClass]
public class MoveTests
{
    [TestMethod]
    public void TerrainTypeTests()
    {
        Assert.IsTrue(Terrain.AllLand.HasFlag(TerrainType.Desert));
        Assert.IsTrue(Terrain.AllLand.HasFlag(TerrainType.Hill));

        Assert.IsTrue(!Terrain.NonMountainousLand.HasFlag(TerrainType.Mountain));
    }

    [TestMethod]
    public void LandUnitMoveList()
    {
        var board = new GameState(new Board(BoardTests.GameBoard, BoardTests.TileEdges));

        var units = new List<MilitaryUnit> { new(new UnitTemplate(), location: board[335]) };

        var moves = units[0].PossibleMoves();

        moves.ToList().ForEach(x => x.Edge.Destination.IsSelected = true);

        GameBoardRenderer.RenderAndSave("LandUnitMoves.png", board.Width, board.Height, board.Tiles, board.Edges, board.Settlements, null, null, units);

        Assert.HasCount(11, moves);

        Assert.Contains(x => x.Edge.Destination.Index == 334, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 361, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 336, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 309, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 310, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 308, moves);

        // Can't go into the ocean
        Assert.DoesNotContain(x => x.Edge.Destination.Index == 281, moves);
        Assert.DoesNotContain(x => x.Edge.Destination.Index == 306, moves);
        Assert.DoesNotContain(x => x.Edge.Destination.Index == 333, moves);
        Assert.DoesNotContain(x => x.Edge.Destination.Index == 360, moves);

        // Can't go over mountains
        Assert.DoesNotContain(x => x.Edge.Destination.Index == 337, moves);
        Assert.DoesNotContain(x => x.Edge.Destination.Index == 363, moves);
        Assert.DoesNotContain(x => x.Edge.Destination.Index == 362, moves);
        Assert.DoesNotContain(x => x.Edge.Destination.Index == 388, moves);
        Assert.DoesNotContain(x => x.Edge.Destination.Index == 364, moves);
        Assert.DoesNotContain(x => x.Edge.Destination.Index == 390, moves);
        Assert.DoesNotContain(x => x.Edge.Destination.Index == 389, moves);
    }

    [TestMethod]
    public void LandUnitMoveList2()
    {
        //var board = new GameState(new Board(BoardTests.GameBoard, BoardTests.TileEdges));

        //var units = new List<MilitaryUnit> { new MilitaryUnit(location: board[335]) };

        ////var moves = units[0].PossibleMoves();

        //var blockedHexes = board.Tiles.Where(x => x.TerrainType == TerrainType.Mountain || x.TerrainType == TerrainType.Water).Select(x => x.Hex).ToList();

        //var moves = CalculateRange.UnitRangeForTurn(units[0].Location, units[0].MovementPoints, units[0].UsesRoads, units[0].EdgeMovementCosts, units[0].TerrainMovementCosts);

        //moves.ToList().ForEach(x => board[x.Destination.Index].IsSelected = true);

        //GameBoardRenderer.RenderAndSave("LandUnitMoves2.png", board.Height, board.Tiles, board.Edges, board.Settlements, null, null, units);

        //Assert.AreEqual(11, moves.Count());

        //Assert.IsTrue(moves.Any(x => x.Destination.Index == 334));
        //Assert.IsTrue(moves.Any(x => x.Destination.Index == 361));
        //Assert.IsTrue(moves.Any(x => x.Destination.Index == 336));
        //Assert.IsTrue(moves.Any(x => x.Destination.Index == 309));
        //Assert.IsTrue(moves.Any(x => x.Destination.Index == 310));
        //Assert.IsTrue(moves.Any(x => x.Destination.Index == 308));

        //// Can't go into the ocean
        //Assert.IsFalse(moves.Any(x => x.Destination.Index == 281));
        //Assert.IsFalse(moves.Any(x => x.Destination.Index == 306));
        //Assert.IsFalse(moves.Any(x => x.Destination.Index == 333));
        //Assert.IsFalse(moves.Any(x => x.Destination.Index == 360));

        //// Can't go over mountains
        //Assert.IsFalse(moves.Any(x => x.Destination.Index == 337));
        //Assert.IsFalse(moves.Any(x => x.Destination.Index == 363));
        //Assert.IsFalse(moves.Any(x => x.Destination.Index == 362));
        //Assert.IsFalse(moves.Any(x => x.Destination.Index == 388));
        //Assert.IsFalse(moves.Any(x => x.Destination.Index == 364));
        //Assert.IsFalse(moves.Any(x => x.Destination.Index == 390));
        //Assert.IsFalse(moves.Any(x => x.Destination.Index == 389));
    }

    [TestMethod]
    public void LandUnitMoveListWithRoad()
    {
        var board = new GameState(new Board(BoardTests.GameBoard, BoardTests.TileEdges));

        var units = new List<MilitaryUnit> { new(new UnitTemplate { RoadMovementBonus = 2 }, location: board[345]) };

        var moves = units[0].PossibleMoves();

        moves.ToList().ForEach(x => x.Edge.Destination.IsSelected = true);

        GameBoardRenderer.RenderAndSave("LandUnitMovesOverRoad.png", board.Width, board.Height, board.Tiles, board.Edges, board.Settlements, null, null, units);

        Assert.Contains(x => x.Edge.Destination.Index == 316, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 317, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 343, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 344, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 318, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 373, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 347, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 374, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 402, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 429, moves);

        Assert.DoesNotContain(x => x.Edge.Destination.Index == 346, moves);
        Assert.DoesNotContain(x => x.Edge.Destination.Index == 371, moves);
        Assert.DoesNotContain(x => x.Edge.Destination.Index == 372, moves);
        Assert.DoesNotContain(x => x.Edge.Destination.Index == 400, moves);
    }

    [TestMethod]
    public void LandUnitMoveListWithRoadOverMountain()
    {
        var board = new GameState(new Board(BoardTests.GameBoard, BoardTests.TileEdges));

        var units = new List<MilitaryUnit> { new(new UnitTemplate { RoadMovementBonus = 1 }, 0, 1, board[85], "1st Infantry") };

        var moves = units[0].PossibleMoves();

        moves.ToList().ForEach(x => x.Edge.Destination.IsSelected = true);

        GameBoardRenderer.RenderAndSave("LandUnitMovesOverRoadOverMountain.png", board.Width, board.Height, board.Tiles, board.Edges, board.Settlements, null, null, units);

        Assert.Contains(x => x.Edge.Destination.Index == 30, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 56, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 57, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 59, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 86, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 87, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 114, moves);

        Assert.DoesNotContain(x => x.Edge.Destination.Index == 58, moves);
        Assert.DoesNotContain(x => x.Edge.Destination.Index == 32, moves);
        Assert.DoesNotContain(x => x.Edge.Destination.Index == 60, moves);
        Assert.DoesNotContain(x => x.Edge.Destination.Index == 83, moves);
        Assert.DoesNotContain(x => x.Edge.Destination.Index == 84, moves);
        Assert.DoesNotContain(x => x.Edge.Destination.Index == 112, moves);
        Assert.DoesNotContain(x => x.Edge.Destination.Index == 113, moves);
    }

    [TestMethod]
    public void LandUnitMoveOverMountainWithRoad()
    {
        var board = new GameState(new Board(BoardTests.GameBoard, BoardTests.TileEdges));

        var units = new List<MilitaryUnit>
        {
            new(new UnitTemplate { TransportableBy = [MovementType.Waterbound] }, location: board[4, 3]),
        };

        var moves = units[0].PossibleMoves();

        moves.ToList().ForEach(x => x.Edge.Destination.IsSelected = true);

        GameBoardRenderer.RenderAndSave("InfantryMoveOverMountainWithRoad.png", board.Width, board.Height, board.Tiles, board.Edges, board.Settlements, null, null, units);

        Assert.Contains(x => x.Edge.Destination.Index == 86, moves);
    }

    [TestMethod]
    public void AirborneMoveListWithRoadAndMountain()
    {
        var board = new GameState(new Board(BoardTests.GameBoard, BoardTests.TileEdges));

        var units = new List<MilitaryUnit> { new(new UnitTemplate { MovementType = MovementType.Airborne }, 0, 1, board[85], "1st Infantry") };

        var moves = units[0].PossibleMoves();

        moves.ToList().ForEach(x => x.Edge.Destination.IsSelected = true);

        GameBoardRenderer.RenderAndSave("AirborneUnitMovesWithRoadAndMountain.png", board.Width, board.Height, board.Tiles, board.Edges, board.Settlements, null, null, units);

        Assert.Contains(x => x.Edge.Destination.Index == 30, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 31, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 32, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 56, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 57, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 59, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 60, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 87, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 110, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 111, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 114, moves);

        Assert.Contains(x => x.Edge.Destination.Index == 58 && x.MoveType == MoveType.OnlyPassingThrough, moves);
        Assert.DoesNotContain(x => x.Edge.Destination.Index == 83, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 84 && x.MoveType == MoveType.OnlyPassingThrough, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 86 && x.MoveType == MoveType.OnlyPassingThrough, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 112 && x.MoveType == MoveType.OnlyPassingThrough, moves);
        Assert.DoesNotContain(x => x.Edge.Destination.Index == 113, moves);
    }

    [TestMethod]
    public void AirborneMoveList()
    {
        var board = new GameState(new Board(BoardTests.GameBoard, BoardTests.TileEdges));

        var units = new List<MilitaryUnit> { new(new UnitTemplate { MovementType = MovementType.Airborne, MovementPoints = 3 }, 0, 1, board[364], "1st Airborne") };

        var moves = units[0].PossibleMoves();

        moves.ToList().ForEach(x => x.Edge.Destination.IsSelected = true);

        GameBoardRenderer.RenderAndSave("AirborneUnitMoves.png", board.Width, board.Height, board.Tiles, board.Edges, board.Settlements, null, null, units);

        //Assert.AreEqual(12, moves.Count());

        Assert.Contains(x => x.Edge.Destination.Index == 334, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 308, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 309, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 361, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 335, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 336, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 310, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 311, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 338, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 312, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 389, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 390, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 365, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 339, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 340, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 417, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 366, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 367, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 418, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 419, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 393, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 394, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 445, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 446, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 420, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 421, moves);

        Assert.DoesNotContain(x => x.Edge.Destination.Index == 388, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 362 && x.MoveType == MoveType.OnlyPassingThrough, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 337 && x.MoveType == MoveType.OnlyPassingThrough, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 391 && x.MoveType == MoveType.OnlyPassingThrough, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 392 && x.MoveType == MoveType.OnlyPassingThrough, moves);
        Assert.DoesNotContain(x => x.Edge.Destination.Index == 416, moves);
        Assert.DoesNotContain(x => x.Edge.Destination.Index == 444, moves);
    }


    [TestMethod]
    public void AirborneUnitCanStopOn()
    {
        var airborneUnit = new MilitaryUnit(new UnitTemplate { MovementType = MovementType.Airborne });

        Assert.IsTrue(airborneUnit.CanStopOn.HasFlag(TerrainType.Forest));

        Assert.IsFalse(airborneUnit.CanStopOn.HasFlag(TerrainType.Reef));
        Assert.IsFalse(airborneUnit.CanStopOn.HasFlag(TerrainType.Water));
        Assert.IsFalse(airborneUnit.CanStopOn.HasFlag(TerrainType.Mountain));
    }

    [TestMethod]
    public void AirborneUnitValidMoves()
    {
        var board = new GameState(new Board(BoardTests.GameBoard, BoardTests.TileEdges));

        var unit = new MilitaryUnit(new UnitTemplate { MovementType = MovementType.Airborne }, location: board[1, 1]);
        var moveList = unit.PossibleMoves();

        Assert.Contains(x => x.Edge.Destination == board[1, 2], moveList);
        Assert.Contains(x => x.Edge.Destination == board[1, 3], moveList);
        Assert.Contains(x => x.Edge.Destination == board[2, 2], moveList);
        Assert.Contains(x => x.Edge.Destination == board[2, 1], moveList);
        Assert.Contains(x => x.Edge.Destination == board[3, 1], moveList);
        Assert.Contains(x => x.Edge.Destination == board[3, 2], moveList);
    }

    [TestMethod]
    public void AirborneUnitValidMovesOverWater()
    {
        var board = new GameState(new Board(BoardTests.GameBoard, BoardTests.TileEdges));

        var unit = new MilitaryUnit(new UnitTemplate { MovementType = MovementType.Airborne }, location: board[4, 9]);
        var moveList = unit.PossibleMoves();

        moveList.Where(x => x.MoveType != MoveType.OnlyPassingThrough).ToList().ForEach(x => x.Edge.Destination.IsSelected = true);

        GameBoardRenderer.RenderAndSave("AirborneUnitValidMovesOverWater.png", board.Width, board.Height, board.Tiles, board.Edges, board.Settlements, null, null, [unit]);


        Assert.Contains(x => x.Edge.Destination == board[3, 9] && x.MoveType == MoveType.OnlyPassingThrough, moveList); // Mountain
        Assert.Contains(x => x.Edge.Destination == board[5, 9], moveList);

        Assert.Contains(x => x.Edge.Destination == board[3, 8] && x.MoveType == MoveType.OnlyPassingThrough, moveList); // Water
        Assert.Contains(x => x.Edge.Destination == board[4, 8] && x.MoveType == MoveType.OnlyPassingThrough, moveList); // Water
        Assert.Contains(x => x.Edge.Destination == board[5, 8] && x.MoveType == MoveType.OnlyPassingThrough, moveList); // Water

        Assert.DoesNotContain(x => x.Edge.Destination == board[3, 7], moveList); // Reef
        Assert.Contains(x => x.Edge.Destination == board[4, 7], moveList);
        Assert.Contains(x => x.Edge.Destination == board[5, 7], moveList);

        Assert.DoesNotContain(x => x.Edge.Destination == board[3, 10], moveList); // Water
        Assert.Contains(x => x.Edge.Destination == board[4, 10] && x.MoveType == MoveType.OnlyPassingThrough, moveList); // Water
        Assert.DoesNotContain(x => x.Edge.Destination == board[5, 10], moveList); // Water

        Assert.Contains(x => x.Edge.Destination == board[4, 11], moveList);
    }

    [TestMethod]
    public void AirborneUnitValidMovesOverWaterFromShortestPath()
    {
        var board = new GameState(new Board(BoardTests.GameBoard, BoardTests.TileEdges));

        var unit = new MilitaryUnit(new UnitTemplate { MovementType = MovementType.Airborne, MovementPoints = 4 }, location: board[147]);
        var moveList = unit.PossibleMoves();

        var pathToTransporteesDestination = PathFinder.FindShortestPath(unit.Location, board[196], unit);

        var moveOrder = unit.ShortestPathToMoveCommand(pathToTransporteesDestination.ToArray());

        moveList.Where(x => x.MoveType != MoveType.OnlyPassingThrough).ToList().ForEach(x => x.Edge.Destination.IsSelected = true);
        GameBoardRenderer.RenderAndSave("AirborneUnitValidMovesOverWaterFromShortestPath.png", board.Width, board.Height, board.Tiles, board.Edges, board.Settlements, null, null, [unit]);

        Assert.AreNotEqual(MoveType.OnlyPassingThrough, moveOrder.Moves.Last().MoveType);
    }

    [TestMethod]
    public void AirborneUnitShortestPathWithLongRouteOverWater()
    {
        var board = new GameState(new Board(BoardTests.GameBoard, BoardTests.TileEdges));

        var unit = new MilitaryUnit(new UnitTemplate { MovementType = MovementType.Airborne, MovementPoints = 3 }, location: board[202]);
        var moveList = unit.PossibleMoves();

        var pathToTransporteesDestination = PathFinder.FindShortestPath(unit.Location, board[381], unit);

        var vectors = new List<Centreline>();
        vectors.AddRange(Centreline.PathFindTilesToCentrelines(pathToTransporteesDestination));
        GameBoardRenderer.RenderAndSave("AirborneUnitShortestPathWithLongRouteOverWaterPath.png", board.Width, board.Height, board.Tiles, board.Edges, board.Settlements, null, vectors);

        moveList.Where(x => x.MoveType != MoveType.OnlyPassingThrough).ToList().ForEach(x => x.Edge.Destination.IsSelected = true);
        GameBoardRenderer.RenderAndSave("AirborneUnitShortestPathWithLongRouteOverWater.png", board.Width, board.Height, board.Tiles, board.Edges, board.Settlements, null, null, [unit]);
    }

    [TestMethod]
    public void AirborneUnitShortestPathWithLongerRouteOverWater()
    {
        var board = new GameState(new Board(BoardTests.GameBoard, BoardTests.TileEdges));

        var unit = new MilitaryUnit(new UnitTemplate { MovementType = MovementType.Airborne, MovementPoints = 3 }, location: board[187]);
        var moveList = unit.PossibleMoves();

        var pathToTransporteesDestination = PathFinder.FindShortestPath(unit.Location, board[456], unit);

        var vectors = new List<Centreline>();

        vectors.AddRange(Centreline.PathFindTilesToCentrelines(pathToTransporteesDestination));

        GameBoardRenderer.RenderAndSave("AirborneUnitShortestPathWithLongerRouteOverWater.png", board.Width, board.Height, board.Tiles, board.Edges, board.Settlements, null, vectors);
    }

    [TestMethod]
    public void AirborneUnitShortestPathCornerToCorner()
    {
        var board = new GameState(new Board(BoardTests.GameBoard, BoardTests.TileEdges));

        var unit = new MilitaryUnit(new UnitTemplate { MovementType = MovementType.Airborne, MovementPoints = 3 }, location: board[28]);
        var moveList = unit.PossibleMoves();

        var pathToTransporteesDestination = PathFinder.FindShortestPath(unit.Location, board[484], unit);

        var vectors = new List<Centreline>();

        vectors.AddRange(Centreline.PathFindTilesToCentrelines(pathToTransporteesDestination));

        GameBoardRenderer.RenderAndSave("AirborneUnitShortestPathCornerToCorner.png", board.Width, board.Height, board.Tiles, board.Edges, board.Settlements, null, vectors);
    }

    [TestMethod]
    public void AirborneUnitValidMovesOverContinent()
    {
        var board = new GameState(new Board(BoardTests.GameBoard, BoardTests.TileEdges));

        var unit = new MilitaryUnit(new UnitTemplate { MovementType = MovementType.Airborne, MovementPoints = 3 }, location: board[19, 13]);
        var moveList = unit.PossibleMoves();

        moveList.Where(x => x.MoveType != MoveType.OnlyPassingThrough).ToList().ForEach(x => x.Edge.Destination.IsSelected = true);

        GameBoardRenderer.RenderAndSave("AirborneUnitValidMovesOverContinent.png", board.Width, board.Height, board.Tiles, board.Edges, board.Settlements, null, null, [unit]);

        Assert.HasCount(154, moveList);

        //Assert.IsFalse(moveList.Any(x => x.Neighbour.Tile == board[3, 9])); // Mountain
        //Assert.IsTrue(moveList.Any(x => x.Neighbour.Tile == board[5, 9]));

        //Assert.IsFalse(moveList.Any(x => x.Neighbour.Tile == board[3, 8])); // Water
        //Assert.IsFalse(moveList.Any(x => x.Neighbour.Tile == board[4, 8])); // Water
        //Assert.IsFalse(moveList.Any(x => x.Neighbour.Tile == board[5, 8])); // Water

        //Assert.IsFalse(moveList.Any(x => x.Neighbour.Tile == board[3, 7])); // Reef
        Assert.Contains(x => x.Edge.Destination == board[16, 12], moveList);
        Assert.Contains(x => x.Edge.Destination == board[16, 13], moveList);
        Assert.Contains(x => x.Edge.Destination == board[16, 14], moveList);
        Assert.Contains(x => x.Edge.Destination == board[16, 15], moveList);

        //Assert.IsFalse(moveList.Any(x => x.Neighbour.Tile == board[3, 10])); // Water
        //Assert.IsFalse(moveList.Any(x => x.Neighbour.Tile == board[4, 10])); // Water
        //Assert.IsFalse(moveList.Any(x => x.Neighbour.Tile == board[5, 10])); // Water

        //Assert.IsTrue(moveList.Any(x => x.Neighbour.Tile == board[4, 11]));
    }

    [TestMethod]
    public void LandUnitNearRiverAndRoad()
    {
        var board = new GameState(new Board(BoardTests.GameBoard, BoardTests.TileEdges));

        var units = new List<MilitaryUnit> { new(new UnitTemplate(), 0, 0, board[1, 1], "1st Infantry") };

        var moves = units[0].PossibleMoves();

        moves.ToList().ForEach(x => x.Edge.Destination.IsSelected = true);

        GameBoardRenderer.RenderAndSave("LandUnitNearRiverAndRoad.png", board.Width, board.Height, board.Tiles, board.Edges, board.Settlements, null, null, units);

        Assert.Contains(x => x.Edge.Destination == board[1, 2], moves);
        Assert.Contains(x => x.Edge.Destination == board[2, 2], moves);
        Assert.Contains(x => x.Edge.Destination == board[2, 1], moves);
        Assert.Contains(x => x.Edge.Destination == board[3, 1], moves);

        Assert.DoesNotContain(x => x.Edge.Destination == board[1, 3], moves); // Can't cross river

        Assert.HasCount(7, moves);
    }

    [TestMethod]
    public void LandUnitNearBridgeAndRoad()
    {
        var board = new GameState(new Board(BoardTests.GameBoard, BoardTests.TileEdges));

        var units = new List<MilitaryUnit> { new(new UnitTemplate(), 0, 0, board[141], "1st Infantry") };

        var moves = units[0].PossibleMoves();

        moves.ToList().ForEach(x => x.Edge.Destination.IsSelected = true);

        GameBoardRenderer.RenderAndSave("LandUnitNearBridgeAndRoad.png", board.Width, board.Height, board.Tiles, board.Edges, board.Settlements, null, null, units);

        Assert.HasCount(15, moves);

        Assert.Contains(x => x.Edge.Destination == board[87], moves);
        Assert.Contains(x => x.Edge.Destination == board[88], moves);
        Assert.Contains(x => x.Edge.Destination == board[114], moves);
        Assert.Contains(x => x.Edge.Destination == board[115], moves);
        Assert.Contains(x => x.Edge.Destination == board[140], moves);
        Assert.Contains(x => x.Edge.Destination == board[142], moves);
        Assert.Contains(x => x.Edge.Destination == board[168], moves);

        // Blocked by mountain hexside and hill/wetland terrain            
        Assert.DoesNotContain(x => x.Edge.Destination == board[169], moves);
    }

    [TestMethod]
    public void AmphibiousUnitNearRiverAndRoad()
    {
        var board = new GameState(new Board(BoardTests.GameBoard, BoardTests.TileEdges));

        var units = new List<MilitaryUnit> { new(new UnitTemplate { MovementType = MovementType.Land }, 0, 1, board[1, 1], "1st Amphibious") };
        units[0].TerrainMovementCosts[TerrainType.Swamp] = 1;
        units[0].EdgeMovementCosts[EdgeType.River] = 0;

        var moves = units[0].PossibleMoves();

        moves.ToList().ForEach(x => x.Edge.Destination.IsSelected = true);

        GameBoardRenderer.RenderAndSave("AmphibiousUnitNearRiverAndRoad.png", board.Width, board.Height, board.Tiles, board.Edges, board.Settlements, null, null, units);

        Assert.HasCount(8, moves);

        Assert.Contains(x => x.Edge.Destination == board[1, 2], moves);
        Assert.Contains(x => x.Edge.Destination == board[2, 2], moves);
        Assert.Contains(x => x.Edge.Destination == board[2, 1], moves);
        Assert.Contains(x => x.Edge.Destination == board[3, 1], moves);
        Assert.Contains(x => x.Edge.Destination == board[1, 3], moves);
    }

    [TestMethod]
    public void AquaticUnitMoves()
    {
        var board = new GameState(new Board(BoardTests.GameBoard, BoardTests.TileEdges));

        var units = new List<MilitaryUnit> { new(new UnitTemplate { MovementType = MovementType.Waterbound, MovementPoints = 3 }, 0, 2, board[225], "1st Fleet") };

        var moves = units[0].PossibleMoves();

        moves.ToList().ForEach(x => x.Edge.Destination.IsSelected = true);

        GameBoardRenderer.RenderAndSave("AquaticUnitMoves.png", board.Width, board.Height, board.Tiles, board.Edges, board.Settlements, null, null, units);

        Assert.HasCount(138, moves);

        Assert.Contains(x => x.Edge.Destination.Index == 198, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 226, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 253, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 252, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 251, moves);
        Assert.Contains(x => x.Edge.Destination.Index == 224, moves);

        Assert.DoesNotContain(x => x.Edge.Destination.Index == 196, moves);
        Assert.DoesNotContain(x => x.Edge.Destination.Index == 199, moves);
    }

    [TestMethod]
    public void EmbarkOnShip()
    {
        var board = new GameState(new Board(BoardTests.GameBoard, BoardTests.TileEdges));

        board.Units =
        [
            new(new UnitTemplate { MovementPoints = 3, TransportableBy = [MovementType.Waterbound] }, 0, location: board[196]),
            new(new UnitTemplate { MovementType = MovementType.Waterbound, IsTransporter = true }, 1, location: board[224]),
        ];

        var computerPlayer = new ComputerPlayer(board.Units);
        computerPlayer.SetStrategicAction(board);

        var unitOrders = computerPlayer.CreateOrders(board, board.Units);
        board.ResolveOrders(unitOrders);

        //var lines = new List<Centreline>();
        //unitOrders.ForEach(x => unitOrders.AddRange(Centreline.MoveOrderToCentrelines((MoveOrder)x)));

        //GameBoardRenderer.RenderAndSave("EmbarkOnShip.png", board.Height, board.Tiles, board.Edges, board.Settlements, null, null, board.Units);

        Assert.AreEqual(board[8, 8], board.Units[0].Location);
    }

    [TestMethod]
    public void ResolveMove()
    {
        var board = new GameState(new Board(BoardTests.GameBoard, BoardTests.TileEdges));

        board.Units =
        [
            new(new UnitTemplate { MovementPoints = 5 }, location: board[1, 1]),
            new(new UnitTemplate(), location: board[1, 1]),
        ];

        var moves1 = new Move[]
                {
                    new(board[1, 1], board[1, 2], null, 5, 1),
                    new(board[1, 2], board[2, 2], null, 4, 2),
                    new(board[2, 2], board[3, 2], null, 3, 3),
                };

        var moves2 = new Move[]
                {
                    new(board[1, 1], board[1, 2], null, 2, 1),
                    new(board[1, 2], board[2, 2], null, 1, 2),
                };

        var moveOrders = new List<IUnitCommand>
        {
            new MoveCommand(moves1, board.Units[0]),
            new MoveCommand(moves2, board.Units[1]),
        };

        board.ResolveOrders(moveOrders);

        Assert.AreEqual(board[3, 2], board.Units[0].Location);
        Assert.AreEqual(board[2, 2], board.Units[1].Location);
    }

    [TestMethod]
    public void AdjacentUnitsOfSameStrengthSwapHexes()
    {
        var board = new GameState(new Board(BoardTests.GameBoard, BoardTests.TileEdges));

        board.Units =
        [
            new(new UnitTemplate(), location: board[1, 1]),
            new(new UnitTemplate(), 0, 1, board[2, 2]),
        ];

        var moves1 = new Move[]
                            {
                                new(board[1, 1], board[2, 2], null, 2, 1),
                            };

        var moves2 = new Move[]
                            {
                                new(board[2, 2], board[1, 1], null, 2, 1),
                            };

        var moveOrders = new List<IUnitCommand>
        {
            new MoveCommand(moves1, board.Units[0]),
            new MoveCommand(moves2, board.Units[1]),
        };

        board.ResolveOrders(moveOrders);

        Assert.AreEqual(board[1, 1], board.Units[0].Location);
        Assert.AreEqual(board[2, 2], board.Units[1].Location);
    }

    [TestMethod]
    public void AdjacentUnitsOfDifferentStrengthSwapHexes()
    {
        var board = new GameState(new Board(BoardTests.GameBoard, BoardTests.TileEdges));

        board.Units =
        [
            new(new UnitTemplate(), location: board[1, 1]),
            new(new UnitTemplate(), 0, 1, board[2, 2]),
            new(new UnitTemplate(), 0, 1, board[2, 2]),
        ];

        var moves1 = new Move[]
                            {
                                new(board[1, 1], board[2, 2], null, 1, 1),
                            };

        var moves2 = new Move[]
                            {
                                new(board[2, 2], board[1, 1], null, 1, 1),
                            };

        var moveOrders = new List<IUnitCommand>
        {
            new MoveCommand(moves1, board.Units[0]),
            new MoveCommand(moves2, board.Units[1]),
            new MoveCommand(moves2, board.Units[2]),
        };

        board.ResolveOrders(moveOrders);

        Assert.AreEqual(board[1, 1], board.Units[0].Location); // Unit 0 is prevented from moving because a larger army is moving into their hex from the same hexside
        Assert.AreEqual(board[1, 1], board.Units[1].Location);
        Assert.AreEqual(board[1, 1], board.Units[2].Location);
    }

    [TestMethod]
    public void ResolveMove_ConflictArrises_ConflictedUnitsStop()
    {
        var board = new GameState(new Board(BoardTests.GameBoard, BoardTests.TileEdges));
        board.Units =
        [
            new(new UnitTemplate { MovementPoints = 3 }, 0, 1, board[1, 1], "1st Infantry"),
            new(new UnitTemplate { MovementPoints = 3 }, 1, 2, board[4, 1], "2nd Infantry"),

            new(new UnitTemplate { MovementPoints = 6 }, 2, 1, board[10, 2], "1st Infantry"),
            new(new UnitTemplate(), 3, 2, board[10, 3], "2nd Infantry"),
        ];

        var moves1 = new Move[]
                {
                    new(board[1, 1], board[1, 2], null, 3, 1),
                    new(board[1, 2], board[2, 2], null, 2, 2),
                    new(board[2, 2], board[3, 2], null, 1, 3),
                };
        var moves2 = new Move[]
                {
                    new(board[4, 1], board[3, 1], null, 3, 1),
                    new(board[3, 1], board[2, 2], null, 2, 2),
                    new(board[2, 2], board[2, 1], null, 1, 3),
                };
        var moves3 = new Move[]
                {
                    new(board[10, 2], board[11, 2], null, 6, 1),
                    new(board[11, 2], board[12, 2], null, 5, 2),
                };
        var moves4 = new Move[]
                {
                    new(board[10, 3], board[11, 2], null, 2, 1),
                    new(board[11, 2], board[11, 1], null, 1, 2),
                };

        var moveOrders = new List<IUnitCommand>
        {
            new MoveCommand(moves1, board.Units[0]),
            new MoveCommand(moves2, board.Units[1]),
            new MoveCommand(moves3, board.Units[2]),
            new MoveCommand(moves4, board.Units[3]),
        };

        var lines = new List<Centreline>();
        moveOrders.ForEach(x => lines.AddRange(Centreline.MoveOrderToCentrelines((MoveCommand)x)));

        GameBoardRenderer.RenderAndSave("UnitsPreMove.png", board.Width, board.Height, board.Tiles, board.Edges, board.Settlements, null, lines, board.Units);

        board.ResolveOrders(moveOrders);

        GameBoardRenderer.RenderAndSave("UnitsPostMove.png", board.Width, board.Height, board.Tiles, board.Edges, board.Settlements, null, null, board.Units);

        Assert.AreEqual(board[2, 2], board.Units[0].Location);
        Assert.AreEqual(board[2, 2], board.Units[1].Location);

        Assert.AreEqual(board[12, 2], board.Units[2].Location);
        Assert.AreEqual(board[11, 1], board.Units[3].Location);
    }

    [TestMethod]
    public void ConflictTest()
    {
        var tile1 = new Tile(1, 1, 1);
        var tile2 = new Tile(2, 1, 2);

        var units = new List<MilitaryUnit>
        {
                        new(new UnitTemplate()) { Location = tile1 },
                        new(new UnitTemplate()) { OwnerIndex = 2, Location = tile1 },
                        new(new UnitTemplate()) { Location = tile2 },
        };

        var movingUnits = new List<MilitaryUnit>
        {
            units[0],
            units[2],
        };

        var conflictedUnits = CommandResolver.DetectConflictedUnits(movingUnits, units);

        Assert.HasCount(1, conflictedUnits);
    }
}

