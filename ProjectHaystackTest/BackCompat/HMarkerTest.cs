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
using ProjectHaystack.io;

namespace ProjectHaystackTest 
{
    [TestClass]
    public class HMarkerTest : HValTest
    {
        [TestMethod]
        public void testEquality()
        {
            Assert.IsTrue(HMarker.VAL.hequals(HMarker.VAL));
        }

        [TestMethod]
        public void testToString()
        {
            Assert.AreEqual(HMarker.VAL.ToString(), "marker");
        }

        [TestMethod]
        public void testZinc()
        {
            verifyZinc(HMarker.VAL, "M");
        }

    }
}
