using ComputerOpponent;
using GameModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ComputerOpponent.ComputerPlayer;

namespace Tests
{
    [TestClass]
    public class StrategicActionTests
    {
        public static string[] GameBoard = File.ReadAllLines("BasicBoard.txt");
        public static string[] TileEdges = File.ReadAllLines("BasicBoardEdges.txt");
        static string[] Structures = File.ReadAllLines("BasicBoardStructures.txt");

        [TestMethod]
        public void EnemyNearNavelUnitSoDontDock()
        {
            var board = new Board(GameBoard, TileEdges, Structures);
            var labels = new string[board.Width, board.Height];

            var units = new List<MilitaryUnit>
            {
                new MilitaryUnit(0, location: board[20, 5], movementType: MovementType.Water, baseMovementPoints: 5, isTransporter: true, role: Role.Besieger),
               
                new MilitaryUnit(1, ownerIndex: 1, location: board[18, 7], movementType: MovementType.Water, baseMovementPoints: 3, isTransporter: true, role: Role.Besieger),
            };

            board.Units = units;

            ComputerPlayer.SetStrategicAction(board, units);

            Assert.AreEqual(StrategicAction.None, units[0].StrategicAction);
        }

        [TestMethod]
        public void NoEnemyNearNavelUnitSoDock()
        {
            var board = new Board(GameBoard, TileEdges, Structures);
            var labels = new string[board.Width, board.Height];

            var units = new List<MilitaryUnit>
            {
                new MilitaryUnit(0, location: board[20, 5], movementType: MovementType.Water, baseMovementPoints: 5, isTransporter: true, role: Role.Besieger),
            };

            board.Units = units;

            ComputerPlayer.SetStrategicAction(board, units);

            Assert.AreEqual(StrategicAction.Dock, units[0].StrategicAction);
        }


        [TestMethod]
        public void EnemyNearAirborneUnitSoDontPickup()
        {
            var board = new Board(GameBoard, TileEdges, Structures);
            var labels = new string[board.Width, board.Height];

            var units = new List<MilitaryUnit>
            {
                new MilitaryUnit(0, location: board[24, 11], movementType: MovementType.Airborne, baseMovementPoints: 4, isTransporter: true, role: Role.Besieger),
                new MilitaryUnit(1, location: board[22, 15], transportableBy: new List<MovementType> { MovementType.Airborne }, roadMovementBonus: 1),


                new MilitaryUnit(0, ownerIndex: 1, location: board[25, 12], movementType: MovementType.Airborne, baseMovementPoints: 4, isTransporter: true, role: Role.Besieger),
            };

            board.Units = units;

            ComputerPlayer.SetStrategicAction(board, units);

            Assert.AreEqual(StrategicAction.None, units[0].StrategicAction);
        }

        [TestMethod]
        public void EnemyNearAirborneUnitSoPickup()
        {
            var board = new Board(GameBoard, TileEdges, Structures);
            var labels = new string[board.Width, board.Height];

            var units = new List<MilitaryUnit>
            {
                new MilitaryUnit(0, location: board[24, 11], movementType: MovementType.Airborne, baseMovementPoints: 4, isTransporter: true, role: Role.Besieger),
                new MilitaryUnit(1, location: board[22, 15], transportableBy: new List<MovementType> { MovementType.Airborne }, roadMovementBonus: 1),

            };

            board.Units = units;

            ComputerPlayer.SetStrategicAction(board, units);

            Assert.AreEqual(StrategicAction.Pickup, units[0].StrategicAction);
        }
    }
}
