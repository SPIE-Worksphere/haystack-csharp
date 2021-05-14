using System;
using System.Globalization;
using ProjectHaystack.io;
using M = ProjectHaystack.HaystackValueMapper;

namespace ProjectHaystack
{
    [Obsolete("Use HaystackDate")]
    public class HDate : HVal
    {
        public HDate(HaystackDate source)
        {
            Source = source;
        }
        private HDate(int year, int month, int day)
        {
            Source = new HaystackDate(year, month, day);
        }
        public HaystackDate Source { get; }
        public int Year => Source.Value.Year;
        public int Month => Source.Value.Month;
        public int Day => Source.Value.Day;
        public static HDate make(int year, int month, int day) => M.Map(new HaystackDate(year, month, day));
        public static HDate make(DateTime dt) => M.Map(new HaystackDate(dt));
        public static HDate make(string s)
        {
            DateTime dtParsed = DateTime.Now;
            if (!DateTime.TryParseExact(s, "yyyy'-'MM'-'dd",
                           CultureInfo.InvariantCulture,
                           DateTimeStyles.None,
                           out dtParsed))
            {
                throw new FormatException("Invalid date string: " + s);
            }
            return new HDate(dtParsed.Year, dtParsed.Month, dtParsed.Day);
        }
        public static HDate today() => M.Map(new HaystackDate(DateTime.Today));
        public HDateTime midnight(HTimeZone tz) => M.Map(new HaystackDateTime(Source.Value, M.Map(tz)));
        public override int GetHashCode() => Source.GetHashCode();
        public override bool Equals(object that) => that != null && that is HDate date && Source.Equals(M.Map(date));
        public override string toZinc() => ZincWriter.ToZinc(M.Map(this));
        public override string toJson() => HaysonWriter.ToHayson(M.Map(this));
        public HDate plusDays(int numDays) => M.Map(new HaystackDate(Source.Value.AddDays(numDays)));
        public HDate minusDays(int numDays) => M.Map(new HaystackDate(Source.Value.AddDays(-numDays)));
        public static bool isLeapYear(int year) => DateTime.IsLeapYear(year);
        public DayOfWeek weekday() => Source.Value.DayOfWeek;
        public DateTime ToDateTime() => Source.Value;
    }
}