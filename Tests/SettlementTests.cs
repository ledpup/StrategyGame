using GameModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Tests;


[TestClass]
public class SettlementTests
{
    static string[] GameBoard = File.ReadAllLines("BasicBoard.txt");
    static string[] Settlements = File.ReadAllLines("BasicBoardSettlements.txt");

    [TestMethod]
    public void ReadSettlementsTest()
    {
        var board = new Board(GameBoard, null, Settlements);
    }
}
