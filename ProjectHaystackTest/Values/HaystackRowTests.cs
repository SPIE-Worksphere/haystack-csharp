using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectHaystack;

namespace ProjectHaystackTest
{
    [TestClass]
    public class HaystackRowTests
    {
        [TestMethod]
        public void TestEmpty()
        {
            var row = BuildRows(Array.Empty<string>(), Array.Empty<HaystackValue>()).First();
            Assert.IsTrue(row.IsEmpty());
            Assert.AreEqual(row.Count, 0);
            Assert.IsFalse(row.ContainsKey("foo"));
        }

        [TestMethod]
        public void TestCheckedImplicitMissing()
        {
            var row = BuildRows(Array.Empty<string>(), Array.Empty<HaystackValue>()).First();
            Assert.ThrowsException<HaystackUnknownNameException>(() => row["foo"]);
        }

        [TestMethod]
        public void TestBasics()
        {
            var row = BuildRows(
                new[] { "id", "site", "geoAddr", "area", "date", "null" },
                new HaystackValue[] { new HaystackReference("aaaa-bbbb"), new HaystackMarker(), new HaystackString("Richmond, Va"), new HaystackNumber(1200, "ft"), new HaystackDate(2000, 12, 3), null })
                .First();

            // size
            Assert.AreEqual(row.Count, 6);
            Assert.IsFalse(row.IsEmpty());

            // configured tags
            Assert.IsTrue(row["id"].Equals(new HaystackReference("aaaa-bbbb")));
            Assert.IsTrue(row["site"].Equals(new HaystackMarker()));
            Assert.IsTrue(row["geoAddr"].Equals(new HaystackString("Richmond, Va")));
            Assert.IsTrue(row.Get("area").Equals(new HaystackNumber(1200, "ft")));
            Assert.IsTrue(row.Get("date").Equals(new HaystackDate(2000, 12, 3)));
            Assert.ThrowsException<HaystackUnknownNameException>(() => row.Get("null"));

            // missing tag
            Assert.IsFalse(row.ContainsKey("foo"));
            Assert.ThrowsException<HaystackUnknownNameException>(() => row.Get("foo"));
        }

        [TestMethod]
        public void TestToArray()
        {
            var row = BuildRows(new[] { "x", "y" }, new HaystackValue[] { new HaystackMarker(), new HaystackString("str") }).First();
            var array = row.ToArray();
            Assert.AreEqual(2, array.Length);
            Assert.AreEqual("x", array[0].Key);
            Assert.AreEqual(new HaystackMarker(), array[0].Value);
            Assert.AreEqual("y", array[1].Key);
            Assert.AreEqual(new HaystackString("str"), array[1].Value);
        }

        [TestMethod]
        public void TestAdd()
        {
            var row = BuildRows(new[] { "x", "y" }, new HaystackValue[] { new HaystackMarker(), new HaystackString("y") }).First();
            Assert.ThrowsException<InvalidOperationException>(() => row.Add("z", new HaystackMarker()));
        }

        [TestMethod]
        public void TestRemove()
        {
            var row = BuildRows(new[] { "x", "y" }, new HaystackValue[] { new HaystackMarker(), new HaystackString("y") }).First();
            Assert.ThrowsException<InvalidOperationException>(() => row.Remove("y"));
        }

        public static IEnumerable<HaystackRow> BuildRows(ICollection<string> cols, params HaystackValue[][] rows)
        {
            var grid = new HaystackGrid();
            foreach (var col in cols)
            {
                grid.AddColumn(col);
            }
            foreach (var row in rows)
            {
                grid.AddRow(row);
            }
            return grid.Rows;
        }
    }
}