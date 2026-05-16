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
    GameState gameState;

    [TestInitialize]
    public void TestInitialize()
    {
        gameState = new GameState(new Board(BoardTests.GameBoard, BoardTests.TileEdges));
    }

    [TestMethod]
    public void TerrainType_NonMountainousLand_ExcludesMountain()
    {
        Assert.IsTrue(Terrain.AllLand.HasFlag(TerrainType.Desert));
        Assert.IsTrue(Terrain.AllLand.HasFlag(TerrainType.Hill));

        Assert.IsTrue(!Terrain.NonMountainousLand.HasFlag(TerrainType.Mountain));
    }

    [TestMethod]
    public void PossibleMoves_LandUnit_ReturnsExpectedMoves()
    {

        var units = new List<MilitaryUnit> { new(new UnitTemplate(), gameState.Players[0], gameState[335]) };

        var moves = units[0].PossibleMoves();

        moves.ToList().ForEach(x => x.Edge.Destination.IsSelected = true);

        GameBoardRenderer.RenderAndSave("LandUnitMoves.png", gameState.Width, gameState.Height, gameState.Tiles, gameState.Edges, gameState.Settlements, null, null, units);

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
    public void PossibleMoves_LandUnitWithRoad_ReturnsRoadMoves()
    {

        var units = new List<MilitaryUnit> { new(new UnitTemplate { RoadMovementBonus = 2 }, gameState.Players[0], gameState[345]) };

        var moves = units[0].PossibleMoves();

        moves.ToList().ForEach(x => x.Edge.Destination.IsSelected = true);

        GameBoardRenderer.RenderAndSave("LandUnitMovesOverRoad.png", gameState.Width, gameState.Height, gameState.Tiles, gameState.Edges, gameState.Settlements, null, null, units);

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
    public void PossibleMoves_LandUnitWithRoadOverMountain_ReturnsMountainPassMoves()
    {

        var units = new List<MilitaryUnit> { new(new UnitTemplate { RoadMovementBonus = 1 }, gameState.Players[0], gameState[85]) };

        var moves = units[0].PossibleMoves();

        moves.ToList().ForEach(x => x.Edge.Destination.IsSelected = true);

        GameBoardRenderer.RenderAndSave("LandUnitMovesOverRoadOverMountain.png", gameState.Width, gameState.Height, gameState.Tiles, gameState.Edges, gameState.Settlements, null, null, units);

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
    public void PossibleMoves_LandUnitAtMountainRoad_CanMoveOverMountain()
    {

        var units = new List<MilitaryUnit>
        {
            new(new UnitTemplate { TransportableBy = [OperationalDomain.Waterbound] }, gameState.Players[0], gameState[4, 3]),
        };

        var moves = units[0].PossibleMoves();

        moves.ToList().ForEach(x => x.Edge.Destination.IsSelected = true);

        GameBoardRenderer.RenderAndSave("InfantryMoveOverMountainWithRoad.png", gameState.Width, gameState.Height, gameState.Tiles, gameState.Edges, gameState.Settlements, null, null, units);

        Assert.Contains(x => x.Edge.Destination.Index == 86, moves);
    }

    [TestMethod]
    public void PossibleMoves_AirborneUnitNearRoadAndMountain_ReturnsAirborneMoves()
    {

        var units = new List<MilitaryUnit> { new(new UnitTemplate { OperationalDomain = OperationalDomain.Airborne }, gameState.Players[0], gameState[85]) };

        var moves = units[0].PossibleMoves();

        moves.ToList().ForEach(x => x.Edge.Destination.IsSelected = true);

        GameBoardRenderer.RenderAndSave("AirborneUnitMovesWithRoadAndMountain.png", gameState.Width, gameState.Height, gameState.Tiles, gameState.Edges, gameState.Settlements, null, null, units);

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
    public void PossibleMoves_AirborneUnit_ReturnsExpectedMoves()
    {

        var units = new List<MilitaryUnit> { new(new UnitTemplate { OperationalDomain = OperationalDomain.Airborne, MovementPoints = 3 }, gameState.Players[0], gameState[364]) };

        var moves = units[0].PossibleMoves();

        moves.ToList().ForEach(x => x.Edge.Destination.IsSelected = true);

        GameBoardRenderer.RenderAndSave("AirborneUnitMoves.png", gameState.Width, gameState.Height, gameState.Tiles, gameState.Edges, gameState.Settlements, null, null, units);

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
    public void CanStopOn_AirborneUnit_ExcludesWaterReefAndMountain()
    {
        var airborneUnit = new MilitaryUnit(new UnitTemplate { OperationalDomain = OperationalDomain.Airborne }, gameState.Players[0], gameState[1, 1]);

        Assert.IsTrue(airborneUnit.CanStopOn.HasFlag(TerrainType.Forest));

        Assert.IsFalse(airborneUnit.CanStopOn.HasFlag(TerrainType.Reef));
        Assert.IsFalse(airborneUnit.CanStopOn.HasFlag(TerrainType.Water));
        Assert.IsFalse(airborneUnit.CanStopOn.HasFlag(TerrainType.Mountain));
    }

    [TestMethod]
    public void PossibleMoves_AirborneUnitOnLand_ReturnsValidMoves()
    {

        var unit = new MilitaryUnit(new UnitTemplate { OperationalDomain = OperationalDomain.Airborne }, gameState.Players[0], gameState[1, 1]);
        var moveList = unit.PossibleMoves();

        Assert.Contains(x => x.Edge.Destination == gameState[1, 2], moveList);
        Assert.Contains(x => x.Edge.Destination == gameState[1, 3], moveList);
        Assert.Contains(x => x.Edge.Destination == gameState[2, 2], moveList);
        Assert.Contains(x => x.Edge.Destination == gameState[2, 1], moveList);
        Assert.Contains(x => x.Edge.Destination == gameState[3, 1], moveList);
        Assert.Contains(x => x.Edge.Destination == gameState[3, 2], moveList);
    }

    [TestMethod]
    public void PossibleMoves_AirborneUnitNearWater_ReturnsPassingThroughWaterMoves()
    {

        var unit = new MilitaryUnit(new UnitTemplate { OperationalDomain = OperationalDomain.Airborne }, gameState.Players[0], gameState[4, 9]);
        var moveList = unit.PossibleMoves();

        moveList.Where(x => x.MoveType != MoveType.OnlyPassingThrough).ToList().ForEach(x => x.Edge.Destination.IsSelected = true);

        GameBoardRenderer.RenderAndSave("PossibleMoves_AirborneUnitNearWater_ReturnsPassingThroughWaterMoves.png", gameState.Width, gameState.Height, gameState.Tiles, gameState.Edges, gameState.Settlements, null, null, [unit]);


        Assert.Contains(x => x.Edge.Destination == gameState[3, 9] && x.MoveType == MoveType.OnlyPassingThrough, moveList); // Mountain
        Assert.Contains(x => x.Edge.Destination == gameState[5, 9], moveList);

        Assert.Contains(x => x.Edge.Destination == gameState[3, 8] && x.MoveType == MoveType.OnlyPassingThrough, moveList); // Water
        Assert.Contains(x => x.Edge.Destination == gameState[4, 8] && x.MoveType == MoveType.OnlyPassingThrough, moveList); // Water
        Assert.Contains(x => x.Edge.Destination == gameState[5, 8] && x.MoveType == MoveType.OnlyPassingThrough, moveList); // Water

        Assert.DoesNotContain(x => x.Edge.Destination == gameState[3, 7], moveList); // Reef
        Assert.Contains(x => x.Edge.Destination == gameState[4, 7], moveList);
        Assert.Contains(x => x.Edge.Destination == gameState[5, 7], moveList);

        Assert.DoesNotContain(x => x.Edge.Destination == gameState[3, 10], moveList); // Water
        Assert.Contains(x => x.Edge.Destination == gameState[4, 10] && x.MoveType == MoveType.OnlyPassingThrough, moveList); // Water
        Assert.DoesNotContain(x => x.Edge.Destination == gameState[5, 10], moveList); // Water

        Assert.Contains(x => x.Edge.Destination == gameState[4, 11], moveList);
    }

    [TestMethod]
    public void ShortestPath_AirborneUnitOverWater_ReturnsMoveOrderEndingOnLand()
    {

        var unit = new MilitaryUnit(new UnitTemplate { OperationalDomain = OperationalDomain.Airborne, MovementPoints = 4 }, gameState.Players[0], gameState[147]);
        var moveList = unit.PossibleMoves();

        var pathToTransporteesDestination = PathFinder.FindShortestPath(unit.Location, gameState[196], unit);

        var moveOrder = unit.ShortestPathToMoveCommand(pathToTransporteesDestination.ToArray());

        moveList.Where(x => x.MoveType != MoveType.OnlyPassingThrough).ToList().ForEach(x => x.Edge.Destination.IsSelected = true);
        GameBoardRenderer.RenderAndSave("ShortestPath_AirborneUnitOverWater_ReturnsMoveOrderEndingOnLand.png", gameState.Width, gameState.Height, gameState.Tiles, gameState.Edges, gameState.Settlements, null, null, [unit]);

        Assert.AreNotEqual(MoveType.OnlyPassingThrough, moveOrder.Moves.Last().MoveType);
    }

    [TestMethod]
    public void FindShortestPath_AirborneUnitWithLongRouteOverWater_RendersPath()
    {

        var unit = new MilitaryUnit(new UnitTemplate { OperationalDomain = OperationalDomain.Airborne, MovementPoints = 3 }, gameState.Players[0], gameState[202]);
        var moveList = unit.PossibleMoves();

        var pathToTransporteesDestination = PathFinder.FindShortestPath(unit.Location, gameState[381], unit);

        var vectors = new List<Centreline>();
        vectors.AddRange(Centreline.PathFindTilesToCentrelines(pathToTransporteesDestination));
        GameBoardRenderer.RenderAndSave("AirborneUnitShortestPathWithLongRouteOverWaterPath.png", gameState.Width, gameState.Height, gameState.Tiles, gameState.Edges, gameState.Settlements, null, vectors);

        moveList.Where(x => x.MoveType != MoveType.OnlyPassingThrough).ToList().ForEach(x => x.Edge.Destination.IsSelected = true);
        GameBoardRenderer.RenderAndSave("FindShortestPath_AirborneUnitWithLongRouteOverWater_RendersPath.png", gameState.Width, gameState.Height, gameState.Tiles, gameState.Edges, gameState.Settlements, null, null, [unit]);
    }

    [TestMethod]
    public void FindShortestPath_AirborneUnitWithLongerRouteOverWater_RendersPath()
    {

        var unit = new MilitaryUnit(new UnitTemplate { OperationalDomain = OperationalDomain.Airborne, MovementPoints = 3 }, gameState.Players[0], gameState[187]);
        var moveList = unit.PossibleMoves();

        var pathToTransporteesDestination = PathFinder.FindShortestPath(unit.Location, gameState[456], unit);

        var vectors = new List<Centreline>();

        vectors.AddRange(Centreline.PathFindTilesToCentrelines(pathToTransporteesDestination));

        GameBoardRenderer.RenderAndSave("FindShortestPath_AirborneUnitWithLongerRouteOverWater_RendersPath.png", gameState.Width, gameState.Height, gameState.Tiles, gameState.Edges, gameState.Settlements, null, vectors);
    }

    [TestMethod]
    public void FindShortestPath_AirborneUnitCornerToCorner_RendersPath()
    {

        var unit = new MilitaryUnit(new UnitTemplate { OperationalDomain = OperationalDomain.Airborne, MovementPoints = 3 }, gameState.Players[0], gameState[28]);
        var moveList = unit.PossibleMoves();

        var pathToTransporteesDestination = PathFinder.FindShortestPath(unit.Location, gameState[484], unit);

        var vectors = new List<Centreline>();

        vectors.AddRange(Centreline.PathFindTilesToCentrelines(pathToTransporteesDestination));

        GameBoardRenderer.RenderAndSave("FindShortestPath_AirborneUnitCornerToCorner_RendersPath.png", gameState.Width, gameState.Height, gameState.Tiles, gameState.Edges, gameState.Settlements, null, vectors);
    }

    [TestMethod]
    public void PossibleMoves_AirborneUnitOverContinent_ReturnsExpectedMoves()
    {

        var unit = new MilitaryUnit(new UnitTemplate { OperationalDomain = OperationalDomain.Airborne, MovementPoints = 3 }, gameState.Players[0], gameState[19, 13]);
        var moveList = unit.PossibleMoves();

        moveList.Where(x => x.MoveType != MoveType.OnlyPassingThrough).ToList().ForEach(x => x.Edge.Destination.IsSelected = true);

        GameBoardRenderer.RenderAndSave("PossibleMoves_AirborneUnitOverContinent_ReturnsExpectedMoves.png", gameState.Width, gameState.Height, gameState.Tiles, gameState.Edges, gameState.Settlements, null, null, [unit]);

        Assert.HasCount(154, moveList);

        Assert.Contains(x => x.Edge.Destination == gameState[16, 12], moveList);
        Assert.Contains(x => x.Edge.Destination == gameState[16, 13], moveList);
        Assert.Contains(x => x.Edge.Destination == gameState[16, 14], moveList);
        Assert.Contains(x => x.Edge.Destination == gameState[16, 15], moveList);
    }

    [TestMethod]
    public void PossibleMoves_LandUnitNearRiverAndRoad_BlocksRiverWithoutBridge()
    {

        var units = new List<MilitaryUnit> { new(new UnitTemplate(), gameState.Players[0], gameState[1, 1], "1st Infantry") };

        var moves = units[0].PossibleMoves();

        moves.ToList().ForEach(x => x.Edge.Destination.IsSelected = true);

        GameBoardRenderer.RenderAndSave("PossibleMoves_LandUnitNearRiverAndRoad_BlocksRiverWithoutBridge.png", gameState.Width, gameState.Height, gameState.Tiles, gameState.Edges, gameState.Settlements, null, null, units);

        Assert.Contains(x => x.Edge.Destination == gameState[1, 2], moves);
        Assert.Contains(x => x.Edge.Destination == gameState[2, 2], moves);
        Assert.Contains(x => x.Edge.Destination == gameState[2, 1], moves);
        Assert.Contains(x => x.Edge.Destination == gameState[3, 1], moves);

        Assert.DoesNotContain(x => x.Edge.Destination == gameState[1, 3], moves); // Can't cross river

        Assert.HasCount(7, moves);
    }

    [TestMethod]
    public void PossibleMoves_LandUnitNearBridgeAndRoad_AllowsBridgeCrossing()
    {

        var units = new List<MilitaryUnit> { new(new UnitTemplate(), gameState.Players[0], gameState[141], "1st Infantry") };

        var moves = units[0].PossibleMoves();

        moves.ToList().ForEach(x => x.Edge.Destination.IsSelected = true);

        GameBoardRenderer.RenderAndSave("PossibleMoves_LandUnitNearBridgeAndRoad_AllowsBridgeCrossing.png", gameState.Width, gameState.Height, gameState.Tiles, gameState.Edges, gameState.Settlements, null, null, units);

        Assert.HasCount(15, moves);

        Assert.Contains(x => x.Edge.Destination == gameState[87], moves);
        Assert.Contains(x => x.Edge.Destination == gameState[88], moves);
        Assert.Contains(x => x.Edge.Destination == gameState[114], moves);
        Assert.Contains(x => x.Edge.Destination == gameState[115], moves);
        Assert.Contains(x => x.Edge.Destination == gameState[140], moves);
        Assert.Contains(x => x.Edge.Destination == gameState[142], moves);
        Assert.Contains(x => x.Edge.Destination == gameState[168], moves);

        // Blocked by mountain hexside and hill/wetland terrain            
        Assert.DoesNotContain(x => x.Edge.Destination == gameState[169], moves);
    }

    [TestMethod]
    public void PossibleMoves_AmphibiousLandUnitNearRiver_AllowsRiverCrossing()
    {

        var units = new List<MilitaryUnit> { new(new UnitTemplate { OperationalDomain = OperationalDomain.Land }, gameState.Players[0], gameState[1, 1]) };
        units[0].TerrainMovementCosts[TerrainType.Swamp] = 1;
        units[0].EdgeMovementCosts[EdgeType.River] = 0;

        var moves = units[0].PossibleMoves();

        moves.ToList().ForEach(x => x.Edge.Destination.IsSelected = true);

        GameBoardRenderer.RenderAndSave("PossibleMoves_AmphibiousLandUnitNearRiver_AllowsRiverCrossing.png", gameState.Width, gameState.Height, gameState.Tiles, gameState.Edges, gameState.Settlements, null, null, units);

        Assert.HasCount(8, moves);

        Assert.Contains(x => x.Edge.Destination == gameState[1, 2], moves);
        Assert.Contains(x => x.Edge.Destination == gameState[2, 2], moves);
        Assert.Contains(x => x.Edge.Destination == gameState[2, 1], moves);
        Assert.Contains(x => x.Edge.Destination == gameState[3, 1], moves);
        Assert.Contains(x => x.Edge.Destination == gameState[1, 3], moves);
    }

    [TestMethod]
    public void PossibleMoves_WaterboundUnit_ReturnsExpectedMoves()
    {

        var units = new List<MilitaryUnit> { new(new UnitTemplate { OperationalDomain = OperationalDomain.Waterbound, MovementPoints = 3 }, gameState.Players[0], gameState[225]) };

        var moves = units[0].PossibleMoves();

        moves.ToList().ForEach(x => x.Edge.Destination.IsSelected = true);

        GameBoardRenderer.RenderAndSave("PossibleMoves_WaterboundUnit_ReturnsExpectedMoves.png", gameState.Width, gameState.Height, gameState.Tiles, gameState.Edges, gameState.Settlements, null, null, units);

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
    public void ResolveOrders_LandUnitAndWaterboundAtCoastalSettlement_EmbarksLandUnit()
    {
        // NM3: a land unit may embark on a waterbound unit if they are both in a coastal settlement and there is a port edge between the land and water hexes
        var settlements = new[] { "196,City,1,6" };
        gameState.Board.ParseSettlements(settlements, gameState.Players);

        gameState.Units =
        [
            new(new UnitTemplate { TransportableBy = [OperationalDomain.Waterbound] }, gameState.Players[0], gameState[196]),
            new(new UnitTemplate { OperationalDomain = OperationalDomain.Waterbound, IsTransporter = true }, gameState.Players[0], gameState[196]),
        ];

        var unitOrders = new List<IUnitCommand>
        {
            new TransportCommand(gameState.Units[1], gameState.Units[0]),
        };
        gameState.ResolveOrders(unitOrders);

        Assert.AreEqual(gameState.Units[1], gameState.Units[0].TransportedBy, "Land unit should be embarked on the waterbound unit at the coastal settlement");
    }

    [TestMethod]
    public void ResolveOrders_LandUnitAtSettlementAndShipOnWater_DoesNotEmbarkLandUnit()
    {
        // NM3 requires both units to be at the coastal settlement; if the ship is on water the land unit cannot embark without moving
        var settlements = new[] { "196,City,1,6" };
        gameState.Board.ParseSettlements(settlements, gameState.Players);

        gameState.Units =
        [
            new(new UnitTemplate { TransportableBy = [OperationalDomain.Waterbound] }, gameState.Players[0], gameState[196]),
            new(new UnitTemplate { OperationalDomain = OperationalDomain.Waterbound, IsTransporter = true }, gameState.Players[0], gameState[224]),
        ];

        var unitOrders = new List<IUnitCommand>
        {
            new TransportCommand(gameState.Units[1], gameState.Units[0]),
        };
        gameState.ResolveOrders(unitOrders);

        Assert.IsNull(gameState.Units[0].TransportedBy, "Land unit should not be embarked when the waterbound unit is on water, not at the settlement");
    }

    [TestMethod]
    public void ResolveOrders_LandUnitAndShip_CreateEmbarkMove()
    {

        gameState.Units =
        [
            new(new UnitTemplate { MovementPoints = 3, TransportableBy = [OperationalDomain.Waterbound] }, gameState.Players[0], gameState[196]),
            new(new UnitTemplate { OperationalDomain = OperationalDomain.Waterbound, IsTransporter = true }, gameState.Players[0], gameState[224]),
        ];

        var computerPlayer = new ComputerPlayer(gameState.Units);
        computerPlayer.SetStrategicAction(gameState);

        var unitOrders = computerPlayer.CreateOrders(gameState, gameState.Units);
        gameState.ResolveOrders(unitOrders);

        Assert.AreEqual(gameState[8, 8], gameState.Units[0].Location);
    }

    [TestMethod]
    public void ResolveOrders_MultipleMoveCommands_MovesUnitsToDestinations()
    {

        gameState.Units =
        [
            new(new UnitTemplate { MovementPoints = 5 }, gameState.Players[0], gameState[1, 1]),
            new(new UnitTemplate(), gameState.Players[0], gameState[1, 1]),
        ];

        var moves1 = new Move[]
                {
                    new(gameState[1, 1], gameState[1, 2], null, 5, 1),
                    new(gameState[1, 2], gameState[2, 2], null, 4, 2),
                    new(gameState[2, 2], gameState[3, 2], null, 3, 3),
                };

        var moves2 = new Move[]
                {
                    new(gameState[1, 1], gameState[1, 2], null, 2, 1),
                    new(gameState[1, 2], gameState[2, 2], null, 1, 2),
                };

        var moveOrders = new List<IUnitCommand>
        {
            new MoveCommand(moves1, gameState.Units[0]),
            new MoveCommand(moves2, gameState.Units[1]),
        };

        gameState.ResolveOrders(moveOrders);

        Assert.AreEqual(gameState[3, 2], gameState.Units[0].Location);
        Assert.AreEqual(gameState[2, 2], gameState.Units[1].Location);
    }

    [TestMethod]
    public void ResolveOrders_AdjacentEnemiesOfSameStrengthSwapHexes_BothStayInOriginHexes()
    {

        gameState.Units =
        [
            new(new UnitTemplate(), gameState.Players[0], gameState[1, 1]),
            new(new UnitTemplate(), gameState.Players[1], gameState[2, 2]),
        ];

        var moves1 = new Move[]
                            {
                                new(gameState[1, 1], gameState[2, 2], null, 2, 1),
                            };

        var moves2 = new Move[]
                            {
                                new(gameState[2, 2], gameState[1, 1], null, 2, 1),
                            };

        var moveOrders = new List<IUnitCommand>
        {
            new MoveCommand(moves1, gameState.Units[0]),
            new MoveCommand(moves2, gameState.Units[1]),
        };

        gameState.ResolveOrders(moveOrders);

        Assert.AreEqual(gameState[1, 1], gameState.Units[0].Location);
        Assert.AreEqual(gameState[2, 2], gameState.Units[1].Location);
    }

    [TestMethod]
    public void ResolveOrders_AdjacentEnemiesOfDifferentStrengthSwapHexes_StrongerForceAdvances()
    {

        gameState.Units =
        [
            new(new UnitTemplate(), gameState.Players[0], gameState[1, 1]),
            new(new UnitTemplate(), gameState.Players[1], gameState[2, 2]),
            new(new UnitTemplate(), gameState.Players[1], gameState[2, 2]),
        ];

        var moves1 = new Move[]
                            {
                                new(gameState[1, 1], gameState[2, 2], null, 1, 1),
                            };

        var moves2 = new Move[]
                            {
                                new(gameState[2, 2], gameState[1, 1], null, 1, 1),
                            };

        var moveOrders = new List<IUnitCommand>
        {
            new MoveCommand(moves1, gameState.Units[0]),
            new MoveCommand(moves2, gameState.Units[1]),
            new MoveCommand(moves2, gameState.Units[2]),
        };

        gameState.ResolveOrders(moveOrders);

        Assert.AreEqual(gameState[1, 1], gameState.Units[0].Location); // Unit 0 is prevented from moving because a larger army is moving into their hex from the same hexside
        Assert.AreEqual(gameState[1, 1], gameState.Units[1].Location);
        Assert.AreEqual(gameState[1, 1], gameState.Units[2].Location);
    }

    [TestMethod]
    public void ResolveOrders_WhenConflictArises_ConflictedUnitsStop()
    {
        gameState.Units =
        [
            new(new UnitTemplate { MovementPoints = 3 }, gameState.Players[0], gameState[1, 1]),
            new(new UnitTemplate { MovementPoints = 3 }, gameState.Players[1], gameState[4, 1]),

            new(new UnitTemplate { MovementPoints = 6 }, gameState.Players[0], gameState[10, 2]),
            new(new UnitTemplate(), gameState.Players[1], gameState[10, 3]),
        ];

        var moves1 = new Move[]
                {
                    new(gameState[1, 1], gameState[1, 2], null, 3, 1),
                    new(gameState[1, 2], gameState[2, 2], null, 2, 2),
                    new(gameState[2, 2], gameState[3, 2], null, 1, 3),
                };
        var moves2 = new Move[]
                {
                    new(gameState[4, 1], gameState[3, 1], null, 3, 1),
                    new(gameState[3, 1], gameState[2, 2], null, 2, 2),
                    new(gameState[2, 2], gameState[2, 1], null, 1, 3),
                };
        var moves3 = new Move[]
                {
                    new(gameState[10, 2], gameState[11, 2], null, 6, 1),
                    new(gameState[11, 2], gameState[12, 2], null, 5, 2),
                };
        var moves4 = new Move[]
                {
                    new(gameState[10, 3], gameState[11, 2], null, 2, 1),
                    new(gameState[11, 2], gameState[11, 1], null, 1, 2),
                };

        var moveOrders = new List<IUnitCommand>
        {
            new MoveCommand(moves1, gameState.Units[0]),
            new MoveCommand(moves2, gameState.Units[1]),
            new MoveCommand(moves3, gameState.Units[2]),
            new MoveCommand(moves4, gameState.Units[3]),
        };

        var lines = new List<Centreline>();
        moveOrders.ForEach(x => lines.AddRange(Centreline.MoveOrderToCentrelines((MoveCommand)x)));

        GameBoardRenderer.RenderAndSave("UnitsPreMove.png", gameState.Width, gameState.Height, gameState.Tiles, gameState.Edges, gameState.Settlements, null, lines, gameState.Units);

        gameState.ResolveOrders(moveOrders);

        GameBoardRenderer.RenderAndSave("UnitsPostMove.png", gameState.Width, gameState.Height, gameState.Tiles, gameState.Edges, gameState.Settlements, null, null, gameState.Units);

        Assert.AreEqual(gameState[2, 2], gameState.Units[0].Location);
        Assert.AreEqual(gameState[2, 2], gameState.Units[1].Location);

        Assert.AreEqual(gameState[12, 2], gameState.Units[2].Location);
        Assert.AreEqual(gameState[11, 1], gameState.Units[3].Location);
    }

    [TestMethod]
    public void PossibleMoves_WaterboundUnitAdjacentToCoastalSettlementViaPortEdge_CanStopOnSettlementTile()
    {
        // NM2: a waterbound unit may stop on a coastal settlement land tile if there is a port edge between the water tile and the settlement tile
        var settlements = new[] { "196,City,1,6" };
        var board = new GameState(new Board(BoardTests.GameBoard, BoardTests.TileEdges));
        board.Board.ParseSettlements(settlements, board.Players);

        var t196 = board[196];
        var t224 = board[224];

        Assert.IsNotNull(t196.Settlement, "Tile 196 should have a settlement for NM2 test");
        Assert.IsTrue(t224.Neighbours.Any(n => n.Destination.Index == 196 && n.EdgeType == EdgeType.Port), "Tile 224 should have a port edge to tile 196");

        var units = new List<MilitaryUnit> { new(new UnitTemplate { OperationalDomain = OperationalDomain.Waterbound, MovementPoints = 3 }, board.Players[0], board[224]) };

        var moves = units[0].PossibleMoves();

        // Tile 196 is a grassland settlement accessible via port edge from tile 224 (water)
        Assert.Contains(x => x.Edge.Destination.Index == 196, moves);
    }

    [TestMethod]
    public void PossibleMoves_WaterboundUnitAdjacentToLandTileWithoutSettlementViaPortEdge_CannotStopOnLandTile()
    {
        // NM2 only applies when there is a settlement on the land tile; without a settlement the unit cannot stop there

        var units = new List<MilitaryUnit> { new(new UnitTemplate { OperationalDomain = OperationalDomain.Waterbound, MovementPoints = 3 }, gameState.Players[0], gameState[224]) };

        var moves = units[0].PossibleMoves();

        // Tile 196 is a grassland tile with a port edge from tile 224 but has no settlement, so the unit cannot stop there
        Assert.DoesNotContain(x => x.Edge.Destination.Index == 196, moves);
    }

    [TestMethod]
    public void PossibleMoves_WaterboundUnitAtCoastalSettlement_CanLeaveToAdjacentWaterTileViaPortEdge()
    {
        // NM2: a waterbound unit that has stopped at a coastal settlement must be able to leave back to adjacent water tiles via the port edge
        var settlements = new[] { "196,City,1,6" };
        var gameState = new GameState(new Board(BoardTests.GameBoard, BoardTests.TileEdges));
        gameState.Board.ParseSettlements(settlements, gameState.Players);

        var units = new List<MilitaryUnit> { new(new UnitTemplate { OperationalDomain = OperationalDomain.Waterbound, MovementPoints = 3 }, gameState.Players[0], gameState[196]) };

        var moves = units[0].PossibleMoves();

        // Tile 224 is the adjacent water tile connected to the settlement at tile 196 via a port edge
        Assert.Contains(x => x.Edge.Destination.Index == 224, moves);
    }

    [TestMethod]
    public void DetectConflictedUnits_OpposingMovingUnitsInSameTile_ReturnsBothConflictedUnits()
    {
        var tile1 = new Tile(1, 1, 1);
        var tile2 = new Tile(2, 1, 2);

        var units = new List<MilitaryUnit>
        {
                        new(new UnitTemplate(), gameState.Players[0], tile1),
                        new(new UnitTemplate(), gameState.Players[0], tile2),

                        new(new UnitTemplate(), gameState.Players[1], tile1),
        };

        var movingUnits = new List<MilitaryUnit>
        {
            units[0],
            units[2],
        };

        var conflictedUnits = CommandResolver.DetectConflictedUnits(movingUnits, units);

        Assert.HasCount(2, conflictedUnits);
        Assert.Contains(units[0], conflictedUnits);
        Assert.Contains(units[2], conflictedUnits);
        Assert.DoesNotContain(units[1], conflictedUnits);
    }
}


