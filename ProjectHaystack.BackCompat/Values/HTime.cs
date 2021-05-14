using System;
using ProjectHaystack.io;
using M = ProjectHaystack.HaystackValueMapper;

namespace ProjectHaystack
{
    [Obsolete("Use HaystackTime")]
    public class HTime : HVal
    {
        public HTime(HaystackTime source)
        {
            Source = source;
        }
        public HTime(TimeSpan time)
        {
            Source = new HaystackTime(time);
        }
        public HaystackTime Source { get; }
        public int Hour => Source.Value.Hours;
        public int Minute => Source.Value.Minutes;
        public int Second => Source.Value.Seconds;
        public int Millisecond => Source.Value.Milliseconds;

        public static HTime make(int hour, int min, int sec, int ms)
        {
            if (hour < 0 || hour > 23) throw new ArgumentException("Invalid hour", "hour");
            if (min < 0 || min > 59) throw new ArgumentException("Invalid min", "min");
            if (sec < 0 || sec > 59) throw new ArgumentException("Invalid sec", "sec");
            if (ms < 0 || ms > 999) throw new ArgumentException("Invalid ms", "ms");
            return new HTime(new TimeSpan(0, hour, min, sec, ms));
        }
        public static HTime make(int hour, int min, int sec)
        {
            return make(hour, min, sec, 0);
        }
        public static HTime make(int hour, int min)
        {
            return make(hour, min, 0, 0);
        }
        public static HTime make(DateTime dt) => M.Map(new HaystackTime(dt.TimeOfDay));
        public static HTime make(string s) => M.Map(ZincReader.ReadValue<HaystackTime>(s));
        public static readonly HTime MIDNIGHT = new HTime(TimeSpan.Zero);
        public override int GetHashCode() => Source.GetHashCode();
        public override bool Equals(object that) => that != null && that is HTime time && Source.Equals(M.Map(time));
        public override int CompareTo(object obj) => obj is HTime time ? Source.CompareTo(M.Map(time)) : -1;
        public override string toZinc() => ZincWriter.ToZinc(M.Map(this));
        public override string toJson() => HaysonWriter.ToHayson(M.Map(this));
        public override bool hequals(object that) => Equals(that);
    }
}