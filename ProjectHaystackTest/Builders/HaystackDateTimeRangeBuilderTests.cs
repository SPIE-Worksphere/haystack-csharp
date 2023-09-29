using System;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectHaystack;
using ProjectHaystack.Builders;

namespace ProjectHaystackTest.Builders
{
    [TestClass]
    public class HaystackDateTimeRangeBuilderTests
    {
        [TestMethod]
        public void Today_InvariantCulture()
        {
            var timeZone = new HaystackTimeZone("UTC");
            var builder = new HaystackDateTimeRangeBuilder(timeZone, () => new DateTime(2023, 4, 5, 6, 7, 8), CultureInfo.InvariantCulture);
            var range = builder.Today();

            Assert.AreEqual(timeZone, range.Start.TimeZone);
            Assert.AreEqual(timeZone, range.End.TimeZone);
            Assert.AreEqual(new DateTimeOffset(2023, 4, 5, 0, 0, 0, TimeSpan.Zero), range.Start.Value);
            Assert.AreEqual(new DateTimeOffset(2023, 4, 6, 0, 0, 0, TimeSpan.Zero), range.End.Value);
        }

        [TestMethod]
        public void Yesterday_InvariantCulture()
        {
            var timeZone = new HaystackTimeZone("UTC");
            var builder = new HaystackDateTimeRangeBuilder(timeZone, () => new DateTime(2023, 4, 5, 6, 7, 8), CultureInfo.InvariantCulture);
            var range = builder.Yesterday();

            Assert.AreEqual(timeZone, range.Start.TimeZone);
            Assert.AreEqual(timeZone, range.End.TimeZone);
            Assert.AreEqual(new DateTimeOffset(2023, 4, 4, 0, 0, 0, TimeSpan.Zero), range.Start.Value);
            Assert.AreEqual(new DateTimeOffset(2023, 4, 5, 0, 0, 0, TimeSpan.Zero), range.End.Value);
        }


        [TestMethod]
        public void ThisWeek_InvariantCulture()
        {
            var timeZone = new HaystackTimeZone("UTC");
            var builder = new HaystackDateTimeRangeBuilder(timeZone, () => new DateTime(2023, 4, 5, 6, 7, 8), CultureInfo.InvariantCulture);
            var range = builder.ThisWeek();

            Assert.AreEqual(timeZone, range.Start.TimeZone);
            Assert.AreEqual(timeZone, range.End.TimeZone);
            Assert.AreEqual(new DateTimeOffset(2023, 4, 2, 0, 0, 0, TimeSpan.Zero), range.Start.Value);
            Assert.AreEqual(new DateTimeOffset(2023, 4, 9, 0, 0, 0, TimeSpan.Zero), range.End.Value);
        }


        [TestMethod]
        public void ThisWeek_Netherlands()
        {
            var timeZone = new HaystackTimeZone("UTC");
            var builder = new HaystackDateTimeRangeBuilder(timeZone, () => new DateTime(2023, 4, 5, 6, 7, 8), CultureInfo.GetCultureInfo("nl-NL"));
            var range = builder.ThisWeek();

            Assert.AreEqual(timeZone, range.Start.TimeZone);
            Assert.AreEqual(timeZone, range.End.TimeZone);
            Assert.AreEqual(new DateTimeOffset(2023, 4, 3, 0, 0, 0, TimeSpan.Zero), range.Start.Value);
            Assert.AreEqual(new DateTimeOffset(2023, 4, 10, 0, 0, 0, TimeSpan.Zero), range.End.Value);
        }

        [TestMethod]
        public void LastWeek_InvariantCulture()
        {
            var timeZone = new HaystackTimeZone("UTC");
            var builder = new HaystackDateTimeRangeBuilder(timeZone, () => new DateTime(2023, 4, 5, 6, 7, 8), CultureInfo.InvariantCulture);
            var range = builder.LastWeek();

            Assert.AreEqual(timeZone, range.Start.TimeZone);
            Assert.AreEqual(timeZone, range.End.TimeZone);
            Assert.AreEqual(new DateTimeOffset(2023, 3, 26, 0, 0, 0, TimeSpan.Zero), range.Start.Value);
            Assert.AreEqual(new DateTimeOffset(2023, 4, 2, 0, 0, 0, TimeSpan.Zero), range.End.Value);
        }

