using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectHaystack;
using ProjectHaystack.io;

namespace ProjectHaystackTest.io
{
    [TestClass]
    public class TokenizerTests
    {
        [TestMethod]
        public void TestEmpty()
        {
            VerifyToks("", new object[] { });
        }

        [TestMethod]
        public void TestId()
        {
            var id = HaystackToken.id;
            VerifyToks("x", new object[] { id, "x" });
            VerifyToks("fooBar", new object[] { id, "fooBar" });
            VerifyToks("fooBar1999x", new object[] { id, "fooBar1999x" });
            VerifyToks("foo_23", new object[] { id, "foo_23" });
            VerifyToks("Foo", new object[] { id, "Foo" });
            VerifyToks("^foo", new object[] { id, "^foo" });
        }

        [TestMethod]
        public void TestInts()
        {
            var num = HaystackToken.num;
            VerifyToks("5", new object[] { num, new HaystackNumber(5) });
            VerifyToks("0x1234_abcd", new object[] { num, new HaystackNumber(0x1234abcd) });
        }

        [TestMethod]
        public void TestFloats()
        {
            var num = HaystackToken.num;
            VerifyToks("5.0", new object[] { num, new HaystackNumber(5d) });
            VerifyToks("5.42", new object[] { num, new HaystackNumber(5.42d) });
            VerifyToks("123.2e32", new object[] { num, new HaystackNumber(123.2e32d) });
            VerifyToks("123.2e+32", new object[] { num, new HaystackNumber(123.2e32d) });
            VerifyToks("2_123.2e+32", new object[] { num, new HaystackNumber(2123.2e32d) });
            VerifyToks("4.2e-7", new object[] { num, new HaystackNumber(4.2e-7d) });
        }

        [TestMethod]
        public void TestNumberWithUnits()
        {
            var num = HaystackToken.num;
            VerifyToks("-40ms", new object[] { num, new HaystackNumber(-40, "ms") });
            VerifyToks("1sec", new object[] { num, new HaystackNumber(1, "sec") });
            VerifyToks("5hr", new object[] { num, new HaystackNumber(5, "hr") });
            VerifyToks("2.5day", new object[] { num, new HaystackNumber(2.5d, "day") });
            VerifyToks("12%", new object[] { num, new HaystackNumber(12, "%") });
            VerifyToks("987_foo", new object[] { num, new HaystackNumber(987, "_foo") });
            VerifyToks("-1.2m/s", new object[] { num, new HaystackNumber(-1.2d, "m/s") });
            VerifyToks("12kWh/ft\u00B2", new object[] { num, new HaystackNumber(12, "kWh/ft\u00B2") });
            VerifyToks("3_000.5J/kg_dry", new object[] { num, new HaystackNumber(3000.5d, "J/kg_dry") });
        }

        [TestMethod]
        public void TestStrings()
        {
            var str = HaystackToken.str;
            VerifyToks("\"\"", new object[] { str, new HaystackString("") });
            VerifyToks("\"x y\"", new object[] { str, new HaystackString("x y") });
            VerifyToks("\"x\\\"y\"", new object[] { str, new HaystackString("x\"y") });
            VerifyToks("\"_\\u012f \\n \\t \\\\_\"", new object[] { str, new HaystackString("_\u012f \n \t \\_") });
        }

        [TestMethod]
        public void TestDate()
        {
            var date = HaystackToken.date;
            VerifyToks("2016-06-06", new object[] { date, new HaystackDate(2016, 6, 6) });
        }

        [TestMethod]
        public void TestTime()
        {
            var time = HaystackToken.time;
            VerifyToks("00:00:00", new object[] { time, new HaystackTime(0, 0, 0) });
            VerifyToks("01:02:03", new object[] { time, new HaystackTime(1, 2, 3) });
            VerifyToks("23:59:59", new object[] { time, new HaystackTime(23, 59, 59) });
            VerifyToks("12:00:12.9", new object[] { time, new HaystackTime(new TimeSpan(0, 12, 00, 12, 900)) });
            VerifyToks("12:00:12.99", new object[] { time, new HaystackTime(new TimeSpan(0, 12, 00, 12, 990)) });
            VerifyToks("12:00:12.999", new object[] { time, new HaystackTime(new TimeSpan(0, 12, 00, 12, 999)) });
            VerifyToks("12:00:12.000", new object[] { time, new HaystackTime(new TimeSpan(0, 12, 00, 12, 0)) });
            VerifyToks("12:00:12.001", new object[] { time, new HaystackTime(new TimeSpan(0, 12, 00, 12, 1)) });
        }

