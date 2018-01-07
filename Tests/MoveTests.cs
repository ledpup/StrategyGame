using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GameModel;
using ComputerOpponent;
using Visualise;
using Hexagon;

namespace Tests
{
    [TestClass]
    public class MoveTests
    {
        [TestMethod]
        public void TerrainTypeTests()
        {
            Assert.IsTrue(Terrain.All_Land.HasFlag(TerrainType.Steppe));
            Assert.IsTrue(Terrain.All_Land.HasFlag(TerrainType.Hill));

            Assert.IsTrue(!Terrain.Non_Mountainous_Land.HasFlag(TerrainType.Mountain));
        }

        [TestMethod]
        public void LandUnitMoveList()
        {
            var board = new Board(BoardTests.GameBoard, BoardTests.TileEdges);

            var units = new List<MilitaryUnit> { new MilitaryUnit(location: board[335]) };

            var moves = units[0].PossibleMoves();

            moves.ToList().ForEach(x => x.Edge.Destination.IsSelected = true);

            GameBoardRenderer.RenderAndSave("LandUnitMoves.png", board.Height, board.Tiles, board.Edges, board.Structures, null, null, units);

            Assert.AreEqual(11, moves.Count());

            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 334));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 361));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 336));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 309));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 310));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 308));

            // Can't go into the ocean
            Assert.IsFalse(moves.Any(x => x.Edge.Destination.Index == 281));
            Assert.IsFalse(moves.Any(x => x.Edge.Destination.Index == 306));
            Assert.IsFalse(moves.Any(x => x.Edge.Destination.Index == 333));
            Assert.IsFalse(moves.Any(x => x.Edge.Destination.Index == 360));

            // Can't go over mountains
            Assert.IsFalse(moves.Any(x => x.Edge.Destination.Index == 337));
            Assert.IsFalse(moves.Any(x => x.Edge.Destination.Index == 363));
            Assert.IsFalse(moves.Any(x => x.Edge.Destination.Index == 362));
            Assert.IsFalse(moves.Any(x => x.Edge.Destination.Index == 388));
            Assert.IsFalse(moves.Any(x => x.Edge.Destination.Index == 364));
            Assert.IsFalse(moves.Any(x => x.Edge.Destination.Index == 390));
            Assert.IsFalse(moves.Any(x => x.Edge.Destination.Index == 389));
        }

        [TestMethod]
        public void LandUnitMoveList2()
        {
            //var board = new Board(BoardTests.GameBoard, BoardTests.TileEdges);

            //var units = new List<MilitaryUnit> { new MilitaryUnit(location: board[335]) };

            ////var moves = units[0].PossibleMoves();

            //var blockedHexes = board.Tiles.Where(x => x.TerrainType == TerrainType.Mountain || x.TerrainType == TerrainType.Water).Select(x => x.Hex).ToList();

            //var moves = CalculateRange.UnitRangeForTurn(units[0].Location, units[0].MovementPoints, units[0].UsesRoads, units[0].EdgeMovementCosts, units[0].TerrainMovementCosts);

            //moves.ToList().ForEach(x => board[x.Destination.Index].IsSelected = true);

            //GameBoardRenderer.RenderAndSave("LandUnitMoves2.png", board.Height, board.Tiles, board.Edges, board.Structures, null, null, units);

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
            var board = new Board(BoardTests.GameBoard, BoardTests.TileEdges);

            var units = new List<MilitaryUnit> { new MilitaryUnit(location: board[345], roadMovementBonus: 2) };

            var moves = units[0].PossibleMoves();

            moves.ToList().ForEach(x => x.Edge.Destination.IsSelected = true);

            GameBoardRenderer.RenderAndSave("LandUnitMovesOverRoad.png", board.Height, board.Tiles, board.Edges, board.Structures, null, null, units);

            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 316));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 317));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 343));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 344));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 318));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 373));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 347));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 374));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 402));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 429));

            Assert.IsFalse(moves.Any(x => x.Edge.Destination.Index == 346));
            Assert.IsFalse(moves.Any(x => x.Edge.Destination.Index == 371));
            Assert.IsFalse(moves.Any(x => x.Edge.Destination.Index == 372));
            Assert.IsFalse(moves.Any(x => x.Edge.Destination.Index == 400));
        }

        [TestMethod]
        public void LandUnitMoveListWithRoadOverMountain()
        {
            var board = new Board(BoardTests.GameBoard, BoardTests.TileEdges);

            var units = new List<MilitaryUnit> { new MilitaryUnit(0, "1st Infantry", 1, board[85], MovementType.Land, 2, roadMovementBonus: 1) };

            var moves = units[0].PossibleMoves();

            moves.ToList().ForEach(x => x.Edge.Destination.IsSelected = true);

            GameBoardRenderer.RenderAndSave("LandUnitMovesOverRoadOverMountain.png", board.Height, board.Tiles, board.Edges, board.Structures, null, null, units);

            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 30));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 56));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 57));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 59));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 86));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 87));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 114));

            Assert.IsFalse(moves.Any(x => x.Edge.Destination.Index == 58));
            Assert.IsFalse(moves.Any(x => x.Edge.Destination.Index == 32));
            Assert.IsFalse(moves.Any(x => x.Edge.Destination.Index == 60));
            Assert.IsFalse(moves.Any(x => x.Edge.Destination.Index == 83));
            Assert.IsFalse(moves.Any(x => x.Edge.Destination.Index == 84));
            Assert.IsFalse(moves.Any(x => x.Edge.Destination.Index == 112));
            Assert.IsFalse(moves.Any(x => x.Edge.Destination.Index == 113));
        }

        [TestMethod]
        public void LandUnitMoveOverMountainWithRoad()
        {
            var board = new Board(BoardTests.GameBoard, BoardTests.TileEdges);

            var units = new List<MilitaryUnit>
            {
                new MilitaryUnit(location: board[4, 3], transportableBy: new List<MovementType> { MovementType.Water }, role: Role.Besieger),
            };

            var moves = units[0].PossibleMoves();

            moves.ToList().ForEach(x => x.Edge.Destination.IsSelected = true);

            GameBoardRenderer.RenderAndSave("InfantryMoveOverMountainWithRoad.png", board.Height, board.Tiles, board.Edges, board.Structures, null, null, units);

            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 86));
        }

        [TestMethod]
        public void AirborneMoveListWithRoadAndMountain()
        {
            var board = new Board(BoardTests.GameBoard, BoardTests.TileEdges);

            var units = new List<MilitaryUnit> { new MilitaryUnit(0, "1st Infantry", 1, board[85], MovementType.Airborne, 2) };

            var moves = units[0].PossibleMoves();

            moves.ToList().ForEach(x => x.Edge.Destination.IsSelected = true);

            GameBoardRenderer.RenderAndSave("AirborneUnitMovesWithRoadAndMountain.png", board.Height, board.Tiles, board.Edges, board.Structures, null, null, units);

            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 30));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 31));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 32));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 56));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 57));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 59));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 60));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 87));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 110));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 111));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 114));

            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 58 && x.MoveType == MoveType.OnlyPassingThrough));
            Assert.IsFalse(moves.Any(x => x.Edge.Destination.Index == 83));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 84 && x.MoveType == MoveType.OnlyPassingThrough));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 86 && x.MoveType == MoveType.OnlyPassingThrough));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 112 && x.MoveType == MoveType.OnlyPassingThrough));
            Assert.IsFalse(moves.Any(x => x.Edge.Destination.Index == 113));
        }

        [TestMethod]
        public void AirborneMoveList()
        {
            var board = new Board(BoardTests.GameBoard, BoardTests.TileEdges);

            var units = new List<MilitaryUnit> { new MilitaryUnit(0, "1st Airborne", 1, board[364], MovementType.Airborne, 3) };

            var moves = units[0].PossibleMoves();

            moves.ToList().ForEach(x => x.Edge.Destination.IsSelected = true);

            GameBoardRenderer.RenderAndSave("AirborneUnitMoves.png", board.Height, board.Tiles, board.Edges, board.Structures, null, null, units);

            //Assert.AreEqual(12, moves.Count());

            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 334));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 308));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 309));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 361));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 335));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 336));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 310));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 311));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 338));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 312));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 389));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 390));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 365));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 339));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 340));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 417));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 366));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 367));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 418));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 419));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 393));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 394));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 445));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 446));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 420));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 421));

            Assert.IsFalse(moves.Any(x => x.Edge.Destination.Index == 388));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 362 && x.MoveType == MoveType.OnlyPassingThrough));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 337 && x.MoveType == MoveType.OnlyPassingThrough));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 391 && x.MoveType == MoveType.OnlyPassingThrough));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 392 && x.MoveType == MoveType.OnlyPassingThrough));
            Assert.IsFalse(moves.Any(x => x.Edge.Destination.Index == 416));
            Assert.IsFalse(moves.Any(x => x.Edge.Destination.Index == 444));
        }


        [TestMethod]
        public void AirborneUnitCanStopOn()
        {
            var airborneUnit = new MilitaryUnit(movementType: MovementType.Airborne);

            Assert.IsTrue(airborneUnit.CanStopOn.HasFlag(TerrainType.Forest));

            Assert.IsFalse(airborneUnit.CanStopOn.HasFlag(TerrainType.Reef));
            Assert.IsFalse(airborneUnit.CanStopOn.HasFlag(TerrainType.Water));
            Assert.IsFalse(airborneUnit.CanStopOn.HasFlag(TerrainType.Mountain));
        }

        [TestMethod]
        public void AirborneUnitValidMoves()
        {
            var board = new Board(BoardTests.GameBoard, BoardTests.TileEdges);

            var unit = new MilitaryUnit(location: board[1, 1], movementType: MovementType.Airborne);
            var moveList = unit.PossibleMoves();

            Assert.IsTrue(moveList.Any(x => x.Edge.Destination == board[1, 2]));
            Assert.IsTrue(moveList.Any(x => x.Edge.Destination == board[1, 3]));
            Assert.IsTrue(moveList.Any(x => x.Edge.Destination == board[2, 2]));
            Assert.IsTrue(moveList.Any(x => x.Edge.Destination == board[2, 1]));
            Assert.IsTrue(moveList.Any(x => x.Edge.Destination == board[3, 1]));
            Assert.IsTrue(moveList.Any(x => x.Edge.Destination == board[3, 2]));
        }

        [TestMethod]
        public void AirborneUnitValidMovesOverWater()
        {
            var board = new Board(BoardTests.GameBoard, BoardTests.TileEdges);

            var unit = new MilitaryUnit(location: board[4, 9], movementType: MovementType.Airborne);
            var moveList = unit.PossibleMoves();

            moveList.Where(x => x.MoveType != MoveType.OnlyPassingThrough).ToList().ForEach(x => x.Edge.Destination.IsSelected = true);

            GameBoardRenderer.RenderAndSave("AirborneUnitValidMovesOverWater.png", board.Height, board.Tiles, board.Edges, board.Structures, null, null, new List<MilitaryUnit> { unit });


            Assert.IsTrue(moveList.Any(x => x.Edge.Destination == board[3, 9] && x.MoveType == MoveType.OnlyPassingThrough)); // Mountain
            Assert.IsTrue(moveList.Any(x => x.Edge.Destination == board[5, 9]));

            Assert.IsTrue(moveList.Any(x => x.Edge.Destination == board[3, 8] && x.MoveType == MoveType.OnlyPassingThrough)); // Water
            Assert.IsTrue(moveList.Any(x => x.Edge.Destination == board[4, 8] && x.MoveType == MoveType.OnlyPassingThrough)); // Water
            Assert.IsTrue(moveList.Any(x => x.Edge.Destination == board[5, 8] && x.MoveType == MoveType.OnlyPassingThrough)); // Water

            Assert.IsFalse(moveList.Any(x => x.Edge.Destination == board[3, 7])); // Reef
            Assert.IsTrue(moveList.Any(x => x.Edge.Destination == board[4, 7]));
            Assert.IsTrue(moveList.Any(x => x.Edge.Destination == board[5, 7]));

            Assert.IsFalse(moveList.Any(x => x.Edge.Destination == board[3, 10])); // Water
            Assert.IsTrue(moveList.Any(x => x.Edge.Destination == board[4, 10] && x.MoveType == MoveType.OnlyPassingThrough)); // Water
            Assert.IsFalse(moveList.Any(x => x.Edge.Destination == board[5, 10])); // Water

            Assert.IsTrue(moveList.Any(x => x.Edge.Destination == board[4, 11]));
        }

        [TestMethod]
        public void AirborneUnitValidMovesOverWaterFromShortestPath()
        {
            var board = new Board(BoardTests.GameBoard, BoardTests.TileEdges);

            var unit = new MilitaryUnit(location: board[147], movementType: MovementType.Airborne, baseMovementPoints: 4);
            var moveList = unit.PossibleMoves();

            var pathFindTiles = board.ValidMovesWithMoveCostsForUnit(unit);
            var pathToTransporteesDestination = Board.FindShortestPath(pathFindTiles, unit.Location.Point, board[196].Point, unit.MovementPoints);
            
            var moveOrder = unit.ShortestPathToMoveOrder(pathToTransporteesDestination.ToArray());

            moveList.Where(x => x.MoveType != MoveType.OnlyPassingThrough).ToList().ForEach(x => x.Edge.Destination.IsSelected = true);
            GameBoardRenderer.RenderAndSave("AirborneUnitValidMovesOverWaterFromShortestPath.png", board.Height, board.Tiles, board.Edges, board.Structures, null, null, new List<MilitaryUnit> { unit });

            Assert.IsFalse(moveOrder.Moves.Last().MoveType == MoveType.OnlyPassingThrough);
        }

        [TestMethod]
        public void AirborneUnitShortestPathWithLongRouteOverWater()
        {
            var board = new Board(BoardTests.GameBoard, BoardTests.TileEdges);

            var unit = new MilitaryUnit(location: board[202], movementType: MovementType.Airborne, baseMovementPoints: 3);
            var moveList = unit.PossibleMoves();

            var pathFindTiles = board.ValidMovesWithMoveCostsForUnit(unit);
            var pathToTransporteesDestination = Board.FindShortestPath(pathFindTiles, unit.Location.Point, board[381].Point, unit.MovementPoints);

            var vectors = new List<Centreline>();
            vectors.AddRange(Centreline.PathFindTilesToCentrelines(pathToTransporteesDestination));
            GameBoardRenderer.RenderAndSave("AirborneUnitShortestPathWithLongRouteOverWaterPath.png", board.Height, board.Tiles, board.Edges, board.Structures, null, vectors);

            moveList.Where(x => x.MoveType != MoveType.OnlyPassingThrough).ToList().ForEach(x => x.Edge.Destination.IsSelected = true);
            GameBoardRenderer.RenderAndSave("AirborneUnitShortestPathWithLongRouteOverWater.png", board.Height, board.Tiles, board.Edges, board.Structures, null, null, new List<MilitaryUnit> { unit });
        }

        [TestMethod]
        public void AirborneUnitShortestPathWithLongerRouteOverWater()
        {
            var board = new Board(BoardTests.GameBoard, BoardTests.TileEdges);

            var unit = new MilitaryUnit(location: board[187], movementType: MovementType.Airborne, baseMovementPoints: 3);
            var moveList = unit.PossibleMoves();

            var pathFindTiles = board.ValidMovesWithMoveCostsForUnit(unit);
            var pathToTransporteesDestination = Board.FindShortestPath(pathFindTiles, unit.Location.Point, board[456].Point, unit.MovementPoints);

            var vectors = new List<Centreline>();

            vectors.AddRange(Centreline.PathFindTilesToCentrelines(pathToTransporteesDestination));

            GameBoardRenderer.RenderAndSave("AirborneUnitShortestPathWithLongerRouteOverWater.png", board.Height, board.Tiles, board.Edges, board.Structures, null, vectors);
        }

        [TestMethod]
        public void AirborneUnitShortestPathCornerToCorner()
        {
            var board = new Board(BoardTests.GameBoard, BoardTests.TileEdges);

            var unit = new MilitaryUnit(location: board[28], movementType: MovementType.Airborne, baseMovementPoints: 3);
            var moveList = unit.PossibleMoves();

            var pathFindTiles = board.ValidMovesWithMoveCostsForUnit(unit);
            var pathToTransporteesDestination = Board.FindShortestPath(pathFindTiles, unit.Location.Point, board[484].Point, unit.MovementPoints);

            var vectors = new List<Centreline>();

            vectors.AddRange(Centreline.PathFindTilesToCentrelines(pathToTransporteesDestination));

            GameBoardRenderer.RenderAndSave("AirborneUnitShortestPathCornerToCorner.png", board.Height, board.Tiles, board.Edges, board.Structures, null, vectors);
        }

        [TestMethod]
        public void AirborneUnitValidMovesOverContinent()
        {
            var board = new Board(BoardTests.GameBoard, BoardTests.TileEdges);

            var unit = new MilitaryUnit(location: board[19, 13], movementType: MovementType.Airborne, baseMovementPoints: 3);
            var moveList = unit.PossibleMoves();

            moveList.Where(x => x.MoveType != MoveType.OnlyPassingThrough).ToList().ForEach(x => x.Edge.Destination.IsSelected = true);

            GameBoardRenderer.RenderAndSave("AirborneUnitValidMovesOverContinent.png", board.Height, board.Tiles, board.Edges, board.Structures, null, null, new List<MilitaryUnit> { unit });

            Assert.AreEqual(154, moveList.Count());

            //Assert.IsFalse(moveList.Any(x => x.Neighbour.Tile == board[3, 9])); // Mountain
            //Assert.IsTrue(moveList.Any(x => x.Neighbour.Tile == board[5, 9]));

            //Assert.IsFalse(moveList.Any(x => x.Neighbour.Tile == board[3, 8])); // Water
            //Assert.IsFalse(moveList.Any(x => x.Neighbour.Tile == board[4, 8])); // Water
            //Assert.IsFalse(moveList.Any(x => x.Neighbour.Tile == board[5, 8])); // Water

            //Assert.IsFalse(moveList.Any(x => x.Neighbour.Tile == board[3, 7])); // Reef
            Assert.IsTrue(moveList.Any(x => x.Edge.Destination == board[16, 12]));
            Assert.IsTrue(moveList.Any(x => x.Edge.Destination == board[16, 13]));
            Assert.IsTrue(moveList.Any(x => x.Edge.Destination == board[16, 14]));
            Assert.IsTrue(moveList.Any(x => x.Edge.Destination == board[16, 15]));

            //Assert.IsFalse(moveList.Any(x => x.Neighbour.Tile == board[3, 10])); // Water
            //Assert.IsFalse(moveList.Any(x => x.Neighbour.Tile == board[4, 10])); // Water
            //Assert.IsFalse(moveList.Any(x => x.Neighbour.Tile == board[5, 10])); // Water

            //Assert.IsTrue(moveList.Any(x => x.Neighbour.Tile == board[4, 11]));
        }

        [TestMethod]
        public void LandUnitNearRiverAndRoad()
        {
            var board = new Board(BoardTests.GameBoard, BoardTests.TileEdges);

            var units = new List<MilitaryUnit> { new MilitaryUnit(0, "1st Infantry", 0, board[1, 1]) };

            var moves = units[0].PossibleMoves();

            moves.ToList().ForEach(x => x.Edge.Destination.IsSelected = true);

            GameBoardRenderer.RenderAndSave("LandUnitNearRiverAndRoad.png", board.Height, board.Tiles, board.Edges, board.Structures, null, null, units);

            Assert.IsTrue(moves.Any(x => x.Edge.Destination == board[1, 2]));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination == board[2, 2]));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination == board[2, 1]));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination == board[3, 1]));

            Assert.IsFalse(moves.Any(x => x.Edge.Destination == board[1, 3])); // Can't cross river

            Assert.AreEqual(7, moves.Count());
        }

        [TestMethod]
        public void LandUnitNearBridgeAndRoad()
        {
            var board = new Board(BoardTests.GameBoard, BoardTests.TileEdges);

            var units = new List<MilitaryUnit> { new MilitaryUnit(0, "1st Infantry", 0, board[141]) };

            var moves = units[0].PossibleMoves();

            moves.ToList().ForEach(x => x.Edge.Destination.IsSelected = true);

            GameBoardRenderer.RenderAndSave("LandUnitNearBridgeAndRoad.png", board.Height, board.Tiles, board.Edges, board.Structures, null, null, units);

            Assert.AreEqual(15, moves.Count());

            Assert.IsTrue(moves.Any(x => x.Edge.Destination == board[87]));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination == board[88]));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination == board[114]));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination == board[115]));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination == board[140]));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination == board[142]));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination == board[168]));

            // Blocked by mountain hexside and hill/wetland terrain            
            Assert.IsFalse(moves.Any(x => x.Edge.Destination == board[169]));
        }

        [TestMethod]
        public void AmphibiousUnitNearRiverAndRoad()
        {
            var board = new Board(BoardTests.GameBoard, BoardTests.TileEdges);

            var units = new List<MilitaryUnit> { new MilitaryUnit(0, "1st Amphibious", 1, board[1, 1], MovementType.Land) };
            units[0].TerrainMovementCosts[TerrainType.Wetland] = 1;
            units[0].EdgeMovementCosts[EdgeType.River] = 0;

            var moves = units[0].PossibleMoves();

            moves.ToList().ForEach(x => x.Edge.Destination.IsSelected = true);

            GameBoardRenderer.RenderAndSave("AmphibiousUnitNearRiverAndRoad.png", board.Height, board.Tiles, board.Edges, board.Structures, null, null, units);

            Assert.AreEqual(8, moves.Count());

            Assert.IsTrue(moves.Any(x => x.Edge.Destination == board[1, 2]));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination == board[2, 2]));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination == board[2, 1]));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination == board[3, 1]));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination == board[1, 3]));
        }

        [TestMethod]
        public void AquaticUnitMoves()
        {
            var board = new Board(BoardTests.GameBoard, BoardTests.TileEdges);

            var units = new List<MilitaryUnit> { new MilitaryUnit(0, "1st Fleet", 2, board[225], MovementType.Water, 3) };

            var moves = units[0].PossibleMoves();

            moves.ToList().ForEach(x => x.Edge.Destination.IsSelected = true);

            GameBoardRenderer.RenderAndSave("AquaticUnitMoves.png", board.Height, board.Tiles, board.Edges, board.Structures, null, null, units);

            Assert.AreEqual(138, moves.Count());

            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 198));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 226));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 253));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 252));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 251));
            Assert.IsTrue(moves.Any(x => x.Edge.Destination.Index == 224));

            Assert.IsFalse(moves.Any(x => x.Edge.Destination.Index == 196));
            Assert.IsFalse(moves.Any(x => x.Edge.Destination.Index == 199));
        }

        [TestMethod]
        public void EmbarkOnShip()
        {
            var board = new Board(BoardTests.GameBoard, BoardTests.TileEdges);

            board.Units = new List<MilitaryUnit>
            {
                new MilitaryUnit(0, location: board[196], baseMovementPoints: 3, transportableBy: new List<MovementType>{ MovementType.Water }),
                new MilitaryUnit(1, location: board[224], movementType: MovementType.Water, isTransporter: true),
            };

            ComputerPlayer.SetStrategicAction(board, board.Units);

            var unitOrders = ComputerPlayer.CreateOrders(board, board.Units);
            board.ResolveOrders(unitOrders);

            //var lines = new List<Centreline>();
            //unitOrders.ForEach(x => unitOrders.AddRange(Centreline.MoveOrderToCentrelines((MoveOrder)x)));

            //GameBoardRenderer.RenderAndSave("EmbarkOnShip.png", board.Height, board.Tiles, board.Edges, board.Structures, null, null, board.Units);

            Assert.AreEqual(board[8, 8], board.Units[0].Location);
        }

        [TestMethod]
        public void ResolveMove()
        {
            var board = new Board(BoardTests.GameBoard, BoardTests.TileEdges);

            board.Units = new List<MilitaryUnit>
            { 
                new MilitaryUnit(location: board[1, 1], baseMovementPoints: 5),
                new MilitaryUnit(location: board[1, 1]),
            };

            var moves1 = new Move[]
                    {
                        new Move(board[1, 1], board[1, 2], null, 5, 1),
                        new Move(board[1, 2], board[2, 2], null, 4, 2),
                        new Move(board[2, 2], board[3, 2], null, 3, 3),
                    };

            var moves2 = new Move[]
                    {
                        new Move(board[1, 1], board[1, 2], null, 2, 1),
                        new Move(board[1, 2], board[2, 2], null, 1, 2),
                    };

            var moveOrders = new List<IUnitOrder>
            {
                new MoveOrder(moves1, board.Units[0]),
                new MoveOrder(moves2, board.Units[1]),
            };

            board.ResolveOrders(moveOrders);

            Assert.AreEqual(board[3, 2], board.Units[0].Location);
            Assert.AreEqual(board[2, 2], board.Units[1].Location);
        }

        [TestMethod]
        public void AdjacentUnitsOfSameStrengthSwapHexes()
        {
            var board = new Board(BoardTests.GameBoard, BoardTests.TileEdges);

            board.Units = new List<MilitaryUnit>
            {
                new MilitaryUnit(location: board[1, 1]),
                new MilitaryUnit(ownerIndex: 1, location: board[2, 2]),
            };

            var moves1 = new Move[]
                                {
                                    new Move(board[1, 1], board[2, 2], null, 2, 1),
                                };

            var moves2 = new Move[]
                                {
                                    new Move(board[2, 2], board[1, 1], null, 2, 1),
                                };

            var moveOrders = new List<IUnitOrder>
            {
                new MoveOrder(moves1, board.Units[0]),
                new MoveOrder(moves2, board.Units[1]),
            };

            board.ResolveOrders(moveOrders);

            Assert.AreEqual(board[1, 1], board.Units[0].Location);
            Assert.AreEqual(board[2, 2], board.Units[1].Location);
        }

        [TestMethod]
        public void AdjacentUnitsOfDifferentStrengthSwapHexes()
        {
            var board = new Board(BoardTests.GameBoard, BoardTests.TileEdges);

            board.Units = new List<MilitaryUnit>
            {
                new MilitaryUnit(location: board[1, 1]),
                new MilitaryUnit(ownerIndex: 1, location: board[2, 2]),
                new MilitaryUnit(ownerIndex: 1, location: board[2, 2]),
            };

            var moves1 = new Move[]
                                {
                                    new Move(board[1, 1], board[2, 2], null, 1, 1),
                                };

            var moves2 = new Move[]
                                {
                                    new Move(board[2, 2], board[1, 1], null, 1, 1),
                                };

            var moveOrders = new List<IUnitOrder>
            {
                new MoveOrder(moves1, board.Units[0]),
                new MoveOrder(moves2, board.Units[1]),
                new MoveOrder(moves2, board.Units[2]),
            };

            board.ResolveOrders(moveOrders);

            Assert.AreEqual(board[1, 1], board.Units[0].Location); // Unit 0 is prevented from moving because a larger army is moving into their hex from the same hexside
            Assert.AreEqual(board[1, 1], board.Units[1].Location);
            Assert.AreEqual(board[1, 1], board.Units[2].Location);
        }

        [TestMethod]
        public void ResolveMove_ConflictArrises_ConflictedUnitsStop()
        {
            var board = new Board(BoardTests.GameBoard, BoardTests.TileEdges);
            board.Units = new List<MilitaryUnit>
            {
                new MilitaryUnit(0, "1st Infantry", 1, board[1, 1], baseMovementPoints : 3),
                new MilitaryUnit(1, "2nd Infantry", 2, board[4, 1], baseMovementPoints : 3),

                new MilitaryUnit(2, "1st Infantry", 1, board[10, 2], baseMovementPoints : 6),
                new MilitaryUnit(3, "2nd Infantry", 2, board[10, 3]),
            };

            var moves1 = new Move[]
                    {
                        new Move(board[1, 1], board[1, 2], null, 3, 1),
                        new Move(board[1, 2], board[2, 2], null, 2, 2),
                        new Move(board[2, 2], board[3, 2], null, 1, 3),
                    };
            var moves2 = new Move[]
                    {
                        new Move(board[4, 1], board[3, 1], null, 3, 1),
                        new Move(board[3, 1], board[2, 2], null, 2, 2),
                        new Move(board[2, 2], board[2, 1], null, 1, 3),
                    };
            var moves3 = new Move[]
                    {
                        new Move(board[10, 2], board[11, 2], null, 6, 1),
                        new Move(board[11, 2], board[12, 2], null, 5, 2),
                    };
            var moves4 = new Move[]
                    {
                        new Move(board[10, 3], board[11, 2], null, 2, 1),
                        new Move(board[11, 2], board[11, 1], null, 1, 2),
                    };

            var moveOrders = new List<IUnitOrder>
            {
                new MoveOrder(moves1, board.Units[0]),               
                new MoveOrder(moves2, board.Units[1]),
                new MoveOrder(moves3, board.Units[2]),
                new MoveOrder(moves4, board.Units[3]),
            };

            var lines = new List<Centreline>();
            moveOrders.ForEach(x => lines.AddRange(Centreline.MoveOrderToCentrelines((MoveOrder)x)));

            GameBoardRenderer.RenderAndSave("UnitsPreMove.png", board.Height, board.Tiles, board.Edges, board.Structures, null, lines, board.Units);

            board.ResolveOrders(moveOrders);

            GameBoardRenderer.RenderAndSave("UnitsPostMove.png", board.Height, board.Tiles, board.Edges, board.Structures, null, null, board.Units);

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
                            new MilitaryUnit() { Location = tile1, }, 
                            new MilitaryUnit() { OwnerIndex = 2, Location = tile1, },
                            new MilitaryUnit() { Location = tile2, },
            };

            var movingUnits = new List<MilitaryUnit> 
            {
                units[0],
                units[2],
            };

            var conflictedUnits = Board.DetectConflictedUnits(movingUnits, units);

            Assert.AreEqual(1, conflictedUnits.Count());
        }

        [TestMethod]
        public void ForcedMarch()
        {
            var board = new Board(BoardTests.GameBoard, BoardTests.TileEdges);

            board.Units = new List<MilitaryUnit>
            {
                new MilitaryUnit(location: board[1, 1], baseMovementPoints: 4, moraleMoveCost: new float[] {0, 0, .5F, .5F}),
                new MilitaryUnit(location: board[1, 1], baseMovementPoints: 4, moraleMoveCost: new float[] {0, 0, .5F, .5F}),
                new MilitaryUnit(location: board[1, 1], baseMovementPoints: 4, moraleMoveCost: new float[] {0, 0, .5F, .5F}),
            };

            var moves1 = new Move[]
                    {
                        new Move(board[1, 1], board[1, 2], null, 4, 1),
                        new Move(board[1, 2], board[2, 2], null, 3, 2),
                        new Move(board[2, 2], board[3, 2], null, 2, 3),
                    };

            var moves2 = new Move[]
                    {
                        new Move(board[1, 1], board[1, 2], null, 4, 1),
                        new Move(board[1, 2], board[2, 2], null, 3, 2),
                        new Move(board[2, 2], board[3, 2], null, 2, 3),
                        new Move(board[3, 2], board[4, 3], null, 1, 4),
                    };

            var moves3 = new Move[]
            {
                        new Move(board[1, 1], board[1, 2], null, 4, 1, MoveType.Road),
                        new Move(board[1, 2], board[2, 2], null, 3, 2, MoveType.Road),
                        new Move(board[2, 2], board[3, 2], null, 2, 3, MoveType.Road),
                        new Move(board[3, 2], board[4, 3], null, 1, 4, MoveType.Road),
            };

            var moveOrders = new List<IUnitOrder>
            {
                new MoveOrder(moves1, board.Units[0]),
                new MoveOrder(moves2, board.Units[1]),
                new MoveOrder(moves3, board.Units[2]),
            };

            board.ResolveOrders(moveOrders);

            Assert.AreEqual(board[3, 2], board.Units[0].Location);
            Assert.AreEqual(board[4, 3], board.Units[1].Location);
            Assert.AreEqual(board[4, 3], board.Units[2].Location);

            Assert.AreEqual(4.5, board.Units[0].Morale);
            Assert.AreEqual(4, board.Units[1].Morale);
            Assert.AreEqual(5, board.Units[2].Morale);
        }
    }
}

