//
// Copyright (c) 2018
// Licensed under the Academic Free License version 3.0
//
// History:
//   16 August 2018 Ian Davies Creation based on Java Toolkit at same time from project-haystack.org downloads
//
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectHaystack;

namespace ProjectHaystackTest
{
    [TestClass]
    public class HRowTest : HValTest
    {
        [TestMethod]
        public void testEmpty()
        {
            // Instance Empty
            HRow row = BuildRows(Array.Empty<string>(), Array.Empty<HVal>()).First();
            Assert.AreEqual(row, HRow.Empty);

            // size
            Assert.AreEqual(row.size(), 0);
            Assert.IsTrue(row.isEmpty());

            // missing tag
            Assert.IsFalse(row.has("foo"));
            Assert.IsTrue(row.missing("foo"));
            Assert.IsNull(row.get("foo", false));
        }

        [TestMethod]
        [ExpectedException(typeof(HaystackUnknownNameException))]
        public void testCheckedImplicitMissing()
        {
            HRow row = BuildRows(Array.Empty<string>(), Array.Empty<HVal>()).First();
            row.get("foo");
        }

        [TestMethod]
        [ExpectedException(typeof(HaystackUnknownNameException))]
        public void testCheckedExplicitMissing()
        {
            HRow row = BuildRows(Array.Empty<string>(), Array.Empty<HVal>()).First();
            row.get("foo", true);
        }

        [TestMethod]
        public void testIsTagName()
        {
            Assert.IsFalse(HRow.isTagName(""));
            Assert.IsFalse(HRow.isTagName("A"));
            Assert.IsFalse(HRow.isTagName(" "));
            Assert.IsTrue(HRow.isTagName("a"));
            Assert.IsTrue(HRow.isTagName("a_B_19"));
            Assert.IsFalse(HRow.isTagName("a b"));
            Assert.IsFalse(HRow.isTagName("a\u0128"));
            Assert.IsFalse(HRow.isTagName("a\u0129x"));
            Assert.IsFalse(HRow.isTagName("a\uabcdx"));
            Assert.IsTrue(HRow.isTagName("^tag"));
        }

        [TestMethod]
        public void testBasics()
        {
            HRow row = BuildRows(
                new[] { "id", "site", "geoAddr", "area", "date", "null" },
                new HVal[] { HRef.make("aaaa-bbbb"), HMarker.VAL, HStr.make("Richmond, Va"), HNum.make(1200, "ft"), HDate.make(2000, 12, 3), null })
                .First();

            // size
            Assert.AreEqual(row.size(), 5);
            Assert.IsFalse(row.isEmpty());

            // configured tags
            Assert.IsTrue(row.get("id").hequals(HRef.make("aaaa-bbbb")));
            Assert.IsTrue(row.get("site").hequals(HMarker.VAL));
            Assert.IsTrue(row.get("geoAddr").hequals(HStr.make("Richmond, Va")));
            Assert.IsTrue(row.get("area").hequals(HNum.make(1200, "ft")));
            Assert.IsTrue(row.get("date").hequals(HDate.make(2000, 12, 3)));
            Assert.AreEqual(row.get("null", false), null);
            try
            {
                row.get("null");
                Assert.Fail();
            }
            catch (HaystackUnknownNameException)
            {
                Assert.IsTrue(true);
            }

            // missing tag
            Assert.IsFalse(row.has("foo"));
            Assert.IsTrue(row.missing("foo"));
            Assert.IsNull(row.get("foo", false));
            try { row.get("foo"); Assert.Fail(); } catch (HaystackUnknownNameException) { Assert.IsTrue(true); }
            try { row.get("foo", true); Assert.Fail(); } catch (HaystackUnknownNameException) { Assert.IsTrue(true); }
        }

        [TestMethod]
        public void testZinc()
        {
            verifyZinc(
                HRow.Empty,
                "{}");
            verifyZinc(
                BuildRows(new[] { "fooBar" }, new[] { HNum.make(123, "ft") }).First(),
                "{fooBar:123ft}");
            verifyZinc(
                BuildRows(new[] { "dis", "bday", "marker" }, new HVal[] { HStr.make("Bob"), HDate.make(1970, 6, 3), HMarker.VAL }).First(),
                "{dis:\"Bob\" bday:1970-06-03 marker}");

            // nested dict
            verifyZinc(
                BuildRows(new[] { "auth" }, new HVal[] { HDict.Empty }).First(),
                "{auth:{}}");
            verifyZinc(
                BuildRows(new[] { "auth" }, BuildRows(new[] { "alg", "c", "marker" }, new HVal[] { HStr.make("scram"), HNum.make(10000), HMarker.VAL }).ToArray()).First(),
                "{auth:{alg:\"scram\" c:10000 marker}}");

            // nested list
            verifyZinc(
                BuildRows(new[] { "arr", "x" }, new HVal[] { HList.make(new HVal[] { HNum.make(1.0), HNum.make(2), HNum.make(3) }), HMarker.VAL }).First(),
                "{arr:[1,2,3] x}"); // Was "{arr:[1.0,2,3] x}" - double in .NET will not recognise the difference between 1.0 and 1
        }

        [TestMethod]
        public void testToArray()
        {
            var row = BuildRows(new[] { "x", "y" }, new HVal[] { HMarker.VAL, HStr.make("str") }).First();
            var array = row.ToArray();
            Assert.AreEqual(2, array.Length);
            Assert.AreEqual("x", array[0].Key);
            Assert.AreEqual(HMarker.VAL, array[0].Value);
            Assert.AreEqual("y", array[1].Key);
            Assert.AreEqual(HStr.make("str"), array[1].Value);
        }

        [TestMethod]
        public void testAdd()
        {
            var row = BuildRows(new[] { "x", "y" }, new HVal[] { HMarker.VAL, HStr.make("y") }).First();
            Assert.ThrowsException<InvalidOperationException>(() => row.Add("z", HMarker.VAL));
        }

        [TestMethod]
        public void testRemove()
        {
            var row = BuildRows(new[] { "x", "y" }, new HVal[] { HMarker.VAL, HStr.make("y") }).First();
            Assert.ThrowsException<InvalidOperationException>(() => row.Remove("y"));
        }

        [TestMethod]
        public void testToDict()
        {
            var row = BuildRows(new[] { "x", "y" }, new HVal[] { HMarker.VAL, HStr.make("y") }).First();
            var dict = row.ToDict();
            Assert.AreEqual(2, dict.Size);
            Assert.AreEqual(HMarker.VAL, dict["x"]);
            Assert.AreEqual(HStr.make("y"), dict["y"]);
        }

        private IEnumerable<HRow> BuildRows(ICollection<string> cols, params HVal[][] rows)
        {
            var gridBuilder = new HGridBuilder();
            foreach (var col in cols)
            {
                gridBuilder.addCol(col);
            }
            foreach (var row in rows)
            {
                gridBuilder.addRow(row);
            }
            return gridBuilder.toGrid().Rows;
        }
    }
}