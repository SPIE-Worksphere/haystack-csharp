using System.Linq;
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
            Assert.IsTrue(new[] { "UTC", "Etc/UTC" }.Contains(tz.dntz.Id));
        }

        [TestMethod]
        public void make_sidney()
        {
            var tz = HTimeZone.make("Sydney", false);
            Assert.IsTrue(new[] { "AUS Eastern Standard Time", "Australia/Sydney" }.Contains(tz.dntz.Id));
        }
    }
}