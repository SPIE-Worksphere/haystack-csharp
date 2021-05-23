using System;
using ProjectHaystack.Validation;

namespace ProjectHaystack
{
    /// <summary>
    /// Binary data with mime type.
    /// </summary>
    public class HaystackBinary : HaystackValue
    {
        public string Mime { get; }

        public HaystackBinary(string mimeType)
        {
            Mime = HaystackValidator.IsMimeType(mimeType)
                ? mimeType
                : throw new ArgumentException($"Invalid mime type: {mimeType}", "mimeType");
        }

        public override int GetHashCode() => Mime.GetHashCode();

        public override bool Equals(object other) => other != null && other is HaystackBinary bin && Mime.Equals(bin.Mime);
    }
}