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
    public class HUriTest : HValTest
    {
        [TestMethod]
        public void testEquality()
        {
            Assert.IsTrue(HUri.make("a").hequals(HUri.make("a")));
            Assert.IsFalse(HUri.make("a").hequals(HUri.make("b")));
            Assert.IsTrue(HUri.make("") == HUri.make(""));
        }

        [TestMethod]
        public void testCompare()
        {
            Assert.IsTrue(HUri.make("abc").CompareTo(HUri.make("z")) < 0);
            Assert.AreEqual(HUri.make("Foo").CompareTo(HUri.make("Foo")), 0);
        }

        [TestMethod]
        public void testZinc()
        {
            verifyZinc(HUri.make("http://foo.com/f?q"), "`http://foo.com/f?q`");
            verifyZinc(HUri.make("a$b"), "`a$b`");
            verifyZinc(HUri.make("a`b"), "`a\\`b`");
            verifyZinc(HUri.make("http\\:a\\?b"), "`http\\:a\\?b`");
            verifyZinc(HUri.make("\u01ab.txt"), "`\u01ab.txt`");
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void testBadZinc()
        {
            string[] badZincs = new string[] 
            {
                "`no end",
                "`new\nline`"
            };
            foreach (string zinc in badZincs)
                read(zinc);
        }
    }
}
