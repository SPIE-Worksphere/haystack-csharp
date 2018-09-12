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
    public class HTzTest
    {
        [TestMethod]
        public void testTz()
        {
            verifyTz("New_York", "Eastern Standard Time");
            verifyTz("Chicago", "Central Standard Time");
            verifyTz("Phoenix", "US Mountain Standard Time");
            verifyTz("London", "GMT Standard Time");
            verifyTz("UTC", "UTC");
            verifyTz("GMT", "Arabian Standard Time");
            verifyTz("Rel", "Arabian Standard Time"); // GMT
        }

        private void verifyTz(string name, string dntzId)
        {
            HTimeZone tz = HTimeZone.make(name, false);
            TimeZoneInfo dntz = tz.dntz;
            Assert.AreEqual(tz.ToString(), name);
            Assert.AreEqual(dntz.Id, dntzId);
            try
            {
                TimeZoneInfo dntzByID = TimeZoneInfo.FindSystemTimeZoneById(dntzId);
                Assert.IsTrue(tz.hequals(HTimeZone.make(dntzByID, false)));
            }
            catch (Exception)
            {
                Assert.Fail();
            }
            
        }
    }
}
