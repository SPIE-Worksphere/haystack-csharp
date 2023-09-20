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
    public class HGridTest : HValTest
    {
        [TestMethod]
        public void testEmpty()
        {
            HGrid g = new HGridBuilder().toGrid();
            Assert.AreEqual(g.meta, HDict.Empty);
            Assert.AreEqual(g.numRows, 0);
            Assert.IsTrue(g.isEmpty());
            Assert.IsNull(g.col("foo", false));
            try
            {
                g.col("foo"); Assert.Fail();
            }
            catch (HaystackUnknownNameException)
            {
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void testNoRows()
        {
            HGridBuilder b = new HGridBuilder();
            b.Meta.add("dis", "Title");
            b.addCol("a").add("dis", "Alpha");
            b.addCol("b");
            HGrid g = b.toGrid();

            // meta
            Assert.AreEqual(g.meta.size(), 1);
            Assert.IsTrue(g.meta.get("dis").hequals(HStr.make("Title")));

            // cols
            HCol c;
            Assert.AreEqual(g.numCols, 2);
            c = verifyCol(g, 0, "a");
            Assert.AreEqual(c.dis(), "Alpha");
            Assert.AreEqual(c.meta.size(), 1);
            Assert.IsTrue(c.meta.get("dis").hequals(HStr.make("Alpha")));

            // rows
            Assert.AreEqual(g.numRows, 0);
            Assert.AreEqual(g.isEmpty(), true);

            // iterator
            verifyGridIterator(g);
        }

        [TestMethod]
        public void testSimple()
        {
            HGridBuilder b = new HGridBuilder();
            b.addCol("id");
            b.addCol("dis");
            b.addCol("area");
            b.addRow(new HVal[] { HRef.make("a"), HStr.make("Alpha"), HNum.make(1200) });
            b.addRow(new HVal[] { HRef.make("b"), null, HNum.make(1400) });

            // meta
            HGrid g = b.toGrid();
            Assert.AreEqual(g.meta.size(), 0);

            // cols
            //HCol c;
            Assert.AreEqual(g.numCols, 3);
            verifyCol(g, 0, "id");
            verifyCol(g, 1, "dis");
            verifyCol(g, 2, "area");

            // rows
            Assert.AreEqual(g.numRows, 2);
            Assert.IsFalse(g.isEmpty());
            HRow r;
            r = g.row(0);
            Assert.IsTrue(r.get("id").hequals(HRef.make("a")));
            Assert.IsTrue(r.get("dis").hequals(HStr.make("Alpha")));
            Assert.IsTrue(r.get("area").hequals(HNum.make(1200)));
            r = g.row(1);
            Assert.IsTrue(r.get("id").hequals(HRef.make("b")));
            Assert.IsNull(r.get("dis", false));
            Assert.IsTrue(r.get("area").hequals(HNum.make(1400)));
            try { r.get("dis"); Assert.Fail(); } catch (HaystackUnknownNameException) { Assert.IsTrue(true); }
            Assert.IsNull(r.get("fooBar", false));
            try { r.get("fooBar"); Assert.Fail(); } catch (HaystackUnknownNameException) { Assert.IsTrue(true); }

            // HRow no-nulls
            HRow it = g.row(0);
            Assert.IsFalse(it.Size > 3);
            verifyRowIterator(it, 0, "id", HRef.make("a"));
            verifyRowIterator(it, 1, "dis", HStr.make("Alpha"));
            verifyRowIterator(it, 2, "area", HNum.make(1200));
            

            // HRow with nulls
            it = g.row(1);
            Assert.IsFalse(it.Size > 3);
            verifyRowIterator(it, 0, "id", HRef.make("b"));
            verifyRowIterator(it, 1, "area", HNum.make(1400));

            // iterating
            verifyGridIterator(g);
        }

        HCol verifyCol(HGrid g, int i, string n)
        {
            HCol col = g.col(i);
            Assert.IsTrue(g.col(i).hequals(g.col(n)));
            Assert.AreEqual(col.Name, n);
            return col;
        }

        void verifyRowIterator(HRow it, int iIndex, string name, HVal val)
        {

            Assert.IsTrue(it.Size > iIndex);
            string strKeyValue = it.getKeyAt(iIndex, false);
            Assert.AreEqual(strKeyValue, name);
            Assert.IsTrue(it.get(strKeyValue, false).hequals(val));
        }

        void verifyGridIterator(HGrid g)
        {
            int c = 0;
            while (c < g.numRows)
            {
                c++;
            }
            Assert.AreEqual(g.numRows, c);
        }
    }
}