        [TestMethod]
        public void TestDateTime()
        {
            var dt = HaystackToken.dateTime;
            var ny = new HaystackTimeZone("New_York");
            var utc = HaystackTimeZone.UTC;
            var london = new HaystackTimeZone("London");
            // Ignore issues with locally installed timezones.
            if (ny != null)
            {
                VerifyToks("2016-01-13T09:51:33-05:00 New_York", new object[] { dt, new HaystackDateTime(new DateTime(2016, 1, 13, 9, 51, 33), ny/*, tzOffset(-5, 0)*/) });
                VerifyToks("2016-01-13T09:51:33.353-05:00 New_York", new object[] { dt, new HaystackDateTime(new HaystackDate(2016, 1, 13), new HaystackTime(new TimeSpan(0, 9, 51, 33, 353)), ny/*, tzOffset(-5, 0)*/) });
            }
            VerifyToks("2010-12-18T14:11:30.924Z", new object[] { dt, new HaystackDateTime(new HaystackDate(2010, 12, 18), new HaystackTime(new TimeSpan(0, 14, 11, 30, 924)), utc) });
            VerifyToks("2010-12-18T14:11:30.924Z UTC", new object[] { dt, new HaystackDateTime(new HaystackDate(2010, 12, 18), new HaystackTime(new TimeSpan(0, 14, 11, 30, 924)), utc) });
            // Ignore issues with locally installed timezones.
            if (london != null)
                VerifyToks("2010-12-18T14:11:30.924Z London", new object[] { dt, new HaystackDateTime(new HaystackDate(2010, 12, 18), new HaystackTime(new TimeSpan(0, 14, 11, 30, 924)), london) });
            // Apparently PST8PDT is not valid in java? - Not Tested for windows either
            //    verifyToks("2015-01-02T06:13:38.701-08:00 PST8PDT", new Object[] {dt, new HaystackDateTime(new HaystackDate(2015,1,2), new HaystackTime(6,13,38,701), new HaystackTimeZone("PST8PDT"), tzOffset(-8,0))});
            var tz = new HaystackTimeZone("GMT+5");
            // Ignore issues with locally installed timezones.
            if (tz != null)
                VerifyToks("2010-03-01T23:55:00.013-05:00 GMT+5", new object[] { dt, new HaystackDateTime(new HaystackDate(2010, 3, 1), new HaystackTime(new TimeSpan(0, 23, 55, 0, 13)), tz/*, tzOffset(-5, 0)*/) });
            tz = new HaystackTimeZone("GMT-10");
            // Ignore issues with locally installed timezones.
            if (tz != null)
                VerifyToks("2010-03-01T23:55:00.013+10:00 GMT-10 ", new object[] { dt, new HaystackDateTime(new HaystackDate(2010, 3, 1), new HaystackTime(new TimeSpan(0, 23, 55, 0, 13)), tz/*, tzOffset(10, 0)*/) });
        }

        [TestMethod]
        public void TestRef()
        {
            HaystackToken hrefVal = HaystackToken.@ref;
            VerifyToks("@125b780e-0684e169", new object[] { hrefVal, new HaystackReference("125b780e-0684e169") });
            VerifyToks("@demo:_:-.~", new object[] { hrefVal, new HaystackReference("demo:_:-.~") });
        }

        [TestMethod]
        public void TestUri()
        {
            HaystackToken uri = HaystackToken.uri;
            VerifyToks("`http://foo/`", new object[] { uri, new HaystackUri("http://foo/") });
            VerifyToks("`_ \\n \\\\ \\`_`", new object[] { uri, new HaystackUri("_ \n \\\\ `_") });
        }

