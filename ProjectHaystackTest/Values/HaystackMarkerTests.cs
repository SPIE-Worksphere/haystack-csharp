using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectHaystack;

namespace ProjectHaystackTest
{
    [TestClass]
    public class HaystackMarkerTests
    {
        [TestMethod]
        public void TestEquality()
        {
            Assert.IsTrue(new HaystackMarker().Equals(new HaystackMarker()));
        }
    }
}