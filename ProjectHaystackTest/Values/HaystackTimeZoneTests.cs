using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectHaystack;

namespace ProjectHaystackTest
{
    [TestClass]
    public class HaystackTimeZoneTests
    {
        [TestMethod]
        public void Make_utc()
        {
            var tz = new HaystackTimeZone("UTC");
            Assert.IsTrue(new[] { "UTC", "Etc/UTC" }.Contains(tz.TimeZoneInfo.Id));
        }

        [TestMethod]
        public void Make_sidney()
        {
            var tz = new HaystackTimeZone("Sydney");
            Assert.IsTrue(new[] { "AUS Eastern Standard Time", "Australia/Sydney" }.Contains(tz.TimeZoneInfo.Id));
        }

        [TestMethod]
        public void TestTz_NewYork()
        {
            VerifyTz("New_York", "Eastern Standard Time", "America/New_York");
        }

        [TestMethod]
        public void TestTz_Chicago()
        {
            VerifyTz("Chicago", "Central Standard Time", "America/Chicago");
        }

        [TestMethod]
        public void TestTz_Phoenix()
        {
            VerifyTz("Phoenix", "US Mountain Standard Time", "America/Phoenix");
        }

        [TestMethod]
        public void TestTz_London()
        {
            VerifyTz("London", "GMT Standard Time", "Europe/London");
        }

        [TestMethod]
        public void TestTz_UTC()
        {
            VerifyTz("UTC", "UTC", "Etc/UTC");
        }

        [TestMethod]
        public void TestTz_GMT()
        {
            VerifyTz("GMT", "UTC", "GMT");
        }

        [TestMethod]
        public void TestTz_Rel()
        {
            VerifyTz("Rel", "UTC", "GMT"); // GMT
        }

        private void VerifyTz(string name, params string[] dntzIds)
        {
            var tz = new HaystackTimeZone(name);
            // Ignore issues with locally installed timezones.
            if (tz == null)
            {
                return;
            }
            var dntz = tz.TimeZoneInfo;
            Assert.AreEqual(tz.Name, name);
            Assert.IsTrue(dntzIds.Contains(dntz.Id), $"{dntz.Id} not in [{string.Join(", ", dntzIds)}]");
        }
    }
}