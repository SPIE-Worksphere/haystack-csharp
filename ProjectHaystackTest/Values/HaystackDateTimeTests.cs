using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectHaystack;

namespace ProjectHaystackTest
{
    [TestClass]
    public class HaystackDateTimeTests
    {
        public static HaystackTimeZone utc = HaystackTimeZone.UTC;
        public static HaystackTimeZone london = new HaystackTimeZone("London");

        [TestMethod]
        public void TestEquality()
        {
            Assert.IsTrue(new HaystackDateTime(new DateTime(2011, 1, 2, 3, 4, 5), utc).Equals(new HaystackDateTime(new DateTime(2011, 1, 2, 3, 4, 5), utc)));
            Assert.IsFalse(new HaystackDateTime(new DateTime(2011, 1, 2, 3, 4, 5), utc).Equals(new HaystackDateTime(new DateTime(2009, 1, 2, 3, 4, 5), utc)));
            Assert.IsFalse(new HaystackDateTime(new DateTime(2011, 1, 2, 3, 4, 5), utc).Equals(new HaystackDateTime(new DateTime(2011, 9, 2, 3, 4, 5), utc)));
            Assert.IsFalse(new HaystackDateTime(new DateTime(2011, 1, 2, 3, 4, 5), utc).Equals(new HaystackDateTime(new DateTime(2011, 1, 9, 3, 4, 5), utc)));
            Assert.IsFalse(new HaystackDateTime(new DateTime(2011, 1, 2, 3, 4, 5), utc).Equals(new HaystackDateTime(new DateTime(2011, 1, 2, 9, 4, 5), utc)));
            Assert.IsFalse(new HaystackDateTime(new DateTime(2011, 1, 2, 3, 4, 5), utc).Equals(new HaystackDateTime(new DateTime(2011, 1, 2, 3, 9, 5), utc)));
            Assert.IsFalse(new HaystackDateTime(new DateTime(2011, 1, 2, 3, 4, 5), utc).Equals(new HaystackDateTime(new DateTime(2011, 1, 2, 3, 4, 9), utc)));
            Assert.IsFalse(new HaystackDateTime(new DateTime(2011, 1, 2, 3, 4, 5), utc).Equals(new HaystackDateTime(new DateTime(2011, 1, 2, 3, 4, 5), london)));
        }

        [TestMethod]
        public void TestCompare()
        {
            Assert.AreEqual(new HaystackDateTime(new DateTime(2011, 1, 2, 3, 4, 5), utc).CompareTo(new HaystackDateTime(new DateTime(2011, 1, 2, 3, 4, 5), utc)), 0);
            Assert.IsTrue(new HaystackDateTime(new DateTime(2011, 1, 2, 3, 4, 5), utc).CompareTo(new HaystackDateTime(new DateTime(2011, 1, 2, 3, 4, 6), utc)) < 0);
            Assert.IsTrue(new HaystackDateTime(new DateTime(2011, 1, 2, 3, 4, 5), utc).CompareTo(new HaystackDateTime(new DateTime(2011, 1, 2, 3, 5, 5), utc)) < 0);
            Assert.IsTrue(new HaystackDateTime(new DateTime(2011, 1, 2, 3, 4, 5), utc).CompareTo(new HaystackDateTime(new DateTime(2011, 1, 2, 4, 4, 5), utc)) < 0);
            Assert.IsTrue(new HaystackDateTime(new DateTime(2011, 1, 2, 3, 4, 5), utc).CompareTo(new HaystackDateTime(new DateTime(2011, 1, 3, 3, 4, 5), utc)) < 0);
            Assert.IsTrue(new HaystackDateTime(new DateTime(2011, 1, 2, 3, 4, 5), utc).CompareTo(new HaystackDateTime(new DateTime(2011, 2, 2, 3, 4, 5), utc)) < 0);
            Assert.IsTrue(new HaystackDateTime(new DateTime(2011, 1, 2, 3, 4, 5), utc).CompareTo(new HaystackDateTime(new DateTime(2012, 1, 2, 3, 4, 5), utc)) < 0);
            Assert.IsTrue(new HaystackDateTime(new DateTime(2011, 1, 2, 3, 4, 5), utc).CompareTo(new HaystackDateTime(new DateTime(2011, 1, 2, 3, 4, 0), utc)) > 0);
        }

        [TestMethod]
        public void TestTimeZoneOffset()
        {
            Assert.AreEqual(TimeSpan.FromHours(0), new HaystackDateTime(new DateTime(2011, 1, 2, 3, 4, 5), london).Value.Offset);
            Assert.AreEqual(TimeSpan.FromHours(1), new HaystackDateTime(new DateTime(2011, 7, 2, 3, 4, 5), london).Value.Offset);
        }
    }
}