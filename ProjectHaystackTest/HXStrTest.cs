using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectHaystack;

namespace ProjectHaystackTest
{
    [TestClass]
    public class HXStrTest : HValTest
    {
        [TestMethod]
        public void testTypeStartsWithUpper()
        {
            HXStr.decode("Type", "a");
            Assert.ThrowsException<ArgumentException>(() => HXStr.decode("type", "a"));
        }

        [TestMethod]
        public void testTypeContainsValidCharacters()
        {
            HXStr.decode("TyP0_s", "a");
            Assert.ThrowsException<ArgumentException>(() => HXStr.decode("T.", "a"));
            Assert.ThrowsException<ArgumentException>(() => HXStr.decode("T,", "a"));
        }

        [TestMethod]
        public void testEquality()
        {
            Assert.IsTrue(HXStr.decode("Type", "a").hequals(HXStr.decode("Type", "a")));
            Assert.IsFalse(HXStr.decode("X", "a").hequals(HXStr.decode("X", "b")));
            Assert.IsTrue(HXStr.decode("X", "").hequals(HXStr.decode("X", "")));
        }

        [TestMethod]
        public void testCompare()
        {
            Assert.IsTrue(HXStr.decode("Type", "abc").CompareTo(HXStr.decode("Type", "z")) < 0);
            Assert.AreEqual(HXStr.decode("Type", "Foo").CompareTo(HXStr.decode("Type", "Foo")), 0);
        }

        [TestMethod]
        public void testZinc()
        {
            verifyZinc(HXStr.decode("Type", "hello"), "Type(\"hello\")");
            verifyZinc(HXStr.decode("Type", "\u0abc"), "Type(\"\u0abc\")");
        }

        [TestMethod]
        public void testHex()
        {
            // Changed test - unicode code must be valid not random like original test. - this is "NA"
            Assert.IsTrue(read("Type(\"[\\u004e \\u0041]\")").hequals(HXStr.decode("Type", "[\u004e \u0041]")));
            Assert.IsTrue(read("Type(\"[\\u004E \\u0041]\")").hequals(HXStr.decode("Type", "[\u004E \u0041]")));
        }
    }
}