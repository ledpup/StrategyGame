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
        [TestMethod]
        public void LoadBoardTest()
        {
            var boardData = "SSSSSSSSSSSSSSSSSSSSSSSSSSS\r\nSFFGGGAASSFFGGGAASSFFGGGAAS\r\nSGFGMHGASSGGGMHGASSGGGMHGAS\r\nSGMMGGAASSGMMGGAASSGMMGGAAS\r\nSMGGLLGGSSMGGLLGGSSMGGLLGGS\r\nSSSFFWGGSSSSFFWGGSSSSFFWGGS\r\nSGMSFWGGSSGMSFWGGSSGMSFWGGS\r\nSGRRSGSGSSGSSSGSGSSGSSSGSGS\r\nSSSSSSSSSSSSSSSSSSSSSSSSSSS\r\nSSSSSSSSSSSSSSSSSSSSSSSSSSS\r\nSFFGGGAASSFFGGGAASSFFGGGAAS\r\nSGGGMHGASSGGGMHGASSGGGMHGAS\r\nSGMMGGAASSGMMGGAASSGMMGGAAS\r\nSMGGLLGGSSMGGLLGGSSMGGLLGGS\r\nSSSFFWGGSSSSFFWGGSSSSFFWGGS\r\nSGMSFWGGSSGMSFWGGSSGMSFWGGS\r\nSGSSSGSGSSGSSSGSGSSGSSSGSGS\r\nSSSSSSSSSSSSSSSSSSSSSSSSSSS";

            var board = Board.LoadBoard(boardData);

            board.Tiles.ToList().ForEach(t => Assert.IsFalse(t.AdjacentTiles.Any(at => at == null)));
        }
    }
}
