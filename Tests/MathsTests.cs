using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameModel;

namespace Tests
{
    [TestClass]
    public class MathsTests
    {
        [TestMethod]
        public void Normalise()
        {
            Assert.AreEqual(1, Maths.Normalise(361, 0, 360));
            Assert.AreEqual(0, Maths.Normalise(720, 0, 360));
        }

        [TestMethod]
        public void DegreesToRadians()
        {
            Assert.AreEqual(0, Math.Round(Maths.DegreeToRadian(0), 5));
            Assert.AreEqual(1.5708D, Math.Round(Maths.DegreeToRadian(90), 5));
            Assert.AreEqual(3.14159D, Math.Round(Maths.DegreeToRadian(180), 5));
            Assert.AreEqual(4.71239D, Math.Round(Maths.DegreeToRadian(270), 5));
            Assert.AreEqual(6.26573D, Math.Round(Maths.DegreeToRadian(359), 5));
            Assert.AreEqual(0, Math.Round(Maths.DegreeToRadian(360), 5));
            Assert.AreEqual(0, Math.Round(Maths.DegreeToRadian(720), 5));
        }

        [TestMethod]
        public void GetAngleFromPoints()
        {
            Assert.AreEqual(0, PointD.Theta(new PointD(0, 0), new PointD(1, 0)));
            Assert.AreEqual(90, PointD.Theta(new PointD(0, 0), new PointD(0, -1)));
            Assert.AreEqual(135, PointD.Theta(new PointD(0, 0), new PointD(-1, -1)));
            Assert.AreEqual(180, PointD.Theta(new PointD(0, 0), new PointD(-1, 0)));
            Assert.AreEqual(270, PointD.Theta(new PointD(0, 0), new PointD(0, 1)));
            Assert.AreEqual(315, PointD.Theta(new PointD(0, 0), new PointD(1, 1)));
        }

        [TestMethod]
        public void EdgeToCentrePoint()
        {
            var layout = new Layout(Layout.flat, new PointD(10, 10), new PointD(0, 0));
            var points = Layout.PolygonCorners(layout, new Hex(1, 1, -2)).ToArray();

            var point = PointD.EdgeToCentrePoint(points, 10, 0);

            Assert.AreEqual(22.5, Math.Round(point.X, 1));
            Assert.AreEqual(30.31, Math.Round(point.Y, 2));
        }
    }
}
