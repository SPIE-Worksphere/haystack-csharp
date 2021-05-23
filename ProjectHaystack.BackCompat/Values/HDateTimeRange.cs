using System;
using ProjectHaystack.io;
using M = ProjectHaystack.HaystackValueMapper;

namespace ProjectHaystack
{
    [Obsolete("Use HaystackDateTimeRange")]
    public class HDateTimeRange
    {
        public HDateTimeRange(HaystackDateTimeRange source)
        {
            Source = source;
        }
        public HaystackDateTimeRange Source { get; }
        public HDateTime Start => M.Map(Source.Start);
        public HDateTime End => M.Map(Source.End);
        public static HDateTimeRange make(HDate date, HTimeZone tz)
            => M.Map(new HaystackDateTimeRange(new HaystackDateTime(date.Source.Value, M.Map(tz)), new HaystackDateTime(date.Source.Value.AddDays(1), M.Map(tz))));
        public static HDateTimeRange make(HDate start, HDate end, HTimeZone tz)
            => M.Map(new HaystackDateTimeRange(new HaystackDateTime(start.Source.Value, M.Map(tz)), new HaystackDateTime(end.Source.Value, M.Map(tz))));
        public static HDateTimeRange make(HDateTime start, HDateTime end)
            => M.Map(new HaystackDateTimeRange(M.Map(start), M.Map(end)));
        public static HDateTimeRange thisWeek(HTimeZone tz)
        {
            var firstOfWeek = DateTime.Today.AddDays(DayOfWeek.Sunday - DateTime.Today.DayOfWeek);
            return M.Map(new HaystackDateTimeRange(
                new HaystackDateTime(firstOfWeek, M.Map(tz)),
                new HaystackDateTime(firstOfWeek.AddDays(7), M.Map(tz))));
        }
        public static HDateTimeRange thisMonth(HTimeZone tz)
        {
            var firstOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            return M.Map(new HaystackDateTimeRange(
                new HaystackDateTime(firstOfMonth, M.Map(tz)),
                new HaystackDateTime(firstOfMonth.AddMonths(1), M.Map(tz))));
        }
        public static HDateTimeRange thisYear(HTimeZone tz)
        {
            var firstOfYear = new DateTime(DateTime.Today.Year, 1, 1);
            return M.Map(new HaystackDateTimeRange(
                new HaystackDateTime(firstOfYear, M.Map(tz)),
                new HaystackDateTime(firstOfYear.AddYears(1), M.Map(tz))));
        }
        public static HDateTimeRange lastWeek(HTimeZone tz)
        {
            var firstOfWeek = DateTime.Today.AddDays(DayOfWeek.Sunday - DateTime.Today.DayOfWeek).AddDays(-7);
            return M.Map(new HaystackDateTimeRange(
                new HaystackDateTime(firstOfWeek, M.Map(tz)),
                new HaystackDateTime(firstOfWeek.AddDays(7), M.Map(tz))));
        }
        public static HDateTimeRange lastMonth(HTimeZone tz)
        {
            var firstOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-1);
            return M.Map(new HaystackDateTimeRange(
                new HaystackDateTime(firstOfMonth, M.Map(tz)),
                new HaystackDateTime(firstOfMonth.AddMonths(1), M.Map(tz))));
        }
        public static HDateTimeRange lastYear(HTimeZone tz)
        {
            var firstOfYear = new DateTime(DateTime.Today.Year, 1, 1).AddYears(-1);
            return M.Map(new HaystackDateTimeRange(
                new HaystackDateTime(firstOfYear, M.Map(tz)),
                new HaystackDateTime(firstOfYear.AddYears(1), M.Map(tz))));
        }
        public override string ToString() => ZincWriter.ToZinc(Source.Start) + "," + ZincWriter.ToZinc(Source.End);
    }
}