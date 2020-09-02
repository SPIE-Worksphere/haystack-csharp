//
// Copyright (c) 2018
// Licensed under the Academic Free License version 3.0
//
// History:
//   16 August 2018 Ian Davies Creation based on Java Toolkit at same time from project-haystack.org downloads
//
using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectHaystack;

namespace ProjectHaystackTest
{
    [TestClass]
    public class HTzTest
    {
        [TestMethod]
        public void testTz_NewYork()
        {
            verifyTz("New_York", "Eastern Standard Time", "America/New_York");
        }

        [TestMethod]
        public void testTz_Chicago()
        {
            verifyTz("Chicago", "Central Standard Time", "America/Chicago");
        }

        [TestMethod]
        public void testTz_Phoenix()
        {
            verifyTz("Phoenix", "US Mountain Standard Time", "America/Phoenix");
        }

        [TestMethod]
        public void testTz_London()
        {
            verifyTz("London", "GMT Standard Time", "Europe/London");
        }

        [TestMethod]
        public void testTz_UTC()
        {
            verifyTz("UTC", "UTC", "Etc/UTC");
        }

        [TestMethod]
        public void testTz_GMT()
        {
            verifyTz("GMT", "UTC", "GMT");
        }

        [TestMethod]
        public void testTz_Rel()
        {
            verifyTz("Rel", "UTC", "GMT"); // GMT
        }

        private void verifyTz(string name, params string[] dntzIds)
        {
            HTimeZone tz = HTimeZone.make(name, false);
            // Ignore issues with locally installed timezones.
            if (tz == null)
                return;
            TimeZoneInfo dntz = tz.dntz;
            Assert.AreEqual(tz.ToString(), name);
            Assert.IsTrue(dntzIds.Contains(dntz.Id), $"{dntz.Id} not in [{string.Join(", ", dntzIds)}]");
            // TODO: What is this testing? Move into another test?
            //TimeZoneInfo dntzByID = TimeZoneConverter.TZConvert.GetTimeZoneInfo(dntz.Id);
            //Assert.IsTrue(tz.hequals(HTimeZone.make(dntzByID, false)), $"{tz} does not equal {dntzByID}");
        }
    }
}