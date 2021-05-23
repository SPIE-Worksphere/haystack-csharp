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
    public class HRefTest : HValTest
    {
        [TestMethod]
        public void testEquality()
        {
            Assert.IsTrue(HRef.make("foo").hequals( HRef.make("foo")));
            Assert.IsTrue(HRef.make("foo").hequals(HRef.make("foo", "Foo")));
            Assert.IsFalse(HRef.make("foo").hequals(HRef.make("Foo")));
        }

        [TestMethod]
        public void testZinc()
        {
            verifyZinc(HRef.make("1234-5678.foo:bar"), "@1234-5678.foo:bar");
            verifyZinc(HRef.make("1234-5678", "Foo Bar"), "@1234-5678 \"Foo Bar\"");
            verifyZinc(HRef.make("1234-5678", "Foo \"Bar\""), "@1234-5678 \"Foo \\\"Bar\\\"\"");
        }

        [TestMethod]
        public void testIsId()
        {
            Assert.IsFalse(HRef.isId(""));
            Assert.IsFalse(HRef.isId("%"));
            Assert.IsTrue(HRef.isId("a"));
            Assert.IsTrue(HRef.isId("a-b:c"));
            Assert.IsFalse(HRef.isId("a b"));
            Assert.IsFalse(HRef.isId("a\u0129b"));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void testBadRefConstruction()
        {
            string[] badRefs = new string[]
            {
                "@a",
                "a b",
                "a\n",
                "@"
            };
            foreach (string strID in badRefs)
                HRef.make(strID);
        }
    }
}
