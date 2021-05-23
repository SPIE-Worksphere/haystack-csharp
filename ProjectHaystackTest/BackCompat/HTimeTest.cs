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
    public class HTimeTest : HValTest
    {
        [TestMethod]
        public void testEquality()
        {
            Assert.IsTrue(HTime.make(1, 2, 3, 4).hequals(HTime.make(1, 2, 3, 4)));
            Assert.IsFalse(HTime.make(1, 2, 3, 4).hequals(HTime.make(9, 2, 3, 4)));
            Assert.IsFalse(HTime.make(1, 2, 3, 4).hequals( HTime.make(1, 9, 3, 4)));
            Assert.IsFalse(HTime.make(1, 2, 3, 4).hequals(HTime.make(1, 2, 9, 9)));
        }

        [TestMethod]
        public void testCompare()
        {
            Assert.IsTrue(HTime.make(0, 0, 0, 0).CompareTo(HTime.make(0, 0, 0, 9)) < 0);
            Assert.IsTrue(HTime.make(0, 0, 0, 0).CompareTo(HTime.make(0, 0, 1, 0)) < 0);
            Assert.IsTrue(HTime.make(0, 1, 0, 0).CompareTo(HTime.make(0, 0, 0, 0)) > 0);
            Assert.IsTrue(HTime.make(0, 0, 0, 0).CompareTo(HTime.make(2, 0, 0, 0)) < 0);
            Assert.AreEqual(HTime.make(2, 0, 0, 0).CompareTo(HTime.make(2, 0, 0, 0)), 0);
        }

        [TestMethod]
        public void testZinc()
        {
            verifyZinc(HTime.make(2, 3), "02:03:00");
            verifyZinc(HTime.make(2, 3, 4), "02:03:04");
            verifyZinc(HTime.make(2, 3, 4, 5), "02:03:04.005");
            verifyZinc(HTime.make(2, 3, 4, 56), "02:03:04.056");
            verifyZinc(HTime.make(2, 3, 4, 109), "02:03:04.109");
            verifyZinc(HTime.make(2, 3, 10, 109), "02:03:10.109");
            verifyZinc(HTime.make(2, 10, 59), "02:10:59");
            verifyZinc(HTime.make(10, 59, 30), "10:59:30");
            verifyZinc(HTime.make(23, 59, 59, 999), "23:59:59.999");
            verifyZinc(HTime.make(3, 20, 0), "03:20:00");
            Assert.IsTrue(HTime.make("10:04:19.181511").hequals( HTime.make(10, 04, 19, 181)));
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void testBadZinc()
        {
            string[] badZincs = new string[]
            {
              "13:xx:00",
              "13:45:0x",
              "13:45:00.",
              "13:45:00.x"
            };
            foreach (string zinc in badZincs)
                read(zinc);
        }
    }
}
