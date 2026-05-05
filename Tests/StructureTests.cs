using GameModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Tests
{

    [TestClass]
    public class StructureTests
    {
        static string[] GameBoard = File.ReadAllLines("BasicBoard.txt");
        static string[] Structures = File.ReadAllLines("BasicBoardStructures.txt");

        [TestMethod]
        public void ReadStructuresTest()
        {
            var board = new Board(GameBoard, null, Structures) ;
        }
    }
}
