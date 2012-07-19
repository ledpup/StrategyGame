using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Model;

namespace Tests
{
    [TestClass]
    public class BoardTests
    {
        public static string GameBoard = "SSSSSSSSSSSSSSSSSSSSSSSSSSS\r\nSFFGGGDDSSFFGGGDDSSFFGGGDDS\r\nSGFGMHGDSSGGGMHGDSSGGGMHGDS\r\nSGMMGGDDSSGMMGGDDSSGMMGGDDS\r\nSMGGLLGGSSMGGLLGGSSMGGLLGGS\r\nSSSFFWGGSSSSFFWGGSSSSFFWGGS\r\nSGMSFWGGSSGMSFWGGSSGMSFWGGS\r\nSGRRSGSGSSGSSSGSGSSGSSSGSGS\r\nSSSSSSSSSSSSSSSSSSSSSSSSSSS\r\nSSSSSSSSSSSSSSSSSSSSSSSSSSS\r\nSFFGGGDDSSFFGGGDDSSFFGGGDDS\r\nSGGGMHGDSSGGGMHGDSSGGGMHGDS\r\nSGMMGGDDSSGMMGGDDSSGMMGGDDS\r\nSMGGLLGGSSMGGLLGGSSMGGLLGGS\r\nSSSFFWGGSSSSFFWGGSSSSFFWGGS\r\nSGMSFWGGSSGMSFWGGSSGMSFWGGS\r\nSGSSSGSGSSGSSSGSGSSGSSSGSGS\r\nSSSSSSSSSSSSSSSSSSSSSSSSSSS";
        public static string TileEdges = "20,21,River\r\n19,20,Road\r\n20,38,Road\r\n38,56,Road";

        [TestMethod]
        public void LoadBoard_AdjacentTilesAndCoastal_NoNullTilesAndCoasts()
        {
            var board = Board.LoadBoard(GameBoard, TileEdges);

            var tiles = board.Tiles.ToList();

            tiles.ForEach(t => Assert.AreEqual(1, tiles.Count(x => x.Id == t.Id)));

            // Ensure that adjacent tiles have been populated correctly
            tiles.ForEach(t => Assert.IsFalse(t.AdjacentTiles.Any(at => at == null)));
            
            // Ensure that some tiles are coastal.
            var coastal = tiles.Count(t => t.IsCoastal);
            Assert.AreNotEqual(0, coastal);
        }
    }
}
