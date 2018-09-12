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
using ProjectHaystack.io;

namespace ProjectHaystackTest.io
{
    [TestClass]
    public class JsonTest
    { // YET TO DO : fix null issue
        [TestMethod]
        public void testWriter()
        {
            HGridBuilder gb = new HGridBuilder();
            gb.addCol("a");
            gb.addCol("b");
            gb.addRow(new HVal[] { HNA.VAL, HBool.TRUE }); // Original Unit test had null - this is not valid
            gb.addRow(new HVal[] { HMarker.VAL, HNA.VAL });
            gb.addRow(new HVal[] { HRemove.VAL, HNA.VAL });
            gb.addRow(new HVal[] { HStr.make("test"), HStr.make("with:colon") });
            gb.addRow(new HVal[] { HNum.make(12), HNum.make(72.3, "\u00b0F") });
            gb.addRow(new HVal[] { HNum.make(double.NegativeInfinity), HNum.make(Double.NaN) });
            gb.addRow(new HVal[] { HDate.make(2015, 6, 9), HTime.make(1, 2, 3) });
            gb.addRow(new HVal[] { HDateTime.make(634429600180690000L, HTimeZone.make("New_York", false)), HUri.make("foo.txt") });
            gb.addRow(new HVal[] { HRef.make("abc"), HRef.make("abc", "A B C") });
            gb.addRow(new HVal[] { HBin.make("text/plain"), HCoord.make(90, -123) });
            HGrid grid = gb.toGrid();

            string actual = HJsonWriter.gridToString(grid);
            // System.out.println(actual);
            string[] lines = actual.Split('\n');
            Assert.AreEqual(lines[0], "{");
            Assert.AreEqual(lines[1], "\"meta\": {\"ver\":\"2.0\"},");
            Assert.AreEqual(lines[2], "\"cols\":[");
            Assert.AreEqual(lines[3], "{\"name\":\"a\"},");
            Assert.AreEqual(lines[4], "{\"name\":\"b\"}");
            Assert.AreEqual(lines[5], "],");
            Assert.AreEqual(lines[6], "\"rows\":[");
            Assert.AreEqual(lines[7], "{\"a\":\"z:\", \"b\":true},");
            Assert.AreEqual(lines[8], "{\"a\":\"m:\", \"b\":\"z:\"},");
            Assert.AreEqual(lines[9], "{\"a\":\"x:\", \"b\":\"z:\"},");
            Assert.AreEqual(lines[10], "{\"a\":\"test\", \"b\":\"s:with:colon\"},");
            Assert.AreEqual(lines[11], "{\"a\":\"n:12\", \"b\":\"n:72.3 \u00b0F\"},");
            Assert.AreEqual(lines[12], "{\"a\":\"n:-INF\", \"b\":\"n:NaN\"},");
            Assert.AreEqual(lines[13], "{\"a\":\"d:2015-06-09\", \"b\":\"h:01:02:03\"},");
            Assert.AreEqual(lines[14], "{\"a\":\"t:2011-06-06T12:26:58.069-05:00 New_York\", \"b\":\"u:foo.txt\"},");
            Assert.AreEqual(lines[15], "{\"a\":\"r:abc\", \"b\":\"r:abc A B C\"},");
            Assert.AreEqual(lines[16], "{\"a\":\"b:text/plain\", \"b\":\"c:90.0,-123.0\"}");
            Assert.AreEqual(lines[17], "]");
            Assert.AreEqual(lines[18], "}");
        }

    }
}
