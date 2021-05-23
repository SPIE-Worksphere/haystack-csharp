using System;

namespace ProjectHaystack
{
    /// <summary>
    /// Haystack URI value.
    /// </summary>
    public class HaystackUri : HaystackValue
    {
        public HaystackUri(string value)
        {
            Value = value ?? throw new ArgumentException("Uri cannot be null", nameof(value));
        }

        public string Value { get; }

        // Hash code is based on string value 
        public override int GetHashCode() => Value.GetHashCode();

        // Equals is based on string value 
        public override bool Equals(object other) => other != null && other is HaystackUri uri && uri.Value.Equals(Value);
    }
}