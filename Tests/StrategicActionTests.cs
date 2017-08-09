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
    public class StrategicActionTests
    {
        public static string[] GameBoard = File.ReadAllLines("BasicBoard.txt");
        public static string[] TileEdges = File.ReadAllLines("BasicBoardEdges.txt");
        static string[] Structures = File.ReadAllLines("BasicBoardStructures.txt");

        [TestMethod]
        public void EnemyNearNavelUnit()
        {
            var board = new Board(GameBoard, TileEdges, Structures);
            var numberOfPlayers = 2;
            var labels = new string[board.Width, board.Height];

            var units = new List<MilitaryUnit>
            {
                new MilitaryUnit(0, location: board[20, 5], movementType: MovementType.Water, baseMovementPoints: 5, isTransporter: true, role: Role.Besieger),
               
                new MilitaryUnit(1, ownerIndex: 1, location: board[18, 7], movementType: MovementType.Water, baseMovementPoints: 3, isTransporter: true, role: Role.Besieger),
            };

            board.Units = units;


            ComputerPlayer.GenerateInfluenceMaps(board, numberOfPlayers);

            ComputerPlayer.SetStrategicAction(board, units);

            Assert.AreEqual(StrategicAction.None, units[0].StrategicAction);
        }
    }
}
