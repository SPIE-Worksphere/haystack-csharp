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
    public class HStrTest : HValTest
    {
        [TestMethod]
        public void testEquality()
        {
            Assert.IsTrue(HStr.make("a").hequals(HStr.make("a")));
            Assert.IsFalse(HStr.make("a").hequals( HStr.make("b")));
            Assert.IsTrue(HStr.make("").hequals(HStr.make("")));
        }

        [TestMethod]
        public void testCompare()
        {
            Assert.IsTrue(HStr.make("abc").CompareTo(HStr.make("z")) < 0);
            Assert.AreEqual(HStr.make("Foo").CompareTo(HStr.make("Foo")), 0);
        }

        [TestMethod]
        public void testZinc()
        {
            verifyZinc(HStr.make("hello"), "\"hello\"");
            verifyZinc(HStr.make("_ \\ \" \n \r \t \u0011 _"), "\"_ \\\\ \\\" \\n \\r \\t \\u0011 _\"");
            verifyZinc(HStr.make("\u0abc"), "\"\u0abc\"");
        }

        [TestMethod]
        public void testHex()
        {  
            // Changed test - unicode code must be valid not random like original test. - this is "NA"
            Assert.IsTrue(read("\"[\\u004e \\u0041]\"").hequals(HStr.make("[\u004e \u0041]")));
            Assert.IsTrue(read("\"[\\u004E \\u0041]\"").hequals(HStr.make("[\u004E \u0041]")));
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void testNoEndQuote()
        {
            read("\"end...");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void testBadUnicodeEsc()
        {
            read("\"\\u1x34\"");
        }
    }
}
