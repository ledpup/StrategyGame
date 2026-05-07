using ComputerOpponent;
using GameModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;

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

            board.Units = new List<MilitaryUnit>
            {
                new(0, location: board[20, 5], movementType: MovementType.Water, baseMovementPoints: 5, isTransporter: true),
                new(1, ownerIndex: 1, location: board[18, 7], movementType: MovementType.Water, baseMovementPoints: 3, isTransporter: true),
            };

            var computerPlayer = new ComputerPlayer(board.Units);
            computerPlayer.SetStrategicAction(board);

            Assert.AreEqual(StrategicAction.None, computerPlayer.GetUnitState(board.Units[0]).StrategicAction);
        }

        [TestMethod]
        public void NoEnemyNearNavelUnitSoDock()
        {
            var board = new Board(GameBoard, TileEdges, Structures);
            var labels = new string[board.Width, board.Height];

            var units = new List<MilitaryUnit>
            {
                new(0, location: board[20, 5], movementType: MovementType.Water, baseMovementPoints: 5, isTransporter: true),
            };

            board.Units = units;

            var computerPlayer = new ComputerPlayer(board.Units);
            computerPlayer.SetStrategicAction(board);

            Assert.AreEqual(StrategicAction.Dock, computerPlayer.GetUnitState(units[0]).StrategicAction);
        }


        [TestMethod]
        public void EnemyNearAirborneUnitSoDontPickup()
        {
            var board = new Board(GameBoard, TileEdges, Structures);
            var labels = new string[board.Width, board.Height];

            var units = new List<MilitaryUnit>
            {
                new(0, location: board[24, 11], movementType: MovementType.Airborne, baseMovementPoints: 4, isTransporter: true),
                new(1, location: board[22, 15], transportableBy: new List<MovementType> { MovementType.Airborne }, roadMovementBonus: 1),

                new(2, ownerIndex: 1, location: board[25, 12], movementType: MovementType.Airborne, baseMovementPoints: 4, isTransporter: true),
            };

            board.Units = units;

            var computerPlayer = new ComputerPlayer(board.Units);
            computerPlayer.SetStrategicAction(board);

            Assert.AreEqual(StrategicAction.None, computerPlayer.GetUnitState(units[0]).StrategicAction);
        }

        [TestMethod]
        public void EnemyNearAirborneUnitSoPickup()
        {
            var board = new Board(GameBoard, TileEdges, Structures);
            var labels = new string[board.Width, board.Height];

            var units = new List<MilitaryUnit>
            {
                new(0, location: board[24, 11], movementType: MovementType.Airborne, baseMovementPoints: 4, isTransporter: true),
                new(1, location: board[22, 15], transportableBy: new List<MovementType> { MovementType.Airborne }, roadMovementBonus: 1),
            };

            board.Units = units;

            var computerPlayer = new ComputerPlayer(board.Units);
            computerPlayer.SetStrategicAction(board);

            Assert.AreEqual(StrategicAction.Pickup, computerPlayer.GetUnitState(units[0]).StrategicAction);
        }
    }
}
