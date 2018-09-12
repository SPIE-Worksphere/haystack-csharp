//
// Copyright (c) 2018
// Licensed under the Academic Free License version 3.0
//
// History:
//   16 August 2018 Ian Davies Creation based on Java Toolkit at same time from project-haystack.org downloads
//
using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectHaystack;

namespace ProjectHaystackTest
{
    [TestClass]
    public class HListTest : HValTest
    {
        [TestMethod]
        public void testEmpty()
        {
            Assert.IsTrue(HList.EMPTY.hequals(HList.make(new List<HVal>())));
            Assert.IsTrue(HList.EMPTY.hequals(HList.make(new HVal[0])));
            Assert.AreEqual(HList.EMPTY.size(), 0);
            try
            {
                HList.EMPTY.get(0);
                Assert.Fail();
            }
            catch (Exception)
            {
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void testBasics()
        {
            HRef hrefTest = HRef.make("a");
            HStr str = HStr.make("string");
            List<HVal> items = new List<HVal>();
            items.Add(hrefTest);
            items.Add(str);

            HList list = HList.make(items);
            Assert.AreEqual(list.size(), 2);
            Assert.AreEqual(list.get(0), hrefTest);
            Assert.AreEqual(list.get(1), str);
        }

        [TestMethod]
        public void testZinc()
        {
            verifyZinc(HList.EMPTY, "[]");
            // TODO: more tests
        }
    }
}
