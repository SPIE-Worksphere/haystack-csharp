using System;

namespace ProjectHaystack
{
    /// <summary>
    /// Haystack date value.
    /// </summary>
    public class HaystackDate : HaystackValue, IComparable
    {
        public HaystackDate(DateTime date)
        {
            Value = date.Date;
        }

        public HaystackDate(int year, int month, int day)
        {
            Value = new DateTime(year, month, day);
        }

        public DateTime Value { get; }

        public static HaystackDate Today => new HaystackDate(DateTime.Today);

        public override int GetHashCode() => Value.GetHashCode();

        public override bool Equals(object other) => other != null && other is HaystackDate date && date.Value == Value;

        public int CompareTo(object obj)
        {
#if NETSTANDARD2_0 || NETSTANDARD2_1
            if (obj == null || !(obj is HaystackDate date))
            {
                return 1;
            }
#else
            if (obj == null || obj is not HaystackDate date)
            {
                return 1;
            }
#endif

            return Value.CompareTo(date.Value);
        }
    }
}