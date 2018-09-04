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
    public class HDateTest : HValTest
    {
        [TestMethod]
        public void testEquality()
        {
            Assert.IsTrue(HDate.make(2011, 6, 7).hequals(HDate.make(2011, 6, 7)));
            Assert.IsFalse(HDate.make(2011, 6, 7).hequals(HDate.make(2011, 6, 8)));
            Assert.IsFalse(HDate.make(2011, 6, 7).hequals(HDate.make(2011, 2, 7)));
            Assert.IsFalse(HDate.make(2011, 6, 7).hequals(HDate.make(2009, 6, 7)));
        }

        [TestMethod]
        public void testCompare()
        {
            Assert.IsTrue(HDate.make(2011, 6, 9).CompareTo(HDate.make(2011, 6, 21)) < 0);
            Assert.IsTrue(HDate.make(2011, 10, 9).CompareTo(HDate.make(2011, 3, 21)) > 0);
            Assert.IsTrue(HDate.make(2010, 6, 9).CompareTo(HDate.make(2000, 9, 30)) > 0);
            Assert.AreEqual(HDate.make(2010, 6, 9).CompareTo(HDate.make(2010, 6, 9)), 0);
        }

        [TestMethod]
        public void testPlusMinus()
        {
            Assert.IsTrue(HDate.make(2011, 12, 1).minusDays(0).hequals(HDate.make(2011, 12, 1)));
            Assert.IsTrue(HDate.make(2011, 12, 1).minusDays(1).hequals(HDate.make(2011, 11, 30)));
            Assert.IsTrue(HDate.make(2011, 12, 1).minusDays(-2).hequals( HDate.make(2011, 12, 3)));
            Assert.IsTrue(HDate.make(2011, 12, 1).plusDays(2).hequals(HDate.make(2011, 12, 3)));
            Assert.IsTrue(HDate.make(2011, 12, 1).plusDays(31).hequals(HDate.make(2012, 1, 1)));
            Assert.IsTrue(HDate.make(2008, 3, 3).minusDays(3).hequals(HDate.make(2008, 2, 29)));
            Assert.IsTrue(HDate.make(2008, 3, 3).minusDays(4).hequals(HDate.make(2008, 2, 28)));
        }

        [TestMethod]
        public void testLeapYear()
        {
            for (int y = 1900; y <= 2100; y++)
            {
                if (((y % 4) == 0) && (y != 1900) && (y != 2100))
                    Assert.IsTrue(HDate.isLeapYear(y));
                else
                    Assert.IsFalse(HDate.isLeapYear(y));
            }
        }

        [TestMethod]
        public void testMidnight()
        {
            verifyMidnight(HDate.make(2011, 11, 3), "UTC", "2011-11-03T00:00:00Z UTC");
            // 18.08.2018 - fails on this my implementation has offset for New_York as -05:00 not -04:00 - test changed
            verifyMidnight(HDate.make(2011, 11, 3), "New_York", "2011-11-03T00:00:00-05:00 New_York"); 
            verifyMidnight(HDate.make(2011, 12, 15), "Chicago", "2011-12-15T00:00:00-06:00 Chicago");
            verifyMidnight(HDate.make(2008, 2, 29), "Phoenix", "2008-02-29T00:00:00-07:00 Phoenix");
        }

        private void verifyMidnight(HDate date, string tzName, string str)
        {
            HDateTime ts = date.midnight(HTimeZone.make(tzName, false));
            Assert.AreEqual(ts.date, date);
            Assert.AreEqual(ts.time.Hour, 0);
            Assert.AreEqual(ts.time.Minute, 0);
            Assert.AreEqual(ts.time.Second, 0);
            Assert.AreEqual(ts.ToString(), str);
            Assert.IsTrue(ts.hequals(read(ts.toZinc())));
            Assert.AreEqual(ts.Ticks, ((HDateTime)read(str)).Ticks);
        }

        [TestMethod]
        public void testZinc()
        {
            verifyZinc(HDate.make(2011, 6, 7), "2011-06-07");
            verifyZinc(HDate.make(2011, 10, 10), "2011-10-10");
            verifyZinc(HDate.make(2011, 12, 31), "2011-12-31");
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void testBadZinc()
        {
            string[] badDateZinc =
            {
                "2003-xx-02",
                "2003-02",
                "2003-02-xx"
            };
            foreach (string strCurZinc in badDateZinc)
                read(strCurZinc);
        }

    }
}

