using System;
using ProjectHaystack.io;
using M = ProjectHaystack.HaystackValueMapper;

namespace ProjectHaystack
{
    [Obsolete("Use HaystackDateTime")]
    public class HDateTime : HVal
    {
        public HDateTime(HaystackDateTime source)
        {
            Source = source;
        }
        public HaystackDateTime Source { get; }
        public HDate date => HDate.make(Source.Value.DateTime.Date);
        public HTime time => new HTime(Source.Value.DateTime.TimeOfDay);
        public TimeSpan Offset => Source.Value.Offset;
        public HTimeZone TimeZone => M.Map(Source.TimeZone);
        public long Ticks => Source.Value.Ticks;
        public DateTimeOffset CopyOfDTO => Source.Value;
        public static HDateTime make(HDate date, HTime time, HTimeZone tz) => M.Map(new HaystackDateTime(M.Map(date), M.Map(time), M.Map(tz)));
        public static HDateTime make(int year, int month, int day, int hour, int min, int sec, HTimeZone tz)
            => M.Map(new HaystackDateTime(new DateTime(year, month, day, hour, min, sec), M.Map(tz)));
        public static HDateTime make(int year, int month, int day, int hour, int min, HTimeZone tz)
            => M.Map(new HaystackDateTime(new DateTime(year, month, day, hour, min, 0), M.Map(tz)));
        public static HDateTime make(long ticks)
            => M.Map(new HaystackDateTime(new DateTime(ticks), HaystackTimeZone.UTC));
        public static HDateTime make(long ticks, HTimeZone tz)
            => M.Map(new HaystackDateTime(new DateTime(ticks), M.Map(tz)));
        public static HDateTime make(string s, bool bException)
            => M.Checked(() => M.Map(ZincReader.ReadValue<HaystackDateTime>(s)), bException);
        public static HDateTime now(HTimeZone tz) => M.Map(new HaystackDateTime(DateTime.Now, M.Map(tz)));
        public static HDateTime now() => M.Map(new HaystackDateTime(DateTime.Now, HaystackTimeZone.UTC));
        public override string toZinc() => ZincWriter.ToZinc(M.Map(this));
        public override string toJson() => HaysonWriter.ToHayson(M.Map(this));
        public override int GetHashCode() => Source.GetHashCode();
        public override bool Equals(object that) => that != null && that is HDateTime dateTime && Source.Equals(M.Map(dateTime));
        public override string ToString() => ZincWriter.ToZinc(M.Map(this));
        public int CompareTo(HDateTime that) => Source.Value.ToUniversalTime().CompareTo(that.Source.Value.ToUniversalTime());
    }
}