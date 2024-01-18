using System;
using System.Globalization;

namespace ProjectHaystack.Builders
{
    public class HaystackDateTimeRangeBuilder
    {
        private readonly HaystackTimeZone _timeZone;
        private readonly Func<DateTime> _currentTimeProvider;
        private readonly CultureInfo _cultureInfo;

        public HaystackDateTimeRangeBuilder(HaystackTimeZone timeZone, Func<DateTime> currentTimeProvider, CultureInfo cultureInfo)
        {
            _timeZone = timeZone;
            _currentTimeProvider = currentTimeProvider;
            _cultureInfo = cultureInfo;
        }

        public HaystackDateTimeRangeBuilder(HaystackTimeZone timeZone) : this(timeZone, () => DateTime.Now, CultureInfo.InvariantCulture)
        {
        }

        public HaystackDateTimeRange Today()
        {
            var currentTime = _currentTimeProvider();
            var start = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day);
            return new HaystackDateTimeRange(
                new HaystackDateTime(start, _timeZone),
                new HaystackDateTime(start.AddDays(1), _timeZone));
        }

        public HaystackDateTimeRange Yesterday()
        {
            var currentTime = _currentTimeProvider();
            var start = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day).AddDays(-1);
            return new HaystackDateTimeRange(
                new HaystackDateTime(start, _timeZone),
                new HaystackDateTime(start.AddDays(1), _timeZone));
        }

        public HaystackDateTimeRange ThisWeek()
        {
            var currentTime = _currentTimeProvider();
            int diff = (7 + (currentTime.DayOfWeek - _cultureInfo.DateTimeFormat.FirstDayOfWeek)) % 7;
            var start = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day).AddDays(-1 * diff);
            return new HaystackDateTimeRange(
                new HaystackDateTime(start, _timeZone),
                new HaystackDateTime(start.AddDays(7), _timeZone));
        }

        public HaystackDateTimeRange LastWeek()
        {
            var currentTime = _currentTimeProvider();
            int diff = (7 + (currentTime.DayOfWeek - _cultureInfo.DateTimeFormat.FirstDayOfWeek)) % 7;
            var start = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day).AddDays(-1 * diff - 7);
            return new HaystackDateTimeRange(
                new HaystackDateTime(start, _timeZone),
                new HaystackDateTime(start.AddDays(7), _timeZone));
        }
        public HaystackDateTimeRange ThisMonth()
        {
            var currentTime = _currentTimeProvider();
            var start = new DateTime(currentTime.Year, currentTime.Month, 1);
            return new HaystackDateTimeRange(
                new HaystackDateTime(start, _timeZone),
                new HaystackDateTime(start.AddMonths(1), _timeZone));
        }

        public HaystackDateTimeRange LastMonth()
        {
            var currentTime = _currentTimeProvider();
            var start = new DateTime(currentTime.Year, currentTime.Month, 1).AddMonths(-1);
            return new HaystackDateTimeRange(
                new HaystackDateTime(start, _timeZone),
                new HaystackDateTime(start.AddMonths(1), _timeZone));
        }

        public HaystackDateTimeRange ThisYear()
        {
            var currentTime = _currentTimeProvider();
            var start = new DateTime(currentTime.Year, 1, 1);
            return new HaystackDateTimeRange(
                new HaystackDateTime(start, _timeZone),
                new HaystackDateTime(start.AddYears(1), _timeZone));
        }

        public HaystackDateTimeRange LastYear()
        {
            var currentTime = _currentTimeProvider();
            var start = new DateTime(currentTime.Year, 1, 1).AddYears(-1);
            return new HaystackDateTimeRange(
                new HaystackDateTime(start, _timeZone),
                new HaystackDateTime(start.AddYears(1), _timeZone));
        }
    }
}