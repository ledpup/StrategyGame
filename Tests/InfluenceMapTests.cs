using ComputerOpponent;
using GameModel;
using GameModel.Commands;
using Hexagon;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Visualise;

namespace Tests;

[TestClass]
public class InfluenceMapTests
{

    static string[] GameBoard = File.ReadAllLines("BasicBoard.txt");
    static string[] Edges = File.ReadAllLines("BasicBoardEdges.txt");
    static string[] Settlements = File.ReadAllLines("BasicBoardSettlements.txt");

    GameState gameState;

    [TestInitialize]
    public void TestInitialize()
    {
        gameState = new GameState(new Board(GameBoard, Edges));
        gameState.Board.ParseSettlements(Settlements, gameState.Players);
    }

    [TestMethod]
    public void GenerateInfluenceMaps_BasicScenario_RendersInfluenceMoves()
    {

        var numberOfPlayers = 2;

        gameState.Units =
        [
            new(new UnitTemplate { OperationalDomain = OperationalDomain.Airborne, MovementPoints = 3 }, gameState.Players[0], gameState[114]),
            new(new UnitTemplate { MovementPoints = 3 }, gameState.Players[0], gameState[110]),
            new(new UnitTemplate(), gameState.Players[0], gameState[31]),
            new(new UnitTemplate(), gameState.Players[0], gameState[56]),
            new(new UnitTemplate(), gameState.Players[0], gameState[65]),

            new(new UnitTemplate(), gameState.Players[1], gameState[111]),
            new(new UnitTemplate(), gameState.Players[1], gameState[111]),

            new(new UnitTemplate(), gameState.Players[1], gameState[168]),
        ];

        gameState.Units[0].TerrainTypeBattleModifier[TerrainType.Swamp] = 1;
        gameState.Units[1].TerrainTypeBattleModifier[TerrainType.Forest] = 1;

        var computerPlayer = new ComputerPlayer(gameState.Units);

        computerPlayer.GenerateInfluenceMaps(gameState, numberOfPlayers);

        var moveOrders = new List<IUnitCommand>();

        gameState.Units.Where(x => x.IsAlive).ToList().ForEach(x =>
        {
            var moveOrder = computerPlayer.FindBestMoveOrderForUnit(x, gameState);
            if (moveOrder != null)
                moveOrders.Add(moveOrder);
        });

        var vectors = new List<Centreline>();
        moveOrders.ForEach(x => vectors.AddRange(Centreline.MoveOrderToCentrelines((MoveCommand)x)));

        GameBoardRenderer.RenderAndSave("AggregateInfluenceMoveOrders.png", gameState.Width, gameState.Height, gameState.Tiles, gameState.Edges, gameState.Settlements, null, vectors, gameState.Units);

        gameState.ResolveOrders(moveOrders);

        GameBoardRenderer.RenderAndSave("AggregateInfluenceMovesResolved.png", gameState.Width, gameState.Height, gameState.Tiles, gameState.Edges, gameState.Settlements, null, null, gameState.Units);
    }

    [TestMethod]
    public void FindBestMoveOrder_InfluenceMap_SelectsReachableDestination()
    {

        var numberOfPlayers = 2;

        gameState.Units =
        [
            new(new UnitTemplate { OperationalDomain = OperationalDomain.Airborne, MovementPoints = 3 }, gameState.Players[0], gameState[114]),
            new(new UnitTemplate { MovementPoints = 3 }, gameState.Players[0], gameState[110]),
            new(new UnitTemplate(), gameState.Players[0], gameState[31]),
            new(new UnitTemplate(), gameState.Players[0], gameState[56]),
            new(new UnitTemplate(), gameState.Players[0], gameState[65]),
            new(new UnitTemplate(), gameState.Players[1], gameState[111]),
            new(new UnitTemplate(), gameState.Players[1], gameState[111]),
            new(new UnitTemplate(), gameState.Players[1], gameState[168]),
        ];


        var computerPlayer = new ComputerPlayer(gameState.Units);
        computerPlayer.GenerateInfluenceMaps(gameState, numberOfPlayers);

        var results = Hex.HexesWithinArea(gameState.Units[1].Location.Hex, 4, gameState.Width, gameState.Height);
        results.ToList().ForEach(x => gameState[Hex.HexToIndex(x, gameState.Width, gameState.Height)].IsSelected = true);

        GameBoardRenderer.RenderAndSave("HexesConsideredForHighestInfluence.png", gameState.Width, gameState.Height, gameState.Tiles, gameState.Edges, gameState.Settlements, null, null, gameState.Units);

        var roleMovementType = computerPlayer.GetUnitState(gameState.Units[1]).GetRoleMovementType(gameState.Units[1]);
        var tilesOrderedInfluence = gameState.Tiles
            .Where(x => results.Contains(x.Hex))
            .OrderByDescending(x => computerPlayer.AggregateInfluence[x.Index][roleMovementType][gameState.Units[1].Owner.Id])
            .ToList();

        IEnumerable<PathFindTile> bestPossibleDestination = null;
        foreach (var tile in tilesOrderedInfluence)
        {
            bestPossibleDestination = PathFinder.FindShortestPath(gameState.Units[1].Location, tile, gameState.Units[1]);
            if (bestPossibleDestination != null)
                break;
        }

        if (bestPossibleDestination != null)
        {
            var moveOrder = gameState.Units[1].ShortestPathToMoveCommand(bestPossibleDestination.ToArray());
        }
    }
}