        [TestMethod]
        public void LastWeek_Netherlands()
        {
            var timeZone = new HaystackTimeZone("UTC");
            var builder = new HaystackDateTimeRangeBuilder(timeZone, () => new DateTime(2023, 4, 5, 6, 7, 8), CultureInfo.GetCultureInfo("nl-NL"));
            var range = builder.LastWeek();

            Assert.AreEqual(timeZone, range.Start.TimeZone);
            Assert.AreEqual(timeZone, range.End.TimeZone);
            Assert.AreEqual(new DateTimeOffset(2023, 3, 27, 0, 0, 0, TimeSpan.Zero), range.Start.Value);
            Assert.AreEqual(new DateTimeOffset(2023, 4, 3, 0, 0, 0, TimeSpan.Zero), range.End.Value);
        }

        [TestMethod]
        public void ThisMonth_InvariantCulture()
        {
            var timeZone = new HaystackTimeZone("UTC");
            var builder = new HaystackDateTimeRangeBuilder(timeZone, () => new DateTime(2023, 4, 5, 6, 7, 8), CultureInfo.InvariantCulture);
            var range = builder.ThisMonth();

            Assert.AreEqual(timeZone, range.Start.TimeZone);
            Assert.AreEqual(timeZone, range.End.TimeZone);
            Assert.AreEqual(new DateTimeOffset(2023, 4, 1, 0, 0, 0, TimeSpan.Zero), range.Start.Value);
            Assert.AreEqual(new DateTimeOffset(2023, 5, 1, 0, 0, 0, TimeSpan.Zero), range.End.Value);
        }

        [TestMethod]
        public void LastMonth_InvariantCulture()
        {
            var timeZone = new HaystackTimeZone("UTC");
            var builder = new HaystackDateTimeRangeBuilder(timeZone, () => new DateTime(2023, 4, 5, 6, 7, 8), CultureInfo.InvariantCulture);
            var range = builder.LastMonth();

            Assert.AreEqual(timeZone, range.Start.TimeZone);
            Assert.AreEqual(timeZone, range.End.TimeZone);
            Assert.AreEqual(new DateTimeOffset(2023, 3, 1, 0, 0, 0, TimeSpan.Zero), range.Start.Value);
            Assert.AreEqual(new DateTimeOffset(2023, 4, 1, 0, 0, 0, TimeSpan.Zero), range.End.Value);
        }

        [TestMethod]
        public void ThisYear_InvariantCulture()
        {
            var timeZone = new HaystackTimeZone("UTC");
            var builder = new HaystackDateTimeRangeBuilder(timeZone, () => new DateTime(2023, 4, 5, 6, 7, 8), CultureInfo.InvariantCulture);
            var range = builder.ThisYear();

            Assert.AreEqual(timeZone, range.Start.TimeZone);
            Assert.AreEqual(timeZone, range.End.TimeZone);
            Assert.AreEqual(new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero), range.Start.Value);
            Assert.AreEqual(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero), range.End.Value);
        }

        [TestMethod]
        public void LastYear_InvariantCulture()
        {
            var timeZone = new HaystackTimeZone("UTC");
            var builder = new HaystackDateTimeRangeBuilder(timeZone, () => new DateTime(2023, 4, 5, 6, 7, 8), CultureInfo.InvariantCulture);
            var range = builder.LastYear();

            Assert.AreEqual(timeZone, range.Start.TimeZone);
            Assert.AreEqual(timeZone, range.End.TimeZone);
            Assert.AreEqual(new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero), range.Start.Value);
            Assert.AreEqual(new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero), range.End.Value);
        }
    }
}