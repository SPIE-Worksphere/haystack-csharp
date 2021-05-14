using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectHaystack;

namespace ProjectHaystackTest
{
    [TestClass]
    public class HaystackDateTests
    {
        [TestMethod]
        public void TestEquality()
        {
            Assert.IsTrue(new HaystackDate(2011, 6, 7).Equals(new HaystackDate(2011, 6, 7)));
            Assert.IsFalse(new HaystackDate(2011, 6, 7).Equals(new HaystackDate(2011, 6, 8)));
            Assert.IsFalse(new HaystackDate(2011, 6, 7).Equals(new HaystackDate(2011, 2, 7)));
            Assert.IsFalse(new HaystackDate(2011, 6, 7).Equals(new HaystackDate(2009, 6, 7)));
        }

        [TestMethod]
        public void TestCompare()
        {
            Assert.IsTrue(new HaystackDate(2011, 6, 9).CompareTo(new HaystackDate(2011, 6, 21)) < 0);
            Assert.IsTrue(new HaystackDate(2011, 10, 9).CompareTo(new HaystackDate(2011, 3, 21)) > 0);
            Assert.IsTrue(new HaystackDate(2010, 6, 9).CompareTo(new HaystackDate(2000, 9, 30)) > 0);
            Assert.AreEqual(new HaystackDate(2010, 6, 9).CompareTo(new HaystackDate(2010, 6, 9)), 0);
        }
    }
}