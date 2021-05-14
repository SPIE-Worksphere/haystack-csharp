//
// Copyright (c) 2018
// Licensed under the Academic Free License version 3.0
//
// History:
//   16 August 2018 Ian Davies Creation based on Java Toolkit at same time from project-haystack.org downloads
//
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectHaystack;

namespace ProjectHaystackTest
{
    [TestClass]
    public class HBoolTest : HValTest
    {
        [TestMethod]
        public void testEquality()
        {
            Assert.IsTrue(HBool.TRUE.hequals(HBool.TRUE));
            Assert.IsFalse(HBool.TRUE.hequals(HBool.FALSE));
            Assert.IsTrue(HBool.make(true).hequals(HBool.TRUE));
            Assert.IsTrue(HBool.make(false).hequals(HBool.FALSE));
        }

        [TestMethod]
        public void testCompare()
        {
            Assert.IsTrue(HBool.FALSE.CompareTo(HBool.TRUE) < 0);
            Assert.AreEqual(HBool.TRUE.CompareTo(HBool.TRUE), 0);
        }

        [TestMethod]
        public void testZinc()
        {
            verifyZinc(HBool.TRUE, "T");
            verifyZinc(HBool.FALSE, "F");
        }
    }
}