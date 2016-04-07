using GameModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{

    [TestClass]
    public class StructureTests
    {
        static string[] GameBoard = File.ReadAllLines("BasicBoard.txt");
        static string[] TilePoints = File.ReadAllLines("BasicBoardPoints.txt");

        [TestMethod]
        public void ReadPointsTest()
        {
            var board = new Board(GameBoard, null, TilePoints) ;
            
        }
    }
}
