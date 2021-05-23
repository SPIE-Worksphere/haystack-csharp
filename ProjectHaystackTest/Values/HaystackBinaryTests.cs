using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectHaystack;

namespace ProjectHaystackTest
{
    [TestClass]
    public class HaystackBinaryTests
    {
        [TestMethod]
        public void TestEquality()
        {
            var bin1 = new HaystackBinary("text/plain");
            var bin2 = new HaystackBinary("text/plain");
            var bin3 = new HaystackBinary("text/xml");

            Assert.AreEqual(bin1, bin2);
            Assert.AreNotEqual(bin1, bin3);
        }
    }
}