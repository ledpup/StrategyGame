using GameModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Tests;

[TestClass]
public class SupplyTests
{
    static string[] GameBoard = File.ReadAllLines("BasicBoard.txt");
    static string[] Edges = File.ReadAllLines("BasicBoardEdges.txt");
    static string[] Settlements = File.ReadAllLines("BasicBoardSettlements.txt");

    [TestMethod]
    public void SupplyTest()
    {
        var board = new GameState(new Board(GameBoard, Edges, Settlements));

        board[3, 4].OwnerId = 2;
        board.Units = [new(new UnitTemplate(), 0, 2, board[3, 4], "1st Enemy")];
        board.InitialiseSupply();

        var labels = new string[board.Width * board.Height];
        board.Tiles.ToList().ForEach(x => labels[x.Index] = x.Supply.ToString());

        Visualise.GameBoardRenderer.RenderAndSave("BasicBoardWithSettlementsAndSupply.png", board.Width, board.Height, board.Tiles, board.Edges, board.Settlements, labels, null, board.Units);
    }
}
