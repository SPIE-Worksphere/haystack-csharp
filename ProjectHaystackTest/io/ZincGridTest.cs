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
    // NOTE: Auto-generated java toolkit code not copied /ported due to version 2.0 zinc
    [TestClass]
    public class ZincGridTest : HValTest
    {
        [TestMethod]
        public void testNullGridMetaAndColMeta()
        {
            verifyGrid("ver:\"3.0\" tag:N\n" +
                "a nullmetatag:N, b markermetatag\n" +
                "",
              new HDictBuilder().add("tag", (HVal)null).toDict(), // meta
              new object[] { "a", new HDictBuilder().add("nullmetatag", (HVal)null).toDict(), "b", new HDictBuilder().add("markermetatag").toDict() },
              new HVal[][] { }
            );
        }

        [TestMethod]
        public void valToString_InnerGrid()
        {
            var builder = new HGridBuilder();
            builder.addCol("val");
            builder.addCol("other");
            builder.addRow(new HVal[] { HStr.make("value"), HNum.make(10) });
            var grid = builder.toGrid();
            builder = new HGridBuilder();
            builder.addCol("val");
            builder.addCol("inner");
            builder.addRow(new HVal[] { HStr.make("value"), grid });
            grid = builder.toGrid();
            var str = HZincWriter.valToString(grid);
            Assert.AreEqual("ver:\"3.0\"\nval,inner\n\"value\",<<\nver:\"3.0\"\nval,other\n\"value\",10\n>>", str.Trim());
        }

        [TestMethod]
        public void gridToString_InnerGrid()
        {
            var builder = new HGridBuilder();
            builder.addCol("val");
            builder.addCol("other");
            builder.addRow(new HVal[] { HStr.make("value"), HNum.make(10) });
            var grid = builder.toGrid();
            builder = new HGridBuilder();
            builder.addCol("val");
            builder.addCol("inner");
            builder.addRow(new HVal[] { HStr.make("value"), grid });
            grid = builder.toGrid();
            var str = HZincWriter.gridToString(grid);
            Assert.AreEqual("ver:\"3.0\"\nval,inner\n\"value\",<<\nver:\"3.0\"\nval,other\n\"value\",10\n>>", str.Trim());
        }

        [TestMethod]
        public void gridToString_InnerGrids()
        {
            var builder = new HGridBuilder();
            builder.addCol("val");
            builder.addCol("other");
            builder.addRow(new HVal[] { HStr.make("value"), HNum.make(10) });
            var grid = builder.toGrid();
            builder = new HGridBuilder();
            builder.addCol("val");
            builder.addCol("inner");
            builder.addRow(new HVal[] { HStr.make("value"), grid });
            builder.addRow(new HVal[] { HStr.make("value"), grid });
            grid = builder.toGrid();
            var str = HZincWriter.gridToString(grid);
            Assert.AreEqual("ver:\"3.0\"\nval,inner\n\"value\",<<\nver:\"3.0\"\nval,other\n\"value\",10\n>>\n\"value\",<<\nver:\"3.0\"\nval,other\n\"value\",10\n>>", str.Trim());
        }

        [TestMethod]
        public void gridToString_NestedGrids()
        {
            var builder = new HGridBuilder();
            builder.addCol("val");
            builder.addCol("other");
            builder.addRow(new HVal[] { HStr.make("value"), HNum.make(10) });
            var grid = builder.toGrid();
            for (var i = 0; i < 2; i++)
            {
                builder = new HGridBuilder();
                builder.addCol("val");
                builder.addCol("inner");
                builder.addRow(new HVal[] { HStr.make("value"), grid });
                grid = builder.toGrid();
            }
            var str = HZincWriter.gridToString(grid);
            Assert.AreEqual("ver:\"3.0\"\nval,inner\n\"value\",<<\nver:\"3.0\"\nval,inner\n\"value\",<<\nver:\"3.0\"\nval,other\n\"value\",10\n>>\n>>", str.Trim());
        }

        ///////////////////////////////////////////////////////////////////i///////
        // Utils
        //////////////////////////////////////////////////////////////////////////

        void verifyGrid(string str, HDict meta, object[] cols, HVal[][] rows)
        {
            /*
            System.out.println();
            System.out.println("###############################################");
            System.out.println();
            System.out.println(str);
            */

            // normalize nulls
            if (meta == null) meta = HDict.Empty;
            for (int i = 0; i < cols.Length; ++i)
                if (cols[i] == null) cols[i] = HDict.Empty;

            // read from zinc
            HGrid grid = new HZincReader(str).readGrid();
            verifyGridEq(grid, meta, cols, rows);

            // write grid and verify we can parse that too
            string writeStr = HZincWriter.gridToString(grid);
            HGrid writeGrid = new HZincReader(writeStr).readGrid();
            verifyGridEq(writeGrid, meta, cols, rows);
        }

        void verifyGridEq(HGrid grid, HDict meta, Object[] cols, HVal[][] rows)
        {
            // meta
            Assert.IsTrue(grid.meta.hequals(meta));

            // cols
            Assert.AreEqual(grid.numCols, cols.Length / 2);
            for (int i = 0; i < grid.numCols; ++i)
            {
                Assert.AreEqual(grid.col(i).Name, cols[i * 2 + 0]);
                Assert.IsTrue(grid.col(i).meta.hequals(cols[i * 2 + 1]));
            }

            // rows
            Assert.AreEqual(grid.numRows, rows.Length);
            for (int ri = 0; ri < rows.Length; ++ri)
            {
                HVal[] expected = rows[ri];
                HRow actual = grid.row(ri);
                for (int ci = 0; ci < expected.Length; ++ci)
                {
                    Assert.AreEqual(expected[ci], actual.get(grid.col(ci).Name, false));
                }
            }
        }

    }
}