        [TestMethod]
        public void TestWhitespace()
        {

            HaystackToken id = HaystackToken.id;
            VerifyToks("a\n  b   \rc \r\nd\n\ne",
              new object[] {
                id, "a", HaystackToken.nl, null,
                id, "b", HaystackToken.nl, null,
                id, "c", HaystackToken.nl, null,
                id, "d", HaystackToken.nl, null, HaystackToken.nl, null,
                id, "e"
              });
        }

        [TestMethod]
        public void TestComplexString()
        {
            VerifyToks(
                @"{ id: ""1234:ab"", date: 2019-01-01T12:13:14Z UTC, num: 100ms, def: ^tagdef }",
                new object[] {
                    HaystackToken.lbrace, null,
                    HaystackToken.id, "id", HaystackToken.colon, null, HaystackToken.str, new HaystackString("1234:ab"),
                    HaystackToken.comma, null,
                    HaystackToken.id, "date", HaystackToken.colon, null, HaystackToken.dateTime, new HaystackDateTime(new DateTime(2019, 1, 1, 12, 13, 14), HaystackTimeZone.UTC),
                    HaystackToken.comma, null,
                    HaystackToken.id, "num", HaystackToken.colon, null, HaystackToken.num, new HaystackNumber(100, "ms"),
                    HaystackToken.comma, null,
                    HaystackToken.id, "def", HaystackToken.colon, null, HaystackToken.id, "^tagdef",
                    HaystackToken.rbrace, null,
                });
        }

        [TestMethod]
        public void TestConjunctDef()
        {
            VerifyToks(
                @"{ id: ""1234:ab"", def: ^chilled-water, is: ^water }",
                new object[] {
                    HaystackToken.lbrace, null,
                    HaystackToken.id, "id", HaystackToken.colon, null, HaystackToken.str, new HaystackString("1234:ab"),
                    HaystackToken.comma, null,
                    HaystackToken.id, "def", HaystackToken.colon, null, HaystackToken.caretSymbol, new HaystackCaretSymbol("chilled-water"),
                    HaystackToken.comma, null,
                    HaystackToken.id, "is", HaystackToken.colon, null, HaystackToken.caretSymbol, new HaystackCaretSymbol("water"),
                    HaystackToken.rbrace, null,
                });
        }


        private void VerifyToks(string zinc, object[] toks)
        {
            List<object> acc = new List<object>();
            // Streamreader thing
            MemoryStream msRdr = new MemoryStream(Encoding.UTF8.GetBytes(zinc));
            HaystackTokenizer t = new HaystackTokenizer(new StreamReader(msRdr));
            while (true)
            {
                HaystackToken x = t.Next();
                Assert.AreEqual(x, t.Token);
                if (x == HaystackToken.eof) break;
                acc.Add(t.Token);
                acc.Add(t.Val);
            }
            object[] actual = acc.ToArray();
            bool bEquals = true;
            if (toks.Length != actual.Length) bEquals = false;
            else
            {
                for (int iIndex = 0; iIndex < toks.Length; iIndex++)
                {
                    if (toks[iIndex] == null)
                    {
                        if (acc[iIndex] != null)
                        {
                            bEquals = false;
                            break;
                        }
                    }
                    else if (acc[iIndex] == null)
                    {
                        bEquals = false;
                        break;
                    }
                    else if (!toks[iIndex].Equals(actual[iIndex]))
                    {
                        bEquals = false;
                        break;
                    }
                }
            }
            if (!bEquals)
            {
                string toksStr = "";
                string accStr = "";
                bool first = true;
                foreach (object tok in toks)
                {
                    if (!first) toksStr += ",";
                    toksStr += "[" + tok?.ToString() + "]";
                    first = false;
                }
                first = true;
                foreach (object tok in actual)
                {
                    if (!first) accStr += ",";
                    accStr += "[" + tok?.ToString() + "]";
                    first = false;
                }

                // Sending it to debug and trace - vs2013 and vs2017 change the use of Testcontext making
                //   TestContext.writeline not a reliable output
                Debug.WriteLine("expected: " + toksStr);
                Trace.WriteLine("expected: " + toksStr);
                Debug.WriteLine("actual:   " + accStr);
                Trace.WriteLine("actual:   " + accStr);
            }
        }
    }
}
