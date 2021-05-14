using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectHaystack;

namespace ProjectHaystackTest
{
    [TestClass]
    public class HaystackGridTests
    {
        [TestMethod]
        public void TestEmpty()
        {
            var grid = new HaystackGrid();
            Assert.AreEqual(grid.Meta, new HaystackDictionary());
            Assert.AreEqual(grid.RowCount, 0);
            Assert.IsTrue(grid.IsEmpty());
            Assert.IsFalse(grid.HasColumn("foo"));
            Assert.ThrowsException<HaystackUnknownNameException>(() => grid.Column("foo"));
        }

        [TestMethod]
        public void TestNoRows()
        {
            var grid = new HaystackGrid();
            grid.Meta.Add("dis", new HaystackString("Title"));
            grid.AddColumn("a", col => col.Meta.Add("dis", new HaystackString("Alpha")));
            grid.AddColumn("b");

            //.Meta
            Assert.AreEqual(grid.Meta.Count, 1);
            Assert.IsTrue(grid.Meta["dis"].Equals(new HaystackString("Title")));

            // cols
            Assert.AreEqual(grid.ColumnCount, 2);
            var c = VerifyCol(grid, 0, "a");
            Assert.AreEqual(c.Display, "Alpha");
            Assert.AreEqual(c.Meta.Count, 1);
            Assert.IsTrue(c.Meta["dis"].Equals(new HaystackString("Alpha")));

            // rows
            Assert.AreEqual(grid.RowCount, 0);
            Assert.AreEqual(grid.IsEmpty(), true);

            // iterator
            VerifyGridIterator(grid);
        }

        [TestMethod]
        public void TestSimple()
        {
            var grid = new HaystackGrid();
            grid.AddColumn("id");
            grid.AddColumn("dis");
            grid.AddColumn("area");
            grid.AddRow(new HaystackReference("a"), new HaystackString("Alpha"), new HaystackNumber(1200));
            grid.AddRow(new HaystackReference("b"), null, new HaystackNumber(1400));

            //.Meta
            Assert.AreEqual(grid.Meta.Count, 0);

            // cols
            //HCol c;
            Assert.AreEqual(grid.ColumnCount, 3);
            VerifyCol(grid, 0, "id");
            VerifyCol(grid, 1, "dis");
            VerifyCol(grid, 2, "area");

            // rows
            Assert.AreEqual(grid.RowCount, 2);
            Assert.IsFalse(grid.IsEmpty());
            var row = grid.Row(0);
            Assert.IsTrue(row.Get("id").Equals(new HaystackReference("a")));
            Assert.IsTrue(row.Get("dis").Equals(new HaystackString("Alpha")));
            Assert.IsTrue(row.Get("area").Equals(new HaystackNumber(1200)));
            row = grid.Row(1);
            Assert.IsTrue(row.Get("id").Equals(new HaystackReference("b")));
            Assert.IsFalse(row.ContainsKey("dis"));
            Assert.IsTrue(row.Get("area").Equals(new HaystackNumber(1400)));
            Assert.ThrowsException<HaystackUnknownNameException>(() => row["dis"]);
            Assert.IsFalse(row.ContainsKey("fooBar"));
            Assert.ThrowsException<HaystackUnknownNameException>(() => row["fooBar"]);

            // HaystackRow no-nulls
            HaystackRow it = grid.Row(0);
            Assert.IsFalse(it.Count > 3);
            VerifyRowIterator(it, 0, "id", new HaystackReference("a"));
            VerifyRowIterator(it, 1, "dis", new HaystackString("Alpha"));
            VerifyRowIterator(it, 2, "area", new HaystackNumber(1200));


            // HaystackRow with nulls
            it = grid.Row(1);
            Assert.IsFalse(it.Count > 3);
            VerifyRowIterator(it, 0, "id", new HaystackReference("b"));
            VerifyRowIterator(it, 2, "area", new HaystackNumber(1400));

            // iterating
            VerifyGridIterator(grid);
        }

        HaystackColumn VerifyCol(HaystackGrid g, int i, string n)
        {
            var col = g.Column(i);
            Assert.IsTrue(g.Column(i).Equals(g.Column(n)));
            Assert.AreEqual(col.Name, n);
            return col;
        }

        void VerifyRowIterator(HaystackRow it, int iIndex, string name, HaystackValue val)
        {
            Assert.IsTrue(it.Count > iIndex);
            var strKeyValue = it.Keys.Skip(iIndex).First();
            Assert.AreEqual(name, strKeyValue);
            Assert.IsTrue(it[strKeyValue].Equals(val));
        }

        void VerifyGridIterator(HaystackGrid g)
        {
            int c = 0;
            while (c < g.RowCount)
            {
                c++;
            }
            Assert.AreEqual(g.RowCount, c);
        }
    }
}