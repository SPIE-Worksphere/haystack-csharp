//
// Copyright (c) 2018
// Licensed under the Academic Free License version 3.0
//
// History:
//   16 August 2018 Ian Davies Creation based on Java Toolkit at same time from project-haystack.org downloads
//
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectHaystack;

namespace ProjectHaystackTest
{
    [TestClass]
    public class HCoordTest : HValTest
    {
        [TestMethod]
        public void testLatBoundaries()
        {
            verifyCoord(90, 123, "C(90,123)");
            verifyCoord(-90, 123, "C(-90,123)");
            verifyCoord(89.888999, 123, "C(89.888999,123)");
            verifyCoord(-89.888999, 123, "C(-89.888999,123)");
        }

        [TestMethod]
        public void testLonBoundaries()
        {
            verifyCoord(45, 180, "C(45,180)");
            verifyCoord(45, -180, "C(45,-180)");
            verifyCoord(45, 179.999129, "C(45,179.999129)");
            verifyCoord(45, -179.999129, "C(45,-179.999129)");
        }

        [TestMethod]
        public void testDecimalPlaces()
        {
            verifyCoord(9.1, -8.1, "C(9.1,-8.1)");
            verifyCoord(9.12, -8.13, "C(9.12,-8.13)");
            verifyCoord(9.123, -8.134, "C(9.123,-8.134)");
            verifyCoord(9.1234, -8.1346, "C(9.1234,-8.1346)");
            verifyCoord(9.12345, -8.13456, "C(9.12345,-8.13456)");
            verifyCoord(9.123452, -8.134567, "C(9.123452,-8.134567)");
        }

        [TestMethod]
        public void testZeroBoundaries()
        {
            verifyCoord(0, 0, "C(0,0)");
            verifyCoord(0.3, -0.3, "C(0.3,-0.3)");
            verifyCoord(0.03, -0.03, "C(0.03,-0.03)");
            verifyCoord(0.003, -0.003, "C(0.003,-0.003)");
            verifyCoord(0.0003, -0.0003, "C(0.0003,-0.0003)");
            verifyCoord(0.02003, -0.02003, "C(0.02003,-0.02003)");
            verifyCoord(0.020003, -0.020003, "C(0.020003,-0.020003)");
            verifyCoord(0.000123, -0.000123, "C(0.000123,-0.000123)");
            verifyCoord(7.000123, -7.000123, "C(7.000123,-7.000123)");
        }

        [TestMethod]
        public void testIsLat()
        {
            Assert.IsFalse(HCoord.isLat(-91.0));
            Assert.IsTrue(HCoord.isLat(-90.0));
            Assert.IsTrue(HCoord.isLat(-89.0));
            Assert.IsTrue(HCoord.isLat(90.0));
            Assert.IsFalse(HCoord.isLat(91.0));
        }

        [TestMethod]
        public void testIsLng()
        {
            Assert.IsFalse(HCoord.isLng(-181.0));
            Assert.IsTrue(HCoord.isLng(-179.99));
            Assert.IsTrue(HCoord.isLng(180.0));
            Assert.IsFalse(HCoord.isLng(181.0));
        }

        [TestMethod]
        public void testMakeErrors()
        {
            try { HCoord.make(91, 12); Assert.Fail(); } catch (ArgumentException) { Assert.IsTrue(true); }
            try { HCoord.make(-90.2, 12); Assert.Fail(); } catch (ArgumentException) { Assert.IsTrue(true); }
            try { HCoord.make(13, 180.009); Assert.Fail(); } catch (ArgumentException) { Assert.IsTrue(true); }
            try { HCoord.make(13, -181); Assert.Fail(); } catch (ArgumentException) { Assert.IsTrue(true); }
        }

        private void verifyCoord(double lat, double lng, string s)
        {
            HCoord c = HCoord.make(lat, lng);
            Assert.AreEqual(c.lat, lat);
            Assert.AreEqual(c.lng, lng);
            Assert.AreEqual(c.ToString(), s);
            HCoord strCoord = HCoord.make(s);
            Assert.IsTrue(strCoord.hequals(c));
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))] // In Java this was parse exception
        public void RunBadZincProviderTest ()
        {
            string[] badZincs =
                    {  "C(0.123,-.789)",
                    "1,2",
                    "(1,2)",
                    "C(1,2",
                    "C1,2)",
                    "C(x,9)" };
            foreach (string strCurZinc in badZincs)
                HCoord.make(strCurZinc);
        }

    }
}