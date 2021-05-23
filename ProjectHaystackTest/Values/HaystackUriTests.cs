using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectHaystack;

namespace ProjectHaystackTest
{
    [TestClass]
    public class HaystackUriTests
    {
        [TestMethod]
        public void TestEquality()
        {
            Assert.IsTrue(new HaystackUri("a").Equals(new HaystackUri("a")));
            Assert.IsFalse(new HaystackUri("a").Equals(new HaystackUri("b")));
            Assert.IsTrue(new HaystackUri("") == new HaystackUri(""));
        }
    }
}