using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectHaystack;

namespace ProjectHaystackTest
{
    [TestClass]
    public class HaystackStringTests
    {
        [TestMethod]
        public void TestEquality()
        {
            Assert.IsTrue(new HaystackString("a").Equals(new HaystackString("a")));
            Assert.IsFalse(new HaystackString("a").Equals(new HaystackString("b")));
            Assert.IsTrue(new HaystackString("").Equals(new HaystackString("")));
        }

        [TestMethod]
        public void TestCompare()
        {
            Assert.IsTrue(new HaystackString("abc").CompareTo(new HaystackString("z")) < 0);
            Assert.AreEqual(new HaystackString("Foo").CompareTo(new HaystackString("Foo")), 0);
        }
    }
}