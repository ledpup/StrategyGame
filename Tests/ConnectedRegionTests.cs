using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ComputerOpponent;
using GameModel;
using Visualise;

namespace Tests;

[TestClass]
public class ConnectedRegionTests
{
    [TestMethod]
    public void ConnectedRegion_MountainEdgesBetweenFourSurroundedLandHexes_CreatesThreeRegions()
    {
        string[] gameBoard =
        [
            "SSSS",
            "SGGS",
            "SGGS",
            "SSSS",
        ];

        string[] tileEdges =
        [
            "5,6,Mountain,false",
            "5,10,Mountain,false",
            "9,10,Mountain,false",
        ];

        var board = new Board(gameBoard, tileEdges);
        ConnectedRegionCalculator.Calculate(board);

        var labels = new string[board.Width * board.Height];
        board.Tiles.ToList().ForEach(x => labels[x.Index] = x.ConnectedRegionId.ToString());
        GameBoardRenderer.RenderAndSave("ConnectedRegionMountainEdgesTestBoard.png", board.Width, board.Height, board.Tiles, board.Edges, board.Settlements, labels);

        var regionCount = board.Tiles.Select(x => x.ConnectedRegionId).Distinct().Count();

        Assert.AreEqual(3, regionCount);
    }

    [TestMethod]
    public void ConnectedRegion_RoadAcrossMountainRange_ConnectsSurroundedLandHexes()
    {
        string[] gameBoard =
        [
            "SSSS",
            "SGGS",
            "SGGS",
            "SSSS",
        ];

        string[] tileEdges =
        [
            "5,6,Mountain,false",
            "5,10,Mountain,true",
            "9,10,Mountain,false",
        ];

        var board = new Board(gameBoard, tileEdges);
        ConnectedRegionCalculator.Calculate(board);

        var labels = new string[board.Width * board.Height];
        board.Tiles.ToList().ForEach(x => labels[x.Index] = x.ConnectedRegionId.ToString());
        GameBoardRenderer.RenderAndSave("ConnectedRegionRoadAcrossMountainRangeTestBoard.png", board.Width, board.Height, board.Tiles, board.Edges, board.Settlements, labels);

        var regionCount = board.Tiles.Select(x => x.ConnectedRegionId).Distinct().Count();

        Assert.AreEqual(2, regionCount);
    }

    [TestMethod]
    public void ConnectedRegion_RoadThroughMountain_ConnectsLandRegions()
    {
        string[] gameBoard = File.ReadAllLines("ConnectedRegionTestBoard.txt");
        string[] tileEdges = File.ReadAllLines("ConnectedRegionTestEdges.txt");

        var board = new Board(gameBoard, tileEdges);
        ConnectedRegionCalculator.Calculate(board);

        Assert.AreEqual(board[17].ConnectedRegionId, board[27].ConnectedRegionId);
        Assert.AreEqual(board[17].ConnectedRegionId, board[37].ConnectedRegionId);
    }

    [TestMethod]
    public void ConnectedRegion_UnroadedMountain_DoesNotConnectLandRegions()
    {
        string[] gameBoard = File.ReadAllLines("ConnectedRegionTestBoard.txt");
        string[] tileEdges = File.ReadAllLines("ConnectedRegionTestEdges.txt");

        var board = new Board(gameBoard, tileEdges);
        ConnectedRegionCalculator.Calculate(board);

        Assert.AreNotEqual(board[12].ConnectedRegionId, board[22].ConnectedRegionId);
        Assert.AreNotEqual(board[12].ConnectedRegionId, board[32].ConnectedRegionId);
    }
}
