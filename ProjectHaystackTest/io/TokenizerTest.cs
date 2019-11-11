//
// Copyright (c) 2018
// Licensed under the Academic Free License version 3.0
//
// History:
//   16 August 2018 Ian Davies Creation based on Java Toolkit at same time from project-haystack.org downloads
//
using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectHaystack;
using ProjectHaystack.io;


namespace ProjectHaystackTest.io
{
    [TestClass]
    public class TokenizerTest
    {
        [TestMethod]
        public void testEmpty()
        {
            verifyToks("", new object[] {});
        }

        [TestMethod]
        public void testId()
        {
            HaystackToken id = HaystackToken.id;
            verifyToks("x", new object[] { id, "x" });
            verifyToks("fooBar", new object[] { id, "fooBar" });
            verifyToks("fooBar1999x", new object[] { id, "fooBar1999x" });
            verifyToks("foo_23", new object[] { id, "foo_23" });
            verifyToks("Foo", new object[] { id, "Foo" });
        }

        [TestMethod]
        public void testInts()
        {
            HaystackToken num = HaystackToken.num;
            verifyToks("5", new object[] { num, n(5) });
            verifyToks("0x1234_abcd", new object[] { num, n(0x1234abcd) });
        }

        [TestMethod]
        public void testFloats()
        {
            HaystackToken num = HaystackToken.num;
            verifyToks("5.0", new object[] { num, n(5d) });
            verifyToks("5.42", new object[] { num, n(5.42d) });
            verifyToks("123.2e32", new object[] { num, n(123.2e32d) });
            verifyToks("123.2e+32", new object[] { num, n(123.2e32d) });
            verifyToks("2_123.2e+32", new object[] { num, n(2123.2e32d) });
            verifyToks("4.2e-7", new object[] { num, n(4.2e-7d) });
        }

        [TestMethod]
        public void testNumberWithUnits()
        {
            HaystackToken num = HaystackToken.num;
            verifyToks("-40ms", new object[] { num, n(-40, "ms") });
            verifyToks("1sec", new object[] { num, n(1, "sec") });
            verifyToks("5hr", new object[] { num, n(5, "hr") });
            verifyToks("2.5day", new object[] { num, n(2.5d, "day") });
            verifyToks("12%", new object[] { num, n(12, "%") });
            verifyToks("987_foo", new object[] { num, n(987, "_foo") });
            verifyToks("-1.2m/s", new object[] { num, n(-1.2d, "m/s") });
            verifyToks("12kWh/ft\u00B2", new object[] { num, n(12, "kWh/ft\u00B2") });
            verifyToks("3_000.5J/kg_dry", new object[] { num, n(3000.5d, "J/kg_dry") });
        }

        [TestMethod]
        public void testStrings()
        {
            HaystackToken str = HaystackToken.str;
            verifyToks("\"\"", new object[] { str, HStr.make("") });
            verifyToks("\"x y\"", new object[] { str, HStr.make("x y") });
            verifyToks("\"x\\\"y\"", new object[] { str, HStr.make("x\"y") });
            verifyToks("\"_\\u012f \\n \\t \\\\_\"", new object[] { str, HStr.make("_\u012f \n \t \\_") });
        }

        [TestMethod]
        public void testDate()
        {
            HaystackToken date = HaystackToken.date;
            verifyToks("2016-06-06", new object[] { date, HDate.make(2016, 6, 6) });
        }

        [TestMethod]
        public void testTime()
        {
            HaystackToken time = HaystackToken.time;
            verifyToks("8:30", new object[] { time, HTime.make(8, 30) });
            verifyToks("20:15", new object[] { time, HTime.make(20, 15) });
            verifyToks("00:00", new object[] { time, HTime.make(0, 0) });
            verifyToks("00:00:00", new object[] { time, HTime.make(0, 0, 0) });
            verifyToks("01:02:03", new object[] { time, HTime.make(1, 2, 3) });
            verifyToks("23:59:59", new object[] { time, HTime.make(23, 59, 59) });
            verifyToks("12:00:12.9", new object[] { time, HTime.make(12, 00, 12, 900) });
            verifyToks("12:00:12.99", new object[] { time, HTime.make(12, 00, 12, 990) });
            verifyToks("12:00:12.999", new object[] { time, HTime.make(12, 00, 12, 999) });
            verifyToks("12:00:12.000", new object[] { time, HTime.make(12, 00, 12, 0) });
            verifyToks("12:00:12.001", new object[] { time, HTime.make(12, 00, 12, 1) });
        }

