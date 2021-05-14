using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectHaystack;

namespace ProjectHaystackTest
{
    [TestClass]
    public class HaystackNumberTests
    {
        [TestMethod]
        public void TestEquality()
        {
            Assert.IsTrue(new HaystackNumber(2).Equals(new HaystackNumber(2.0, null)));
            Assert.IsFalse(new HaystackNumber(2).Equals(new HaystackNumber(2, "%")));
            Assert.IsFalse(new HaystackNumber(2, "%").Equals(new HaystackNumber(2)));
            Assert.IsTrue(new HaystackNumber(0).Equals(new HaystackNumber(0.0)));
        }

        [TestMethod]
        public void TestCompare()
        {
            Assert.IsTrue(new HaystackNumber(9).CompareTo(new HaystackNumber(11)) < 0);
            Assert.IsTrue(new HaystackNumber(-3).CompareTo(new HaystackNumber(-4)) > 0);
            Assert.AreEqual(new HaystackNumber(-23).CompareTo(new HaystackNumber(-23)), 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestBadUnitConstruction()
        {
            string[] badunitNames = new string[]
            {
                "foo bar",
                "foo,bar"
            };
            foreach (string curUnit in badunitNames)
            {
                new HaystackNumber(123.4, curUnit);
            }
        }
    }
}