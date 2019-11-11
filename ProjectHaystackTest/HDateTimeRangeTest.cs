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
    public class HDateTimeRangeTest
    {
        [TestMethod]
        public void testRange()
        {
            HTimeZone ny = HTimeZone.make("New_York", false);
            // Ignore issues with locally installed timezones.
            if (ny == null)
                return;
            HDate today = HDate.today();
            HDate yesterday = today.minusDays(1);
            HDate x = HDate.make(2011, 7, 4);
            HDate y = HDate.make(2011, 11, 4);
            HDateTime xa = HDateTime.make(x, HTime.make(2, 30), ny);
            HDateTime xb = HDateTime.make(x, HTime.make(22, 5), ny);

            verifyRange(HDateTimeRange.make("today", ny), today, today);
            verifyRange(HDateTimeRange.make("yesterday", ny), yesterday, yesterday);
            verifyRange(HDateTimeRange.make("2011-07-04", ny), x, x);
            verifyRange(HDateTimeRange.make("2011-07-04,2011-11-04", ny), x, y);
            verifyRange(HDateTimeRange.make("" + xa + "," + xb, ny), xa, xb);

            HDateTimeRange r = HDateTimeRange.make(xb.ToString(), ny);
            Assert.IsTrue(r.Start.hequals(xb));
            Assert.IsTrue(r.End.date.hequals(today));
            Assert.IsTrue(r.End.TimeZone.hequals(ny));

            // this week
            HDate sun = today;
            HDate sat = today;
            while (sun.weekday() > DayOfWeek.Sunday) sun = sun.minusDays(1);
            while (sat.weekday() < DayOfWeek.Saturday) sat = sat.plusDays(1);
            verifyRange(HDateTimeRange.thisWeek(ny), sun, sat);

            // this month
            HDate first = today;
            HDate last = today;
            while (first.Day > 1) first = first.minusDays(1);
            while (last.Day < DateTime.DaysInMonth(today.Year, today.Month)) last = last.plusDays(1);
            verifyRange(HDateTimeRange.thisMonth(ny), first, last);

            // this year
            first = HDate.make(today.Year, 1, 1);
            last = HDate.make(today.Year, 12, 31);
            verifyRange(HDateTimeRange.thisYear(ny), first, last);

            // last week
            HDate prev = today.minusDays(7);
            sun = prev;
            sat = prev;
            while (sun.weekday() > DayOfWeek.Sunday) sun = sun.minusDays(1);
            while (sat.weekday() < DayOfWeek.Saturday) sat = sat.plusDays(1);
            verifyRange(HDateTimeRange.lastWeek(ny), sun, sat);

            // last month
            last = today;
            while (last.Month == today.Month) last = last.minusDays(1);
            first = HDate.make(last.Year, last.Month, 1);
            verifyRange(HDateTimeRange.lastMonth(ny), first, last);

            // last year
            first = HDate.make(today.Year - 1, 1, 1);
            last = HDate.make(today.Year - 1, 12, 31);
            verifyRange(HDateTimeRange.lastYear(ny), first, last);
        }

        private void verifyRange(HDateTimeRange r, HDate start, HDate end)
        {
            Assert.IsTrue(r.Start.date.hequals(start));
            Assert.IsTrue(r.Start.time.hequals(HTime.MIDNIGHT));
            Assert.AreEqual(r.Start.TimeZone.ToString(), "New_York");
            Assert.IsTrue(r.End.date.hequals(end.plusDays(1)));
            Assert.IsTrue(r.End.time.hequals( HTime.MIDNIGHT));
            Assert.AreEqual(r.End.TimeZone.ToString(), "New_York");
        }

        private void verifyRange(HDateTimeRange r, HDateTime start, HDateTime end)
        {
            Assert.IsTrue(r.Start.hequals(start));
            Assert.IsTrue(r.End.hequals(end));
        }
    }
}