        [TestMethod]
        public void testDateTime()
        {
            HaystackToken dt = HaystackToken.dateTime;
            HTimeZone ny = HTimeZone.make("New_York", false);
            HTimeZone utc = HTimeZone.UTC;
            HTimeZone london = HTimeZone.make("London", false);
            // Ignore issues with locally installed timezones.
            if (ny != null)
            {
                verifyToks("2016-01-13T09:51:33-05:00 New_York", new object[] { dt, HDateTime.make(2016, 1, 13, 9, 51, 33, ny/*, tzOffset(-5, 0)*/) });
                verifyToks("2016-01-13T09:51:33.353-05:00 New_York", new object[] { dt, HDateTime.make(HDate.make(2016, 1, 13), HTime.make(9, 51, 33, 353), ny/*, tzOffset(-5, 0)*/) });
            }
            verifyToks("2010-12-18T14:11:30.924Z", new object[] { dt, HDateTime.make(HDate.make(2010, 12, 18), HTime.make(14, 11, 30, 924), utc) });
            verifyToks("2010-12-18T14:11:30.924Z UTC", new object[] { dt, HDateTime.make(HDate.make(2010, 12, 18), HTime.make(14, 11, 30, 924), utc) });
            // Ignore issues with locally installed timezones.
            if (london != null)
                verifyToks("2010-12-18T14:11:30.924Z London", new object[] { dt, HDateTime.make(HDate.make(2010, 12, 18), HTime.make(14, 11, 30, 924), london) });
            // Apparently PST8PDT is not valid in java? - Not tested for windows either
            //    verifyToks("2015-01-02T06:13:38.701-08:00 PST8PDT", new Object[] {dt, HDateTime.make(HDate.make(2015,1,2), HTime.make(6,13,38,701), HTimeZone.make("PST8PDT"), tzOffset(-8,0))});
            var tz = HTimeZone.make("GMT+5", false);
            // Ignore issues with locally installed timezones.
            if (tz != null)
                verifyToks("2010-03-01T23:55:00.013-05:00 GMT+5", new object[] { dt, HDateTime.make(HDate.make(2010, 3, 1), HTime.make(23, 55, 0, 13), tz/*, tzOffset(-5, 0)*/) });
            tz = HTimeZone.make("GMT-10", false);
            // Ignore issues with locally installed timezones.
            if (tz != null)
                verifyToks("2010-03-01T23:55:00.013+10:00 GMT-10 ", new object[] { dt, HDateTime.make(HDate.make(2010, 3, 1), HTime.make(23, 55, 0, 13), tz/*, tzOffset(10, 0)*/) });
        }

        [TestMethod]
        public void testRef()
        {
            HaystackToken hrefVal = HaystackToken.refh;
            verifyToks("@125b780e-0684e169", new object[] {hrefVal, HRef.make("125b780e-0684e169") });
            verifyToks("@demo:_:-.~", new object[] { hrefVal, HRef.make("demo:_:-.~") });
        }

        [TestMethod]
        public void testUri()
        {
            HaystackToken uri = HaystackToken.uri;
            verifyToks("`http://foo/`", new object[] { uri, HUri.make("http://foo/") });
            verifyToks("`_ \\n \\\\ \\`_`", new object[] { uri, HUri.make("_ \n \\\\ `_") });
        }

        [TestMethod]
        public void testWhitespace()
        {

            HaystackToken id = HaystackToken.id;
            verifyToks("a\n  b   \rc \r\nd\n\ne",
              new object[] {
                id, "a", HaystackToken.nl, null,
                id, "b", HaystackToken.nl, null,
                id, "c", HaystackToken.nl, null,
                id, "d", HaystackToken.nl, null, HaystackToken.nl, null,
                id, "e"
              });
        }

        private HNum n(long val) { return HNum.make(val); }
        private HNum n(double val) { return HNum.make(val); }
        private HNum n(double val, String unit) { return HNum.make(val, unit); }
        private int tzOffset(int hours, int mins) { return (hours * 3600) + (mins * 60); }

        private void verifyToks(string zinc, object[] toks)
        {
            List<object> acc = new List<object>();
            // Streamreader thing
            MemoryStream msRdr = new MemoryStream(Encoding.UTF8.GetBytes(zinc));
            HaystackTokenizer t = new HaystackTokenizer(new StreamReader(msRdr));
            while (true)
            {
                HaystackToken x = t.next();
                Assert.AreEqual(x, t.Token);
                if (x == HaystackToken.eof) break;
                acc.Add(t.Token);
                acc.Add(t.Val);
            }
            object[] actual = acc.ToArray();
            // NOTE: this requires higher than .NET 3.5 for SequenceEqual
            // SequenceEqual will not work for this
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
                    else if (toks[iIndex].ToString().CompareTo(actual[iIndex].ToString()) != 0)
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
                    toksStr += "[" + tok.ToString() + "]";
                    first = false;
                }
                first = true;
                foreach (object tok in actual)
                {
                    if (!first) accStr += ",";
                    accStr += "[" + tok.ToString() + "]";
                    first = false;
                }
                
                // Sending it to debug and trace - vs2013 and vs2017 change the use of testcontext making
                //   TestContext.writeline not a reliable output
                Debug.WriteLine("expected: " + toksStr);
                Trace.WriteLine("expected: " + toksStr);
                Debug.WriteLine("actual:   " + accStr);
                Trace.WriteLine("actual:   " + accStr);
            }
        }
    }
}
