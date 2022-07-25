using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectHaystack;
using ProjectHaystack.io;

namespace ProjectHaystackTest.io
{
    [TestClass]
    public class ZincWriterTests
    {
        [TestMethod]
        public void TestNullGridMetaAndColMeta()
        {
            var meta = new HaystackDictionary();
            meta.AddMarker("tag");
            var col0Meta = new HaystackDictionary();
            var col1Meta = new HaystackDictionary();
            col1Meta.AddMarker("markermetatag");
            VerifyGrid("ver:\"3.0\" tag:M\na nullmetatag:N, b markermetatag\n",
                meta,
                new object[] { "a", col0Meta, "b", col1Meta },
                new HaystackValue[0][]
            );
        }

        [TestMethod]
        public void ValToString_InnerGrid()
        {
            var innerGrid = new HaystackGrid()
                .AddColumn("val")
                .AddColumn("other")
                .AddRow(new HaystackString("value"), new HaystackNumber(10));
            var grid = new HaystackGrid()
                .AddColumn("val")
                .AddColumn("inner")
                .AddRow(new HaystackString("value"), innerGrid);
            var str = ZincWriter.ToZinc(grid);
            Assert.AreEqual("ver:\"3.0\"\nval,inner\n\"value\",<<\nver:\"3.0\"\nval,other\n\"value\",10\n>>", str.Trim());
        }

        [TestMethod]
        public void GridToString_InnerGrid()
        {
            var innerGrid = new HaystackGrid()
                .AddColumn("val")
                .AddColumn("other")
                .AddRow(new HaystackString("value"), new HaystackNumber(10));
            var grid = new HaystackGrid()
                .AddColumn("val")
                .AddColumn("inner")
                .AddRow(new HaystackString("value"), innerGrid);
            var str = ZincWriter.ToZinc(grid);
            Assert.AreEqual("ver:\"3.0\"\nval,inner\n\"value\",<<\nver:\"3.0\"\nval,other\n\"value\",10\n>>", str.Trim());
        }

        [TestMethod]
        public void GridToString_InnerGrids()
        {
            var innerGrid = new HaystackGrid()
                .AddColumn("val")
                .AddColumn("other")
                .AddRow(new HaystackString("value"), new HaystackNumber(10));
            var grid = new HaystackGrid()
                .AddColumn("val")
                .AddColumn("inner")
                .AddRow(new HaystackString("value"), innerGrid)
                .AddRow(new HaystackString("value"), innerGrid);
            var str = ZincWriter.ToZinc(grid);
            Assert.AreEqual("ver:\"3.0\"\nval,inner\n\"value\",<<\nver:\"3.0\"\nval,other\n\"value\",10\n>>\n\"value\",<<\nver:\"3.0\"\nval,other\n\"value\",10\n>>", str.Trim());
        }

        [TestMethod]
        public void GridToString_NestedGrids()
        {
            var grid = new HaystackGrid()
                .AddColumn("val")
                .AddColumn("other")
                .AddRow(new HaystackString("value"), new HaystackNumber(10));
            for (var i = 0; i < 2; i++)
            {
                grid = new HaystackGrid()
                    .AddColumn("val")
                    .AddColumn("inner")
                    .AddRow(new HaystackString("value"), grid);
            }
            var str = ZincWriter.ToZinc(grid);
            Assert.AreEqual("ver:\"3.0\"\nval,inner\n\"value\",<<\nver:\"3.0\"\nval,inner\n\"value\",<<\nver:\"3.0\"\nval,other\n\"value\",10\n>>\n>>", str.Trim());
        }

        [TestMethod]
        public void GridToString_NullColumn()
        {
            var grid = new HaystackGrid()
                .AddColumn("val")
                .AddColumn("null")
                .AddRow(new HaystackString("value"), null);
            var str = ZincWriter.ToZinc(grid);
            Assert.AreEqual("ver:\"3.0\"\nval,null\n\"value\",N", str.Trim());
        }

