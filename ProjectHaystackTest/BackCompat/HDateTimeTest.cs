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
    public class HDateTimeTest : HValTest
    {
        public static HTimeZone utc = HTimeZone.UTC;
        public static HTimeZone london = HTimeZone.make("London", false);

        [TestMethod]
        public void testEquality()
        {
            Assert.IsTrue(HDateTime.make(2011, 1, 2, 3, 4, 5, utc).hequals(HDateTime.make(2011, 1, 2, 3, 4, 5, utc)));
            Assert.IsFalse(HDateTime.make(2011, 1, 2, 3, 4, 5, utc).hequals(HDateTime.make(2009, 1, 2, 3, 4, 5, utc)));
            Assert.IsFalse(HDateTime.make(2011, 1, 2, 3, 4, 5, utc).hequals(HDateTime.make(2011, 9, 2, 3, 4, 5, utc)));
            Assert.IsFalse(HDateTime.make(2011, 1, 2, 3, 4, 5, utc).hequals(HDateTime.make(2011, 1, 9, 3, 4, 5, utc)));
            Assert.IsFalse(HDateTime.make(2011, 1, 2, 3, 4, 5, utc).hequals(HDateTime.make(2011, 1, 2, 9, 4, 5, utc)));
            Assert.IsFalse(HDateTime.make(2011, 1, 2, 3, 4, 5, utc).hequals(HDateTime.make(2011, 1, 2, 3, 9, 5, utc)));
            Assert.IsFalse(HDateTime.make(2011, 1, 2, 3, 4, 5, utc).hequals(HDateTime.make(2011, 1, 2, 3, 4, 9, utc)));
            // Ignore issues with locally installed timezones.
            if (london != null)
                Assert.IsFalse(HDateTime.make(2011, 1, 2, 3, 4, 5, utc).hequals(HDateTime.make(2011, 1, 2, 3, 4, 5, london)));
            //Assert.AreNotEqual(HDateTime.make(2011, 1, 2, 3, 4, 5, utc), HDateTime.make(2011, 1, 2, 3, 4, 5, london, 3600));
        }

        [TestMethod]
        public void testCompare()
        {
            Assert.AreEqual(HDateTime.make(2011, 1, 2, 3, 4, 5, utc).CompareTo(HDateTime.make(2011, 1, 2, 3, 4, 5, utc)), 0);
            Assert.IsTrue(HDateTime.make(2011, 1, 2, 3, 4, 5, utc).CompareTo(HDateTime.make(2011, 1, 2, 3, 4, 6, utc)) < 0);
            Assert.IsTrue(HDateTime.make(2011, 1, 2, 3, 4, 5, utc).CompareTo(HDateTime.make(2011, 1, 2, 3, 5, 5, utc)) < 0);
            Assert.IsTrue(HDateTime.make(2011, 1, 2, 3, 4, 5, utc).CompareTo(HDateTime.make(2011, 1, 2, 4, 4, 5, utc)) < 0);
            Assert.IsTrue(HDateTime.make(2011, 1, 2, 3, 4, 5, utc).CompareTo(HDateTime.make(2011, 1, 3, 3, 4, 5, utc)) < 0);
            Assert.IsTrue(HDateTime.make(2011, 1, 2, 3, 4, 5, utc).CompareTo(HDateTime.make(2011, 2, 2, 3, 4, 5, utc)) < 0);
            Assert.IsTrue(HDateTime.make(2011, 1, 2, 3, 4, 5, utc).CompareTo(HDateTime.make(2012, 1, 2, 3, 4, 5, utc)) < 0);
            Assert.IsTrue(HDateTime.make(2011, 1, 2, 3, 4, 5, utc).CompareTo(HDateTime.make(2011, 1, 2, 3, 4, 0, utc)) > 0);
        }

        [TestMethod]
        public void testZinc()
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
            var tz = HTimeZone.make("New_York", false);
            HDateTime ts;
            // Ignore issues with locally installed timezones.
            if (tz != null)
            {
                ts = HDateTime.make(634429600180690000L, tz);
                verifyZinc(ts, "2011-06-06T12:26:58.069-05:00 New_York");
                Assert.AreEqual(ts.date.ToString(), "2011-06-06");
                Assert.AreEqual(ts.time.ToString(), "12:26:58.069");
                Assert.AreEqual(ts.Offset.TotalSeconds, -5 * 60 * 60);
                Assert.AreEqual(ts.TimeZone.dntz, tziNewYork);
                Assert.AreEqual(ts.Ticks, 634429600180690000L);
            }

            // convert back to millis
            ts = HDateTime.make("2011-06-06T12:26:58.069-04:00 New_York", false);
            // Ignore issues with locally installed timezones.
            if (ts != null)
                Assert.AreEqual(ts.Ticks, 634429600180690000L);

            // different timezones
            ts = HDateTime.make(630850574400000000L, HTimeZone.make("New_York", false));
            // Ignore issues with locally installed timezones.
            if (ts != null)
                verifyZinc(ts, "2000-02-02T03:04:00-05:00 New_York");
            ts = HDateTime.make(630850754400000000L, HTimeZone.make("UTC", false));
            // Ignore issues with locally installed timezones.
            if (ts != null)
                verifyZinc(ts, "2000-02-02T08:04:00Z UTC");
            ts = HDateTime.make(630851042400000000L, HTimeZone.make("Taipei", false));
            // Ignore issues with locally installed timezones.
            if (ts != null)
            {
                verifyZinc(ts, "2000-02-02T16:04:00+08:00 Taipei");
                verifyZinc(HDateTime.make("2011-06-07T11:03:43-10:00 GMT+10", false),
                  "2011-06-07T11:03:43-10:00 GMT+10");
                verifyZinc(HDateTime.make("2011-06-08T04:07:33.771+07:00 GMT-7", false),
                  "2011-06-08T04:07:33.771+07:00 GMT-7");
            }
        }

        [TestMethod]
        public void testTicks()
        {
            HDate date = HDate.make(2014, 12, 24);
            HTime time = HTime.make(11, 12, 13, 456);
            HTimeZone newYork = HTimeZone.make("New_York", false);
            // Ignore issues with locally installed timezones.
            if (newYork == null)
                return;
            long utcTicks = 635550163334560000L; //635550163334560000

            HDateTime a = HDateTime.make(date, time, newYork);
            //  Not valid for us - HDateTime b = HDateTime.make(date, time, newYork, a.tzOffset);
            HDateTime c = HDateTime.make(utcTicks, newYork);
            HDateTime d = HDateTime.make("2014-12-24T11:12:13.456-05:00 New_York", false);

            Assert.AreEqual(a.Ticks, utcTicks);
            //Not Valid for us - Assert.AreEqual(b.millis(), utcTicks);
            Assert.AreEqual(c.Ticks, utcTicks);
            Assert.AreEqual(d.Ticks, utcTicks);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void testBadZinc()
        {
            string[] badzincdt = new string[] {
                "2000-02-02T03:04:00-0x:00 New_York",
                 "2000-02-02T03:04:00-05 New_York",
                 "2000-02-02T03:04:00-05:!0 New_York",
                 "2000-02-02T03:04:00-05:00",
                "2000-02-02T03:04:00-05:00 @"
            };
            foreach (string zinc in badzincdt)
                read(zinc);
        }

    }
}
