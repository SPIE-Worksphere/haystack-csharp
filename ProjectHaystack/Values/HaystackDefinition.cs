using System;
using ProjectHaystack.Validation;

namespace ProjectHaystack
{
    public class HaystackDefinition : HaystackValue
    {
        public HaystackDefinition(string value)
        {
            Value = value != null && value.StartsWith("^") && HaystackValidator.IsTagName(value)
                ? value
                : throw new ArgumentException($"Invalid definition value: {value}");
        }

        public string Value { get; }

        public override int GetHashCode() => Value.GetHashCode();

        public override bool Equals(object other) => other != null && other is HaystackDefinition def && Value.Equals(def.Value);
    }
}