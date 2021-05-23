using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectHaystack;

namespace ProjectHaystackTest
{
    [TestClass]
    public class HaystackTimeTests
    {
        [TestMethod]
        public void TestEquality()
        {
            Assert.IsTrue(new HaystackTime(new TimeSpan(0, 1, 2, 3, 4)).Equals(new HaystackTime(new TimeSpan(0, 1, 2, 3, 4))));
            Assert.IsFalse(new HaystackTime(new TimeSpan(0, 1, 2, 3, 4)).Equals(new HaystackTime(new TimeSpan(0, 9, 2, 3, 4))));
            Assert.IsFalse(new HaystackTime(new TimeSpan(0, 1, 2, 3, 4)).Equals(new HaystackTime(new TimeSpan(0, 1, 9, 3, 4))));
            Assert.IsFalse(new HaystackTime(new TimeSpan(0, 1, 2, 3, 4)).Equals(new HaystackTime(new TimeSpan(0, 1, 2, 9, 9))));
        }

        [TestMethod]
        public void TestCompare()
        {
            Assert.IsTrue(new HaystackTime(new TimeSpan(0, 0, 0, 0, 0)).CompareTo(new HaystackTime(new TimeSpan(0, 0, 0, 0, 9))) < 0);
            Assert.IsTrue(new HaystackTime(new TimeSpan(0, 0, 0, 0, 0)).CompareTo(new HaystackTime(new TimeSpan(0, 0, 0, 1, 0))) < 0);
            Assert.IsTrue(new HaystackTime(new TimeSpan(0, 0, 1, 0, 0)).CompareTo(new HaystackTime(new TimeSpan(0, 0, 0, 0, 0))) > 0);
            Assert.IsTrue(new HaystackTime(new TimeSpan(0, 0, 0, 0, 0)).CompareTo(new HaystackTime(new TimeSpan(0, 2, 0, 0, 0))) < 0);
            Assert.AreEqual(new HaystackTime(new TimeSpan(0, 2, 0, 0, 0)).CompareTo(new HaystackTime(new TimeSpan(0, 2, 0, 0, 0))), 0);
        }
    }
}