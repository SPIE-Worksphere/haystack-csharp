using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectHaystack;

namespace ProjectHaystackTest
{
    [TestClass]
    public class HaystackBooleanTests
    {
        [TestMethod]
        public void TestEquality()
        {
            Assert.IsTrue(new HaystackBoolean(true).Equals(new HaystackBoolean(true)));
            Assert.IsFalse(new HaystackBoolean(true).Equals(new HaystackBoolean(false)));
            Assert.IsTrue(new HaystackBoolean(true).Equals(new HaystackBoolean(true)));
            Assert.IsTrue(new HaystackBoolean(false).Equals(new HaystackBoolean(false)));
        }
    }
}