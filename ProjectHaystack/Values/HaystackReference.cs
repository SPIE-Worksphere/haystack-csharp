using System;
using ProjectHaystack.Validation;

namespace ProjectHaystack
{
    public class HaystackReference : HaystackValue
    {
        public HaystackReference(string value, string dis = null)
        {
            Value = value != null && HaystackValidator.IsReferenceId(value)
                ? value
                : throw new ArgumentException($"Invalid id val: {value}", "value");
            Display = dis;
        }

        public static HaystackReference nullRef = new HaystackReference("null", null);

        public string Value { get; }
        public string Display { get; }
        public string Code => "@" + Value;

        public override int GetHashCode() => Value.GetHashCode();

        public override bool Equals(object other) => other != null && other is HaystackReference reference && (reference.Value?.Equals(Value) ?? false);
    }
}