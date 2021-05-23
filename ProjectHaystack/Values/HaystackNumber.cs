using System;
using ProjectHaystack.Validation;

namespace ProjectHaystack
{
    /// <summary>
    /// Haystack number with optional unit.
    /// </summary>
    public class HaystackNumber : HaystackValue
    {
        public HaystackNumber(double value, string unit = null)
        {
            if (!HaystackValidator.IsUnitName(unit))
            {
                throw new ArgumentException($"Invalid unit name: {unit}", nameof(unit));
            }

            Value = value;
            Unit = unit;
        }

        public double Value { get; }
        public string Unit { get; }

        public static HaystackNumber ZERO = new HaystackNumber(0.0, null);
        public static HaystackNumber POS_INF = new HaystackNumber(double.PositiveInfinity, null);
        public static HaystackNumber NEG_INF = new HaystackNumber(double.NegativeInfinity, null);
        public static HaystackNumber NaN = new HaystackNumber(double.NaN, null);

        public override int GetHashCode()
        {
            var unsigned = Convert.ToUInt64(BitConverter.DoubleToInt64Bits(Value));
            int hash = (int)(unsigned ^ (unsigned >> 32));
            if (Unit != null)
                hash ^= Unit.GetHashCode();
            return hash;
        }

        public override bool Equals(object other) => other != null && other is HaystackNumber number && number.Value.Equals(Value) && number.Unit == Unit;

        public int CompareTo(object other)
        {
            if (other == null || other is not HaystackNumber number)
            {
                return 1;
            }

            return Value.CompareTo(number.Value);
        }
    }
}