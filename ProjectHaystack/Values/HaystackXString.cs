using System;
using ProjectHaystack.Validation;

namespace ProjectHaystack
{
    /// <summary>
    /// Haystack extended string containing a type and value encoded as a string.
    /// It is used as a generic value when no predefined type is used.
    /// </summary>
    public class HaystackXString : HaystackValue
    {
        public HaystackXString(string value, string type = null)
        {
            Value = value;
            Type = HaystackValidator.IsTypeName(type)
                ? type
                : throw new ArgumentException($"Invalid type name: {type}", nameof(type));
        }

        public string Value { get; }

        public string Type { get; }

        public override int GetHashCode() => Type.GetHashCode() * 31 + Value.GetHashCode();

        public override bool Equals(object other) => other != null && other is HaystackXString xstr && xstr.Value == Value && xstr.Type == Type;
    }
}