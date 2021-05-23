using System;

namespace ProjectHaystack
{
    public class HaystackString : HaystackValue, IComparable
    {
        private static HaystackString _empty = new HaystackString(string.Empty);

        public HaystackString(string value)
        {
            Value = value;
        }

        public static HaystackString Empty => _empty;

        public string Value { get; }

        public override int GetHashCode() => Value.GetHashCode();

        public override bool Equals(object other) => other != null && other is HaystackString str && str.Value == Value;

        public int CompareTo(object obj)
        {
            if (obj == null || obj is not HaystackString str)
            {
                return 1;
            }
            return Value.CompareTo(str.Value);
        }
    }
}