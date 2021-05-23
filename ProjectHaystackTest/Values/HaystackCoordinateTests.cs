using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectHaystack;
using ProjectHaystack.io;

namespace ProjectHaystackTest
{
    [TestClass]
    public class HaystackCoordinateTest
    {
        [TestMethod]
        public void TestLatBoundaries()
        {
            VerifyCoord(90, 123, "C(90,123)");
            VerifyCoord(-90, 123, "C(-90,123)");
            VerifyCoord(89.888999m, 123, "C(89.888999,123)");
            VerifyCoord(-89.888999m, 123, "C(-89.888999,123)");
        }

        [TestMethod]
        public void TestLonBoundaries()
        {
            VerifyCoord(45, 180, "C(45,180)");
            VerifyCoord(45, -180, "C(45,-180)");
            VerifyCoord(45, 179.999129m, "C(45,179.999129)");
            VerifyCoord(45, -179.999129m, "C(45,-179.999129)");
        }

        [TestMethod]
        public void TestDecimalPlaces()
        {
            VerifyCoord(9.1m, -8.1m, "C(9.1,-8.1)");
            VerifyCoord(9.12m, -8.13m, "C(9.12,-8.13)");
            VerifyCoord(9.123m, -8.134m, "C(9.123,-8.134)");
            VerifyCoord(9.1234m, -8.1346m, "C(9.1234,-8.1346)");
            VerifyCoord(9.12345m, -8.13456m, "C(9.12345,-8.13456)");
            VerifyCoord(9.123452m, -8.134567m, "C(9.123452,-8.134567)");
        }

        [TestMethod]
        public void TestZeroBoundaries()
        {
            VerifyCoord(0, 0, "C(0,0)");
            VerifyCoord(0.3m, -0.3m, "C(0.3,-0.3)");
            VerifyCoord(0.03m, -0.03m, "C(0.03,-0.03)");
            VerifyCoord(0.003m, -0.003m, "C(0.003,-0.003)");
            VerifyCoord(0.0003m, -0.0003m, "C(0.0003,-0.0003)");
            VerifyCoord(0.02003m, -0.02003m, "C(0.02003,-0.02003)");
            VerifyCoord(0.020003m, -0.020003m, "C(0.020003,-0.020003)");
            VerifyCoord(0.000123m, -0.000123m, "C(0.000123,-0.000123)");
            VerifyCoord(7.000123m, -7.000123m, "C(7.000123,-7.000123)");
        }

        [TestMethod]
        public void TestMakeErrors()
        {
            try { new HaystackCoordinate(91, 12); Assert.Fail(); } catch (ArgumentException) { Assert.IsTrue(true); }
            try { new HaystackCoordinate(-90.2m, 12); Assert.Fail(); } catch (ArgumentException) { Assert.IsTrue(true); }
            try { new HaystackCoordinate(13, 180.009m); Assert.Fail(); } catch (ArgumentException) { Assert.IsTrue(true); }
            try { new HaystackCoordinate(13, -181); Assert.Fail(); } catch (ArgumentException) { Assert.IsTrue(true); }
        }

        private void VerifyCoord(decimal latitude, decimal longitude, string s)
        {
            var coordinate = new HaystackCoordinate(latitude, longitude);
            Assert.AreEqual(latitude, coordinate.Latitude);
            Assert.AreEqual(longitude, coordinate.Longitude);
            Assert.AreEqual(s, ZincWriter.ToZinc(coordinate));
        }
    }
}