using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectHaystack;

namespace ProjectHaystackTest
{
    [TestClass]
    public class HaystackXStringTests
    {
        [TestMethod]
        public void TestTypeStartsWithUpper()
        {
            new HaystackXString("a", "Type");
            Assert.ThrowsException<ArgumentException>(() => new HaystackXString("a", "type"));
        }

        [TestMethod]
        public void TestTypeContainsValidCharacters()
        {
            new HaystackXString("a", "TyP0_s");
            Assert.ThrowsException<ArgumentException>(() => new HaystackXString("a", "T."));
            Assert.ThrowsException<ArgumentException>(() => new HaystackXString("a", "T,"));
        }

        [TestMethod]
        public void TestEquality()
        {
            Assert.IsTrue(new HaystackXString("a", "Type").Equals(new HaystackXString("a", "Type")));
            Assert.IsFalse(new HaystackXString("a", "X").Equals(new HaystackXString("b", "X")));
            Assert.IsTrue(new HaystackXString("", "X").Equals(new HaystackXString("", "X")));
        }
    }
}