        [TestMethod]
        public void TestDate()
        {
            VerifyZinc(new HaystackDate(2011, 6, 7), "2011-06-07");
            VerifyZinc(new HaystackDate(2011, 10, 10), "2011-10-10");
            VerifyZinc(new HaystackDate(2011, 12, 31), "2011-12-31");
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void TestBadDate()
        {
            string[] badDateZinc =
            {
                "2003-xx-02",
                "2003-02",
                "2003-02-xx"
            };
            foreach (string strCurZinc in badDateZinc)
            {
                ZincReader.ReadValue(strCurZinc);
            }
        }

        [TestMethod]
        public void TestDefinition()
        {
            VerifyZinc(new HaystackDefinition("^testDef"), "^testDef");
        }

        [TestMethod]
        public void TestCaretSymbol()
        {
            VerifyZinc(new HaystackCaretSymbol("test-symbol"), "^test-symbol");
        }

        void VerifyGrid(string str, HaystackDictionary meta, object[] cols, HaystackValue[][] rows)
        {
            // normalize nulls
            if (meta == null) meta = new HaystackDictionary();
            for (int i = 0; i < cols.Length; ++i)
            {
                if (cols[i] == null) cols[i] = new HaystackDictionary();
            }

            // read from zinc
            var grid = new ZincReader(str).ReadValue<HaystackGrid>();
            VerifyGridEquals(grid, meta, cols, rows);

            // write grid and verify we can parse that too
            string writeStr = ZincWriter.ToZinc(grid);
            var writeGrid = new ZincReader(writeStr).ReadValue<HaystackGrid>();
            VerifyGridEquals(writeGrid, meta, cols, rows);
        }

        void VerifyGridEquals(HaystackGrid grid, HaystackDictionary meta, object[] cols, HaystackValue[][] rows)
        {
            // meta
            Assert.IsTrue(grid.Meta.Equals(meta));

            // cols
            Assert.AreEqual(grid.ColumnCount, cols.Length / 2);
            for (int i = 0; i < grid.ColumnCount; ++i)
            {
                Assert.AreEqual(grid.Column(i).Name, cols[i * 2 + 0]);
                Assert.IsTrue(grid.Column(i).Meta.Equals(cols[i * 2 + 1]));
            }

            // rows
            Assert.AreEqual(grid.RowCount, rows.Length);
            for (int ri = 0; ri < rows.Length; ++ri)
            {
                var expected = rows[ri];
                var actual = grid.Row(ri);
                for (int ci = 0; ci < expected.Length; ++ci)
                {
                    Assert.AreEqual(expected[ci], actual[grid.Column(ci).Name]);
                }
            }
        }

        [TestMethod]
        public void TestXString()
        {
            VerifyZinc(new HaystackXString("hello", "Type"), "Type(\"hello\")");
            VerifyZinc(new HaystackXString("\u0abc", "Type"), "Type(\"\u0abc\")");
            Assert.IsTrue(ZincReader.ReadValue<HaystackXString>("Type(\"[\\u004e \\u0041]\")").Equals(new HaystackXString("[\u004e \u0041]", "Type")));
            Assert.IsTrue(ZincReader.ReadValue<HaystackXString>("Type(\"[\\u004E \\u0041]\")").Equals(new HaystackXString("[\u004E \u0041]", "Type")));
        }

        [TestMethod]
        public void TestUri()
        {
            VerifyZinc(new HaystackUri("http://foo.com/f?q"), "`http://foo.com/f?q`");
            VerifyZinc(new HaystackUri("a$b"), "`a$b`");
            VerifyZinc(new HaystackUri("a`b"), "`a\\`b`");
            VerifyZinc(new HaystackUri("http\\:a\\?b"), "`http\\:a\\?b`");
            VerifyZinc(new HaystackUri("\u01ab.txt"), "`\u01ab.txt`");
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void TestBadUri()
        {
            string[] badZincs = new string[]
            {
                "`no end",
                "`new\nline`"
            };
            foreach (string zinc in badZincs)
            {
                ZincReader.ReadValue(zinc);
            }
        }

        [TestMethod]
        public void TestTime()
        {
            VerifyZinc(new HaystackTime(new TimeSpan(0, 2, 3, 4)), "02:03:04");
            VerifyZinc(new HaystackTime(new TimeSpan(0, 2, 3, 4, 5)), "02:03:04.005");
            VerifyZinc(new HaystackTime(new TimeSpan(0, 2, 3, 4, 56)), "02:03:04.056");
            VerifyZinc(new HaystackTime(new TimeSpan(0, 2, 3, 4, 109)), "02:03:04.109");
            VerifyZinc(new HaystackTime(new TimeSpan(0, 2, 3, 10, 109)), "02:03:10.109");
            VerifyZinc(new HaystackTime(new TimeSpan(0, 2, 10, 59)), "02:10:59");
            VerifyZinc(new HaystackTime(new TimeSpan(0, 10, 59, 30)), "10:59:30");
            VerifyZinc(new HaystackTime(new TimeSpan(0, 23, 59, 59, 999)), "23:59:59.999");
            VerifyZinc(new HaystackTime(new TimeSpan(0, 3, 20, 0)), "03:20:00");
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void TestBadTime()
        {
            string[] badZincs = new string[]
            {
              "13:xx:00",
              "13:45:0x",
              "13:45:00.",
              "13:45:00.x"
            };
            foreach (string zinc in badZincs)
            {
                ZincReader.ReadValue(zinc);
            }
        }

        [TestMethod]
        public void TestString()
        {
            VerifyZinc(new HaystackString("hello"), "\"hello\"");
            VerifyZinc(new HaystackString("_ \\ \" \n \r \t \u0011 _"), "\"_ \\\\ \\\" \\n \\r \\t \\u0011 _\"");
            VerifyZinc(new HaystackString("\u0abc"), "\"\u0abc\"");
            Assert.IsTrue(ZincReader.ReadValue("\"[\\u004e \\u0041]\"").Equals(new HaystackString("[\u004e \u0041]")));
            Assert.IsTrue(ZincReader.ReadValue("\"[\\u004E \\u0041]\"").Equals(new HaystackString("[\u004E \u0041]")));
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void TestStringNoEndQuote()
        {
            ZincReader.ReadValue("\"end...");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestBadStringUnicodeEsc()
        {
            ZincReader.ReadValue("\"\\u1x34\"");
        }

        [TestMethod]
        public void TestRow()
        {
            VerifyZinc(
                new HaystackRow(new HaystackGrid()),
                "{}");
            VerifyZinc(
                HaystackRowTests.BuildRows(new[] { "fooBar" }, new[] { new HaystackNumber(123, "ft") }).First(),
                "{fooBar:123ft}");
            VerifyZinc(
                HaystackRowTests.BuildRows(new[] { "dis", "bday", "marker" }, new HaystackValue[] { new HaystackString("Bob"), new HaystackDate(1970, 6, 3), new HaystackMarker() }).First(),
                "{dis:\"Bob\" bday:1970-06-03 marker}");

            // nested dict
            VerifyZinc(
                HaystackRowTests.BuildRows(new[] { "auth" }, new HaystackValue[] { new HaystackDictionary() }).First(),
                "{auth:{}}");
            VerifyZinc(
                HaystackRowTests.BuildRows(new[] { "auth" }, HaystackRowTests.BuildRows(new[] { "alg", "c", "marker" }, new HaystackValue[] { new HaystackString("scram"), new HaystackNumber(10000), new HaystackMarker() }).ToArray()).First(),
                "{auth:{alg:\"scram\" c:10000 marker}}");

            // nested list
            VerifyZinc(
                HaystackRowTests.BuildRows(new[] { "arr", "x" }, new HaystackValue[] { new HaystackList(new HaystackNumber(1.0), new HaystackNumber(2), new HaystackNumber(3)), new HaystackMarker() }).First(),
                "{arr:[1,2,3] x}");
        }

        [TestMethod]
        public void TestReference()
        {
            VerifyZinc(new HaystackReference("1234-5678.foo:bar"), "@1234-5678.foo:bar");
            VerifyZinc(new HaystackReference("1234-5678", "Foo Bar"), "@1234-5678 \"Foo Bar\"");
            VerifyZinc(new HaystackReference("1234-5678", "Foo \"Bar\""), "@1234-5678 \"Foo \\\"Bar\\\"\"");
        }

        [TestMethod]
        public void TestNumber()
        {
            VerifyZinc(new HaystackNumber(123), "123");
            VerifyZinc(new HaystackNumber(123.4, "m/s"), "123.4m/s");
            VerifyZinc(new HaystackNumber(9.6, "m/s"), "9.6m/s");
            VerifyZinc(new HaystackNumber(-5.2, "\u00b0F"), "-5.2\u00b0F");
            VerifyZinc(new HaystackNumber(23, "%"), "23%");
            VerifyZinc(new HaystackNumber(2.4e-3, "fl_oz"), "0.0024fl_oz");
            VerifyZinc(new HaystackNumber(2.4e5, "$"), "240000$");
            Assert.IsTrue(ZincReader.ReadValue("1234.56fl_oz").Equals(new HaystackNumber(1234.56, "fl_oz")));
            Assert.IsTrue(ZincReader.ReadValue("0.000028fl_oz").Equals(new HaystackNumber(0.000028, "fl_oz")));

            // specials
            VerifyZinc(new HaystackNumber(double.NegativeInfinity), "-INF");
            VerifyZinc(new HaystackNumber(double.PositiveInfinity), "INF");
            VerifyZinc(new HaystackNumber(double.NaN), "NaN");

            // verify units never serialized for special values
            Assert.AreEqual(ZincWriter.ToZinc(new HaystackNumber(double.NaN, "ignore")), "NaN");
            Assert.AreEqual(ZincWriter.ToZinc(new HaystackNumber(double.PositiveInfinity, "%")), "INF");
            Assert.AreEqual(ZincWriter.ToZinc(new HaystackNumber(double.NegativeInfinity, "%")), "-INF");
        }

        [TestMethod]
        public void TestNumberFormatDecimalWithDot()
        {
            string defaultLanguage = CultureInfo.InvariantCulture.ToString();
            Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");
            VerifyZinc(new HaystackNumber(2.4), "2.4");
            Thread.CurrentThread.CurrentCulture = new CultureInfo(defaultLanguage);
        }

        [TestMethod]
        public void TestMarker()
        {
            VerifyZinc(new HaystackMarker(), "M");
        }

        [TestMethod]
        public void TestList()
        {
            VerifyZinc(new HaystackList(), "[]");
        }

        [TestMethod]
        public void TestBoolean()
        {
            VerifyZinc(new HaystackBoolean(true), "T");
            VerifyZinc(new HaystackBoolean(false), "F");
        }

        [TestMethod]
        public void TestDateTime()
        {
            TimeZoneInfo tziNewYork;
            try
            {
                tziNewYork = TimeZoneConverter.TZConvert.GetTimeZoneInfo("Eastern Standard Time");
            }
            catch
            {
                // Ignore issues with locally installed timezones.
                return;
            }
            var tz = new HaystackTimeZone("New_York");
            HaystackDateTime ts;
            ts = new HaystackDateTime(new DateTime(634429600180690000L), tz);
            VerifyZinc(ts, "2011-06-06T12:26:58.069-05:00 New_York");
            Assert.AreEqual(ts.TimeZone.Name, "New_York");
            Assert.AreEqual(ts.TimeZone.TimeZoneInfo, tziNewYork);

            // convert back to millis
            ts = ZincReader.ReadValue<HaystackDateTime>("2011-06-06T12:26:58.069-04:00 New_York");
            Assert.AreEqual(ts.Value.Ticks, 634429600180690000L);

            // different timezones
            ts = new HaystackDateTime(new DateTime(630850574400000000L), new HaystackTimeZone("New_York"));
            VerifyZinc(ts, "2000-02-02T03:04:00-05:00 New_York");
            ts = new HaystackDateTime(new DateTime(630850754400000000L), new HaystackTimeZone("UTC"));
            VerifyZinc(ts, "2000-02-02T08:04:00Z UTC");
            ts = new HaystackDateTime(new DateTime(630851042400000000L), new HaystackTimeZone("Taipei"));
            VerifyZinc(ts, "2000-02-02T16:04:00+08:00 Taipei");
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void TestBadDateTime()
        {
            string[] badzincdt = new string[] {
                "2000-02-02T03:04:00-0x:00 New_York",
                 "2000-02-02T03:04:00-05 New_York",
                 "2000-02-02T03:04:00-05:!0 New_York",
                 "2000-02-02T03:04:00-05:00",
                "2000-02-02T03:04:00-05:00 @"
            };
            foreach (string zinc in badzincdt)
            {
                ZincReader.ReadValue(zinc);
            }
        }

        [TestMethod]
        public void TestZinc()
        {
            VerifyZinc(
              new HaystackDictionary(),
              "{}");
            VerifyZinc(
              new HaystackDictionary().AddMarker("foo_12"),
              "{foo_12}");
            VerifyZinc(
              new HaystackDictionary().AddNumber("fooBar", 123, "ft"),
              "{fooBar:123ft}");
            VerifyZinc(
              new HaystackDictionary().AddString("dis", "Bob").AddValue("bday", new HaystackDate(1970, 6, 3)).AddMarker("marker"),
              "{dis:\"Bob\" bday:1970-06-03 marker}");

            // nested dict
            VerifyZinc(
              new HaystackDictionary().AddValue("auth", new HaystackDictionary()),
              "{auth:{}}");
            VerifyZinc(
              new HaystackDictionary().AddValue("auth",
                new HaystackDictionary().AddString("alg", "scram").AddNumber("c", 10000).AddMarker("marker")
              ),
              "{auth:{alg:\"scram\" c:10000 marker}}");

            // nested list
            VerifyZinc(new HaystackDictionary()
                .AddValue("arr", new HaystackList(new HaystackNumber(1.0), new HaystackNumber(2), new HaystackNumber(3)))
                .AddMarker("x"),
                "{arr:[1,2,3] x}"); // Was "{arr:[1.0,2,3] x}" - double in .NET will not recognise the difference between 1.0 and 1
        }

        protected void VerifyZinc(HaystackValue val, string s)
        {
            Assert.AreEqual(s, ZincWriter.ToZinc(val));
            Assert.IsTrue(ZincReader.ReadValue(s).Equals(val));
        }
    }
}