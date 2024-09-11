using System;

namespace ProjectHaystack
{
    /// <summary>
    /// Haystack date and time value including timezone.
    /// </summary>
    public class HaystackDateTime : HaystackValue
    {
        public HaystackDateTime(DateTimeOffset dateTime, HaystackTimeZone timeZone)
        {
            Value = dateTime;
            TimeZone = timeZone;
        }

        public HaystackDateTime(DateTime dateTime, HaystackTimeZone timeZone)
        {
            Value = new DateTimeOffset(dateTime.Ticks, timeZone.TimeZoneInfo.GetUtcOffset(dateTime));
            TimeZone = timeZone;
        }

        public HaystackDateTime(HaystackDate date, HaystackTime time, HaystackTimeZone timeZone)
            : this(date.Value + time.Value, timeZone)
        {
        }

        public DateTimeOffset Value { get; private set; }

        public HaystackTimeZone TimeZone { get; private set; }

        public static HaystackDateTime Now(HaystackTimeZone tz) => new HaystackDateTime(System.DateTime.Now, tz);

        public static HaystackDateTime Now() => new HaystackDateTime(System.DateTime.Now, HaystackTimeZone.UTC);

        public override int GetHashCode() => Value.GetHashCode() ^ TimeZone.GetHashCode();

        public override bool Equals(object other)
        {
            return other != null
                && other is HaystackDateTime dateTime
                && dateTime.Value.Equals(Value)
                && dateTime.TimeZone.Equals(TimeZone);
        }

        public int CompareTo(HaystackDateTime other) => Value.ToUniversalTime().CompareTo(other.Value.ToUniversalTime());
    }
}