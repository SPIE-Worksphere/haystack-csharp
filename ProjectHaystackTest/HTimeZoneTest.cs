using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectHaystack;

namespace ProjectHaystackTest
{
    [TestClass]
    public class HTimeZoneTest : HaystackTest
    {
        [TestMethod]
        public void make_utc()
        {
            var tz = HTimeZone.make("UTC", false);
            Assert.AreEqual("UTC", tz.dntz.Id);
        }

        [TestMethod]
        public void make_sidney()
        {
            var tz = HTimeZone.make("Sydney", false);
            Assert.AreEqual("AUS Eastern Standard Time", tz.dntz.Id);
        }
    }
}