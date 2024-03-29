using System;
using ProjectHaystack.Validation;

namespace ProjectHaystack
{
    public class HaystackCaretSymbol : HaystackValue
    {
        public HaystackCaretSymbol(string value)
        {
            Value = value == null || HaystackValidator.IsReferenceId(value)
                ? value
                : throw new ArgumentException($"Invalid id val: {value}", "value");
        }

        public string Value { get; }
        public string Code => "^" + Value;

        public override int GetHashCode() => Value.GetHashCode();

        public override bool Equals(object other) => other != null && other is HaystackCaretSymbol caretSymbol && (caretSymbol.Value?.Equals(Value) ?? false);
    }
}